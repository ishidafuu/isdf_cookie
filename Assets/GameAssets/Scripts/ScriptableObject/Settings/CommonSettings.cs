using System;
using UnityEngine;

namespace NKPB
{
    [CreateAssetMenu(menuName = "Settings/CommonSettings", fileName = "CommonSettings")]
    public sealed class CommonSettings : ScriptableObject
    {
        public int FieldCount;
        public int PlayerCount;
        public int GridRowLength;
        public int GridColumnLength;
        public int PieceCount => (GridRowLength * GridColumnLength);
        public int GridSize;
        public int FieldWidth => (GridSize * GridRowLength);
        public int FieldHeight => (GridSize * GridColumnLength);
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
        public int BorderSpeed;
        public int BorderOnGridDist;
    }
}
