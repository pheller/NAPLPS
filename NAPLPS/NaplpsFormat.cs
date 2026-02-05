// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS;

public partial class NaplpsFormat
{
    public bool IsErrored => Errors.Count > 0;

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid { get; private set; }

    /// <summary>If we are streaming, we'll assume there is no end and wait indefinately until more data comes in</summary>
    public bool IsStreaming { get; private set; } = false;

    public List<NaplpsError> Errors { get; } = [];

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

        Commands = ReadStream(reader);

        IsValid = true;
    }

    private NaplpsFormat(NaplpsState state)
    {
        State = state;
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

    private void AddCommand(byte command, NaplpsOperands? operands = null)
    {
        // var newCommand = NaplpsCommand.Factory(State, command, operands);

        // Commands.Add(new NaplpsSequence(newCommand.State.Clone(), newCommand));
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
            // Errors.Add(new NaplpsError());
        }
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
                    if (cmdRef?.CommandType == typeof(ControlCommand) &&
                        cmdRef.Parameters.Count == 1 &&
                        (NaplpsControlCommands)cmdRef.Parameters[0] == End)
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
                    if (cmdRef?.CommandType == typeof(ControlCommand) &&
                        cmdRef.Parameters.Count == 1 &&
                        (NaplpsControlCommands)cmdRef.Parameters[0] == End)
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
                    Debugger.Break();

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
                    // Should never get here??
                    // Debugger.Break();

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
                    else if (controlCommand == ActivePositionDown)
                    {
                        // Move pen down one interrow spacing (NAPLPS Y-up, so subtract)
                        var pen = State.Pen;
                        pen.Y -= State.CharSize.Y * GetInterrowMultiplier(State.TextInterrowSpacing);
                        State.Pen = pen;
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
                        // Carriage return: move pen X to left edge of active field
                        var pen = State.Pen;
                        pen.X = State.Field.Origin.X;
                        State.Pen = pen;
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
                        // Small: half the normal size
                        State.CharSize = new Vector2(1.0f / 80.0f, 5.0f / 256.0f);
                    }
                    else if (controlCommand == MedText)
                    {
                        State.TextSizeMode = 2;
                        // Medium: 3/4 normal size
                        State.CharSize = new Vector2(1.0f / 53.0f, 5.0f / 170.0f);
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
                }

                var finalCommandParams = commandParameters.Concat([State, opcode, additionalParameters]).ToArray();

                if (Activator.CreateInstance(commandType, finalCommandParams) is not NaplpsCommand command)
                {
                    Debugger.Break();

                    continue;
                }

                commands.Add(new NaplpsSequence(currentState, command));
            }
        }
        catch (EndOfStreamException)
        {
            Debugger.Break();
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
        // TODO: Verify these default states!
        // Reset the entire state to its default values
        State.DoShiftIn();  // Reset character set to default
        State.TextRotation = TextRotation.Zero;
        State.TextPath = TextPath.Right;
        State.TextSpacing = TextSpacing.One;
        State.TextInterrowSpacing = TextInterrowSpacing.One;
        State.TextMoveAttributes = TextMoveAttributes.MoveTogether;
        State.TextCursorStyle = TextCursorStyle.Block;
        State.ColorMap.Clear();
        State.ColorMap = new Dictionary<byte, NaplpsColor>(NaplpsState.ColorMapDefaults);
        State.ColorMode = 0;
        State.LogicalPel = new Vector2(0f, 0f);  // Reset logical position
        State.Pen = new Vector3(0f, 0f, 0f);     // Reset drawing position

        // Handle the cursor position if valid bytes follow the NSR
        if (reader.PeekChar() >= 0x40 && reader.PeekChar() <= 0x7F)
        {
            // Read and decode the row address (MSB first, subtract 32)
            var row = (reader.ReadByte() & 0x7F) - 32;
            var column = (reader.ReadByte() & 0x7F) - 32;

            // Normalize cursor position for the screen (assuming 40x25 grid)
            State.LogicalPel = new Vector2(row / 40.0f, column / 25.0f);
        }
        else
        {
            State.LogicalPel = new Vector2(0f, 0f);  // Default if invalid operands
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
        const int charWidth = 8;
        const int charHeight = 10;
        const int bytesPerChar = charHeight; // 1 byte per row for 8-pixel width

        var charCode = startCode;
        var index = 0;

        while (index + bytesPerChar <= data.Count)
        {
            // Create bitmap for this character
            var bitmap = new bool[charHeight, charWidth];

            for (int row = 0; row < charHeight && index < data.Count; row++)
            {
                byte rowByte = data[index++];
                for (int col = 0; col < charWidth; col++)
                {
                    // MSB is leftmost pixel
                    bitmap[row, col] = (rowByte & (0x80 >> col)) != 0;
                }
            }

            // Store the character bitmap
            State.DrcsCharacters[charCode] = bitmap;
            charCode++;
        }
    }

    /// <summary>
    /// Parses texture pattern data and stores the mask definition.
    /// Texture patterns are bitmaps used for fill patterns.
    /// </summary>
    private void ParseTextureData(byte maskId, List<byte> data)
    {
        if (data.Count == 0) return;

        // Determine pattern size from data length
        // Common sizes are 8x8, 16x16, etc.
        int size = (int)Math.Sqrt(data.Count * 8);
        if (size < 1) size = 8;

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
            State.TextureMaskA = pattern;
            break;
            case 1:
            State.TextureMaskB = pattern;
            break;
            case 2:
            State.TextureMaskC = pattern;
            break;
            case 3:
            State.TextureMaskD = pattern;
            break;
        }
    }
}
