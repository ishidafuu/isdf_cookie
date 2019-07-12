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

        /// <summary>
        /// キャラエンティティ作成
        /// </summary>
        /// <param name="i"></param>
        /// <param name="entityManager"></param>
        /// <param name="ariMeshMat"></param>
        /// <param name="aniScriptSheet"></param>
        /// <param name="aniBasePos"></param>
        /// <returns></returns>
        public static Entity CreateEntity(int _i, EntityManager _entityManager,
            ref MeshMatList _meshMatList
        )
        {
            var archetype = _entityManager.CreateArchetype(ComponentTypes.FieldComponentType);
            var entity = _entityManager.CreateEntity(archetype);

            // 必要なキャラのみインプットをつける
            if (_i < Define.Instance.Common.PlayerCount)
            {
                _entityManager.AddComponent(entity, ComponentType.Create<FieldScan>());
            }

            // ID
            _entityManager.SetComponentData(entity, new FieldId
            {
                fieldId = _i,
            });

            _entityManager.SetComponentData(entity, new FieldBanish
            {
                isBanish = false,
                    count = 0,
            });

            // SharedComponentDataのセット
            _entityManager.AddSharedComponentData(entity, _meshMatList);

            return entity;
        }
    }
}
