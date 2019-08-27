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
    public static class EffectEntityFactory
    {

        public static Entity CreateEntity(int _fieldId, int _pieceId, EntityManager _entityManager)
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.EffectComponentType);
            var entity = _entityManager.CreateEntity(archetype);

            _entityManager.SetComponentData(entity, new EffectState
            {
                type = EnumEffectType.None,
            });

            // SharedComponentDataのセット
            // _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
