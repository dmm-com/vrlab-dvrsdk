using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace DVRSDK.Avatar.Tracking.UnityXR
{
    public class UnityXRTracker : MonoBehaviour, ITracker
    {
        [Header("親に指定したいTransform。未指定の場合このオブジェクトの子に配置します")]
        [SerializeField]
        private Transform trackersParent = null;
        public Transform TrackersParent => trackersParent;

        //外部からTransformを指定されたときはそちらをそのまま使用する。無い場合は自動で作成して割り当てる
        [Header("使用したい部位をすべて定義します")]
        public readonly TrackerTarget[] TrackerTargets = new TrackerTarget[]
        {
            new TrackerTarget { TrackerPosition = TrackerPositions.Head, UseDeviceType = TrackingDeviceType.HMD },
            new TrackerTarget { TrackerPosition = TrackerPositions.LeftHand, UseDeviceType = TrackingDeviceType.Controller },
            new TrackerTarget { TrackerPosition = TrackerPositions.RightHand, UseDeviceType = TrackingDeviceType.Controller },
            new TrackerTarget { TrackerPosition = TrackerPositions.Waist, UseDeviceType = TrackingDeviceType.GenericTracker },
            new TrackerTarget { TrackerPosition = TrackerPositions.LeftFoot, UseDeviceType = TrackingDeviceType.GenericTracker },
            new TrackerTarget { TrackerPosition = TrackerPositions.RightFoot, UseDeviceType = TrackingDeviceType.GenericTracker },
        };

        public TrackerTarget GetTrackerTarget(TrackerPositions trackerPosition) => TrackerTargets.FirstOrDefault(d => d.TrackerPosition == trackerPosition && d.PoseIsValid);

        //暫定：SteamVRと同じ
        public Vector3 GetIKOffsetPosition(TrackerPositions targetPosition, TrackingDeviceType deviceType)
        {
            if (targetPosition == TrackerPositions.LeftHand && deviceType == TrackingDeviceType.Controller)
            {
                return new Vector3(-0.04f, 0.04f, -0.15f);
            }
            else if (targetPosition == TrackerPositions.RightHand && deviceType == TrackingDeviceType.Controller)
            {
                return new Vector3(0.04f, 0.04f, -0.15f);
            }
            else
            {
                return Vector3.zero;
            }
        }
        public Quaternion GetIKOffsetRotation(TrackerPositions targetPosition, TrackingDeviceType deviceType)
        {
            if (targetPosition == TrackerPositions.LeftHand && deviceType == TrackingDeviceType.Controller)
            {
                return Quaternion.Euler(-30, 0, 90);
            }
            else if (targetPosition == TrackerPositions.RightHand && deviceType == TrackingDeviceType.Controller)
            {
                return Quaternion.Euler(-30, 0, -90);
            }
            else
            {
                return Quaternion.identity;
            }
        }

        private List<XRNodeState> nodeStates = new List<XRNodeState>();

        private void Awake()
        {
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
        }

        private void Start()
        {
            foreach (var trackerTarget in TrackerTargets)
            {
                if (trackerTarget.TargetTransform == null)
                {
                    var newTarget = new GameObject(trackerTarget.TrackerPosition.ToString());
                    if (trackersParent == null) trackersParent = transform;
                    newTarget.transform.SetParent(TrackersParent, false);
                    trackerTarget.TargetTransform = newTarget.transform;
                }
            }
        }

        private void Update()
        {
            UpdateAllTrackerData();
        }

        private void SetTransformPosition(Transform transform, XRNode deviceType)
        {
            transform.localPosition = InputTracking.GetLocalPosition(deviceType);
            transform.localRotation = InputTracking.GetLocalRotation(deviceType);
        }

        private void UpdateAllTrackerData()
        {
            InputTracking.GetNodeStates(nodeStates);
            //全トラッカーの位置データ更新
            foreach (var trackerTarget in TrackerTargets)
            {
                if (trackerTarget.SourceTransform != null) //スキップ設定、外部CameraRig等ですでに位置データ処理しているときを想定
                {
                    trackerTarget.TargetTransform.localPosition = trackerTarget.SourceTransform.localPosition;
                    trackerTarget.TargetTransform.localRotation = trackerTarget.SourceTransform.localRotation;
                    trackerTarget.PoseIsValid = true;
                }
                else
                {
                    trackerTarget.PoseIsValid = false;

                    if (trackerTarget.TrackerPosition == TrackerPositions.Head)
                    {
                        SetTransformPosition(trackerTarget.TargetTransform, XRNode.CenterEye);
                        trackerTarget.PoseIsValid = true;
                    }
                    else if (trackerTarget.TrackerPosition == TrackerPositions.LeftHand)
                    {
                        SetTransformPosition(trackerTarget.TargetTransform, XRNode.LeftHand);
                        trackerTarget.PoseIsValid = true;
                    }
                    else if (trackerTarget.TrackerPosition == TrackerPositions.RightHand)
                    {
                        SetTransformPosition(trackerTarget.TargetTransform, XRNode.RightHand);
                        trackerTarget.PoseIsValid = true;
                    }
                    else if (trackerTarget.DeviceIndex != 0) //Index番号でトラッカー位置取得するとき
                    {
                        foreach (var node in nodeStates)
                        {
                            if ((int)node.uniqueID == trackerTarget.DeviceIndex)
                            {
                                var pos = Vector3.zero;
                                var rot = Quaternion.identity;
                                if (node.TryGetPosition(out pos)) trackerTarget.TargetTransform.localPosition = pos;
                                if (node.TryGetRotation(out rot)) trackerTarget.TargetTransform.localRotation = rot;
                                trackerTarget.PoseIsValid = true;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 自動で指定したデバイスを指定した部位に割り当て
        /// </summary>
        /// <param name="worldForwardVector">HmdTransform.forward</param>
        /// <param name="worldUpVector">HmdTransform.up</param>
        public void AutoAttachTrackerTargets(Vector3? worldForwardVector = null, Vector3? worldUpVector = null)
        {
            //頭
            var trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.Head);
            trackerTarget.DeviceIndex = 0;
            trackerTarget.UseDeviceType = TrackingDeviceType.HMD;

            var forward = worldForwardVector ?? trackerTarget.TargetTransform.forward;
            var up = worldUpVector ?? trackerTarget.TargetTransform.up;

            //左手
            trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.LeftHand);
            trackerTarget.DeviceIndex = 0;
            trackerTarget.UseDeviceType = TrackingDeviceType.Controller;

            //右手
            trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.RightHand);
            trackerTarget.DeviceIndex = 0;
            trackerTarget.UseDeviceType = TrackingDeviceType.Controller;

            InputTracking.GetNodeStates(nodeStates);


            var trackers = nodeStates.Where(d => d.nodeType == XRNode.HardwareTracker).ToList();

            //トラッカー残数が1で腰がトラッカーの時は足の処理をスキップ
            trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.Waist);
            if (trackers.Count != 1 || trackerTarget == null || trackerTarget.UseDeviceType != TrackingDeviceType.GenericTracker)
            {
                //左足
                trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.LeftFoot);
                AttachIndexDevice(trackerTarget, trackers,
                    t => t.OrderBy(d => GetPosition(d).y)
                          .Take(2)
                          .OrderBy(d => RecenterPoint(forward, up, GetPosition(d)).x));

                //右足
                trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.RightFoot);
                AttachIndexDevice(trackerTarget, trackers,
                    t => t.OrderBy(d => GetPosition(d).y)
                          .Take(2)
                          .OrderByDescending(d => RecenterPoint(forward, up, GetPosition(d)).x));
            }

            //腰
            trackerTarget = TrackerTargets.FirstOrDefault(d => d.TrackerPosition == TrackerPositions.Waist);
            AttachIndexDevice(trackerTarget, trackers,
                t => t.OrderByDescending(d => GetPosition(d).y));
        }

        private Vector3 GetPosition(XRNodeState state)
        {
            Vector3 pos;
            state.TryGetPosition(out pos);
            return pos;
        }

        // forwardVectorとupVectorには正面方向(hmdTransform.forward, hmdTransform.up)を入れる
        private Vector3 RecenterPoint(Vector3 forwardVector, Vector3 upVector, Vector3 pos)
        {
            var rotation = Quaternion.LookRotation(forwardVector, upVector);
            var frontRotation = Quaternion.identity;
            var diffRotation = frontRotation * Quaternion.Inverse(rotation);
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, diffRotation, Vector3.one);
            //Matrix4x4 inverse = matrix.inverse;
            return matrix.MultiplyPoint3x4(pos);
        }

        private void AttachIndexDevice(TrackerTarget trackerTarget, List<XRNodeState> trackers,
            Func<IEnumerable<XRNodeState>, IEnumerable<XRNodeState>> trackerSelector)
        {

            if (trackerTarget == null) return;

            var tracker = trackerSelector(trackers).FirstOrDefault();
            trackerTarget.DeviceIndex = (int)tracker.uniqueID;
        }
    }
}
