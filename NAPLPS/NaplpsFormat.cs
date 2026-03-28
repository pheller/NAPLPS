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

    private NaplpsFormat(NaplpsState state)
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
        var file = File.OpenRead(fullpath);

        return new NaplpsFormat(new BinaryReader(file));
    }

    public static NaplpsFormat New()
    {
        var newFile = new NaplpsFormat(new NaplpsState());

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

    private List<NaplpsSequence> ReadStream(BinaryReader reader)
    {
        var commands = new List<NaplpsSequence>();

        try
        {
            while (!reader.IsEOF())
            {
                var opcode = reader.ReadByte();

                // We operate in 7 bit mode until we get 8 bits,
                // once switched, we can't go back to 7 bit mode.
                if (opcode > 0x80)
                {
                    Is7Bit = false;
                }

                // If we're in macro definition mode, buffer bytes until END
                if (State.MacroBeingDefined != null)
                {
                    // Check for END command (0x85 in C1 set, or via lookup)
                    var cmdRef = State.InUseTable[opcode];
                    if (cmdRef?.CommandType == typeof(ControlCommand) &&
                        cmdRef.Parameters.Count == 1 &&
                        (NaplpsControlCommands)cmdRef.Parameters[0] == End)
                    {
                        // End macro definition
                        var macroName = State.MacroBeingDefined.Value;
                        var macroType = State.MacroDefType;
                        State.Macros[macroName] = [.. State.MacroBuffer];
                        State.MacroBeingDefined = null;
                        State.MacroBuffer.Clear();

                        // If DEFP MACRO (type 1), execute the macro immediately
                        if (macroType == 1 && State.Macros.TryGetValue(macroName, out var macroData))
                        {
                            // Execute macro by parsing its bytes
                            using var macroStream = new MemoryStream(macroData);
                            using var macroReader = new BinaryReader(macroStream);
                            var macroCommands = ReadStream(macroReader);
                            commands.AddRange(macroCommands);
                        }
                        continue;
                    }

                    // Buffer this byte as part of the macro
                    State.MacroBuffer.Add(opcode);
                    continue;
                }

                // If we're in DRCS definition mode, buffer bytes until END
                if (State.DrcsStartCode != null)
                {
                    var cmdRef = State.InUseTable[opcode];

                    if (cmdRef?.CommandType == typeof(ControlCommand) && cmdRef.Parameters.Count == 1 && (NaplpsControlCommands)cmdRef.Parameters[0] == End)
                    {
                        // End DRCS definition - parse the bitmap data
                        ParseDrcsData(State.DrcsStartCode.Value, State.DrcsBuffer);
                        State.DrcsStartCode = null;
                        State.DrcsBuffer.Clear();
                        continue;
                    }

                    // Buffer this byte as part of the DRCS data
                    State.DrcsBuffer.Add(opcode);

                    continue;
                }

                // If we're in texture definition mode, buffer bytes until END
                if (State.TextureBeingDefined != null)
                {
                    var cmdRef = State.InUseTable[opcode];

                    if (cmdRef?.CommandType == typeof(ControlCommand) && cmdRef.Parameters.Count == 1 && (NaplpsControlCommands)cmdRef.Parameters[0] == End)
                    {
                        // End texture definition - parse the pattern data
                        ParseTextureData(State.TextureBeingDefined.Value, State.TextureBuffer);
                        State.TextureBeingDefined = null;
                        State.TextureBuffer.Clear();

                        continue;
                    }

                    // Buffer this byte as part of the texture data
                    State.TextureBuffer.Add(opcode);

                    continue;
                }

                var commandReference = State.InUseTable[opcode];

                if (commandReference == null)
                {
                    RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.UnknownOpcode, "Unknown opcode in InUseTable", opcode, reader.BaseStream.Position - 1);

                    continue;
                }

                var commandType = commandReference.CommandType ?? typeof(NaplpsCommand);
                var commandParameters = commandReference.Parameters;

                var operandType = commandReference.OperandType;
                var additionalParameters = new NaplpsOperands();

                if (operandType != NaplpsOperandType.None)
                {
                    while (IsValidNumericalDataNext(reader))
                    {
                        additionalParameters.Add(reader.ReadByte());
                    }
                }

                if (commandReference.CommandType == typeof(NumericalDataCommand))
                {
                    RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.UnknownOpcode, "NumericalDataCommand reached unexpectedly", opcode, reader.BaseStream.Position);

                    continue;
                }

                // Clone the current state before executing the command
                var currentState = State.Clone();

                if (commandType == typeof(ControlCommand) && commandParameters.Count == 1)
                {
                    var controlCommand = (NaplpsControlCommands)commandParameters[0];

                    if (controlCommand == Escape)
                    {
                        ControlCommandEscape(reader, additionalParameters);

                        State.DoEscape(additionalParameters);
                    }
                    else if (controlCommand == NonSelectiveReset)
                    {
                        ControlCommandNonSelectiveReset(reader);
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
                        // Cancel all running macros and do not treat this as a queued command, if we're queuing...
                        // Noop
                    }
                    else if (controlCommand == ClearScreen)
                    {
                        // ANSI X3.110: Clear screen to nominal black in modes 0/1,
                        // background color in mode 2. Move cursor to upper left.
                        State.Pen = new Vector3(0f, 0.75f - State.CharSize.Y, 0f);
                    }
                    else if (controlCommand == ActivePositionDown)
                    {
                        // ANSI X3.110 §5.3.2.3.6: After an automatic APR+APD, an explicit
                        // APR+APD (or APD+APR) is a null operation. Suppress the APD and
                        // clear the flag (the APR was already a no-op since we're at field origin).
                        if (State.AutoWrapJustOccurred)
                        {
                            State.AutoWrapJustOccurred = false;
                        }
                        else
                        {
                            // Move pen down one interrow spacing (NAPLPS Y-up, so subtract)
                            var pen = State.Pen;
                            var newY = pen.Y - State.CharSize.Y * GetInterrowMultiplier(State.TextInterrowSpacing);

                            if (State.IsScrollMode && newY < State.Field.Origin.Y)
                            {
                                // Scroll: clamp pen to bottom of field and flag scroll event
                                pen.Y = State.Field.Origin.Y;
                                State.ScrollEventOccurred = true;
                            }
                            else
                            {
                                pen.Y = newY;
                                State.ScrollEventOccurred = false;
                            }
                            State.Pen = pen;
                        }
                    }
                    else if (controlCommand == ActivePositionUp)
                    {
                        // Move pen up one interrow spacing
                        var pen = State.Pen;
                        pen.Y += State.CharSize.Y * GetInterrowMultiplier(State.TextInterrowSpacing);
                        State.Pen = pen;
                    }
                    else if (controlCommand == ActivePositionReturn)
                    {
                        if (State.AutoWrapJustOccurred)
                        {
                            // ANSI X3.110 §5.3.2.3.6: Part of the explicit APR+APD pair
                            // after auto-wrap — execute as null operation. Keep the flag
                            // set so the paired APD is also suppressed.
                        }
                        else
                        {
                            // Carriage return: move pen X to left edge of active field
                            var pen = State.Pen;
                            pen.X = State.Field.Origin.X;
                            State.Pen = pen;
                        }
                    }
                    else if (controlCommand == ActivePositionForward)
                    {
                        // Tab forward: advance pen one character width along text path
                        var pen = State.Pen;
                        pen.X += State.CharSize.X;
                        State.Pen = pen;
                    }
                    else if (controlCommand == ActivePositionBackward)
                    {
                        // Backspace: move pen one character width backward along text path
                        var pen = State.Pen;
                        pen.X -= State.CharSize.X;
                        State.Pen = pen;
                    }
                    else if (controlCommand == ActivePositionHome)
                    {
                        // Move pen to top-left of active field (origin X, top Y minus one char height)
                        var pen = State.Pen;
                        pen.X = State.Field.Origin.X;
                        pen.Y = State.Field.Origin.Y + State.Field.Dimensions.Y - State.CharSize.Y;
                        State.Pen = pen;
                    }
                    // C1 Control Commands
                    else if (controlCommand == ReverseVideo)
                    {
                        State.IsReverseVideo = true;
                    }
                    else if (controlCommand == NormalVideo)
                    {
                        State.IsReverseVideo = false;
                    }
                    else if (controlCommand == SmallText)
                    {
                        State.TextSizeMode = 1;
                        // ANSI X3.110: 1/80 wide x 5/128 high (80x19 screen)
                        State.CharSize = new Vector2(1.0f / 80.0f, 5.0f / 128.0f);
                    }
                    else if (controlCommand == MedText)
                    {
                        State.TextSizeMode = 2;
                        // ANSI X3.110: 1/32 wide x 3/64 high (32x15 screen)
                        State.CharSize = new Vector2(1.0f / 32.0f, 3.0f / 64.0f);
                    }
                    else if (controlCommand == NormalText)
                    {
                        State.TextSizeMode = 0;
                        // Normal: default size (1/40 x 5/128)
                        State.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);
                    }
                    else if (controlCommand == DoubleHeight)
                    {
                        State.TextSizeMode = 3;
                        // Double height: normal width, 2x height
                        State.CharSize = new Vector2(1.0f / 40.0f, 10.0f / 128.0f);
                    }
                    else if (controlCommand == DoubleSize)
                    {
                        State.TextSizeMode = 4;
                        // Double size: 2x width and 2x height
                        State.CharSize = new Vector2(2.0f / 40.0f, 10.0f / 128.0f);
                    }
                    else if (controlCommand == UnderLineStart)
                    {
                        State.IsUnderline = true;
                    }
                    else if (controlCommand == UnderLineStop)
                    {
                        State.IsUnderline = false;
                    }
                    else if (controlCommand == BlinkStart)
                    {
                        State.IsBlinkMode = true;
                    }
                    else if (controlCommand == BlinkStop)
                    {
                        State.IsBlinkMode = false;
                    }
                    else if (controlCommand == ScrollOn)
                    {
                        State.IsScrollMode = true;
                    }
                    else if (controlCommand == ScrollOff)
                    {
                        State.IsScrollMode = false;
                    }
                    else if (controlCommand == WordWrapOn)
                    {
                        State.IsWordWrapMode = true;
                    }
                    else if (controlCommand == WordWrapOff)
                    {
                        State.IsWordWrapMode = false;
                    }
                    else if (controlCommand == Protect)
                    {
                        State.IsProtectMode = true;
                    }
                    else if (controlCommand == Unprotect)
                    {
                        State.IsProtectMode = false;
                    }
                    // Macro definition commands
                    else if (controlCommand == DefMacro)
                    {
                        // Read macro name from operands
                        if (additionalParameters.Count > 0)
                        {
                            State.MacroBeingDefined = (char)additionalParameters[0];
                            State.MacroDefType = 0; // Standard macro
                            State.MacroBuffer.Clear();
                        }
                    }
                    else if (controlCommand == DefPMacro)
                    {
                        // Read macro name from operands - execute after definition
                        if (additionalParameters.Count > 0)
                        {
                            State.MacroBeingDefined = (char)additionalParameters[0];
                            State.MacroDefType = 1; // Execute after definition
                            State.MacroBuffer.Clear();
                        }
                    }
                    else if (controlCommand == DefTMacro)
                    {
                        // Transmit macro - sends content to host when invoked
                        if (additionalParameters.Count > 0)
                        {
                            State.MacroBeingDefined = (char)additionalParameters[0];
                            State.MacroDefType = 2; // Transmit macro
                            State.MacroBuffer.Clear();
                        }
                    }
                    else if (controlCommand == SingleShiftTwo)
                    {
                        // SS2 invokes a macro - next byte is macro name
                        if (additionalParameters.Count > 0)
                        {
                            var macroName = (char)additionalParameters[0];
                            if (State.Macros.TryGetValue(macroName, out var macroData))
                            {
                                // Execute macro by parsing its bytes
                                using var macroStream = new MemoryStream(macroData);
                                using var macroReader = new BinaryReader(macroStream);
                                var macroCommands = ReadStream(macroReader);
                                commands.AddRange(macroCommands);
                            }
                        }
                    }
                    else if (controlCommand == DefDRCS)
                    {
                        // Start DRCS definition - first operand is starting character code
                        if (additionalParameters.Count > 0)
                        {
                            State.DrcsStartCode = additionalParameters[0];
                            State.DrcsBuffer.Clear();
                        }
                    }
                    else if (controlCommand == DefTexture)
                    {
                        // Start texture pattern definition
                        // First operand specifies which mask (A=0, B=1, C=2, D=3)
                        if (additionalParameters.Count > 0)
                        {
                            State.TextureBeingDefined = additionalParameters[0];
                            State.TextureBuffer.Clear();
                        }
                    }
                    else if (controlCommand == Repeat)
                    {
                        // Repeat command: read the count byte and store it in operands
                        // Actual repetition happens at render time
                        if (!reader.IsEOF())
                        {
                            var countByte = reader.ReadByte();
                            additionalParameters.Add(countByte);
                        }
                    }
                    // RepeatToEOL doesn't need special handling here - count is calculated at render time
                }

                var finalCommandParams = commandParameters.Concat([State, opcode, additionalParameters]).ToArray();

                NaplpsCommand command;

                try
                {
                    if (Activator.CreateInstance(commandType, finalCommandParams) is not NaplpsCommand cmd)
                    {
                        RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.CommandInstantiationFailed, $"Failed to instantiate {commandType.Name}", opcode, reader.BaseStream.Position);

                        continue;
                    }

                    command = cmd;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.CommandInstantiationFailed, $"{commandType.Name} constructor threw: {ex.InnerException?.Message ?? ex.Message}", opcode, reader.BaseStream.Position);

                    continue;
                }

                commands.Add(new NaplpsSequence(currentState, command));
            }
        }
        catch (EndOfStreamException)
        {
            RecordError(NaplpsErrorSeverity.Error, NaplpsErrorType.UnexpectedEndOfStream, "Stream ended unexpectedly during parsing");
        }

        return commands;
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

    private void ControlCommandNonSelectiveReset(BinaryReader reader)
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

        // NSR cursor positioning: if two bytes 0x40-0x7F follow, decode row/column
        // Origin is UPPER LEFT (row 0, col 0 = top-left) - different from 0x1C which uses bottom-left
        if (reader.BaseStream.Position + 2 <= reader.BaseStream.Length)
        {
            var peek1 = reader.PeekChar();

            if (peek1 >= 0x40 && peek1 <= 0x7F)
            {
                byte rowByte = reader.ReadByte();
                int peek2 = reader.PeekChar();

                if (peek2 >= 0x40 && peek2 <= 0x7F)
                {
                    byte colByte = reader.ReadByte();

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
