﻿using Unity.Entities;
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
    public static class GridEntityFactory
    {
        public static Entity CreateEntity(int _fieldId, int _gridId, EntityManager _entityManager,
            ref MeshMatList _meshMatList
        )
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.GridComponentType);
            var entity = _entityManager.CreateEntity(archetype);

            int posX = _gridId % Define.Instance.Common.GridRowLength;
            int posY = _gridId / Define.Instance.Common.GridColumnLength;

            // 位置
            _entityManager.SetComponentData(entity, new GridState
            {
                gridId = _gridId,
                    fieldId = _fieldId,
                    // position = new Vector2Int(posX, posY),
                    pieceId = _gridId
            });
            // SharedComponentDataのセット
            _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
