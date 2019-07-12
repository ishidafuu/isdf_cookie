// #if ENABLE_DEBUG

using System;
using UnityEngine;

namespace NKPB
{
    /// <summary>
    /// ゲーム設定
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/DebugSettings", fileName = "DebugSettings")]
    public sealed class DebugSettings : ScriptableObject
    {
        // // デバッグ用弾発射
        // [Serializable]
        // public struct SpawnBulletData
        // {
        //     [SerializeField] public Vector2 CreatePosition;
        //     [SerializeField] public float Angle;
        //     [SerializeField] public float Speed;
        //     [SerializeField] public float Lifespan;
        // }

        // [Header("【Spawn Enemy】")]
        // [SerializeField] public BarrageType CreateBarrageType;
        // [SerializeField] public Vector2 CreatePosition;
    }
}

// #endif
