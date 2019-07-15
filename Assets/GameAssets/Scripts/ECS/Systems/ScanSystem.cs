using System;
using System.Collections.Generic;
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
    [UpdateInGroup(typeof(ScanGroup))]
    [UpdateBefore(typeof(InputGroup))]
    public class ScanSystem : ComponentSystem
    {
        ComponentGroup m_group;
        Vector2 m_offset;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.Create<FieldScan>()
            );

            m_offset = new Vector2(-(Screen.width / 2), -(Screen.height / 2));
        }
        protected override void OnUpdate()
        {
            var fieldScans = m_group.ToComponentDataArray<FieldScan>(Allocator.TempJob);
            var fieldBanishs = m_group.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            for (int i = 0; i < fieldScans.Length; i++)
            {
                var fieldScan = fieldScans[i];
                Scan(ref fieldScan);
                fieldScans[i] = fieldScan;
            }
            m_group.CopyFromComponentDataArray(fieldScans);

            fieldScans.Dispose();
            fieldBanishs.Dispose();
        }

        void Scan(ref FieldScan fieldScan)
        {
            TouchPhase phase = TouchPhase.Canceled;
            Vector2 pos = Vector2.zero;
            if (Input.GetMouseButtonDown(0))
            {
                phase = TouchPhase.Began;
                pos = (Vector2)Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                phase = TouchPhase.Moved;
                pos = (Vector2)Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                phase = TouchPhase.Ended;
                pos = (Vector2)Input.mousePosition;
            }

            Vector2 lastPosition = fieldScan.nowPosition;
            Vector2 nowPosition = pos + m_offset;
            fieldScan.phase = phase;
            fieldScan.nowPosition = nowPosition;
            switch (phase)
            {
                case TouchPhase.Began:
                    fieldScan.isTouch = true;
                    fieldScan.startPosition = nowPosition;
                    break;
                case TouchPhase.Ended:
                    fieldScan.isTouch = false;
                    break;
                case TouchPhase.Moved:
                    fieldScan.isTouch = true;
                    fieldScan.delta = nowPosition - lastPosition;
                    break;
            }
        }

    }
}
