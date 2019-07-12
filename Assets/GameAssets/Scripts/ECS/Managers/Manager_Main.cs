using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
// using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace NKPB
{

    /// <summary>
    /// ECSのセットアップを行うManager_Main Component
    /// </summary>
    sealed class Manager_Main : MonoBehaviour
    {
        // ワールドとシーン名を一致させる
        const string SCENE_NAME = "Main";

        // エンティティリスト
        List<Entity> m_playerEntityList = new List<Entity>();

        void Start()
        {
            // シーンの判定
            var scene = SceneManager.GetActiveScene();
            if (scene.name != SCENE_NAME)
                return;

            // ワールド生成
            var manager = InitializeWorld();

            // SharedComponentDataの準備
            ReadySharedComponentData();

            // コンポーネントのキャッシュ
            ComponentCache();

            // エンティティ生成
            InitializeEntities(manager);
        }

        /// <summary>
        /// ワールド生成
        /// </summary>
        /// <returns></returns>
        EntityManager InitializeWorld()
        {
            var worlds = new World[1];
            ref
            var world = ref worlds[0];

            world = new World(SCENE_NAME);
            var manager = world.CreateManager<EntityManager>();

            // ComponentSystemの初期化
            InitializeSystem(world);

            // PlayerLoopへのWorldの登録
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(worlds);

            return manager;
        }

        /// <summary>
        /// ComponentSystemの初期化
        /// </summary>
        /// <param name="world"></param>
        void InitializeSystem(World world)
        {
            // 入力システム
            world.CreateManager(typeof(ScanSystem));
            world.CreateManager(typeof(FieldInputSystem));
            world.CreateManager(typeof(PieceInputSystem));

            world.CreateManager(typeof(FieldCountSystem));
            world.CreateManager(typeof(PieceCountSystem));

            world.CreateManager(typeof(PieceLineCheckSystem));
            world.CreateManager(typeof(ToukiMeterDebugSystem));
            world.CreateManager(typeof(BGDrawSystem));

            // // モーションの時間進行システム
            // world.CreateManager(typeof(CountMotionSystem));
            // // 時間経過によるモーション変更システム
            // world.CreateManager(typeof(ShiftCountMotionSystem));
            // // 入力による状態変化システム
            // world.CreateManager(typeof(InputMotionSystem));
            // // 入力による向き変化システム
            // world.CreateManager(typeof(InputMukiSystem));
            // 入力による座標変化システム

            // // 座標移動システム
            // world.CreateManager(typeof(MovePosSystem));
            // // 描画向き変換
            // world.CreateManager(typeof(LookSystem));
            // // 描画座標変換システム
            // world.CreateManager(typeof(ConvertDrawPosSystem));
            // // Renderer
            // 各パーツの描画位置決定および描画
            world.CreateManager(typeof(PieceDrawSystem));

        }

        // 各コンポーネントのキャッシュ
        void ComponentCache()
        {
            Cache.pixelPerfectCamera = FindObjectOfType<PixelPerfectCamera>();
            // var tileMaps = FindObjectsOfType<Tilemap>();
            // foreach (var item in tileMaps)
            // {
            //     // Debug.Log(item.layoutGrid.name);
            //     if (item.layoutGrid.name == "PheromGrid")
            //     {
            //         Cache.pheromMap = item;
            //         Cache.pheromMap.ClearAllTiles();
            //         Cache.pheromMap.size = new Vector3Int(Define.Instance.GRID_SIZE, Define.Instance.GRID_SIZE, 0);
            //     }
            // }
        }

        // SharedComponentDataの読み込み
        void ReadySharedComponentData()
        {
            Shared.ReadySharedComponentData();
        }

        /// <summary>
        /// エンティティ生成
        /// </summary>
        /// <param name="manager"></param>
        void InitializeEntities(EntityManager manager)
        {
            CreateFieldEntity(manager);
            CreateGridEntity(manager);
            CreatePieceEntity(manager);
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

    }
}
