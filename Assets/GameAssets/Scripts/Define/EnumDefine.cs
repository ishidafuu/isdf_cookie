using System;
using System.Collections.Generic;
using UnityEngine;
namespace NKPB
{
    public enum EnumSwipeVec
    {
        None = 0,
        Vertical,
        Horizontal,
    }

    public enum EnumPieceType
    {
        Normal = 0,
        Special = 1,
    }

    public enum EnumBanishPhase
    {
        None,
        BanishStart,
        Banish,
    }

    public enum EnumPieceAlignVec
    {
        None,
        Up,
        Down,
        Left,
        Right
        // AlignMove,
    }
    public enum EnumDrawLayer
    {
        Normal,
        EffectLayer,
        FieldLayer,
        PieceLayer,
        GridLayer,
    }

    public enum EnumFieldInputPhase
    {
        None,
        Hold,
        Align,
        FinishAlign,
    }

    public enum EnumEffectType
    {
        None,
        Banish,
    }
}
