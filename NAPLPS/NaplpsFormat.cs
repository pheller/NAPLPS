// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NAPLPS;

public enum NaplpsSystemType
{
    /// <summary>Standard NAPLPS (version 709) with default color map</summary>
    NAPLPS,

    /// <summary>Original Telidon format (version 699) - files starting with 0x0E (Shift-Out)</summary>
    Telidon,

    /// <summary>Prodigy-style files (8-bit, start with A1 C8 domain command)</summary>
    Prodigy
}

public partial class NaplpsFormat
{
    public bool IsErrored => Errors.Any(e => e.Severity == NaplpsErrorSeverity.Error);

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid { get; private set; }

    /// <summary>Detected system type based on file header patterns</summary>
    public NaplpsSystemType SystemType { get; private set; } = NaplpsSystemType.NAPLPS;

    /// <summary>If we are streaming, we'll assume there is no end and wait indefinately until more data comes in</summary>
    public bool IsStreaming { get; private set; } = false;

    public List<NaplpsError> Errors => State.Errors;

    public List<NaplpsSequence> Commands { get; } = [];

    /// <summary>
    /// Eventually this doesn't need to be in the Format class, but for now it is; considering:
    /// - It's not part of the NAPLPS specification.
    /// - It's implimentation dependent and can vary between different NAPLPS systems.
    /// - We'll eventually want to support different rendering styles for different systems.
    /// </summary>
    public NaplpsState State { get; }

    private NaplpsFormat(BinaryReader reader) : this(reader, new()) { }

    private NaplpsFormat(BinaryReader reader, NaplpsState state)
    {
        State = state;

        // Detect system type from header before parsing
        SystemType = DetectSystemType(reader);
        ApplySystemDefaults();

        Commands = ReadStream(reader);

        IsValid = true;
    }

    /// <summary>
    /// Bare constructor: creates an empty format with the given state and no commands.
    /// Used by the Telidraw compiler's BareFormat mode where the .td source is the
    /// complete byte specification (no CAN+NSR sentinels added).
    /// </summary>
    internal NaplpsFormat(NaplpsState state)
    {
        State = state;
    }

    /// <summary>
    /// Detects the NAPLPS system type based on file header patterns.
    /// - Telidon (699): First byte is 0x0E (Shift-Out) - original 1978 hardware format
    /// - Prodigy: First two bytes are A1 C8 (Domain command in 8-bit mode)
    /// - Standard NAPLPS (709): Everything else
    /// </summary>
    private static NaplpsSystemType DetectSystemType(BinaryReader reader)
    {
        if (reader.BaseStream.Length < 1)
        {
            return NaplpsSystemType.NAPLPS;
        }

        var position = reader.BaseStream.Position;
        var firstByte = reader.ReadByte();

        // Telidon (version 699): starts with 0x0E (Shift-Out command)
        // Original format from 1978 Telidon hardware
        if (firstByte == 0x0E)
        {
            reader.BaseStream.Position = position;
            return NaplpsSystemType.Telidon;
        }

        // Need second byte for Prodigy detection
        if (reader.BaseStream.Length < 2)
        {
            reader.BaseStream.Position = position;
            return NaplpsSystemType.NAPLPS;
        }

        var secondByte = reader.ReadByte();
        reader.BaseStream.Position = position; // Reset to start

        // Prodigy-style: starts with A1 C8 (Domain command with specific operand)
        if (firstByte == 0xA1 && secondByte == 0xC8)
        {
            return NaplpsSystemType.Prodigy;
        }

        return NaplpsSystemType.NAPLPS;
    }

    /// <summary>
    /// Applies system-specific defaults (color maps, etc.) based on detected system type.
    /// </summary>
    private void ApplySystemDefaults()
    {
        switch (SystemType)
        {
            case NaplpsSystemType.Prodigy:
            State.ColorMap = new Dictionary<byte, NaplpsColor>(NaplpsState.ColorMapProdigyDefaults);
            break;

            case NaplpsSystemType.Telidon:
            // Telidon v699: higher default coordinate precision, restricted PDI set
            State.MultiByteValue = 4;
            NaplpsState.TelidonPDISet.CopyTo(State.InUseTable, NaplpsState.GRight);
            break;

            case NaplpsSystemType.NAPLPS:
            default:
            // Default color map is already set in NaplpsState
            break;
        }
    }

    public static NaplpsFormat FromFile(string fullpath)
    {
        var data = File.ReadAllBytes(fullpath);

        return FromBytes(data);
    }

    public static NaplpsFormat New(NaplpsSystemType systemType = NaplpsSystemType.NAPLPS)
    {
        var state = new NaplpsState();

        if (systemType == NaplpsSystemType.Prodigy)
        {
            state.ColorMap = new Dictionary<byte, NaplpsColor>(NaplpsState.ColorMapProdigyDefaults);
        }

        var newFile = new NaplpsFormat(state)
        {
            SystemType = systemType
        };

        if (systemType == NaplpsSystemType.Prodigy)
        {
            // Prodigy files start with Domain command (A1 C8) for auto-detection
            newFile.AddCommand(0xA1, new NaplpsOperands([0xC8]));
        }

        newFile.AddControlCommand(Cancel);
        newFile.AddControlCommand(NonSelectiveReset);

        return newFile;
    }

    public void Save(string fullpath)
    {
        using var file = File.Create(fullpath);
        using var writer = new BinaryWriter(file);

        foreach (var command in Commands)
        {
            writer.Write(command.Command.OpCode);

            foreach (var operand in command.Command.Operands)
            {
                writer.Write(operand);
            }
        }

        writer.Flush();
        file.Close();
    }

    /// <summary>
    /// Adds a PDI command to the end of the command list.
    /// Looks up the command type from the InUseTable, clones state, instantiates via reflection.
    /// </summary>
    public void AddCommand(byte opcode, NaplpsOperands? operands = null)
    {
        operands ??= [];

        var commandReference = State.InUseTable[opcode];

        if (commandReference == null)
        {
            return;
        }

        var currentState = State.Clone();
        var commandType = commandReference.CommandType ?? typeof(NaplpsCommand);
        var commandParameters = commandReference.Parameters;

        var finalCommandParams = commandParameters.Concat([State, opcode, operands]).ToArray();

        if (Activator.CreateInstance(commandType, finalCommandParams) is NaplpsCommand command)
        {
            Commands.Add(new NaplpsSequence(currentState, command));
        }
    }

    /// <summary>
    /// Inserts a PDI command at the specified index.
    /// </summary>
    public void InsertCommand(int index, byte opcode, NaplpsOperands? operands = null)
    {
        operands ??= [];

        var commandReference = State.InUseTable[opcode];

        if (commandReference == null)
        {
            return;
        }

        var currentState = State.Clone();
        var commandType = commandReference.CommandType ?? typeof(NaplpsCommand);
        var commandParameters = commandReference.Parameters;

        var finalCommandParams = commandParameters.Concat([State, opcode, operands]).ToArray();

        if (Activator.CreateInstance(commandType, finalCommandParams) is NaplpsCommand command)
        {
            Commands.Insert(index, new NaplpsSequence(currentState, command));
        }
    }

    /// <summary>
    /// Removes the command at the specified index.
    /// </summary>
    public void RemoveCommand(int index)
    {
        if (index >= 0 && index < Commands.Count)
        {
            Commands.RemoveAt(index);
        }
    }

    /// <summary>
    /// Creates a NaplpsFormat from raw bytes by parsing them through the standard pipeline.
    /// Useful for re-parsing after edits (undo/redo) to rebuild the state chain.
    /// </summary>
    public static NaplpsFormat FromBytes(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        return new NaplpsFormat(reader);
    }

    /// <summary>
    /// Serializes all commands to a byte array.
    /// </summary>
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var command in Commands)
        {
            writer.Write(command.Command.OpCode);

            foreach (var operand in command.Command.Operands)
            {
                writer.Write(operand);
            }
        }

        writer.Flush();

        return stream.ToArray();
    }

    private void AddControlCommand(NaplpsControlCommands command, NaplpsOperands? operands = null)
    {
        var newCommand = new ControlCommand(command, State, (byte)command, operands ?? []);

        if (newCommand.IsValid)
        {
            Commands.Add(new NaplpsSequence(newCommand.State.Clone(), newCommand));
        }
        else
        {
            RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.InvalidCommand, $"Control command {command} produced an invalid NaplpsCommand");
        }
    }

    private void RecordError(NaplpsErrorSeverity severity, NaplpsErrorType type, string message, byte? opcode = null, long? streamPosition = null)
    {
        State.RecordError(severity, type, message, opcode, streamPosition);
    }

    private List<NaplpsSequence> ReadStream(BinaryReader reader, bool isMacroExpansion = false)
    {
        var commands = new List<NaplpsSequence>();

        try
        {
            while (!reader.IsEOF())
            {
                // ANSI X3.110 §6.1.6.3: CAN terminates currently executing macros immediately.
                // Only check inside macro expansion; at top level CAN is a no-op (the flag
                // is cleared by the outer call when it returns).
                if (isMacroExpansion && State.IsCancelRequested)
                {
                    State.IsCancelRequested = false;
                    break;
                }

                var opcode = reader.ReadByte();

                // We operate in 7 bit mode until we get 8 bits,
                // once switched, we can't go back to 7 bit mode.
                if (opcode > 0x80)
                {
                    Is7Bit = false;
                }

                // Buffered modes: macro/DRCS/texture definition consume bytes until END
                if (HandleBufferedByte(opcode, reader, commands))
                {
                    continue;
                }

                // Use ResolveByte so a pending SS2/SS3 single-shift gets consumed by this byte.
                var commandReference = State.ResolveByte(opcode);

                if (commandReference == null)
                {
                    RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.UnknownOpcode, "Unknown opcode in InUseTable", opcode, reader.BaseStream.Position - 1);
                    // Preserve the unknown byte so ToBytes round-trips. The renderer won't
                    // draw it (not IDrawable), but the byte survives serialization.
                    commands.Add(new NaplpsSequence(State.Clone(), new NaplpsCommand(State, opcode, [])));
                    continue;
                }

                var commandType = commandReference.CommandType ?? typeof(NaplpsCommand);
                var commandParameters = commandReference.Parameters;
                var additionalParameters = ReadOperands(reader, commandReference.OperandType);

                if (commandReference.CommandType == typeof(NumericalDataCommand))
                {
                    RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.UnknownOpcode, "NumericalDataCommand reached unexpectedly", opcode, reader.BaseStream.Position);
                    // Preserve the orphan byte as a bare NaplpsCommand so it round-trips
                    // through ToBytes() — historically these bytes (e.g. 0x41 in card1.nap
                    // after ESC D) were silently dropped, breaking byte-level round-trip.
                    commands.Add(new NaplpsSequence(State.Clone(), new NaplpsCommand(State, opcode, [])));
                    continue;
                }

                // Clone the current state before executing the command
                var currentState = State.Clone();

                if (commandType == typeof(ControlCommand) && commandParameters.Count == 1)
                {
                    HandleControlCommand((NaplpsControlCommands)commandParameters[0], reader, additionalParameters, commands);

                    // Re-clone AFTER control command so the sequence's state snapshot
                    // reflects changes made by the handler (cursor position, scroll flag, etc.)
                    currentState = State.Clone();
                    State.ScrollEventOccurred = false;
                }

                var command = TryInstantiateCommand(commandType, commandParameters, opcode, additionalParameters, reader);

                if (command != null)
                {
                    commands.Add(new NaplpsSequence(currentState, command));
                }

                // One-shot: clear scroll flag set by non-ControlCommand constructors
                // (e.g. IncrementalFieldCommand) so only the triggering command carries it.
                State.ScrollEventOccurred = false;
            }
        }
        catch (EndOfStreamException)
        {
            RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.UnexpectedEndOfStream, "Stream ended unexpectedly during parsing");
        }

        return commands;
    }

    /// <summary>
    /// Handles bytes while in a buffered definition mode (macro, DRCS, texture).
    /// Returns true if the byte was consumed by the buffer, false if normal processing should continue.
    /// </summary>
    private bool HandleBufferedByte(byte opcode, BinaryReader reader, List<NaplpsSequence> commands)
    {
        // If we're in macro definition mode, buffer bytes until END.
        // Body bytes are ALSO injected as synthetic raw commands so the Telidraw
        // decompiler can see them and the round-trip preserves every byte.
        if (State.MacroBeingDefined != null)
        {
            if (IsEndCommand(opcode))
            {
                var macroName = State.MacroBeingDefined.Value;
                var macroType = State.MacroDefType;
                State.Macros[macroName] = [.. State.MacroBuffer];
                State.MacroBeingDefined = null;

                // Inject buffered body bytes as individual raw commands for decompiler fidelity.
                foreach (var b in State.MacroBuffer)
                {
                    commands.Add(new NaplpsSequence(State.Clone(), new NaplpsCommand(State, b, [])));
                }

                State.MacroBuffer.Clear();

                // Inject the END command itself.
                commands.Add(new NaplpsSequence(State.Clone(), new ControlCommand(NaplpsControlCommands.End, State, opcode, [])));

                if (macroType == 1 && State.Macros.TryGetValue(macroName, out var macroData))
                {
                    using var macroStream = new MemoryStream(macroData);
                    using var macroReader = new BinaryReader(macroStream);
                    commands.AddRange(ReadStream(macroReader));
                }
            }
            else
            {
                State.MacroBuffer.Add(opcode);
            }

            return true;
        }

        // DRCS definition mode
        if (State.DrcsStartCode != null)
        {
            if (IsEndCommand(opcode))
            {
                ParseDrcsData(State.DrcsStartCode.Value, State.DrcsBuffer);
                State.DrcsStartCode = null;

                foreach (var b in State.DrcsBuffer)
                {
                    commands.Add(new NaplpsSequence(State.Clone(), new NaplpsCommand(State, b, [])));
                }

                State.DrcsBuffer.Clear();
                commands.Add(new NaplpsSequence(State.Clone(), new ControlCommand(NaplpsControlCommands.End, State, opcode, [])));
            }
            else
            {
                State.DrcsBuffer.Add(opcode);
            }

            return true;
        }

        // Texture definition mode
        if (State.TextureBeingDefined != null)
        {
            if (IsEndCommand(opcode))
            {
                ParseTextureData(State.TextureBeingDefined.Value, State.TextureBuffer);
                State.TextureBeingDefined = null;

                foreach (var b in State.TextureBuffer)
                {
                    commands.Add(new NaplpsSequence(State.Clone(), new NaplpsCommand(State, b, [])));
                }

                State.TextureBuffer.Clear();
                commands.Add(new NaplpsSequence(State.Clone(), new ControlCommand(NaplpsControlCommands.End, State, opcode, [])));
            }
            else
            {
                State.TextureBuffer.Add(opcode);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the given opcode maps to the END control command.
    /// </summary>
    private bool IsEndCommand(byte opcode)
    {
        var cmdRef = State.InUseTable[opcode];

        return cmdRef?.CommandType == typeof(ControlCommand) &&
               cmdRef.Parameters.Count == 1 &&
               (NaplpsControlCommands)cmdRef.Parameters[0] == End;
    }

    /// <summary>
    /// Reads operand bytes following a command opcode.
    /// </summary>
    private NaplpsOperands ReadOperands(BinaryReader reader, NaplpsOperandType operandType)
    {
        var operands = new NaplpsOperands();

        if (operandType != NaplpsOperandType.None)
        {
            while (IsValidNumericalDataNext(reader))
            {
                operands.Add(reader.ReadByte());
            }
        }

        return operands;
    }

    /// <summary>
    /// Dispatches a control command to the appropriate handler.
    /// </summary>
    private void HandleControlCommand(NaplpsControlCommands controlCommand, BinaryReader reader, NaplpsOperands additionalParameters, List<NaplpsSequence> commands)
    {
        // Core C0 controls
        if (controlCommand == Escape)
        {
            ControlCommandEscape(reader, additionalParameters);

            // ESC + byte in 0x40-0x5F = 7-bit encoding of C1 control codes.
            // Only dispatch safe state-flag C1 codes — NOT buffer-mode starters
            // (DefMacro, DefTexture, DefDRCS, End) which would swallow subsequent data.
            if (additionalParameters.Count == 1 && additionalParameters[0] >= 0x40 && additionalParameters[0] <= 0x5F)
            {
                byte c1Code = (byte)(additionalParameters[0] + 0x40); // 0x40→0x80, 0x5F→0x9F
                var c1Ref = State.InUseTable[c1Code];
                if (c1Ref?.CommandType == typeof(ControlCommand) && c1Ref.Parameters.Count == 1)
                {
                    var c1Command = (NaplpsControlCommands)c1Ref.Parameters[0];

                    // Skip buffer-mode commands that would be destructive via ESC
                    if (c1Command != DefMacro && c1Command != DefPMacro && c1Command != DefTMacro &&
                        c1Command != DefDRCS && c1Command != DefTexture && c1Command != End)
                    {
                        HandleControlCommand(c1Command, reader, new NaplpsOperands(), commands);
                    }
                }
            }

            State.DoEscape(additionalParameters);
        }
        else if (controlCommand == NonSelectiveReset)
        {
            ControlCommandNonSelectiveReset(reader, additionalParameters);
        }
        else if (controlCommand == ShiftIn)
        {
            State.DoShiftIn();
        }
        else if (controlCommand == ShiftOut)
        {
            State.DoShiftOut();
        }
        else if (controlCommand == Cancel)
        {
            // ANSI X3.110: CAN terminates all currently executing macros immediately.
            // Effect is immediate — not queued.
            State.MacroBeingDefined = null;
            State.MacroBuffer.Clear();
            State.IsCancelRequested = true;
        }
        else if (controlCommand == Bell)
        {
            // ANSI X3.110: BEL triggers an audible or visual alert.
            State.BellCount++;
        }
        // Cursor positioning
        else if (controlCommand == ActivePositionSet)
        {
            // ANSI X3.110: APS (0x1C) sets cursor to row/column position.
            HandleActivePositionSet(reader);
        }
        else if (controlCommand == ClearScreen)
        {
            // ANSI X3.110: Clear screen to nominal black in modes 0/1,
            // background color in mode 2. Move cursor to upper left.
            State.Pen = new Vector3(0f, 0.75f - State.CharSize.Y, 0f);
        }
        else if (controlCommand == ActivePositionDown)
        {
            HandleActivePositionDown();
        }
        else if (controlCommand == ActivePositionUp)
        {
            var pen = State.Pen;
            pen.Y += State.CharSize.Y * GetInterrowMultiplier(State.TextInterrowSpacing);
            State.Pen = pen;
        }
        else if (controlCommand == ActivePositionReturn)
        {
            HandleActivePositionReturn();
        }
        else if (controlCommand == ActivePositionForward)
        {
            HandleActivePositionForward();
        }
        else if (controlCommand == ActivePositionBackward)
        {
            HandleActivePositionBackward();
        }
        else if (controlCommand == ActivePositionHome)
        {
            var pen = State.Pen;
            pen.X = State.Field.Origin.X;
            pen.Y = State.Field.Origin.Y + State.Field.Dimensions.Y - State.CharSize.Y;
            State.Pen = pen;
        }
        // Text attributes
        else if (controlCommand == ReverseVideo) { State.IsReverseVideo = true; }
        else if (controlCommand == NormalVideo) { State.IsReverseVideo = false; }
        else if (controlCommand == UnderLineStart) { State.IsUnderline = true; }
        else if (controlCommand == UnderLineStop) { State.IsUnderline = false; }
        else if (controlCommand == BlinkStart) { State.IsBlinkMode = true; }
        else if (controlCommand == BlinkStop) { State.IsBlinkMode = false; }
        else if (controlCommand == ScrollOn) { State.IsScrollMode = true; }
        else if (controlCommand == ScrollOff) { State.IsScrollMode = false; }
        else if (controlCommand == WordWrapOn) { State.IsWordWrapMode = true; }
        else if (controlCommand == WordWrapOff) { State.IsWordWrapMode = false; }
        else if (controlCommand == Protect) { State.IsProtectMode = true; }
        else if (controlCommand == Unprotect) { State.IsProtectMode = false; }
        // Text size
        else if (controlCommand == SmallText) { State.TextSizeMode = 1; State.CharSize = new Vector2(1.0f / 80.0f, 5.0f / 128.0f); }
        else if (controlCommand == MedText) { State.TextSizeMode = 2; State.CharSize = new Vector2(1.0f / 32.0f, 3.0f / 64.0f); }
        else if (controlCommand == NormalText) { State.TextSizeMode = 0; State.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f); }
        else if (controlCommand == DoubleHeight) { State.TextSizeMode = 3; State.CharSize = new Vector2(1.0f / 40.0f, 10.0f / 128.0f); }
        else if (controlCommand == DoubleSize) { State.TextSizeMode = 4; State.CharSize = new Vector2(2.0f / 40.0f, 10.0f / 128.0f); }
        // Macro/DRCS/texture definitions
        else if (controlCommand == DefMacro) { StartMacroDefinition(additionalParameters, 0); }
        else if (controlCommand == DefPMacro) { StartMacroDefinition(additionalParameters, 1); }
        else if (controlCommand == DefTMacro) { StartMacroDefinition(additionalParameters, 2); }
        // ANSI X3.110 §6.1.3.3: SS2 invokes G2 into the in-use table for ONE next byte (nonlocking).
        // Spec §5.5 macros are invoked by designating the Macro Set into G1/G2/G3 then transmitting
        // a character from that invoked area — NOT via SS2.
        else if (controlCommand == SingleShiftTwo) { State.DoSingleShiftTwo(); }
        // §6.1.3.4: SS3 — same pattern with G3.
        else if (controlCommand == SingleShiftThree) { State.DoSingleShiftThree(); }
        // §6.1.6.4: SDC — null operation at the presentation layer.
        else if (controlCommand == ServiceDelimiterCharacter) { /* no-op per spec */ }
        else if (controlCommand == DefDRCS) { if (additionalParameters.Count > 0) { State.DrcsStartCode = additionalParameters[0]; State.DrcsBuffer.Clear(); } }
        else if (controlCommand == DefTexture) { if (additionalParameters.Count > 0) { State.TextureBeingDefined = additionalParameters[0]; State.TextureBuffer.Clear(); } }
        else if (controlCommand == Repeat)
        {
            // Repeat command: read the count byte and store it in operands
            // Actual repetition happens at render time
            if (!reader.IsEOF())
            {
                additionalParameters.Add(reader.ReadByte());
            }
        }
        // RepeatToEOL doesn't need special handling here - count is calculated at render time
    }

    private void HandleActivePositionSet(BinaryReader reader)
    {
        // Followed by two bytes: row (0x40-0x5F) and column (0x40-0x7F).
        if (!reader.IsEOF())
        {
            byte rowByte = reader.ReadByte();

            if (!reader.IsEOF())
            {
                byte colByte = reader.ReadByte();
                int row = (rowByte & 0x3F); // Strip header bits
                int col = (colByte & 0x3F);

                // Position pen: column * charWidth from field left, row * charHeight from field top
                var pen = State.Pen;
                pen.X = State.Field.Origin.X + col * State.CharSize.X;
                pen.Y = State.Field.Origin.Y + State.Field.Dimensions.Y - row * State.CharSize.Y;
                State.Pen = pen;
            }
        }
    }

    private void HandleActivePositionDown()
    {
        if (State.AutoWrapJustOccurred)
        {
            State.AutoWrapJustOccurred = false;
            return;
        }

        var pen = State.Pen;
        var newY = pen.Y - State.CharSize.Y * GetInterrowMultiplier(State.TextInterrowSpacing);

        if (State.IsScrollMode)
        {
            // PP3 behavior (FUN_2168_02c6): every APD triggers scroll when scroll mode is on.
            // Direction determined by field position in ScrollImage().
            State.ScrollEventOccurred = true;
            pen.Y = newY < State.Field.Origin.Y ? State.Field.Origin.Y : newY;
        }
        else
        {
            pen.Y = newY;
            State.ScrollEventOccurred = false;
        }

        State.Pen = pen;
    }

    private void HandleActivePositionReturn()
    {
        if (!State.AutoWrapJustOccurred)
        {
            var pen = State.Pen;
            pen.X = State.Field.Origin.X;
            State.Pen = pen;
        }
    }

    private void HandleActivePositionForward()
    {
        var pen = State.Pen;
        float fieldRight = State.Field.Origin.X + State.Field.Dimensions.X;
        float fieldLeft = State.Field.Origin.X;

        switch (State.TextPath)
        {
            case TextPath.Right:
            {
                pen.X += State.CharSize.X;

                if (State.Field.Dimensions.X > 0 && pen.X > fieldRight)
                {
                    pen.X = fieldLeft;
                    pen.Y -= State.CharSize.Y;
                }
            }
            break;

            case TextPath.Left:
            {
                pen.X -= State.CharSize.X;

                if (State.Field.Dimensions.X > 0 && pen.X < fieldLeft)
                {
                    pen.X = fieldRight;
                    pen.Y -= State.CharSize.Y;
                }
            }
            break;

            default:
            {
                pen.X += State.CharSize.X;
            }
            break;
        }

        State.Pen = pen;
    }

    private void HandleActivePositionBackward()
    {
        var pen = State.Pen;
        float fieldRight = State.Field.Origin.X + State.Field.Dimensions.X;
        float fieldLeft = State.Field.Origin.X;

        switch (State.TextPath)
        {
            case TextPath.Right:
            {
                pen.X -= State.CharSize.X;

                if (State.Field.Dimensions.X > 0 && pen.X < fieldLeft)
                {
                    pen.X = fieldRight - State.CharSize.X;
                    pen.Y += State.CharSize.Y;
                }
            }
            break;

            case TextPath.Left:
            {
                pen.X += State.CharSize.X;

                if (State.Field.Dimensions.X > 0 && pen.X > fieldRight)
                {
                    pen.X = fieldLeft + State.CharSize.X;
                    pen.Y += State.CharSize.Y;
                }
            }
            break;

            default:
            {
                pen.X -= State.CharSize.X;
            }
            break;
        }

        State.Pen = pen;
    }

    private void StartMacroDefinition(NaplpsOperands operands, byte macroType)
    {
        if (operands.Count > 0)
        {
            State.MacroBeingDefined = (char)operands[0];
            State.MacroDefType = macroType;
            State.MacroBuffer.Clear();
        }
    }

    private void ExecuteMacro(NaplpsOperands operands, List<NaplpsSequence> commands)
    {
        if (operands.Count > 0)
        {
            var macroName = (char)operands[0];

            if (State.Macros.TryGetValue(macroName, out var macroData))
            {
                using var macroStream = new MemoryStream(macroData);
                using var macroReader = new BinaryReader(macroStream);
                // ANSI X3.110 §6.1.6.3: pass isMacroExpansion = true so a CAN inside the
                // macro body terminates it immediately. The outer ReadStream resumes
                // at the next byte after the macro invocation.
                commands.AddRange(ReadStream(macroReader, isMacroExpansion: true));
                State.IsCancelRequested = false;
            }
        }
    }

    /// <summary>
    /// Attempts to instantiate a command from its type and parameters.
    /// Returns null if instantiation fails.
    /// </summary>
    private NaplpsCommand? TryInstantiateCommand(Type commandType, List<object> commandParameters, byte opcode, NaplpsOperands additionalParameters, BinaryReader reader)
    {
        var finalCommandParams = commandParameters.Concat([State, opcode, additionalParameters]).ToArray();

        try
        {
            if (Activator.CreateInstance(commandType, finalCommandParams) is not NaplpsCommand cmd)
            {
                RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.CommandInstantiationFailed, $"Failed to instantiate {commandType.Name}", opcode, reader.BaseStream.Position);
                return null;
            }

            return cmd;
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.CommandInstantiationFailed, $"{commandType.Name} constructor threw: {ex.InnerException?.Message ?? ex.Message}", opcode, reader.BaseStream.Position);
            return null;
        }
    }

    private bool IsValidNumericalDataNext(BinaryReader reader)
    {
        if (reader.IsEOF())
        {
            return false;
        }

        var nextByte = reader.PeekByte();

        var operandReference = State.InUseTable[nextByte];

        var isNumericalData = operandReference.CommandType == typeof(NumericalDataCommand);

        return isNumericalData;
    }

    private static void ControlCommandEscape(BinaryReader reader, NaplpsOperands additionalParameters)
    {
        bool isEscape = true;

        while (isEscape && !reader.IsEOF())
        {
            var peakValue = reader.PeekByte();

            // Check for valid intermediate and final bytes according to NAPLPS standards
            if ((peakValue >= 0x20 && peakValue <= 0x2F) || // Intermediate bytes
                (peakValue >= 0x30 && peakValue <= 0x7E))   // Final bytes
            {
                additionalParameters.Add(reader.ReadByte());
                isEscape = !(peakValue >= 0x30 && peakValue <= 0x7E); // Stop at final character
            }
            else
            {
                isEscape = false;
            }
        }
    }

    private void ControlCommandNonSelectiveReset(BinaryReader reader, NaplpsOperands additionalParameters)
    {
        // ANSI X3.110 NSR: Reset G0-G3, C0, C1 to defaults; reset GL/GR
        State.Reset();
        State.DoShiftIn();

        // Reset DOMAIN parameters to defaults
        State.Dimensionality = 2;
        State.MultiByteValue = 3;
        State.SingleByteValue = 1;
        State.LogicalPel = new Vector2(0f, 0f);

        // Reset text parameters to defaults
        State.TextRotation = TextRotation.Zero;
        State.TextPath = TextPath.Right;
        State.TextSpacing = TextSpacing.One;
        State.TextInterrowSpacing = TextInterrowSpacing.One;
        State.TextMoveAttributes = TextMoveAttributes.MoveTogether;
        State.TextCursorStyle = TextCursorStyle.Underscore;
        State.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);
        State.TextSizeMode = 0;
        State.IsReverseVideo = false;
        State.IsUnderline = false;
        State.IsWordWrapMode = false;
        State.IsScrollMode = false;

        // Reset active field to unit screen
        State.Field = new NaplpsField();

        // Reset texture attributes (programmable masks are NOT cleared)
        State.Texture = new NaplpsTexture();

        // Reset color mode to 0 and drawing color to nominal white
        // Palette is NOT cleared by NSR
        State.ColorMode = 0;
        State.Foreground = new NaplpsColor(255, 255, 255);
        State.ColorMapForeground = 0x07; // Nominal white

        // Reset drawing position
        State.Pen = new Vector3(0f, 0f, 0f);

        // NSR cursor positioning: if two bytes 0x40-0x7F follow, decode row/column.
        // Origin is UPPER LEFT (row 0, col 0 = top-left) - different from 0x1C which uses bottom-left.
        // Capture both bytes into additionalParameters so the serializer re-emits them on ToBytes().
        if (reader.BaseStream.Position + 2 <= reader.BaseStream.Length)
        {
            var peek1 = reader.PeekChar();

            if (peek1 >= 0x40 && peek1 <= 0x7F)
            {
                byte rowByte = reader.ReadByte();
                additionalParameters.Add(rowByte);
                int peek2 = reader.PeekChar();

                if (peek2 >= 0x40 && peek2 <= 0x7F)
                {
                    byte colByte = reader.ReadByte();
                    additionalParameters.Add(colByte);

                    // Extract row/column from bits 6-1 (6 data bits each)
                    int row = (rowByte & 0x3F);
                    int col = (colByte & 0x3F);

                    // Convert from upper-left origin row/col to NAPLPS normalized coords
                    // Row 0 = top of visible display (Y = 0.75 in NAPLPS)
                    // Using default 40x19 visible grid (char field 1/40 x 5/128)
                    float penX = col * (1.0f / 40.0f);
                    float penY = 0.75f - (row * (5.0f / 128.0f));

                    State.Pen = new Vector3(penX, penY, 0f);
                }
            }
        }
    }

    private static float GetInterrowMultiplier(TextInterrowSpacing spacing) => spacing switch
    {
        TextInterrowSpacing.One => 1.0f,
        TextInterrowSpacing.FiveQuarters => 1.25f,
        TextInterrowSpacing.ThreeHalves => 1.5f,
        TextInterrowSpacing.Two => 2.0f,
        _ => 1.0f
    };

    /// <summary>
    /// Parses DRCS bitmap data and stores character definitions.
    /// DRCS format: each character is an 8x10 bitmap (standard),
    /// encoded as 10 bytes (one byte per row, 8 bits per pixel).
    /// </summary>
    private void ParseDrcsData(byte startCode, List<byte> data)
    {
        if (data.Count == 0)
        {
            // Empty definition = reset to space character
            State.DrcsCharacters.Remove(startCode);
            return;
        }

        // ANSI X3.110: DRCS definitions are NAPLPS command streams rendered to an
        // offscreen monochrome bitmap. The bitmap aspect ratio matches the character
        // field dimensions at DEF DRCS time.

        // Determine offscreen bitmap size from character field aspect ratio
        float charW = Math.Abs(State.CharSize.X);
        float charH = Math.Abs(State.CharSize.Y);
        float aspect = charW > 0 && charH > 0 ? charW / charH : 0.625f; // Default 5/8

        // Use a reasonable resolution (larger = more detail, slower)
        int bitmapHeight = 32;
        int bitmapWidth = Math.Max(8, (int)(bitmapHeight * aspect));
        var offscreenSize = new Size(bitmapWidth, bitmapHeight);

        // Try to parse as NAPLPS commands first
        bool parsedAsCommands = false;

        try
        {
            using var stream = new MemoryStream(data.ToArray());
            using var reader = new BinaryReader(stream);

            // Save pen position (spec: drawing point set to 0,0 after DRCS)
            var savedPen = State.Pen;
            State.Pen = new Vector3(0, 0, 0);

            var drcsCommands = ReadStream(reader);

            if (drcsCommands.Count > 0)
            {
                // Render commands to offscreen monochrome image
                using var offscreen = new Image<Rgba32>(bitmapWidth, bitmapHeight);
                offscreen.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.Black));

                Drawing.Drawable.LivePalette = State.ColorMap;

                foreach (var (command, state) in drcsCommands)
                {
                    var drawable = Drawing.DrawContext.ConvertToDrawable(command, state);
                    drawable?.Draw(offscreen, state, offscreenSize);
                }

                Drawing.Drawable.LivePalette = null;

                // Convert to monochrome bitmap (any non-black pixel = set)
                var bitmap = new bool[bitmapHeight, bitmapWidth];

                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        var pixel = offscreen[x, y];
                        bitmap[y, x] = pixel.R > 10 || pixel.G > 10 || pixel.B > 10;
                    }
                }

                State.DrcsCharacters[startCode] = bitmap;
                parsedAsCommands = true;
            }

            // Spec: drawing point set to (0,0) after DRCS definition
            State.Pen = new Vector3(0, 0, 0);
        }
        catch
        {
            // If NAPLPS parsing fails, fall through to raw bitmap interpretation
        }

        if (!parsedAsCommands)
        {
            // Fallback: interpret as raw 8x10 bitmap data (legacy/simple DRCS)
            const int charWidth = 8;
            const int charHeight = 10;
            const int bytesPerChar = charHeight;

            var charCode = startCode;
            var index = 0;

            while (index + bytesPerChar <= data.Count)
            {
                var bitmap = new bool[charHeight, charWidth];

                for (int row = 0; row < charHeight && index < data.Count; row++)
                {
                    byte rowByte = data[index++];

                    for (int col = 0; col < charWidth; col++)
                    {
                        bitmap[row, col] = (rowByte & (0x80 >> col)) != 0;
                    }
                }

                State.DrcsCharacters[charCode] = bitmap;
                charCode++;
            }
        }
    }

    /// <summary>
    /// Parses texture pattern data and stores the mask definition.
    /// Texture patterns are bitmaps used for fill patterns.
    /// </summary>
    private void ParseTextureData(byte maskId, List<byte> data)
    {
        if (data.Count == 0)
        {
            return;
        }

        // Determine pattern size from data length
        // Common sizes are 8x8, 16x16, etc.
        int size = (int)Math.Sqrt(data.Count * 8);

        if (size < 1)
        {
            size = 8;
        }

        var pattern = new bool[size, size];
        int bitIndex = 0;

        for (int row = 0; row < size && bitIndex / 8 < data.Count; row++)
        {
            for (int col = 0; col < size && bitIndex / 8 < data.Count; col++)
            {
                int byteIndex = bitIndex / 8;
                int bitOffset = 7 - (bitIndex % 8);
                pattern[row, col] = (data[byteIndex] & (1 << bitOffset)) != 0;
                bitIndex++;
            }
        }

        // Store the pattern in the appropriate mask slot
        switch (maskId)
        {
            case 0:
            {
                State.TextureMaskA = pattern;
            }
            break;

            case 1:
            {
                State.TextureMaskB = pattern;
            }
            break;

            case 2:
            {
                State.TextureMaskC = pattern;
            }
            break;

            case 3:
            {
                State.TextureMaskD = pattern;
            }
            break;
        }
    }
}
