using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using Util;

public static class GizmoUtil {
    static List<Vector3> circle32, circle64, circle128;

    static GizmoUtil() {
        circle32 = GenerateCircleSegments(32);
        circle64 = GenerateCircleSegments(64);
        circle128 = GenerateCircleSegments(128);
    }

    public static List<Vector3> GenerateCircleSegments(int segments) {
        List<Vector3> circle = new List<Vector3>();
        float a = 0f;
        float da = MathUtil.TWO_PI / segments;
        const float r = 0.5f;

        for (int i = 0; i <= segments; i++, a += da)
            circle.Add(new Vector3(r * Mathf.Cos(a), r * Mathf.Sin(a), 0f));

        return circle;
    }

    public static void DrawCrossXZ() {
        Gizmos.DrawLine(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f));
        Gizmos.DrawLine(new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f));
    }
    public static void DrawStar(Vector3 pos, float size = 1f) {
        Matrix4x4 m = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.Translate(pos) * Matrix4x4.Scale(new Vector3(size, size, size));
        DrawStar();
        Gizmos.matrix = m;
    }
    public static void DrawStar() {
        Gizmos.DrawLine(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f));
        Gizmos.DrawLine(new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f));
        Gizmos.DrawLine(new Vector3(0f, -0.5f, 0f), new Vector3(0f, 0.5f, 0f));
    }

    /// <returns>End point</returns>
    public static Vector3 DrawPath(List<Vector3> points) {
        Vector3 p_l = default;

        for (int i = 0; i < points.Count; i++) {
            Vector3 p = points[i];
            if (i > 0) Gizmos.DrawLine(p_l, p);
            p_l = p;
        }

        return p_l;
    }
    public static void DrawLineStrip(List<float2> points) {
        Vector3 p_l = default;

        for (int i = 0; i < points.Count; i++) {
            Vector3 p = (Vector2)points[i];
            if (i > 0) Gizmos.DrawLine(p_l, p);
            p_l = p;
        }
    }

    public static void DrawLines(List<Vector3> lines) {
        for (int i = 0; i < lines.Count; )
            Gizmos.DrawLine(lines[i++], lines[i++]);
    }

    public static void DrawPoints(List<Vector3> points, float radius) {
        foreach (Vector3 p in points) Gizmos.DrawWireSphere(p, radius);
    }

    public static void DrawCircle(Matrix4x4 transformation) {
        Matrix4x4 m = Gizmos.matrix;
        Gizmos.matrix = m * transformation;
        DrawCircle();
        Gizmos.matrix = m;
    }

    /// <summary> Draw circle of radius 0.5 around the Z axis </summary>
    public static void DrawCircle(int segments = 64) {
        switch (segments) {
            case 32: DrawPath(circle32); break;
            case 64: DrawPath(circle64); break;
            case 128: DrawPath(circle128); break;
            default:
                float a = 0f;
                float da = MathUtil.TWO_PI / segments;
                Vector3 p_l = default;

                for (int i = 0; i <= segments; i++, a += da) {
                    Vector3 p = new Vector3(0.5f * Mathf.Cos(a), 0.5f * Mathf.Sin(a), 0f);
                    if (i > 0) Gizmos.DrawLine(p_l, p);
                    p_l = p;
                }
                break;
        }
    }


    public static void DrawConeAngle(Matrix4x4 transformation, float degrees, float length, int line_segments = 4) {
        Matrix4x4 m = Gizmos.matrix;
        Gizmos.matrix = m * transformation;
        DrawCone(MathUtil.ConeAngleToRadius(degrees, length), length, line_segments);
        Gizmos.matrix = m;
    }

    public static void DrawConeAngle(float degrees, float length, int line_segments = 4) => DrawCone(MathUtil.ConeAngleToRadius(degrees, length), length, line_segments);

    public static void DrawCone(float radius = 0.5f, float length = 1f, int line_segments = 4) {
        float a = 0f;
        float da = MathUtil.TWO_PI / line_segments;
        for (int i = 0; i < line_segments; i++, a += da)
            Gizmos.DrawLine(Vector3.zero, new Vector3(radius * Mathf.Cos(a), radius * Mathf.Sin(a), length));

        float s = radius / 0.5f;
        DrawCircle(Matrix4x4.Translate(new Vector3(0f, 0f, length)) * Matrix4x4.Scale(new Vector3(s, s, s)));
    }

    public static void DrawSphereHorizon(Vector3 pos, float r, Vector3 cam_pos) {
        Matrix4x4 m = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation((cam_pos - pos).normalized), Vector3.one);
        DrawSphereHorizon(Vector3.Distance(pos, cam_pos), r);
        Gizmos.matrix = m;
    }

    public static void DrawSphereHorizon(float d, float r = 0.5f) {
        Matrix4x4 m = Gizmos.matrix;
        float a1 = Mathf.Acos(r / d);
        Gizmos.matrix = m * Matrix4x4.TRS(new Vector3(0f, 0f, r * Math.Abs(Mathf.Cos(a1))), Quaternion.identity, Vector3.one * 2f * (r * Math.Abs(Mathf.Sin(a1))));
        DrawCircle(128);
        Gizmos.matrix = m;
    }


    [Conditional("UNITY_EDITOR")]
    public static void DrawLabel(Vector3 position, string text) {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(position, text);
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLabel(Vector3 position, string text, GUIStyle style) {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(position, text, style);
#endif
    }
}
