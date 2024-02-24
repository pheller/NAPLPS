// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System;

namespace NAPLPS.Commands;

public enum EscapeCommands : byte
{
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

    PROTECT = 0x80,
    EDC1 = 0x81,
    EDC2 = 0x82,
    EDC3 = 0x83,
    EDC4 = 0x84,
    WORD_WRAP_ON = 0x85,
    WORD_WRAP_OFF = 0x86,
    SCROLL_ON = 0x87,
    SCROLL_OFF = 0x88,
    UNDERLINE_START = 0x89,
    UNDERLINE_STOP = 0x8A,
    FLASH_CURSOR = 0x8B,
    SETADY_CURSOR = 0x8C,
    CURSOR_OFF = 0x8D,
    BLINK_STOP = 0x8E,
    UNPROTECT = 0x8F,
}