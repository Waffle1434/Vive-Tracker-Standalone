using UnityEngine;
using Valve.VR;

public class SteamVR_Standalone : MonoBehaviour {
    public static SteamVR_Standalone Instance;

    public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

    void UpdatePoses() {
        CVRCompositor compositor = OpenVR.Compositor;
        if (compositor != null) {
            compositor.GetLastPoses(poses, gamePoses);
        }
    }

    void OnEnable() => Instance = this;
    void Update() => UpdatePoses();
}
