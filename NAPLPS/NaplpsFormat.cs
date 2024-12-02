// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS;

public partial class NaplpsFormat
{
    public bool IsErrored => Errors.Count > 0;

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid { get; private set; }

    public List<NaplpsError> Errors { get; } = [];

    public List<NaplpsSequence> Commands { get; } = [];

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

        // Default
        newFile.AddCommand(CANCEL);

        return newFile;
    }

    private void AddCommand(byte cmd, NaplpsOperands? operands = null)
    {
        AddCommand((NaplpsCommands)cmd, operands);
    }

    private void AddCommand(NaplpsCommands command, NaplpsOperands? operands = null)
    {
        var newCommand = NaplpsCommand.Factory(State, command, operands);

        Commands.Add(new NaplpsSequence(newCommand.State.Clone(), newCommand));
    }

    public List<NaplpsSequence> ReadStream(BinaryReader reader)
    {
        var commands = new List<NaplpsSequence>();
        byte oldBit = 0x00;

        try
        {
            while (!reader.IsEOF())
            {
                var opcode = reader.ReadByte();

                if (opcode > 0x80) // We're in 8Bit mode, we treat escape sequences "differently"
                {
                    oldBit = opcode;

                    Is7Bit = false;

                    opcode ^= 0x80;
                }

                var operands = new NaplpsOperands();

                var shiftIn = opcode == (byte)SHIFT_IN;
                var escCmd = opcode == (byte)ESC;

                while (!reader.IsEOF() && (!reader.PeekByte().IsOpcode() || shiftIn || escCmd))
                {
                    var operand = reader.ReadByte();

                    operands.Add(operand);

                    if (shiftIn && reader.PeekByte() == (byte)SHIFT_OUT)
                    {
                        break;
                    }
                    else if (escCmd && operand == 0x22)  // Switching to C1 default set
                    {
                        operands.Add(reader.ReadByte());

                        break;
                    }
                    else if (reader.PeekByte().IsOpcode() && escCmd)
                    {
                        break;
                    }
                }

                var command = NaplpsCommand.Factory(State, (NaplpsCommands)opcode, operands);

                commands.Add(new NaplpsSequence(command.State.Clone(), command));
            }
        }
        catch (EndOfStreamException)
        {
            Debugger.Break();
        }

        return commands;
    }
}
