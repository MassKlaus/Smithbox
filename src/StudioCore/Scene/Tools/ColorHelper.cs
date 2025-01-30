﻿using StudioCore.Scene.RenderableProxy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Scene.Tools;

public static class ColorHelper
{
    /// <summary>
    /// Generate a set of shades from a base color. Number of shades MUST be even.
    /// </summary>
    public static Color[] GenerateShades(Color baseColor, int size)
    {
        Color[] shades = new Color[size];

        var halfSize = size / 2;

        for (int i = 0; i < halfSize; i++) // Generate 3 lighter shades
        {
            float factor = 1f + (0.2f * (i + 1)); // Increase brightness
            shades[i] = ChangeBrightness(baseColor, factor);
        }

        for (int i = 0; i < halfSize; i++) // Generate 3 darker shades
        {
            float factor = 1f - (0.2f * (i + 1)); // Decrease brightness
            shades[i + halfSize] = ChangeBrightness(baseColor, factor);
        }

        return shades;
    }

    private static Color ChangeBrightness(Color color, float factor)
    {
        int r = (int)(color.R * factor);
        int g = (int)(color.G * factor);
        int b = (int)(color.B * factor);

        return Color.FromArgb(
            color.A, // Preserve alpha
            Math.Clamp(r, 0, 255),
            Math.Clamp(g, 0, 255),
            Math.Clamp(b, 0, 255)
        );
    }
    public static Color GetRenderableColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    private static float _colorHueIncrement;

    public static void ApplyColorVariance(DebugPrimitiveRenderableProxy rend)
    {
        // Determines how much color varies per-increment.
        const float incrementModifier = 0.721f;

        rend._hasColorVariance = true;

        Vector3 hsv = Utils.ColorToHSV(rend._initialColor);
        var range = 360.0f * CFG.Current.GFX_Wireframe_Color_Variance / 2;
        _colorHueIncrement += range * incrementModifier;
        if (_colorHueIncrement > range)
        {
            _colorHueIncrement -= range * 2;
        }

        hsv.X += _colorHueIncrement;
        if (hsv.X > 360.0f)
        {
            hsv.X -= 360.0f;
        }
        else if (hsv.X < 0.0f)
        {
            hsv.X += 360.0f;
        }

        rend.BaseColor = Utils.ColorFromHSV(hsv);
    }
}
