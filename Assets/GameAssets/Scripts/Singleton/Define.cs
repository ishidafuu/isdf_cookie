using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace NKPB
{
	[ExecuteInEditMode]
	public class Define : SingletonMonoBehaviour<Define>
		{
			public CommonSettings Common;
			public MoveSettings Move;
			public DrawPosSettings DrawPos;
			public DebugSettings Debug;

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
		}

	[CustomEditor(typeof(Define))] // 拡張するクラスを指定
	public class DefineEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			// 元のInspector部分を表示
			base.OnInspectorGUI();

			// ボタンを表示
			if (GUILayout.Button("LoadObject"))
			{
				(target as Define).LoadObject();
			}
		}

	}
}
