using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Fabric.Crashlytics;

[ExecuteInEditMode]
public class FPSManager : SingletonMonoBehaviour<FPSManager>
	{
		public int fps_ = 30;
		public int motionFps_ = 30;
		public bool vSync_ = false;
		public bool fpsDraw_ = true;
		// 測定用
		int tick = 0; // フレーム数
		float elapsed = 0; // 経過時間
		float nowfps = 0; // フレームレート

		// 外観設定
		const float GUI_WIDTH = 75f; // GUI矩形幅
		const float GUI_HEIGHT = 30f; // GUI矩形高さ
		const float MARGIN_X = 10f; // 画面からの横マージン
		const float MARGIN_Y = 10f; // 画面からの縦マージン
		const float INNER_X = 8f; // 文字のGUI外枠からの横マージン
		const float INNER_Y = 5f; // 文字のGUI外枠からの縦マージン

		Rect outer; // 外枠(GUI矩形領域)
		Rect inner; // 内枠(文字領域)

		override protected void Awake()
		{
			base.Awake();
		}

		void Start()
		{

			QualitySettings.vSyncCount = (vSync_)
				? 1
				: 0;
			Application.targetFrameRate = fps_;

			outer = new Rect(MARGIN_X, MARGIN_Y, GUI_WIDTH, GUI_HEIGHT);
			inner = new Rect(MARGIN_X + INNER_X, MARGIN_Y + INNER_Y, GUI_WIDTH - INNER_X * 2f, GUI_HEIGHT - INNER_Y * 2f);

		}

		void Update()
		{
			// DebugPanel.Log("MusicalTimeBeat", Music.MusicalTimeBeat);
			if (Application.targetFrameRate != fps_)Application.targetFrameRate = fps_;
			if (vSync_ && (QualitySettings.vSyncCount == 0))QualitySettings.vSyncCount = 1;
			if (!vSync_ && (QualitySettings.vSyncCount == 1))QualitySettings.vSyncCount = 0;

			UpdateTick();

		}

		public void TestCrash()
		{
			// Crashlytics.ThrowNonFatal();
			// Crashlytics.Crash();
		}

		// Update is called once per frame
		void UpdateTick()
		{
			tick++;
			elapsed += Time.deltaTime;
			if (elapsed >= 1f)
			{
				nowfps = tick / elapsed;
				tick = 0;
				elapsed = 0;
			}
		}

		void OnGUI()
		{
			if (fpsDraw_)
			{
				GUI.Box(outer, "");
				GUILayout.BeginArea(inner);
				{
					GUILayout.BeginVertical();
					GUILayout.Label("fps : " + nowfps.ToString("F1"));
					GUILayout.EndVertical();
				}
				GUILayout.EndArea();
			}
		}
	}