using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

public class AccuracyTester : MonoBehaviour {
    public float period = 1f;

    struct Sample {
        public float3 position;
        public Quaternion rotation;
    }

    Stopwatch sw = new();
    List<Sample> samples = new();
    Quaternion avg_rot;

    void OnEnable() => Restart();

    void LateUpdate() {
        if (transform.hasChanged) {
            transform.hasChanged = false;

            Quaternion rotation = transform.rotation;
            samples.Add(new Sample { position = transform.position, rotation = rotation });

            avg_rot = Quaternion.Slerp(avg_rot, rotation, 1f / samples.Count);

            if (sw.Elapsed.TotalSeconds > period) {
                Calculate();
                Restart();
            }
        }
    }

    void Calculate() {
        float min_d = float.PositiveInfinity;
        float max_d = float.NegativeInfinity;
        float avg_d = 0f;
        float3 avg_pos = default;
        Vector3 avg_x_axis = default;
        Vector3 avg_y_axis = default;
        Vector3 avg_z_axis = default;
        float3 min_axis = new (float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        float3 max_axis = new (float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (Sample sample in samples) {
            avg_pos += sample.position;
            avg_x_axis += sample.rotation * Vector3.right;
            avg_y_axis += sample.rotation * Vector3.up;
            avg_z_axis += sample.rotation * Vector3.forward;
        }
        float coef = 1f / samples.Count;
        avg_pos *= coef;
        avg_x_axis *= coef;
        avg_y_axis *= coef;
        avg_z_axis *= coef;

        float[] distances = new float[samples.Count];

        for (int i = 0; i < samples.Count; i++) {
            Sample sample = samples[i];
            float d = math.distance(avg_pos, sample.position);
            distances[i] = d;
            avg_d += d;

            if (d < min_d) min_d = d;
            else if (d > max_d) max_d = d;


            float3 x_axis = sample.rotation * Vector3.right;
            float3 y_axis = sample.rotation * Vector3.up;
            float3 z_axis = sample.rotation * Vector3.forward;

            float3 axis_diffs = new(
                Vector3.Angle(x_axis, avg_x_axis),
                Vector3.Angle(y_axis, avg_y_axis),
                Vector3.Angle(z_axis, avg_z_axis)
            );

            min_axis = math.min(min_axis, axis_diffs);
            max_axis = math.max(max_axis, axis_diffs);
        }
        avg_d /= samples.Count;

        float std_sum = 0f;

        foreach (float d in distances) {
            float diff = d - avg_d;
            std_sum += diff * diff;
        }

        float std = math.sqrt(std_sum / (distances.Length - 1));

        print($"{samples.Count / period} Hz, Min D: {min_d}, Max D: {max_d}, Std: {std}, Min Axis: {min_axis}, Max Axis: {max_axis}");
    }

    void Restart() {
        samples.Clear();
        sw.Restart();
    }
}
