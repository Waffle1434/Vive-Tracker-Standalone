using System;
using UnityEngine;

namespace Util {
    public static class RandomUtil {
        private static System.Random unrepeatedRandom = new System.Random(DateTime.Now.Millisecond);
        private static int lastSeed = -1;

        /// <summary> Random between 0f inclusive and 1f exclusive. </summary>
        public static float NextFloat(this System.Random prng) => (float)prng.NextDouble();

        public static float Range(this System.Random prng, float minInclusive, float maxExclusive) => minInclusive + (maxExclusive - minInclusive) * prng.NextFloat();// inlined Mathf.Lerp

        public static void RandomizeUnityRandom() {
            int seed = (int)DateTime.Now.Ticks;
            if (seed != lastSeed) {
                lastSeed = seed;
                UnityEngine.Random.InitState(seed);
            }
        }

        /// <summary> Random between 0.0 inclusive and 1.0 exclusive. </summary>
        public static double NextDouble() => unrepeatedRandom.NextDouble();

        /// <summary> Random between 0f inclusive and 1f exclusive. </summary>
        public static float NextFloat() => unrepeatedRandom.NextFloat();

        /// <summary>
        /// Given a value, return a random number between the negative and positive of the input value.
        /// </summary>
        /// <param name="bookendRangeValue">Non-negative value.</param>
        public static float Range(float bookendRangeValue) => unrepeatedRandom.Range(-bookendRangeValue, bookendRangeValue);

        public static float Range(float minInclusive, float maxExclusive) => unrepeatedRandom.Range(minInclusive, maxExclusive);

        public static int Range(int minInclusive, int maxExclusive) => unrepeatedRandom.Next(minInclusive, maxExclusive);


        /// <summary> Random point uniformly distributed on the surface of a sphere of radius 1. </summary>
        public static Vector3 RandomOnUnitSphere() {
            double theta = MathUtil.TWO_PI * NextDouble();
            double phi = Math.Acos(2.0 * NextDouble() - 1.0);
            double sin_phi = Math.Sin(phi);
            return new Vector3(
                (float)(sin_phi * Math.Cos(theta)),
                (float)(sin_phi * Math.Sin(theta)),
                (float)Math.Cos(phi)
            );
        }

        /// <summary> Random point uniformly distributed inside a sphere of radius 1. </summary>
        public static Vector3 RandomInUnitSphere() => RandomOnUnitSphere() * (float)Math.Pow(NextDouble(), 1.0 / 3.0);


        /// <summary> Random point uniformly distributed on the perimeter of a circle of radius 1. </summary>
        public static Vector2 RandomOnUnitCircle() {
            double angle = MathUtil.TWO_PI * NextDouble();
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        /// <summary> Random point uniformly distributed in a circle of radius 1. </summary>
        public static Vector2 RandomInUnitCircle() => RandomOnUnitCircle() * (float)Math.Sqrt(NextDouble());

        /// <summary> Random point uniformly distributed in a ring starting at radius1 and ending at radius2. </summary>
        public static Vector2 RandomInRing(float r1, float r2) {
            float r2sq = r2 * r2;
            return RandomOnUnitCircle() * (float)Math.Sqrt(NextDouble() * (r1 * r1 - r2sq) + r2sq);
        }
    }
}
