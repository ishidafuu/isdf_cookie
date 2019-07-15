using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
// using Unity.Transforms2D;
using Unity.Collections;
// using toinfiniityandbeyond.Rendering2D;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace NKPB
{
    public static class PieceEntityFactory
    {
        public static Entity CreateEntity(int _fieldId, int _pieceId, EntityManager _entityManager,
            ref MeshMatList _meshMatList
        )
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.PieceComponentType);
            var entity = _entityManager.CreateEntity(archetype);
            // ID
            _entityManager.SetComponentData(entity, new PieceId
            {
                fieldId = _fieldId,
                    pieceId = _pieceId,
            });

            // ComponentDataのセット
            // float posX = (_pieceId % Define.Instance.Common.GridLineLength * Define.Instance.Common.GridSize)
            //     + Define.Instance.Common.FieldOffsetX + Define.Instance.Common.PieceOffsetX;
            // float posY = (_pieceId / Define.Instance.Common.GridLineLength * Define.Instance.Common.GridSize)
            //     + Define.Instance.Common.FieldOffsetX + Define.Instance.Common.PieceOffsetY;
            int gridPosX = _pieceId % Define.Instance.Common.GridLineLength;
            int gridPosY = _pieceId / Define.Instance.Common.GridLineLength;
            int posX = gridPosX * Define.Instance.Common.GridSize;
            int posY = gridPosY * Define.Instance.Common.GridSize;

            // 位置
            _entityManager.SetComponentData(entity, new PiecePosition
            {
                position = new Vector2Int(posX, posY),
                    gridPosition = new Vector2Int(gridPosX, gridPosY)
            });

            _entityManager.SetComponentData(entity, new PieceState
            {
                type = EnumPieceType.Normal,
                    color = (int)UnityEngine.Random.Range(0, Define.Instance.Common.PieceColorCount),
            });

            // SharedComponentDataのセット
            _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
