using Unity.Burst;
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
    public class FieldDrawSystem : JobComponentSystem
    {
        ComponentGroup m_group;
        Quaternion m_quaternion;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.ReadOnly<FieldTag>()
            );
            m_quaternion = Quaternion.Euler(new Vector3(-90, 0, 0));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_group.AddDependency(inputDeps);

            NativeArray<PiecePosition> positions = m_group.ToComponentDataArray<PiecePosition>(Allocator.TempJob);
            NativeArray<PieceState> pieceDatas = m_group.ToComponentDataArray<PieceState>(Allocator.TempJob);
            NativeArray<Matrix4x4> pieceMatrixes = new NativeArray<Matrix4x4>(positions.Length, Allocator.TempJob);
            BodyJob job = DoDrawJob(ref inputDeps, positions, pieceMatrixes);

            // var bgScrolls = m_group.ToComponentDataArray<BgScroll>(Allocator.TempJob);
            // var bgScrollsMatrixs = new NativeArray<Matrix4x4>(toukiMeters.Length, Allocator.TempJob);
            // BgScrollJob bgScrollJob = DoBgScrollJob(ref inputDeps, toukiMeters, bgScrollsMatrixs);

            // 描画のためCompleteする
            inputDeps.Complete();

            // JobBody(toukiMeters);
            // DrawFrame();
            // DrawToukiMeter(toukiMeterJob);
            for (int i = 0; i < pieceMatrixes.Length; i++)
            {
                Graphics.DrawMesh(Shared.puzzleMeshMat.meshes[$"piece_{pieceDatas[i].color}"],
                    job.charaMatrixes[i],
                    Shared.puzzleMeshMat.material, 0);
            }

            // NativeArrayの開放
            positions.Dispose();
            pieceDatas.Dispose();
            pieceMatrixes.Dispose();
            // bgScrollsMatrixs.Dispose();

            return inputDeps;
        }

        private BodyJob DoDrawJob(ref JobHandle inputDeps,
            NativeArray<PiecePosition> positions,
            NativeArray<Matrix4x4> charaMatrixes)
        {
            var bodyJob = new BodyJob()
            {
                charaMatrixes = charaMatrixes,
                positions = positions,
                one = Vector3.one,
                q = m_quaternion,
            };
            inputDeps = bodyJob.Schedule(inputDeps);
            m_group.AddDependency(inputDeps);
            return bodyJob;
        }

        [BurstCompileAttribute]
        struct BodyJob : IJob
        {
            public NativeArray<Matrix4x4> charaMatrixes;
            [ReadOnly] public NativeArray<PiecePosition> positions;
            [ReadOnly] public Vector3 one;
            [ReadOnly] public Quaternion q;

            public void Execute()
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    // float bodyDepth = positions[i].poisiton.z;
                    float bodyX = positions[i].position.x;
                    charaMatrixes[i] = Matrix4x4.TRS(
                        new Vector3(bodyX, positions[i].position.y, 0),
                        q, one);
                }
            }
        }
    }
}
