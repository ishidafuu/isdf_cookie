using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace NKPB
{
    /// </summary>
    sealed class Manager_Main : MonoBehaviour
    {
        const string SCENE_NAME = "Main";

        List<Entity> m_playerEntityList = new List<Entity>();

        [SerializeField]
        PixelPerfectCamera pixelPerfectCamera;

        void Start()
        {
            Define.Instance.SetPixelSize(Screen.width / pixelPerfectCamera.refResolutionX);

            var scene = SceneManager.GetActiveScene();
            if (scene.name != SCENE_NAME)
                return;

            var manager = InitializeWorld();

            ReadySharedComponentData();
            ComponentCache();
            InitializeEntities(manager);
        }

        EntityManager InitializeWorld()
        {
            World[] worlds = new World[1];
            ref World world = ref worlds[0];
            world = new World(SCENE_NAME);

            EntityManager manager = world.CreateManager<EntityManager>();

            InitializeSystem(world);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(worlds);

            return manager;
        }

        void InitializeSystem(World world)
        {
            world.CreateManager(typeof(ScanSystem));
            world.CreateManager(typeof(FieldInputSystem));
            world.CreateManager(typeof(PieceInputSystem));

            world.CreateManager(typeof(FieldCountSystem));
            world.CreateManager(typeof(PieceCountSystem));
            world.CreateManager(typeof(EffectCountSystem));

            world.CreateManager(typeof(FieldCheckBanishSystem));
            world.CreateManager(typeof(BGDrawSystem));

            world.CreateManager(typeof(PieceDrawSystem));
            world.CreateManager(typeof(EffectDrawSystem));
        }

        void ComponentCache()
        {
            Cache.pixelPerfectCamera = FindObjectOfType<PixelPerfectCamera>();
        }

        void ReadySharedComponentData()
        {
            Shared.ReadySharedComponentData();
        }

        void InitializeEntities(EntityManager manager)
        {
            CreateFieldEntity(manager);
            CreateGridEntity(manager);
            CreatePieceEntity(manager);
            CreateEffectEntity(manager);
        }

        void CreateFieldEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Define.Instance.Common.FieldCount; fieldId++)
            {
                var fieldEntity = FieldEntityFactory.CreateEntity(fieldId, manager, ref Shared.puzzleMeshMat);
            }
        }

        void CreateGridEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Define.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Define.Instance.Common.PieceCount; gridId++)
                {
                    var gridEntity = GridEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

        void CreatePieceEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Define.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Define.Instance.Common.PieceCount; gridId++)
                {
                    var pieceEntity = PieceEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

        void CreateEffectEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Define.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Define.Instance.Common.PieceCount; gridId++)
                {
                    var effectEntity = EffectEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

    }
}
