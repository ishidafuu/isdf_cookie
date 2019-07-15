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
    public class PieceDrawSystem : JobComponentSystem
    {
        ComponentGroup m_group;
        Quaternion m_quaternion;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.ReadOnly<PiecePosition>(),
                ComponentType.ReadOnly<PieceState>()
            );
            m_quaternion = Quaternion.Euler(new Vector3(-90, 0, 0));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_group.AddDependency(inputDeps);

            NativeArray<PiecePosition> positions = m_group.ToComponentDataArray<PiecePosition>(Allocator.TempJob);
            NativeArray<PieceState> pieceStates = m_group.ToComponentDataArray<PieceState>(Allocator.TempJob);
            NativeArray<Matrix4x4> pieceMatrixes = new NativeArray<Matrix4x4>(positions.Length + 1, Allocator.TempJob);

            NativeArray<int> clipNo = new NativeArray<int>(1, Allocator.TempJob);
            DrawJob job = DoDrawJob(ref inputDeps, positions, pieceMatrixes, clipNo);

            // 描画のためCompleteする
            inputDeps.Complete();
            for (int i = 0; i < pieceMatrixes.Length - 1; i++)
            {
                if (pieceStates[i].isBanish)
                    continue;

                Graphics.DrawMesh(GetMeshe(pieceStates[i]),
                    job.pieceMatrixes[i],
                    Shared.puzzleMeshMat.material, 0);
            }

            if (job.clipNo[0] != -1)
            {
                Graphics.DrawMesh(GetMeshe(pieceStates[job.clipNo[0]]),
                    job.pieceMatrixes[job.pieceMatrixes.Length - 1],
                    Shared.puzzleMeshMat.material, 0);
            }

            // NativeArrayの開放
            positions.Dispose();
            pieceStates.Dispose();
            pieceMatrixes.Dispose();
            clipNo.Dispose();

            return inputDeps;
        }

        Mesh GetMeshe(PieceState state)
        {
            return Shared.puzzleMeshMat.meshes[$"piece_0{state.color}"];
        }

        private DrawJob DoDrawJob(ref JobHandle inputDeps,
            NativeArray<PiecePosition> positions,
            NativeArray<Matrix4x4> pieceMatrixes,
            NativeArray<int> clipNo)
        {
            var job = new DrawJob()
            {
                pieceMatrixes = pieceMatrixes,
                clipNo = clipNo,
                piecePositions = positions,
                one = Vector3.one,
                q = m_quaternion,
                FieldOffsetX = Define.Instance.Common.FieldOffsetX,
                FieldOffsetY = Define.Instance.Common.FieldOffsetY,
                PieceOffsetX = Define.Instance.Common.PieceOffsetX,
                PieceOffsetY = Define.Instance.Common.PieceOffsetY,
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,

            };
            inputDeps = job.Schedule(inputDeps);
            m_group.AddDependency(inputDeps);
            return job;
        }

        [BurstCompileAttribute]
        struct DrawJob : IJob
        {
            public NativeArray<Matrix4x4> pieceMatrixes;
            public NativeArray<int> clipNo;
            [ReadOnly] public NativeArray<PiecePosition> piecePositions;
            [ReadOnly] public Vector3 one;
            [ReadOnly] public Quaternion q;
            [ReadOnly] public int FieldOffsetX;
            [ReadOnly] public int FieldOffsetY;
            [ReadOnly] public int PieceOffsetX;
            [ReadOnly] public int PieceOffsetY;

            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridLineLength;

            public void Execute()
            {
                clipNo[0] = -1;
                int fieldSize = (GridSize * GridLineLength);
                int clipSize = fieldSize - GridSize + 1;
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    Vector3 pos = new Vector3(
                        (piecePositions[i].position.x + FieldOffsetX + PieceOffsetX),
                        (piecePositions[i].position.y + FieldOffsetY + PieceOffsetY),
                        (int)EnumDrawLayer.PieceLayer);
                    pieceMatrixes[i] = Matrix4x4.TRS(pos, q, one);

                    if (clipNo[0] != -1)
                    {
                        continue;
                    }

                    if (piecePositions[i].position.x >= clipSize)
                    {
                        Vector3 clipPos = new Vector3(
                            (piecePositions[i].position.x - fieldSize + FieldOffsetX + PieceOffsetX),
                            (piecePositions[i].position.y + FieldOffsetY + PieceOffsetY),
                            (int)EnumDrawLayer.PieceLayer);
                        pieceMatrixes[pieceMatrixes.Length - 1] = Matrix4x4.TRS(clipPos, q, one);
                        clipNo[0] = i;
                    }
                    else if (piecePositions[i].position.y >= clipSize)
                    {
                        Vector3 clipPos = new Vector3(
                            (piecePositions[i].position.x + FieldOffsetX + PieceOffsetX),
                            (piecePositions[i].position.y - fieldSize + FieldOffsetY + PieceOffsetY),
                            (int)EnumDrawLayer.PieceLayer);
                        pieceMatrixes[pieceMatrixes.Length - 1] = Matrix4x4.TRS(clipPos, q, one);
                        clipNo[0] = i;
                    }
                }
            }
        }
    }
}
