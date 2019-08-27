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
        public static Entity CreateEntity(int _fieldId, int _pieceId, EntityManager _entityManager)
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.PieceComponentType);
            var entity = _entityManager.CreateEntity(archetype);
            // ID
            _entityManager.SetComponentData(entity, new PieceId
            {
                fieldId = _fieldId,
                pieceId = _pieceId,
            });

            int gridPosX = _pieceId % Settings.Instance.Common.GridRowLength;
            int gridPosY = _pieceId / Settings.Instance.Common.GridRowLength;
            int posX = gridPosX * Settings.Instance.Common.GridSize;
            int posY = gridPosY * Settings.Instance.Common.GridSize;

            // 位置
            _entityManager.SetComponentData(entity, new PiecePosition
            {
                position = new Vector2Int(posX, posY),
                gridPosition = new Vector2Int(gridPosX, gridPosY)
            });

            _entityManager.SetComponentData(entity, new PieceState
            {
                type = EnumPieceType.Normal,
                color = (int)UnityEngine.Random.Range(0, Settings.Instance.Common.PieceColorCount),
            });

            // SharedComponentDataのセット
            // _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
