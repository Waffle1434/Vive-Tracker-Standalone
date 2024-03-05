using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

public static class GeomUtil {
    /// <summary> Shortest distance from p to line formed by p1 & p2 </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PerpendicularDistance(float2 p1, float2 p2, float2 p) {
        //float area = 0.5f * math.abs(p1.x * (p2.y - p.y) + p2.x * (p.y - p1.y) + p.x * (p1.y - p2.y));
        //return area / math.distance(p1, p2) * 2f;

        float2 diff = p2 - p1;
        float2 n = new(p2.y - p1.y, p1.x - p2.x);
        float C = p1.y * diff.x - p1.x * diff.y;
        return math.abs(math.dot(n, p) + C) / math.length(n);
    }

    // https://www.codeproject.com/Articles/18936/A-C-Implementation-of-Douglas-Peucker-Line-Appro
    public static List<float2> DouglasPeuckerReduction(List<float2> points, float tolerance) {
        if (points == null || points.Count < 3)
            return points;

        int first = 0;
        int last = points.Count - 1;
        List<int> keepers = new() { first, last };

        // The first and the last point cannot be the same
        while (points[first].Equals(points[last])) last--;

        DouglasPeuckerReduction(points, first, last, tolerance, keepers);

        keepers.Sort();
        List<float2> points_reduced = new(keepers.Count);
        foreach (int index in keepers)
            points_reduced.Add(points[index]);

        return points_reduced;
    }

    static void DouglasPeuckerReduction(List<float2> points, int first, int last, float tolerance, List<int> keepers) {
        float max_d = 0;
        int i_farthest = 0;

        for (int index = first; index < last; index++) {
            float d = PerpendicularDistance(points[first], points[last], points[index]);
            if (d > max_d) {
                max_d = d;
                i_farthest = index;
            }
        }

        if (max_d > tolerance && i_farthest != 0) {
            keepers.Add(i_farthest);

            DouglasPeuckerReduction(points, first, i_farthest, tolerance, keepers);
            DouglasPeuckerReduction(points, i_farthest, last, tolerance, keepers);
        }
    }

    // https://rootllama.wordpress.com/2014/06/20/ray-line-segment-intersection-test-in-2d/
    public static bool TryRaySegmentIntersection(float2 ray_p, float2 ray_dir, float2 p1, float2 p2, out float t_seg) {
        float2 v2 = p2 - p1;
        float2 v3 = new(-ray_dir.y, ray_dir.x);

        float dot = math.dot(v2, v3);
        if (math.abs(dot) < 0.000001f) { // Parallel
            t_seg = -1f;
            return false;
        }

        float2 v1 = ray_p - p1;
        t_seg = math.dot(v1, v3) / dot;
        //t_ray = MathUtil.Cross(v2, v1) / dot;
        return t_seg >= 0f && t_seg <= 1f;
    }

    // https://stackoverflow.com/questions/5666222/3d-line-plane-intersection
    public static bool TryPlaneSegmentIntersection(float3 plane_normal, float plane_d, float3 p1, float3 p2, out float t_seg, out float3 p_intersect) {
        float3 v1 = p2 - p1;
        float dot = math.dot(plane_normal, v1);

        if (math.abs(dot) < 0.000001f) { // Parallel
            t_seg = -1f;
            p_intersect = default;
            return false;
        }

        t_seg = -math.dot(plane_normal, p1 - plane_normal * plane_d) / dot;
        p_intersect = p1 + v1 * t_seg;
        return t_seg >= 0f && t_seg <= 1f;
    }

    public static float3 CubeToSphere(float3 cube) => math.normalize(cube);
    public static double3 CubeToSphere(double3 cube) => math.normalize(cube);
    /*public static float3 SphereToCube(float3 sphere) {
        //float3 sphere * math.length(cube) = cube;
    }*/

    public static float3 CubeToSphereEven(float3 cube) { // https://catlikecoding.com/unity/tutorials/cube-sphere/
        float x2 = cube.x * cube.x;
        float y2 = cube.y * cube.y;
        float z2 = cube.z * cube.z;
        float x2_2 = 0.5f * x2;
        float y2_2 = 0.5f * y2;
        float z2_2 = 0.5f * z2;
        const float _3 = 1f / 3f;
        return new(
            cube.x * math.sqrt(1f - y2_2 - z2_2 + y2 * z2 * _3),
            cube.y * math.sqrt(1f - x2_2 - z2_2 + x2 * z2 * _3),
            cube.z * math.sqrt(1f - x2_2 - y2_2 + x2 * y2 * _3)
        );
    }
    public static double3 CubeToSphereEven(double3 cube) { // https://catlikecoding.com/unity/tutorials/cube-sphere/
        double x2 = cube.x * cube.x;
        double y2 = cube.y * cube.y;
        double z2 = cube.z * cube.z;
        double x2_2 = 0.5f * x2;
        double y2_2 = 0.5f * y2;
        double z2_2 = 0.5f * z2;
        const double _3 = 1.0 / 3.0;
        return new(
            cube.x * math.sqrt(1f - y2_2 - z2_2 + y2 * z2 * _3),
            cube.y * math.sqrt(1f - x2_2 - z2_2 + x2 * z2 * _3),
            cube.z * math.sqrt(1f - x2_2 - y2_2 + x2 * y2 * _3)
        );
    }

    public static double3 cubify(double3 sphere) { // https://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube
        double _2x2 = 2.0 * sphere.x * sphere.x;
        double _2Y2 = 2.0 * sphere.y * sphere.y;
        double2 v = new(_2x2 - _2Y2, _2Y2 - _2x2);

        double i = v.y - 3.0;
        double isqrt = -math.sqrt(i * i - 12.0 * _2x2) + 3.0;
        const double isqrt2 = 1.0 / math.SQRT2_DBL;

        return math.sign(sphere) * new double3(math.sqrt(v + isqrt) * isqrt2, 1.0);
    }

    public static double3 sphere2cube(double3 sphere) {
        double3 f = math.abs(sphere);
        if (f.y >= f.x && f.y >= f.z)
            return cubify(sphere.xzy).xzy;
        if (f.x >= f.z)
            return cubify(sphere.yzx).zxy;
        else
            return cubify(sphere);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceToPlane(float3 n, float d, float3 p) => (math.dot(n, p) + d) / math.length(n);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceToPlaneNormal(float3 n, float3 p) => math.dot(n, p);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceToPlaneNormal(float2 n, float2 p) => math.dot(n, p);
}