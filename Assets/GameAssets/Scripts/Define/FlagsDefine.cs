using System;
using System.Collections.Generic;
using UnityEngine;
namespace NKPB
{
    // モーションフラグ
    [Flags]
    public enum FlagInputCheck
    {
        None = 0x0000,
        Jump = 0x0001,
        Dash = 0x0002,
        Walk = 0x0004,
        Slip = 0x0008,
        Idle = 0x0010,
        };
        public static partial class FlagInputCheckExtend
        {
        public static bool IsFlag(this FlagInputCheck param, FlagInputCheck flag)
        {
        //motionFlags.HasFlagはバーストできないので != 0で代用
        return (param & flag) != 0;
        }

        public static void AddFlag(this FlagInputCheck param, FlagInputCheck flag)
        {
            param |= flag;
        }

        public static void SubFlag(this FlagInputCheck param, FlagInputCheck flag)
        {
            param &= ~flag;
        }
    }

    // モーションフラグ
    [Flags]
    public enum FlagMove
    {
        None = 0x0000,
        Walk = 0x0001,
        Dash = 0x0002,
        Friction = 0x0004,
        Stop = 0x0008,
        };

        public static partial class FlagMoveExtend
        {
        public static bool IsFlag(this FlagMove param, FlagMove flag)
        {
        //motionFlags.HasFlagはバーストできないので != 0で代用
        return (param & flag) != 0;
        }

        public static void AddFlag(this FlagMove param, FlagMove flag)
        {
            param |= flag;
        }

        public static void SubFlag(this FlagMove param, FlagMove flag)
        {
            param &= ~flag;
        }
    }

    // モーションフラグ
    [Flags]
    public enum FlagMotion
    {
        None = 0x0000,
        // 空中
        Air = 0x001,
        // ダッシュ
        Dash = 0x002,
        // ダメージ
        Damage = 0x003
        };
        public static partial class FlagMotionExtend
        {
        public static bool IsFlag(this FlagMotion param, FlagMotion flag)
        {
        //motionFlags.HasFlagはバーストできないので != 0で代用
        return (param & flag) != 0;
        }

        public static void AddFlag(this FlagMotion param, FlagMotion flag)
        {
            param |= flag;
        }

        public static void SubFlag(this FlagMotion param, FlagMotion flag)
        {
            param &= ~flag;
        }
    }

    // 更新フラグ
    [Flags]
    public enum FlagUpdate
    {
        None = 0x0000,
        // 空中
        Air = 0x001,
        // ダッシュ
        Dash = 0x002,
        // ダメージ
        Damage = 0x003,
        };

        public static partial class FlagUpdateExtend
        {
        public static bool IsFlag(this FlagUpdate param, FlagUpdate flag)
        {
        //motionFlags.HasFlagはバーストできないので != 0で代用
        return (param & flag) != 0;
        }

        public static void AddFlag(this FlagUpdate param, FlagUpdate flag)
        {
            param |= flag;
        }

        public static void SubFlag(this FlagUpdate param, FlagUpdate flag)
        {
            param &= ~flag;
        }
    }
}
