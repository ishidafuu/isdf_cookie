using System;
using System.Collections.ObjectModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(MoveGroup))]
    [UpdateAfter(typeof(CountGroup))]
    public class PieceInputSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldInput>()
            );
            m_groupGrid = GetComponentGroup(
                ComponentType.Create<GridState>()
            );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            m_groupGrid.AddDependency(inputDeps);
            m_groupPiece.AddDependency(inputDeps);
            var moveJob = new PieceMoveJob()
            {
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,
            };

            inputDeps = moveJob.Schedule(inputDeps);

            var reflectJob = new GridReflectJob()
            {
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,
            };

            inputDeps = reflectJob.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            [ReadOnly]
            public ComponentDataArray<FieldInput> fieldInputs;
            public ComponentDataArray<GridState> gridStates;
            public ComponentDataArray<PiecePosition> piecePositions;
            public int GridSize;
            public int GridLineLength;
            int FieldSize;

            public void Execute()
            {
                FieldSize = GridSize * GridLineLength;
                for (int i = 0; i < fieldInputs.Length; i++)
                {
                    var fieldInput = fieldInputs[i];
                    if (fieldInput.isHold)
                    {
                        UpdateHold(fieldInput);
                    }
                    else
                    {
                        UpdateAlignment(fieldInput);
                    }
                }
            }

            private void UpdateHold(FieldInput fieldInput)
            {
                if (fieldInput.swipeType == EnumSwipeType.Horizontal)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if ((int)piecePosition.gridPosition.y == (int)fieldInput.gridPosition.y)
                        {
                            ResetStartPosition(ref piecePosition);
                            int posX = RoundPos(piecePosition.startPosition.x + fieldInput.distPosition.x);

                            piecePosition.position = new Vector2Int(posX, piecePosition.position.y);
                            piecePosition.gridPosition = ToGridPosition(piecePosition);
                            piecePosition.isMove = true;
                            piecePositions[i] = piecePosition;
                        }
                    }
                }
                else if (fieldInput.swipeType == EnumSwipeType.Vertical)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if ((int)piecePosition.gridPosition.x == (int)fieldInput.gridPosition.x)
                        {
                            ResetStartPosition(ref piecePosition);
                            int posY = RoundPos(piecePosition.startPosition.y + fieldInput.distPosition.y);
                            piecePosition.position = new Vector2Int(piecePosition.position.x, posY);
                            piecePosition.gridPosition = ToGridPosition(piecePosition);
                            piecePosition.isMove = true;
                            piecePositions[i] = piecePosition;
                        }
                    }
                }
            }

            private void UpdateAlignment(FieldInput fieldInput)
            {
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];
                    if (piecePosition.isMove)
                    {
                        piecePosition.position = FromGridPosition(piecePosition);
                        piecePosition.gridPosition = ToGridPosition(piecePosition);
                        piecePosition.isMove = false;
                        piecePositions[i] = piecePosition;
                    }
                }
            }

            private static void ResetStartPosition(ref PiecePosition piecePosition)
            {
                if (!piecePosition.isMove)
                {
                    piecePosition.startPosition = piecePosition.position;
                }
            }

            private Vector2Int ToGridPosition(PiecePosition piecePosition)
            {
                int offset = GridSize / 2;
                return new Vector2Int(
                    (int)((piecePosition.position.x + offset) / GridSize) % GridLineLength,
                    (int)((piecePosition.position.y + offset) / GridSize) % GridLineLength);
            }

            private Vector2Int FromGridPosition(PiecePosition piecePosition)
            {
                return new Vector2Int(
                    RoundPos((piecePosition.gridPosition.x) * GridSize),
                    RoundPos((piecePosition.gridPosition.y) * GridSize));
            }

            private int RoundPos(float pos)
            {
                return (pos >= 0)
                    ? (int)pos % FieldSize
                    : (FieldSize - ((int)Mathf.Abs(pos) % FieldSize) - 1);
            }
        }

        // [BurstCompileAttribute]
        struct GridReflectJob : IJob
        {
            public ComponentDataArray<GridState> gridStates;
            [ReadOnly]
            public ComponentDataArray<PiecePosition> piecePositions;
            public int GridSize;
            public int GridLineLength;
            int FieldSize;

            public void Execute()
            {
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];
                    int index = (piecePosition.gridPosition.x + piecePosition.gridPosition.y * GridLineLength);
                    var gridState = gridStates[index];
                    gridState.pieceId = i;
                    gridStates[index] = gridState;
                }
            }

        }
    }
}
