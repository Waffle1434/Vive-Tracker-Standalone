using System.Diagnostics;
using UnityEngine;
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

    public Device device = Device.None;
    public bool setPosition = true;
    public bool setRotation = true;
    public float positionSmoothTime = 0f;
    public float rotationSmoothTime = 0f;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public ETrackingResult tracking;

    Stopwatch sw = new();
    Vector3 pos;
    Vector3 pos_vel;
    Quaternion rot;
    Quaternion rot_vel;

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

    public static Quaternion QuatSmoothDamp(Quaternion current, Quaternion target, ref Quaternion vel, float smoothTime, float deltaTime) {
        //if (deltaTime < Mathf.Epsilon) return current;
        if (smoothTime < Mathf.Epsilon) return target;
        // account for double-cover
        float Dot = Quaternion.Dot(current, target);
        float Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        Vector4 Result = new Vector4(
                Mathf.SmoothDamp(current.x, target.x, ref vel.x, smoothTime, float.PositiveInfinity, deltaTime),
                Mathf.SmoothDamp(current.y, target.y, ref vel.y, smoothTime, float.PositiveInfinity, deltaTime),
                Mathf.SmoothDamp(current.z, target.z, ref vel.z, smoothTime, float.PositiveInfinity, deltaTime),
                Mathf.SmoothDamp(current.w, target.w, ref vel.w, smoothTime, float.PositiveInfinity, deltaTime)
            ).normalized;

        // ensure deriv is tangent
        Vector4 derivError = Vector4.Project(new Vector4(vel.x, vel.y, vel.z, vel.w), Result);
        vel.x -= derivError.x;
        vel.y -= derivError.y;
        vel.z -= derivError.z;
        vel.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }

    public static Quaternion GetMatrixRotation(Matrix4x4 matrix) => Quaternion.LookRotation(new Vector3(matrix.m02, matrix.m12, matrix.m22), new Vector3(matrix.m01, matrix.m11, matrix.m21));

    void UpdateTransform(TrackedDevicePose_t[] poses) {
        if (device == Device.None) return;

        TrackedDevicePose_t pose = poses[(int)device];
        if (!pose.bDeviceIsConnected || !pose.bPoseIsValid) return;

        Matrix4x4 m = OVRToMatrix4x4(pose.mDeviceToAbsoluteTracking);
        velocity = new(pose.vVelocity.v0, pose.vVelocity.v1, pose.vVelocity.v2);
        angularVelocity = new(pose.vAngularVelocity.v0, pose.vAngularVelocity.v1, pose.vAngularVelocity.v2);
        tracking = pose.eTrackingResult;

        float dt = (float)sw.Elapsed.TotalSeconds;
        sw.Restart();

        if (setPosition) {
            pos = Vector3.SmoothDamp(pos, m.GetPosition(), ref pos_vel, positionSmoothTime, float.PositiveInfinity, dt);
            transform.localPosition = pos;
        }
        if (setRotation) {
            rot = QuatSmoothDamp(rot, GetMatrixRotation(m), ref rot_vel, rotationSmoothTime, dt);
            transform.localRotation = rot;
        }
    }
    void OnEnable() {
        transform.GetLocalPositionAndRotation(out pos, out rot);
        SteamVR_Standalone.OnNewPose += UpdateTransform;
    }

    void OnDisable() => SteamVR_Standalone.OnNewPose -= UpdateTransform;
}