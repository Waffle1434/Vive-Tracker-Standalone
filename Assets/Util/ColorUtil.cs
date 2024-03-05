using System.Runtime.CompilerServices;
using UnityEngine;

public static class ColorUtil {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasColor(this Color color) => color.r > 0f || color.g > 0f || color.b > 0f;

    public static float GetValue(this Color color) {
        if (color.b > color.g && color.b > color.r) return color.b;
        else if (color.g > color.r) return color.g;
        else return color.r;
    }
}
