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
    [UpdateInGroup(typeof(InputGroup))]
    [UpdateAfter(typeof(ScanGroup))]
    public class FieldInputSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        // ComponentGroup m_groupGrid;
        static int m_offsetConvertPositionX;
        static int m_offsetConvertPositionY;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldScan>(),
                ComponentType.ReadOnly<FieldBanish>(),
                ComponentType.Create<FieldInput>()
            );

            m_offsetConvertPositionX = -Define.Instance.Common.FieldOffsetX - Define.Instance.Common.PieceOffsetX;
            m_offsetConvertPositionY = -Define.Instance.Common.FieldOffsetY - Define.Instance.Common.PieceOffsetY;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            var job = new MoveJob()
            {
                fieldScans = m_groupField.GetComponentDataArray<FieldScan>(),
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                SwipeThreshold = Define.Instance.Common.SwipeThreshold,
                GridSize = Define.Instance.Common.GridSize,
                BorderSpeed = Define.Instance.Common.BorderSpeed,
            };

            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct MoveJob : IJob
        {
            [ReadOnly]
            public ComponentDataArray<FieldScan> fieldScans;
            [ReadOnly]
            public ComponentDataArray<FieldBanish> fieldBanishs;
            public ComponentDataArray<FieldInput> fieldInputs;

            public float SwipeThreshold;
            public int GridSize;
            public int BorderSpeed;

            public void Execute()
            {

                for (int i = 0; i < fieldScans.Length; i++)
                {
                    var fieldScan = fieldScans[i];
                    var fieldInput = fieldInputs[i];
                    var fieldBanish = fieldBanishs[i];
                    Vector2Int gamePosition = ConvertPosition(fieldScan.nowPosition);
                    DebugPanel.Log($"fieldInput.phase", fieldInput.phase.ToString());
                    if (fieldBanish.isBanish)
                    {
                        UpdateEnded(i, fieldScan, fieldInput, gamePosition);
                        break;
                    }

                    switch (fieldInput.phase)
                    {
                        case EnumFieldInputPhase.Align:
                            UpdateAlign(i, fieldScan, fieldInput, gamePosition);
                            break;
                        case EnumFieldInputPhase.FinishAlign:
                            UpdateFinishAlign(i, fieldScan, fieldInput, gamePosition);
                            break;
                        default:
                            switch (fieldScan.phase)
                            {
                                case TouchPhase.Began:
                                    UpdateBegan(i, fieldScan, fieldInput, gamePosition);
                                    break;
                                case TouchPhase.Ended:
                                    UpdateEnded(i, fieldScan, fieldInput, gamePosition);
                                    break;
                                case TouchPhase.Moved:
                                    UpdateMoved(i, fieldScan, fieldInput, gamePosition);
                                    break;
                            }
                            break;
                    }
                }
            }

            private void UpdateBegan(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                if (!CheckInfield(gamePosition))
                    return;
                fieldInput.phase = EnumFieldInputPhase.Hold;
                fieldInput.gridPosition = ConvertGridPosition(gamePosition);
                fieldInput.startPosition = gamePosition;
                fieldInput.swipeType = EnumSwipeType.None;
                // fieldInput.delta = Vector2.zero;
                fieldInput.distPosition = Vector2Int.zero;
                fieldInputs[i] = fieldInput;
            }

            private void UpdateEnded(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                fieldInput.phase = EnumFieldInputPhase.Align;
                fieldInput.alignVec = GetAlignVec(fieldInput, fieldScan);
                fieldInput.alignDelta = GetAlignDelta(fieldInput.alignVec);

                DebugPanel.Log($"fieldInput.alignVec", fieldInput.alignVec.ToString());
                DebugPanel.Log($"fieldInput.alignDelta", fieldInput.alignDelta.ToString());
                // fieldInput.swipeType = EnumSwipeType.None;
                fieldInputs[i] = fieldInput;
            }

            private void UpdateMoved(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                if (fieldInput.phase != EnumFieldInputPhase.Hold)
                    return;

                Vector2 distPosition = (fieldScan.nowPosition - fieldScan.startPosition);
                if (fieldInput.swipeType == EnumSwipeType.None)
                {
                    float distX = Mathf.Abs(distPosition.x);
                    float distY = Mathf.Abs(distPosition.y);

                    if (distX > distY)
                    {
                        if (distX > SwipeThreshold)
                        {
                            fieldInput.swipeType = EnumSwipeType.Horizontal;
                        }
                    }
                    else
                    {
                        if (distY > SwipeThreshold)
                        {
                            fieldInput.swipeType = EnumSwipeType.Vertical;
                        }
                    }
                }
                // fieldInput.delta = fieldScan.delta;
                fieldInput.distPosition = new Vector2Int(
                    (int)(distPosition.x / Define.Instance.PixelSize),
                    (int)(distPosition.y / Define.Instance.PixelSize));
                fieldInputs[i] = fieldInput;
            }

            private Vector2Int ConvertPosition(Vector2 scanPosition)
            {
                int x = (int)(scanPosition.x / Define.Instance.PixelSize) + m_offsetConvertPositionX;
                int y = (int)(scanPosition.y / Define.Instance.PixelSize) + m_offsetConvertPositionY;
                return new Vector2Int(x, y);
            }

            private static bool CheckInfield(Vector2Int position)
            {
                float size = Define.Instance.Common.GridSize * Define.Instance.Common.GridLineLength;
                return ((position.x > 0 && position.x < size)
                    && (position.y > 0 && position.y < size));
            }

            private static Vector2Int ConvertGridPosition(Vector2Int position)
            {
                return new Vector2Int(
                    (position.x / Define.Instance.Common.GridSize),
                    (position.y / Define.Instance.Common.GridSize));
            }

            private void UpdateAlign(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {

                EnumPieceAlignVec lastAlignVec = BackAlignVec(fieldInput.distPosition, fieldInput.swipeType);
                Vector2Int newDistPosition = fieldInput.distPosition + fieldInput.alignDelta;
                EnumPieceAlignVec nowAlignVec = BackAlignVec(newDistPosition, fieldInput.swipeType);
                // Vector2Int lastGridPos = ConvertGridPosition(fieldInput.distPosition);
                // Vector2Int newGridPos = ConvertGridPosition(newDistPosition);

                DebugPanel.Log($"fieldInput.distPosition", fieldInput.distPosition.ToString());
                DebugPanel.Log($"newDistPosition", newDistPosition.ToString());
                DebugPanel.Log($"lastAlignVec", lastAlignVec.ToString());
                DebugPanel.Log($"fieldInput.alignVec", fieldInput.alignVec.ToString());
                if (((lastAlignVec != nowAlignVec) && lastAlignVec == fieldInput.alignVec)
                    || lastAlignVec == EnumPieceAlignVec.None)
                {
                    fieldInput.phase = EnumFieldInputPhase.FinishAlign;
                }

                fieldInput.distPosition = newDistPosition;
                fieldInputs[i] = fieldInput;
            }

            private void UpdateFinishAlign(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                fieldInput.phase = EnumFieldInputPhase.None;
                fieldInput.swipeType = EnumSwipeType.None;
                // fieldInput.delta = Vector2.zero;
                fieldInput.distPosition = Vector2Int.zero;
                fieldInputs[i] = fieldInput;
            }

            private Vector2Int GetAlignDelta(EnumPieceAlignVec alignVec)
            {
                int deltaX = 0;
                int deltaY = 0;
                switch (alignVec)
                {
                    case EnumPieceAlignVec.Left:
                        deltaX = -BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Right:
                        deltaX = +BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Up:
                        deltaY = +BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Down:
                        deltaY = -BorderSpeed;
                        break;
                }
                return new Vector2Int(deltaX, deltaY);
            }

            private EnumPieceAlignVec GetAlignVec(FieldInput fieldInput, FieldScan fieldScan)
            {
                if ((Mathf.Abs(fieldScan.delta.x) > BorderSpeed && fieldInput.swipeType == EnumSwipeType.Horizontal)
                    || (Mathf.Abs(fieldScan.delta.y) > BorderSpeed && fieldInput.swipeType == EnumSwipeType.Vertical))
                {
                    return SlipAlignVec(fieldScan.delta, fieldInput.swipeType);
                }
                else
                {
                    return BackAlignVec(fieldInput.distPosition, fieldInput.swipeType);
                }
                // return BackAlignVec(fieldInput.distPosition);
            }
            private EnumPieceAlignVec BackAlignVec(Vector2Int position, EnumSwipeType swipeType)
            {
                int halfPos = (GridSize / 2);
                if (swipeType == EnumSwipeType.Horizontal)
                {
                    int posX = Mathf.Abs(position.x) % GridSize;
                    if (posX != 0)
                    {
                        return ((posX < halfPos) ^ (position.x < 0))
                            ? EnumPieceAlignVec.Left
                            : EnumPieceAlignVec.Right;
                    }
                }
                else if (swipeType == EnumSwipeType.Vertical)
                {
                    int posY = Mathf.Abs(position.y) % GridSize;
                    if (posY != 0)
                    {
                        return ((posY < halfPos) ^ (position.y < 0))
                            ? EnumPieceAlignVec.Down
                            : EnumPieceAlignVec.Up;
                    }
                }
                return EnumPieceAlignVec.None;
            }

            private EnumPieceAlignVec SlipAlignVec(Vector2 delta, EnumSwipeType swipeType)
            {
                if (swipeType == EnumSwipeType.Horizontal)
                {
                    if (delta.x > 0)
                        return EnumPieceAlignVec.Right;
                    if (delta.x < 0)
                        return EnumPieceAlignVec.Left;
                }
                else if (swipeType == EnumSwipeType.Vertical)
                {
                    if (delta.y > 0)
                        return EnumPieceAlignVec.Up;
                    if (delta.y < 0)
                        return EnumPieceAlignVec.Down;
                }

                return EnumPieceAlignVec.None;
            }

        }
    }
}
