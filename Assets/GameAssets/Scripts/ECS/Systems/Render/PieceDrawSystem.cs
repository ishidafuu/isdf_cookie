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
            ComponentDataArray<PieceState> pieceStates = m_group.GetComponentDataArray<PieceState>();
            NativeArray<Matrix4x4> pieceMatrixes = new NativeArray<Matrix4x4>(Define.Instance.Common.PieceCount + 1, Allocator.TempJob);

            NativeArray<int> clipNo = new NativeArray<int>(1, Allocator.TempJob);
            DrawJob job = DoDrawJob(ref inputDeps, pieceMatrixes, clipNo);
            inputDeps.Complete();

            Draw(pieceStates, pieceMatrixes, job);

            pieceMatrixes.Dispose();
            clipNo.Dispose();

            return inputDeps;
        }

        private void Draw(ComponentDataArray<PieceState> pieceStates, NativeArray<Matrix4x4> pieceMatrixes, DrawJob job)
        {
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
        }

        Mesh GetMeshe(PieceState state)
        {
            return Shared.puzzleMeshMat.meshes[$"piece_0{state.color}"];
        }

        private DrawJob DoDrawJob(ref JobHandle inputDeps,
            NativeArray<Matrix4x4> pieceMatrixes,
            NativeArray<int> clipNo)
        {
            var job = new DrawJob()
            {
                pieceMatrixes = pieceMatrixes,
                clipNo = clipNo,
                piecePositions = m_group.GetComponentDataArray<PiecePosition>(),
                pieceStates = m_group.GetComponentDataArray<PieceState>(),
                one = Vector3.one,
                q = m_quaternion,
                FieldOffsetX = Define.Instance.Common.FieldOffsetX,
                FieldOffsetY = Define.Instance.Common.FieldOffsetY,
                PieceOffsetX = Define.Instance.Common.PieceOffsetX,
                PieceOffsetY = Define.Instance.Common.PieceOffsetY,
                GridSize = Define.Instance.Common.GridSize,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridColumnLength = Define.Instance.Common.GridColumnLength,

            };
            inputDeps = job.Schedule(inputDeps);
            m_group.AddDependency(inputDeps);
            return job;
        }

        // [BurstCompileAttribute]
        struct DrawJob : IJob
        {
            public NativeArray<Matrix4x4> pieceMatrixes;
            public NativeArray<int> clipNo;
            [ReadOnly] public ComponentDataArray<PiecePosition> piecePositions;
            [ReadOnly] public ComponentDataArray<PieceState> pieceStates;
            [ReadOnly] public Vector3 one;
            [ReadOnly] public Quaternion q;
            [ReadOnly] public int FieldOffsetX;
            [ReadOnly] public int FieldOffsetY;
            [ReadOnly] public int PieceOffsetX;
            [ReadOnly] public int PieceOffsetY;

            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridColumnLength;

            public void Execute()
            {
                clipNo[0] = -1;
                int fieldWidth = (GridSize * GridRowLength);
                int fieldHeight = (GridSize * GridColumnLength);
                int clipXSize = fieldWidth - GridSize + 1;
                int clipYSize = fieldHeight - GridSize + 1;
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    Vector3 pos = new Vector3(
                        (piecePositions[i].position.x + FieldOffsetX + PieceOffsetX),
                        (piecePositions[i].position.y + FieldOffsetY + PieceOffsetY),
                        (int)EnumDrawLayer.PieceLayer);
                    pieceMatrixes[i] = Matrix4x4.TRS(pos, q, one);

                    if (clipNo[0] != -1)
                        continue;

                    if (piecePositions[i].fallLength != 0)
                        continue;

                    if (piecePositions[i].position.x >= clipXSize)
                    {
                        Vector3 clipPos = new Vector3(
                            (piecePositions[i].position.x - fieldWidth + FieldOffsetX + PieceOffsetX),
                            (piecePositions[i].position.y + FieldOffsetY + PieceOffsetY),
                            (int)EnumDrawLayer.PieceLayer);
                        pieceMatrixes[pieceMatrixes.Length - 1] = Matrix4x4.TRS(clipPos, q, one);
                        clipNo[0] = i;
                    }
                    else if (piecePositions[i].position.y >= clipYSize)
                    {
                        Vector3 clipPos = new Vector3(
                            (piecePositions[i].position.x + FieldOffsetX + PieceOffsetX),
                            (piecePositions[i].position.y - fieldHeight + FieldOffsetY + PieceOffsetY),
                            (int)EnumDrawLayer.PieceLayer);
                        pieceMatrixes[pieceMatrixes.Length - 1] = Matrix4x4.TRS(clipPos, q, one);
                        clipNo[0] = i;
                    }
                }
            }
        }
    }
}
