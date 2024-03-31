// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS;

public partial class NaplpsFormat
{
    public bool IsErrored => Errors.Count > 0;

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid {  get; private set; }

    public IReadOnlyList<NaplpsError> Errors { get; } = [];

    public IReadOnlyList<NaplpsSequence> Commands { get; }

    public NaplpsState State { get; }

    private NaplpsFormat(BinaryReader reader) : this(reader, new()) { }

    private NaplpsFormat(BinaryReader reader, NaplpsState state)
    {
        State = state;

        Commands = ReadStream(reader);

        // Analysis
        Is7Bit = !Commands.Any(cmd => (byte)cmd.Command.OpCode > 0x80);

        IsValid = true;
    }

    public static NaplpsFormat FromFile(string fullpath)
    {
        var file = File.OpenRead(fullpath);

        return new NaplpsFormat(new BinaryReader(file));
    }

    public List<NaplpsSequence> ReadStream(BinaryReader reader)
    {
        var commands = new List<NaplpsSequence>();

        try
        {
            while (!reader.IsEOF())
            {
                var opcode = reader.ReadByte();
                var operands = new NaplpsOperands();

                var shiftIn = opcode == (byte)SHIFT_IN;

                while (!reader.IsEOF() && (!((byte)reader.PeekChar()).IsOpcode() || shiftIn))
                {
                    var operand = reader.ReadByte();

                    if (operand > 0x80)
                    {
                        // Fabled 8-bit NAPLPS
                        Debugger.Break();
                    }

                    operands.Add(operand);

                    if (shiftIn && (byte)reader.PeekChar() == (byte)SHIFT_OUT)
                    {
                        break;
                    }
                }

                var command = NaplpsCommand.Factory(State, (NaplpsCommands)opcode, operands);

                var newStateJson = command.State.ToJson();

                var newState = NaplpsState.FromJson(newStateJson);

                commands.Add(new NaplpsSequence(newState, command));
            }
        }
        catch (EndOfStreamException)
        {
            Debugger.Break();
        }

        return commands;
    }
}
