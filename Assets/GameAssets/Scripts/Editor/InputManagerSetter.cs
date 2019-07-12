#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// InputManagerを自動的に設定してくれるクラス
/// </summary>
public class InputManagerSetter
{
	const int PLAYERNUM = 3;
	const int FIRENUM = 6;
	/// <summary>
	/// インプットマネージャーを再設定します。
	/// </summary>
	[MenuItem("Tools/Reset InputManager")]
	public static void ResetInputManager()
	{
		Debug.Log("インプットマネージャーの設定を開始します。");
		InputManagerGenerator inputManagerGenerator = new InputManagerGenerator();

		Debug.Log("設定を全てクリアします。");
		inputManagerGenerator.Clear();

		Debug.Log("プレイヤーごとの設定を追加します。");
		for (int i = 0; i < PLAYERNUM; i++)
		{
			AddPlayerInputSettings(inputManagerGenerator, i);
		}

		Debug.Log("グローバル設定を追加します。");
		AddGlobalInputSettings(inputManagerGenerator);

		Debug.Log("インプットマネージャーの設定が完了しました。");
	}

	/// <summary>
	/// グローバルな入力設定を追加する（OK、キャンセルなど）
	/// </summary>
	/// <param name="inputManagerGenerator">Input manager generator.</param>
	private static void AddGlobalInputSettings(InputManagerGenerator inputManagerGenerator)
	{

		// 横方向
		{
			var name = "Horizontal";
			inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "a", "d", "left", "right"));
		}

		// 縦方向
		{
			var name = "Vertical";
			inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "s", "w", "down", "up"));
		}

		// 決定
		{
			var name = "Submit";
			inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "z", "joystick button 0"));
		}

		// キャンセル
		{
			var name = "Cancel";
			inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "x", "joystick button 1"));
		}
	}

	/// <summary>
	/// プレイヤーごとの入力設定を追加する
	/// </summary>
	/// <param name="inputManagerGenerator">Input manager generator.</param>
	/// <param name="playerIndex">Player index.</param>
	private static void AddPlayerInputSettings(InputManagerGenerator inputManagerGenerator, int playerIndex)
	{
		if (playerIndex < 0 || playerIndex > 3)Debug.LogError("プレイヤーインデックスの値が不正です。");
		string upKey = "", downKey = "", leftKey = "", rightKey = "";
		List<string> fireKeys;
		GetAxisKey(out upKey, out downKey, out leftKey, out rightKey, out fireKeys, playerIndex);

		int joystickNum = playerIndex + 1;

		Debug.Log("プレイヤー" + joystickNum + "の設定を追加します。");

		// 横方向
		{
			var name = string.Format("P{0}Horizontal", playerIndex);
			inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 1));
			inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 5)); // 5thAxis
			inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, leftKey, rightKey, "", ""));
		}

		// 縦方向
		{
			var name = string.Format("P{0}Vertical", playerIndex);
			// Yはinvert設定しないとギャクに成る
			inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 2, true));
			inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 6, true)); // 6thAxis
			inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, downKey, upKey, "", ""));
		}

		// 攻撃
		for (int i = 0; i < FIRENUM; i++)
		{
			var axis = new InputAxis();
			var name = string.Format("P{0}Fire{1}", playerIndex, i + 1);
			var button = string.Format("joystick {0} button {1}", joystickNum, i);
			inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, fireKeys[i]));
		}
	}

	/// <summary>
	/// キーボードでプレイした場合、割り当たっているキーを取得する
	/// </summary>
	/// <param name="upKey">Up key.</param>
	/// <param name="downKey">Down key.</param>
	/// <param name="leftKey">Left key.</param>
	/// <param name="rightKey">Right key.</param>
	/// <param name="attackKey">Attack key.</param>
	/// <param name="playerIndex">Player index.</param>
	private static void GetAxisKey(out string upKey, out string downKey, out string leftKey, out string rightKey, out List<string> fireKey, int playerIndex)
	{
		upKey = "";
		downKey = "";
		leftKey = "";
		rightKey = "";
		fireKey = new List<string>();

		switch (playerIndex)
		{
			case 0:
				upKey = "t";
				downKey = "g";
				leftKey = "f";
				rightKey = "h";
				fireKey.Add("1");
				fireKey.Add("2");
				fireKey.Add("3");
				fireKey.Add("4");
				fireKey.Add("5");
				fireKey.Add("6");
				break;
			case 1:
				upKey = "w";
				downKey = "s";
				leftKey = "a";
				rightKey = "d";
				fireKey.Add("z");
				fireKey.Add("x");
				fireKey.Add("c");
				fireKey.Add("v");
				fireKey.Add("b");
				fireKey.Add("n");
				break;
			case 2:
				upKey = "up";
				downKey = "down";
				leftKey = "left";
				rightKey = "right";
				fireKey.Add("[0]");
				fireKey.Add("[1]");
				fireKey.Add("[2]");
				fireKey.Add("[3]");
				fireKey.Add("[4]");
				fireKey.Add("[5]");
				break;
				// case 3:
				// 	upKey = "[8]";
				// 	downKey = "[5]";
				// 	leftKey = "[4]";
				// 	rightKey = "[6]";
				// 	attackKey = "[9]";
				// 	break;
			default:
				// Debug.LogError("プレイヤーインデックスの値が不正です。");
				upKey = "";
				downKey = "";
				leftKey = "";
				rightKey = "";
				break;
		}
	}
}

#endif