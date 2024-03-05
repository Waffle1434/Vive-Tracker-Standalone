using Unity.Mathematics;
using UnityEngine;
using Util;
using static Valve.VR.SteamVR_TrackedObject;

namespace Valve.VR {
    public class SteamVR_Tracker : MonoBehaviour {
        public EIndex index;

        public bool setPosition = true;
        public bool setRotation = true;

        public float rotationSmoothTime = 0f;

        public float3 velocity;
        public float3 angularVelocity;

        public bool isValid { get; private set; }

        SteamVR_Events.Action newPosesAction;
        Quaternion rot;
        Quaternion rot_vel;
        uint update = 0;

        SteamVR_Tracker() => newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);

        void OnNewPoses(TrackedDevicePose_t[] poses) {
            if (index == EIndex.None) return;

            isValid = false;
            int i = (int)index;
            if (poses.Length <= i || !poses[i].bDeviceIsConnected || !poses[i].bPoseIsValid) return;

            isValid = true;
            update++;

            TrackedDevicePose_t pose = poses[i];
            SteamVR_Utils.RigidTransform rt = new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);
            velocity = new(pose.vVelocity.v0, pose.vVelocity.v1, pose.vVelocity.v2);
            angularVelocity = new(pose.vAngularVelocity.v0, pose.vAngularVelocity.v1, pose.vAngularVelocity.v2);

            rot = MathUtil.SmoothDamp(update == 1 ? rt.rot : rot, rt.rot, ref rot_vel, rotationSmoothTime);

            if (setPosition) transform.localPosition = rt.pos;
            if (setRotation) transform.localRotation = rot;
        }

        void OnEnable() {
            if (SteamVR_Render.instance == null) {
                enabled = false;
                return;
            }

            newPosesAction.enabled = true;
        }

        void OnDisable() {
            newPosesAction.enabled = false;
            isValid = false;
        }
    }
}