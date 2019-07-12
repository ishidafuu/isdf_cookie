using Unity.Entities;
namespace NKPB
{
    /// <summary>
    /// キャラタグ
    /// /// </summary>
    public struct FieldId : IComponentData
    {
        public int fieldId { get; set; }
    }
}
