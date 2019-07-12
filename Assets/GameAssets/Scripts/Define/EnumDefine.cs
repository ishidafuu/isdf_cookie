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

    public enum EnumDrawLayer
    {
        Normal,
        FieldLayer,
        PieceLayer,
        GridLayer,
    }
}
