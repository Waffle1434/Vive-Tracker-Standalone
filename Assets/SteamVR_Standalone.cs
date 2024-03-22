using System;
using UnityEngine;
using UnityEngine.Rendering;
using Valve.VR;

public class SteamVR_Standalone : MonoBehaviour {
    public static SteamVR_Standalone Instance;
    public static event Action<TrackedDevicePose_t[]> OnNewPose;

    public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

    void UpdatePoses() {
        OpenVR.Compositor?.GetLastPoses(poses, gamePoses);
        OnNewPose?.Invoke(poses);
    }

    void OnEnable() {
        Instance = this;
        RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
    }
    void OnDisable() => RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
    void Update() => UpdatePoses();
    void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras) => UpdatePoses();
}
