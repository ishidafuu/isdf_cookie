using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NKPB
{
    public static class Shared
    {
        // SharedComponentData
        public static MeshMatList puzzleMeshMat;
        // public static MeshMatList fieldMeshMat;
        // public static MeshMatList gridMeshMat;
        // static readonly string ShaderBg = "Sprites/DefaultSprite";

        public static void ReadySharedComponentData()
        {
            puzzleMeshMat = new MeshMatList(ResourcesPathSettings.PieceSprite, ResourcesPathSettings.DefaultShader);
            // fieldMeshMat = new MeshMatList(PathSettings.FieldSprite, PathSettings.DefaultShader);
            // gridMeshMat = new MeshMatList(PathSettings.GridSprite, PathSettings.DefaultShader);
            // meterMeshMat = new MeshMatList("yyhs/bg/meter", ShaderBg);

            // // スクリプタブルオブジェクトの読み込み
            // aniScriptSheet = new AniScriptSheet();
            // if (Resources.FindObjectsOfTypeAll<AniScriptSheetObject>().Length == 0)
            // 	Debug.LogError("aniScriptSheet 0");
            // aniScriptSheet.scripts = (Resources.FindObjectsOfTypeAll<AniScriptSheetObject>().First()as AniScriptSheetObject).scripts;

            // aniBasePos = new AniBasePos();
            // if (Resources.FindObjectsOfTypeAll<AniBasePosObject>().Length == 0)
            // 	Debug.LogError("aniBasePos 0");
            // aniBasePos = (Resources.FindObjectsOfTypeAll<AniBasePosObject>().First()as AniBasePosObject).aniBasePos;
        }

    }
}
