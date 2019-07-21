using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        PixelPerfectCamera m_pixelPerfectCamera;

        void Start()
        {
            // FireBaseManager.Init();

            Settings.Instance.SetPixelSize(Screen.width / m_pixelPerfectCamera.refResolutionX);

            var scene = SceneManager.GetActiveScene();
            if (scene.name != SCENE_NAME)
                return;

            var manager = InitializeWorld();

            ReadySharedComponentData();
            ComponentCache();
            InitializeEntities(manager);
        }

        void OnDestroy()
        {
            World.DisposeAllWorlds();
            WordStorage.Instance.Dispose();
            WordStorage.Instance = null;
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
        }

        EntityManager InitializeWorld()
        {
            World[] worlds = new World[1];
            ref World world = ref worlds[0];
            world = new World(SCENE_NAME);
            // World.Active
            World.Active = world;

            InitializationSystemGroup initializationSystemGroup = world.GetOrCreateSystem<InitializationSystemGroup>();
            SimulationSystemGroup simulationSystemGroup = world.GetOrCreateSystem<SimulationSystemGroup>();
            PresentationSystemGroup presentationSystemGroup = world.GetOrCreateSystem<PresentationSystemGroup>();

            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<ScanSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<FieldInputMoveSystem>());

            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<PieceInputMoveSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<PieceFallMoveSystem>());

            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<FieldCountSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<PieceCountSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<EffectCountSystem>());

            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<FieldCheckBanishSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<PieceFallStartSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<BGDrawSystem>());

            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<PieceDrawSystem>());
            simulationSystemGroup.AddSystemToUpdateList(world.GetOrCreateSystem<EffectDrawSystem>());

            initializationSystemGroup.SortSystemUpdateList();
            simulationSystemGroup.SortSystemUpdateList();
            presentationSystemGroup.SortSystemUpdateList();

            InitializeSystem(world);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);

            return world.EntityManager;
        }

        void InitializeSystem(World world)
        {

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
            for (int fieldId = 0; fieldId < Settings.Instance.Common.FieldCount; fieldId++)
            {
                var fieldEntity = FieldEntityFactory.CreateEntity(fieldId, manager, ref Shared.puzzleMeshMat);
            }
        }

        void CreateGridEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Settings.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Settings.Instance.Common.PieceCount; gridId++)
                {
                    var gridEntity = GridEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

        void CreatePieceEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Settings.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Settings.Instance.Common.PieceCount; gridId++)
                {
                    var pieceEntity = PieceEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

        void CreateEffectEntity(EntityManager manager)
        {
            for (int fieldId = 0; fieldId < Settings.Instance.Common.FieldCount; fieldId++)
            {
                for (int gridId = 0; gridId < Settings.Instance.Common.PieceCount; gridId++)
                {
                    var effectEntity = EffectEntityFactory.CreateEntity(fieldId, gridId, manager, ref Shared.puzzleMeshMat);
                }
            }
        }

    }
}
