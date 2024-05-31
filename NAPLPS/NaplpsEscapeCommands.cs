// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public enum NaplpsEscapeCommands : byte
{
    C0 = 0x21,
    C1 = 0x22,

    DOLLAR_SIGN = 0x24,

    G0 = 0x28,
    G1 = 0x29,
    G2 = 0x2A,
    G3 = 0x2B,

    G1D = 0x2C,
    G2D = 0x2D,
    G3D = 0x2E,

    DEF_MACRO = 0x40,
    DEFP_MACRO = 0x41,
    DEFT_MACRO = 0x42,
    DEF_DRCS = 0x43,
    DEF_TEXTURE = 0x44,
    END = 0x45,
    REPEAT = 0x46,
    REPEAT_TO_EOL = 0x47,
    REVERSE_VIDEO = 0x48,
    NORMAL_VIDEO = 0x49,
    SMALL_TEXT = 0x4A,
    MED_TEXT = 0x4B,
    NORMAL_TEXT = 0x4C,
    DOUBLE_HEIGHT = 0x4D,
    BLINK_START = 0x4E,
    DOUBLE_SIZE = 0x4F,

    PROTECT = 0x50,
    EDC1 = 0x51,
    EDC2 = 0x52,
    EDC3 = 0x53,
    EDC4 = 0x54,
    WORD_WRAP_ON = 0x55,
    WORD_WRAP_OFF = 0x56,
    SCROLL_ON = 0x57,
    SCROLL_OFF = 0x5,
    UNDERLINE_START = 0x59,
    UNDERLINE_STOP = 0x5A,
    FLASH_CURSOR = 0x5B,
    SETADY_CURSOR = 0x5C,
    CURSOR_OFF = 0x5D,
    BLINK_STOP = 0x5E,
    UNPROTECT = 0x5F,

    LOCKINGSHIFT_THREE = 0x6F,
}