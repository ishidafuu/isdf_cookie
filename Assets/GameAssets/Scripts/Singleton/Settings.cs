using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
namespace NKPB
{
    [ExecuteInEditMode]
    public class Settings : SingletonMonoBehaviour<Settings>
        {
            public CommonSettings Common;
            public MoveSettings Move;
            public DrawPosSettings DrawPos;
            public DebugSettings Debug;
            public int PixelSize { get; private set; }

            /// <summary>
            /// オートセット
            /// </summary>
            public void LoadObject()
            {
                Common = Resources.FindObjectsOfTypeAll<CommonSettings>().First()as CommonSettings;
                Move = Resources.FindObjectsOfTypeAll<MoveSettings>().First()as MoveSettings;
                Debug = Resources.FindObjectsOfTypeAll<DebugSettings>().First()as DebugSettings;
                DrawPos = Resources.FindObjectsOfTypeAll<DrawPosSettings>().First()as DrawPosSettings;
            }
            public void SetPixelSize(int pixelSize)
            {
                PixelSize = pixelSize;
            }
        }

#if UNITY_EDITOR
    [CustomEditor(typeof(Settings))] // 拡張するクラスを指定
    public class DefineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 元のInspector部分を表示
            base.OnInspectorGUI();

            // ボタンを表示
            if (GUILayout.Button("LoadObject"))
            {
                (target as Settings).LoadObject();
            }
        }

    }
#endif

}
