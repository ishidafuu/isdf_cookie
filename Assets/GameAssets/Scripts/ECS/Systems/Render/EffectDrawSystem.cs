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
    [UpdateAfter(typeof(CountGroup))]
    [UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
    public class EffectDrawSystem : JobComponentSystem
    {
        ComponentGroup m_group;
        Quaternion m_quaternion;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.ReadOnly<EffectState>()
            );
            m_quaternion = Quaternion.Euler(new Vector3(-90, 0, 0));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_group.AddDependency(inputDeps);

            NativeArray<EffectState> effectStates = m_group.ToComponentDataArray<EffectState>(Allocator.TempJob);
            NativeArray<Matrix4x4> effectMatrixes = new NativeArray<Matrix4x4>(effectStates.Length, Allocator.TempJob);
            DrawJob job = DoDrawJob(ref inputDeps, effectStates, effectMatrixes);

            // 描画のためCompleteする
            inputDeps.Complete();
            for (int i = 0; i < effectMatrixes.Length - 1; i++)
            {
                if (effectStates[i].type == EnumEffectType.None)
                    continue;

                Graphics.DrawMesh(GetMeshe(effectStates[i]),
                    job.effectMatrixes[i],
                    Shared.puzzleMeshMat.material, 0);
            }

            effectStates.Dispose();
            effectMatrixes.Dispose();
            return inputDeps;
        }

        Mesh GetMeshe(EffectState state)
        {
            return Shared.puzzleMeshMat.meshes[$"banish_0{state.imageNo}"];
        }

        private DrawJob DoDrawJob(ref JobHandle inputDeps,
            NativeArray<EffectState> effectStates,
            NativeArray<Matrix4x4> pieceMatrixes)
        {
            var job = new DrawJob()
            {
                effectMatrixes = pieceMatrixes,
                effectStates = effectStates,
                one = Vector3.one,
                q = m_quaternion,
                FieldOffsetX = Define.Instance.Common.FieldOffsetX,
                FieldOffsetY = Define.Instance.Common.FieldOffsetY,
                PieceOffsetX = Define.Instance.Common.PieceOffsetX,
                PieceOffsetY = Define.Instance.Common.PieceOffsetY,
                // GridSize = Define.Instance.Common.GridSize,
                // GridLineLength = Define.Instance.Common.GridLineLength,

            };
            inputDeps = job.Schedule(inputDeps);
            m_group.AddDependency(inputDeps);
            return job;
        }

        // [BurstCompileAttribute]
        struct DrawJob : IJob
        {
            public NativeArray<Matrix4x4> effectMatrixes;
            [ReadOnly] public NativeArray<EffectState> effectStates;
            [ReadOnly] public Vector3 one;
            [ReadOnly] public Quaternion q;
            [ReadOnly] public int FieldOffsetX;
            [ReadOnly] public int FieldOffsetY;
            [ReadOnly] public int PieceOffsetX;
            [ReadOnly] public int PieceOffsetY;

            // [ReadOnly] public int GridSize;
            // [ReadOnly] public int GridLineLength;

            public void Execute()
            {
                for (int i = 0; i < effectStates.Length; i++)
                {
                    if (effectStates[i].type == EnumEffectType.None)
                        continue;

                    Vector3 pos = new Vector3(
                        (effectStates[i].position.x + FieldOffsetX + PieceOffsetX),
                        (effectStates[i].position.y + FieldOffsetY + PieceOffsetY),
                        (int)EnumDrawLayer.EffectLayer);
                    effectMatrixes[i] = Matrix4x4.TRS(pos, q, one);
                }
            }
        }
    }
}
