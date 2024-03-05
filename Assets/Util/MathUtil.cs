using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Util {
    public static class MathUtil {
        public const float TWO_PI = 2f * Mathf.PI;

        public static int Mod(int x, int m) {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mod(float x, float m) {
            float r = x % m;
            return r < 0 ? r + m : r;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Mod(double x, double m) {
            double r = x % m;
            return r < 0 ? r + m : r;
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAbs(this float value, float abs_max) => System.Math.Clamp(value, -abs_max, abs_max);

        public static float ToSignedAngleDegrees(this float degrees) => Mod(degrees - 180f, 360f) - 180f;
        public static Vector3 ToSignedAngleDegrees(this Vector3 euler) => new(ToSignedAngleDegrees(euler.x), ToSignedAngleDegrees(euler.y), ToSignedAngleDegrees(euler.z));
        public static float3 ToSignedAngleDegrees(this float3 euler) => new(ToSignedAngleDegrees(euler.x), ToSignedAngleDegrees(euler.y), ToSignedAngleDegrees(euler.z));
        public static float2 ToSignedAngleDegrees(this float2 euler) => new(ToSignedAngleDegrees(euler.x), ToSignedAngleDegrees(euler.y));

        public static float SmoothDamp(this float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = float.PositiveInfinity, float deltaTime = 0f) {
            if (deltaTime == 0f) return target;
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pos2D(this Vector3 a) => new Vector2(a.x, a.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Mul(this Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(this Vector2 a) => new Vector2(System.Math.Abs(a.x), System.Math.Abs(a.y));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSq(this Vector3 a, Vector3 b) => (b - a).sqrMagnitude;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) => Vector2.Max(min, Vector2.Min(value, max));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ClampAbs(this Vector2 value, Vector2 abs_max) => value.Clamp(-abs_max, abs_max);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max) => Vector3.Max(min, Vector3.Min(value, max));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampAbs(this Vector3 value, Vector3 abs_max) => value.Clamp(-abs_max, abs_max);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(this Vector3 a) => new Vector3(Mathf.Round(a.x), Mathf.Round(a.y), Mathf.Round(a.z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Mul(this Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Div(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 a) => new Vector3(System.Math.Abs(a.x), System.Math.Abs(a.y), System.Math.Abs(a.z));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 F2ToVec3(this float2 p) => new(p.x, 0f, p.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 F2ToF3(this float2 p) => new(p.x, 0f, p.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 F3ToF2(this float3 p) => new(p.x, p.z);


        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref Quaternion vel, float smoothTime) {
            if (Time.deltaTime < Mathf.Epsilon) return current;
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
                Mathf.SmoothDamp(current.x, target.x, ref vel.x, smoothTime),
                Mathf.SmoothDamp(current.y, target.y, ref vel.y, smoothTime),
                Mathf.SmoothDamp(current.z, target.z, ref vel.z, smoothTime),
                Mathf.SmoothDamp(current.w, target.w, ref vel.w, smoothTime)
            ).normalized;

            // ensure deriv is tangent
            Vector4 derivError = Vector4.Project(new Vector4(vel.x, vel.y, vel.z, vel.w), Result);
            vel.x -= derivError.x;
            vel.y -= derivError.y;
            vel.z -= derivError.z;
            vel.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }

        public static Quaternion SmoothDampApprox(Quaternion current, Quaternion target, ref float angVel, float smoothTime) {
            float delta = Quaternion.Angle(current, target);
            if (delta > 0f) {
                float t = 1f - (Mathf.SmoothDampAngle(delta, 0f, ref angVel, smoothTime) / delta);
                return Quaternion.Slerp(current, target, t);
            }
            else return target;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeRange(float min, float max, float value) => (value - min) / (max - min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizeRange2(float min, float range, float value) => (value - min) / range;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MapRange(float min, float max, float outMin, float outMax, float value) {
            //return Mathf.Lerp(outMin, outMax, NormalizeRange(min,max, value));
            return (outMax - outMin) * ((value - min) / (max - min)) + outMin;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngularSizeAtDistance(float size, float distance) => Mathf.Tan(size / distance) * Mathf.Rad2Deg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SizeAtDistance(float angularSize, float distance) => Mathf.Atan(angularSize * Mathf.Deg2Rad) * distance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConeAngleToRadius(float angularSize, float length) => length * Mathf.Tan(0.5f * Mathf.Deg2Rad * angularSize);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomRange(this System.Random rng, float min, float max) {
            //return MapRange(0f, 1f, min, max, (float)rng.NextDouble());
            return (max - min) * (float)rng.NextDouble() + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomSymRange(this System.Random rng, float magnitude) => (2f * (float)rng.NextDouble() - 1f) * magnitude;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetPosition(this Matrix4x4 matrix) => new Vector3(matrix.m03, matrix.m13, matrix.m23);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion GetRotation(this Matrix4x4 matrix) => Quaternion.LookRotation(new Vector3(matrix.m02, matrix.m12, matrix.m22), new Vector3(matrix.m01, matrix.m11, matrix.m21));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetMatrix(this Transform transform, Matrix4x4 matrix, bool scale = false) {
            transform.SetPositionAndRotation(GetPosition(matrix), GetRotation(matrix));
            if (scale) transform.localScale = new Vector3(matrix.m00, matrix.m11, matrix.m22);
        }


        public static Matrix4x4 BillboardYMatrix(Vector3 pos, Vector3 up, Vector3 cam_pos) {
            Vector3 right = Vector3.Cross(up, cam_pos - pos).normalized;
            return new Matrix4x4(
                right,
                up,
                Vector3.Cross(right, up),
                new Vector4(pos.x, pos.y, pos.z, 1f)
            );
        }

        public static Matrix4x4 BillboardZMatrix(Vector3 pos, Vector3 fwd, Vector3 cam_pos) {
            Vector3 right = Vector3.Cross(fwd, cam_pos - pos).normalized;
            return new Matrix4x4(
                right,
                Vector3.Cross(right, fwd),
                fwd,
                new Vector4(pos.x, pos.y, pos.z, 1f)
            );
        }

        public static bool InTriangle(float3 a, float3 b, float3 c, float3 p) {
            float3 a_p = a - p;
            float3 b_p = b - p;
            float3 c_p = c - p;

            Vector3 u = Vector3.Cross(b_p, c_p);
            if (Vector3.Dot(u, Vector3.Cross(c_p, a_p)) < 0f) return false;
            if (Vector3.Dot(u, Vector3.Cross(a_p, b_p)) < 0f) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(this float2 a, float2 b) => a.x * b.y - a.y * b.x;
        public static bool InTriangle(float2 a, float2 b, float2 c, float2 p) {
            float2 v0 = a;
            float2 v1 = b - v0;
            float2 v2 = c - v0;

            float c_bac = 1f / Cross(v1, v2);
            float u = (Cross(p, v2) - Cross(v0, v2)) * c_bac;
            if (u <= 0f) return false;
            float v = -(Cross(p, v1) - Cross(v0, v1)) * c_bac;
            return v > 0f && u + v < 1f;
        }

        public static bool InTriangleOpt(float2 a, float2 b, float2 c, float2 p) {
            float2 v0 = a;
            float2 v1 = b - v0;
            float2 v2 = c - v0;

            float c_bac = 1f / (v1.x * v2.y - v1.y * v2.x);
            float u = (p.x * v2.y - p.y * v2.x - v0.x * v2.y + v0.y * v2.x) * c_bac;
            if (u <= 0f) return false;
            float v = -(p.x * v1.y - p.y * v1.x - v0.x * v1.y + v0.y * v1.x) * c_bac;
            return v > 0f && u + v < 1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static half2 Half2(float x, float y) => new half2((half)x, (half)y);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 TS(float3 pos, float3 scale) {
            return new float4x4(
                scale.x, 0f, 0f, pos.x,
                0f, scale.y, 0f, pos.y,
                0f, 0f, scale.z, pos.z,
                0f, 0f, 0f, 0f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 TS(float3 pos, float scale) {
            return new float4x4(
                scale, 0f, 0f, pos.x,
                0f, scale, 0f, pos.y,
                0f, 0f, scale, pos.z,
                0f, 0f, 0f, 0f
            );
        }
    }
}
