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

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldScan>(),
                ComponentType.ReadOnly<FieldBanish>(),
                ComponentType.Create<FieldInput>()
            );
            // m_groupGrid = GetComponentGroup(
            //     ComponentType.ReadOnly<GridState>()
            // );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            var job = new MoveJob()
            {
                fieldScans = m_groupField.GetComponentDataArray<FieldScan>(),
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                // gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                // PixelSize = Define.Instance.Common.PixelSize,
                SwipeThreshold = Define.Instance.Common.SwipeThreshold,
            };

            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct MoveJob : IJob
        {
            [ReadOnly]
            public ComponentDataArray<FieldScan> fieldScans;
            [ReadOnly]
            public ComponentDataArray<FieldBanish> fieldBanishs;
            public ComponentDataArray<FieldInput> fieldInputs;

            public float SwipeThreshold;

            public void Execute()
            {

                for (int i = 0; i < fieldScans.Length; i++)
                {
                    var fieldScan = fieldScans[i];
                    var fieldInput = fieldInputs[i];
                    var fieldBanish = fieldBanishs[i];

                    if (fieldBanish.isBanish)
                    {
                        if (!fieldInput.isHold)
                            return;

                        UpdateEnded(i, fieldInput);
                        break;
                    }

                    switch (fieldScan.phase)
                    {
                        case TouchPhase.Began:
                            UpdateBegan(i, fieldScan, fieldInput);
                            break;
                        case TouchPhase.Ended:
                            UpdateEnded(i, fieldInput);
                            break;
                        case TouchPhase.Moved:
                            UpdateMoved(i, fieldScan, fieldInput);
                            break;
                    }
                }
            }

            private void UpdateBegan(int i, FieldScan fieldScan, FieldInput fieldInput)
            {
                if (!fieldScan.isInfield)
                    return;

                fieldInput.isHold = true;
                fieldInput.gridPosition = new Vector2Int((int)fieldScan.gridPosition.x, (int)fieldScan.gridPosition.y);
                fieldInput.startPosition = new Vector2((int)fieldScan.startPosition.x, (int)fieldScan.startPosition.y);
                fieldInputs[i] = fieldInput;
            }

            private void UpdateEnded(int i, FieldInput fieldInput)
            {
                fieldInput.isHold = false;
                fieldInput.swipeType = EnumSwipeType.None;
                fieldInputs[i] = fieldInput;
            }

            private void UpdateMoved(int i, FieldScan fieldScan, FieldInput fieldInput)
            {
                if (!fieldInput.isHold)
                    return;

                Vector2 distPosition = (fieldScan.nowPosition - fieldInput.startPosition);
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
                fieldInput.distPosition = distPosition;
                fieldInputs[i] = fieldInput;
            }

        }
    }
}
