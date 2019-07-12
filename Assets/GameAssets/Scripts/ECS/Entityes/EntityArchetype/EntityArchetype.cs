using Unity.Entities;
using Unity.Transforms;
namespace NKPB
{
    public static class ComponentTypes
    {
        /// <summary>
        /// フィールド
        /// </summary>
        /// <value></value>
        public static ComponentType[] FieldComponentType = {
            typeof(FieldTag), // タグ
            typeof(FieldId), // ID
            typeof(FieldInput), // スキャンから変換されたフィールド上への入力
            typeof(FieldBanish),
        };

        public static ComponentType[] GridComponentType = {
            typeof(GridState),
        };

        /// <summary>
        /// ピース
        /// </summary>
        /// <value></value>
        public static ComponentType[] PieceComponentType = {
            typeof(PieceTag), // タグ
            typeof(PieceId), // ID
            typeof(PiecePosition),
            typeof(PieceState),
        };
    }
}
