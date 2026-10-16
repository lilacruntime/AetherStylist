namespace AetherStylist.Core;

/// <summary>Simple colour harmonies, intentionally a heuristic rather than colour science/ML.</summary>
public static class ColourHarmony
{
    public static double Score(RgbColor a, RgbColor b, PaletteMode mode)
    {
        var x = ToHsl(a);
        var y = ToHsl(b);
        var brightnessBalance = 1.0 - 0.25 * Math.Abs(x.L - y.L);

        // Low-saturation neutrals pair well with most colours; do not mistake grey hue for red.
        if (x.S < 0.13 || y.S < 0.13)
            return Math.Clamp(0.85 * brightnessBalance, 0, 1);

        var diff = Math.Abs(x.H - y.H);
        diff = Math.Min(diff, 360.0 - diff);

        static double Near(double distance, double target, double window) =>
            Math.Max(0.0, 1.0 - Math.Abs(distance - target) / window);

        var harmony = mode switch
        {
            PaletteMode.Monochrome => Near(diff, 0, 75),
            PaletteMode.Complementary => Near(diff, 180, 75),
            _ => Math.Max(Near(diff, 0, 75),
                     Math.Max(0.94 * Near(diff, 32, 75), 0.86 * Near(diff, 180, 75))),
        };
        return Math.Clamp(harmony * brightnessBalance, 0, 1);
    }

    private static (double H, double S, double L) ToHsl(RgbColor c)
    {
        var r = c.R / 255.0;
        var g = c.G / 255.0;
        var b = c.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        var l = (max + min) / 2.0;
        if (delta < 1e-9) return (0, 0, l);

        var s = delta / (1.0 - Math.Abs(2.0 * l - 1.0));
        double hue = max == r ? ((g - b) / delta) % 6.0
            : max == g ? (b - r) / delta + 2.0
            : (r - g) / delta + 4.0;
        return (((hue * 60.0) + 360.0) % 360.0, s, l);
    }
}
