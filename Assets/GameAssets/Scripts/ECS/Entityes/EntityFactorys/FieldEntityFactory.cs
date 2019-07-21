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
    public static class FieldEntityFactory
    {
        public static Entity CreateEntity(int _i, EntityManager _entityManager,
            ref MeshMatList _meshMatList
        )
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.FieldComponentType);
            var entity = _entityManager.CreateEntity(archetype);

            // 必要なキャラのみインプットをつける
            if (_i < Settings.Instance.Common.PlayerCount)
            {
                _entityManager.AddComponent(entity, ComponentType.ReadWrite<FieldScan>());
            }

            _entityManager.SetComponentData(entity, new FieldBanish
            {
                phase = EnumBanishPhase.None,
                    count = 0,
            });

            _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
