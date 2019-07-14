using System;
using System.Collections.Generic;
using UnityEngine;
namespace NKPB
{
    public enum EnumSwipeType
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

    // public enum EnumPieceMoveType
    // {
    //     Stop,
    //     HoldMove,
    //     SlipMove,
    //     // AlignMove,
    // }

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
}
