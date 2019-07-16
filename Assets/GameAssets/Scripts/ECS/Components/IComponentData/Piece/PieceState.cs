using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    public struct PieceState : IComponentData
    {
        public EnumPieceType type;
        public int color;
        public boolean isBanish;
        public int combo;
        // public int count;
        // public int imageNo;
    }
}
