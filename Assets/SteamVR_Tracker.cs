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
    public ETrackingResult tracking;

    Quaternion smoothed_rot;
    Quaternion rot_vel;
    bool initial = true;

    public static Matrix4x4 OVRToMatrix4x4(HmdMatrix34_t m_pose) {
        Matrix4x4 m = default;

        m.m00 = m_pose.m0;
        m.m01 = m_pose.m1;
        m.m02 = -m_pose.m2;
        m.m03 = m_pose.m3;

        m.m10 = m_pose.m4;
        m.m11 = m_pose.m5;
        m.m12 = -m_pose.m6;
        m.m13 = m_pose.m7;

        m.m20 = -m_pose.m8;
        m.m21 = -m_pose.m9;
        m.m22 = m_pose.m10;
        m.m23 = -m_pose.m11;

        return m;
    }

    void Update() {
        if (device == Device.None) return;

        TrackedDevicePose_t[] poses = SteamVR_Standalone.Instance.poses;

        int i_device = (int)device;
        if (poses.Length <= i_device) return;

        TrackedDevicePose_t pose = poses[i_device];
        if (!pose.bDeviceIsConnected || !pose.bPoseIsValid) return;

        Matrix4x4 m = OVRToMatrix4x4(pose.mDeviceToAbsoluteTracking);
        Quaternion rot = m.GetRotation();

        velocity = new(pose.vVelocity.v0, pose.vVelocity.v1, pose.vVelocity.v2);
        angularVelocity = new(pose.vAngularVelocity.v0, pose.vAngularVelocity.v1, pose.vAngularVelocity.v2);
        tracking = pose.eTrackingResult;

        smoothed_rot = MathUtil.SmoothDamp(initial ? rot : smoothed_rot, rot, ref rot_vel, rotationSmoothTime);

        if (setPosition) transform.localPosition = m.GetPosition();
        if (setRotation) transform.localRotation = smoothed_rot;

        initial = false;
    }
}