// using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace NKPB
{
    [UpdateInGroup(typeof(RenderGroup))]
    // [UpdateAfter(typeof(CountGroup))]
    // [UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
    public class BGDrawSystem : JobComponentSystem
    {
        // EntityQuery m_query;
        Quaternion m_quaternion;
        Vector3 m_fieldPosition;
        Vector3 m_gridPosition;
        protected override void OnCreateManager()
        {
            m_quaternion = Quaternion.Euler(new Vector3(-90, 0, 0));

            m_fieldPosition = new Vector3(Settings.Instance.Common.FieldOffsetX,
                Settings.Instance.Common.FieldOffsetY,
                (int)EnumDrawLayer.FieldLayer);

            m_gridPosition = new Vector3(Settings.Instance.Common.FieldOffsetX + Settings.Instance.Common.GridOffsetX,
                Settings.Instance.Common.FieldOffsetY + Settings.Instance.Common.GridOffsetY,
                (int)EnumDrawLayer.GridLayer);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // 描画のためCompleteする
            inputDeps.Complete();
            DrawGrid();
            return inputDeps;
        }

        private void DrawFrame()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(m_fieldPosition, m_quaternion, Vector3.one);
            Graphics.DrawMesh(Shared.puzzleMeshMat.meshes["field"],
                matrix,
                Shared.puzzleMeshMat.material, 0);
        }

        private void DrawGrid()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(m_gridPosition, m_quaternion, Vector3.one);
            Graphics.DrawMesh(Shared.puzzleMeshMat.meshes["grid"],
                matrix,
                Shared.puzzleMeshMat.material, 0);
        }

    }
}
