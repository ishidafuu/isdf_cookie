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
    public static class PathSettings
    {
        public static readonly string DefaultShader = "Sprites/Default";
        public static readonly string PieceSprite = "Sprites/puzzle";
        // public static readonly string FieldSprite = "Sprites/field";
        // public static readonly string GridSprite = "Sprites/grid";
    }
}
