using Unity.Entities;
namespace NKPB
{
    /// <summary>
    /// キャラ識別情報
    /// </summary>
    public struct PieceId : IComponentData
    {
        public int fieldId;
        public int pieceId;
    }
}
