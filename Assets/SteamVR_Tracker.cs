using Unity.Mathematics;
using UnityEngine;
using Util;
using Valve.VR;

public class SteamVR_Tracker : MonoBehaviour {
    public enum Device : int {
        None = -1,
        Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
        Device1,
        Device2,
        Device3,
        Device4,
        Device5,
        Device6,
        Device7,
        Device8,
        Device9,
        Device10,
        Device11,
        Device12,
        Device13,
        Device14,
        Device15,
        Device16
    }

    public Device device;
    public bool setPosition = true;
    public bool setRotation = true;
    public float rotationSmoothTime = 0f;
    public float3 velocity;
    public float3 angularVelocity;

    Quaternion rot;
    Quaternion rot_vel;
    bool initial = true;

    void Update() {
        if (device == Device.None) return;

        TrackedDevicePose_t[] poses = SteamVR_Standalone.Instance.poses;

        int i_device = (int)device;
        if (poses.Length <= i_device) return;

        TrackedDevicePose_t pose = poses[i_device];
        if (!pose.bDeviceIsConnected || !pose.bPoseIsValid) return;

        SteamVR_Utils.RigidTransform rt = new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);
        velocity = new(pose.vVelocity.v0, pose.vVelocity.v1, pose.vVelocity.v2);
        angularVelocity = new(pose.vAngularVelocity.v0, pose.vAngularVelocity.v1, pose.vAngularVelocity.v2);

        rot = MathUtil.SmoothDamp(initial ? rt.rot : rot, rt.rot, ref rot_vel, rotationSmoothTime);

        if (setPosition) transform.localPosition = rt.pos;
        if (setRotation) transform.localRotation = rot;

        initial = false;
    }
}