using Unity.Entities;
namespace NKPB
{
    public struct PieceId : IComponentData
    {
        public int fieldId;
        public int pieceId;
    }
}
