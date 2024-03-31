// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;
using static NAPLPS.NaplpsEscapeCommands;

namespace NAPLPS.Commands;

public class NaplpsCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands)
{
    public NaplpsCommands OpCode { get; } = opcode;

    public NaplpsOperands Operands { get; } = operands;

    public bool IsValid { get; internal set; } = true;

    public NaplpsState State { get; } = state;

    public static NaplpsCommand Factory(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands)
    {
        return opcode switch
        {
            CANCEL => new CancelCommand(state, operands),
            NSR => new NonSelectiveResetCommand(state, operands),
            ESC => ProcessEscapeSequence(state, opcode, operands),
            SHIFT_OUT => new ShiftOutCommand(state, operands),
            SHIFT_IN => new ShiftInCommand(state, operands),
            RESET => new ResetCommand(state, operands),
            DOMAIN => new DomainCommand(state, operands),
            TEXT => new TextCommand(state, operands),
            WAIT => new WaitCommand(state, operands),
            SELECT_COLOR => new SelectColorCommand(state, operands),
            SET_COLOR => new SetColorCommand(state, operands),
            TEXTURE => new TextureCommand(state, operands),
            POLYGON_OUTLINED => new PolygonOutlinedCommand(state, operands),
            POLYGON_FILLED => new PolygonFilledCommand(state, operands),
            POLYGON_SET_OUTLINED => new PolygonSetOutlinedCommand(state, operands),
            POLYGON_SET_FILLED => new PolygonSetFilledCommand(state, operands),
            RECTANGLE_OUTLINED => new RectangleOutlinedCommand(state, operands),
            RECTANGLE_FILLED => new RectangleFilledCommand(state, operands),
            RECTANGLE_SET_OUTLINED => new RectangleSetOutlinedCommand(state, operands),
            RECTANGLE_SET_FILLED => new RectangleSetFilledCommand(state, operands),
            ARC_OUTLINED => new ArcOutlinedCommand(state, operands),
            ARC_FILLED => new ArcFilledCommand(state, operands),
            ARC_SET_OUTLINED => new ArcSetOutlinedCommand(state, operands),
            ARC_SET_FILLED => new ArcSetFilledCommand(state, operands),
            POINT_ABS => new PointAbsoluteCommand(state, operands),
            POINT_REL => new PointRelativeCommand(state, operands),
            POINT_SET_ABS => new PointSetAbsoluteCommand(state, operands),
            POINT_SET_REL => new PointSetRelativeCommand(state, operands),
            LINE_ABS => new LineAbsoluteCommand(state, operands),
            LINE_REL => new LineRelativeCommand(state, operands),
            LINE_SET_ABS => new LineSetAbsoluteCommand(state, operands),
            LINE_SET_REL => new LineSetRelativeCommand(state, operands),
            _ => BreakAndReturn(state, opcode, operands),
        };
    }

    private static NaplpsCommand ProcessEscapeSequence(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands)
    {
        if (operands.Count == 0)
        {
            return new EscCommand(state, operands);
        }

        return (NaplpsEscapeCommands)operands[0] switch
        {
            DEF_TEXTURE => new DefTextureCommand(state, operands),
            END => new EndCommand(state, operands),
            _ => BreakAndReturn(state, opcode, operands),
        };
    }

    private static NaplpsCommand BreakAndReturn(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands)
    {
        var newUnknownCommand = new NaplpsCommand(state, opcode, operands);

        Debugger.Break();

        return newUnknownCommand;
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}