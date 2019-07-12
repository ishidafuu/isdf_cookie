using System;
using UnityEngine;

namespace NKPB
{
    /// <summary>
    /// 座標移動設定
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/CommonSettings", fileName = "CommonSettings")]
    public sealed class CommonSettings : ScriptableObject
    {
        public int PixelSize;
        public int FieldCount;
        public int PlayerCount;
        public int GridLineLength;
        public int PieceCount;
        public int GridSize;
        public int PieceColorCount;

        public int FieldOffsetX;
        public int FieldOffsetY;
        public int PieceOffsetX;
        public int PieceOffsetY;
        public int GridOffsetX;
        public int GridOffsetY;
        public int SwipeThreshold;
        public int BanishEndCount;
        public int BanishImageCount;
    }
}
