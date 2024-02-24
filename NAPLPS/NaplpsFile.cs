// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;
using System.Diagnostics;
using static NAPLPS.Commands.NaplpsCommand;
using static NAPLPS.Commands.NaplpsCommands;

namespace NAPLPS;

public class NaplpsFile
{
    public List<NaplpsCommand> Commands { get; }

    public NaplpsState State { get; } = new();

    private NaplpsFile(BinaryReader reader)
    {
        Commands = ReadStream(reader);
    }

    public NaplpsFile FromFile(string fullpath)
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

                if (opcode > 0x80)
                {
                    Debugger.Break();
                }

                var shiftIn = opcode == (byte)SHIFT_IN;

                if (shiftIn)
                {
                    // Debugger.Break();
                }

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

                var command = Factory(opcode, operands);

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
