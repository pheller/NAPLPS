// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;
using System.Diagnostics;
using static NAPLPS.Commands.NaplpsCommand;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS;

public class NaplpsFile
{
    public bool IsErrored => Errors.Count > 0;

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid {  get; private set; }

    public IReadOnlyList<NaplpsError> Errors { get; } = [];

    public IReadOnlyList<NaplpsCommand> Commands { get; }

    public NaplpsState State { get; } = new();

    private NaplpsFile(BinaryReader reader)
    {
        Commands = ReadStream(reader);

        // Analysis
        Is7Bit = !Commands.Any(cmd => (byte)cmd.OpCode > 0x80);
    }

    public static NaplpsFile FromFile(string fullpath)
    {
        var file = File.OpenRead(fullpath);

        return new NaplpsFile(new BinaryReader(file));
    }

    public static List<NaplpsCommand> ReadStream(BinaryReader reader)
    {
        var commands = new List<NaplpsCommand>();

        try
        {
            while (!reader.IsEOF())
            {
                var opcode = reader.ReadByte();
                var operands = new List<byte>();

                var shiftIn = opcode == (byte)SHIFT_IN;

                while (!reader.IsEOF() && (!IsOpcode((byte)reader.PeekChar()) || shiftIn))
                {
                    var operand = reader.ReadByte();

                    if (operand > 0x80)
                    {
                        Debugger.Break();
                    }

                    operands.Add(operand);

                    if (shiftIn && (byte)reader.PeekChar() == (byte)SHIFT_OUT)
                    {
                        break;
                    }
                }

                var command = Factory((NaplpsCommands)opcode, operands);

                commands.Add(command);
            }
        }
        catch (EndOfStreamException)
        {
            Debugger.Break();
        }

        return commands;
    }
}
