// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

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
            writer.Write((byte)command.Command.OpCode);

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
                    Debugger.Break();

                    continue;
                }

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
                }

                var finalCommandParams = commandParameters.Concat([State, opcode, additionalParameters]).ToArray();

                if (Activator.CreateInstance(commandType, finalCommandParams) is not NaplpsCommand command)
                {
                    Debugger.Break();

                    continue;
                }

                commands.Add(new NaplpsSequence(command.State.Clone(), command));
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
}
