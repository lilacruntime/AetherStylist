namespace AetherStylist.Core;

public enum EquipmentSlot { Head, Body, Hands, Legs, Feet }
public enum Silhouette { Neutral, Fitted, Loose, Structured, Flowing }
public enum Material { Other, Cloth, Knit, Denim, Leather, Metal }
public enum PaletteMode { Auto, Monochrome, Complementary }

public sealed record RgbColor(byte R, byte G, byte B);

/// <summary>
/// A normalized catalogue entry, deliberately independent of Dalamud/Lumina.
/// Appearance features are CURATED metadata in v0.1, not attributes supplied by FFXIV.
/// </summary>
public sealed record GlamourItem(
    uint Id,
    string Name,
    EquipmentSlot Slot,
    string[] Styles,
    RgbColor Color,
    Silhouette Silhouette,
    Material Material,
    string[] AllowedJobs,
    bool IsOwned);

public sealed record OutfitRequest(
    string Style,
    string Job,
    PaletteMode Palette,
    IReadOnlyList<uint> LockedIds,
    bool OwnedOnly = false,
    int MaxResults = 3);

/// <summary>All components are heuristic values between 0 and 1; never a calibrated probability.</summary>
public sealed record MatchBreakdown(double Aesthetic, double Colour, double Silhouette, double Material)
{
    public double Total => 0.40 * Aesthetic + 0.25 * Colour + 0.20 * Silhouette + 0.15 * Material;
}

public sealed record Recommendation(GlamourItem Item, MatchBreakdown Breakdown, string Explanation)
{
    public int MatchScore => (int)Math.Round(Breakdown.Total * 100);
}

public sealed record Outfit(
    IReadOnlyDictionary<EquipmentSlot, GlamourItem> Items,
    double Score,
    IReadOnlyList<Recommendation> SuggestedPieces)
{
    public int MatchScore => (int)Math.Round(Score * 100);
}
