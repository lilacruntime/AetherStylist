namespace AetherStylist.Core;

/// <summary>
/// Pure recommendation code: deterministic, testable, no game process or network access.
/// A compact beam search makes whole outfits rather than choosing each slot greedily.
/// </summary>
public sealed class RecommendationEngine
{
    private static readonly EquipmentSlot[] Order =
        [EquipmentSlot.Body, EquipmentSlot.Legs, EquipmentSlot.Feet,
         EquipmentSlot.Hands, EquipmentSlot.Head];

    private readonly IReadOnlyList<GlamourItem> catalogue;
    private readonly Dictionary<uint, GlamourItem> byId;

    public RecommendationEngine(IEnumerable<GlamourItem> items)
    {
        catalogue = items.ToList();
        if (catalogue.Any(i => i.Id == 0 || string.IsNullOrWhiteSpace(i.Name)))
            throw new ArgumentException("Each item needs a nonzero ID and name.", nameof(items));
        if (catalogue.Select(i => i.Id).Distinct().Count() != catalogue.Count)
            throw new ArgumentException("Item IDs must be unique.", nameof(items));
        byId = catalogue.ToDictionary(i => i.Id);
    }

    public IReadOnlyList<Recommendation> RecommendSlot(
        OutfitRequest request, EquipmentSlot slot, int limit = 10)
    {
        var locked = ResolveLocked(request);
        if (locked.ContainsKey(slot)) return [];
        return Candidates(request, locked, slot)
            .OrderByDescending(x => x.Breakdown.Total)
            .ThenBy(x => x.Item.Id)
            .Take(Math.Max(0, limit))
            .ToList();
    }

    public IReadOnlyList<Outfit> BuildOutfits(
        OutfitRequest request, int beamWidth = 18, int perSlot = 8)
    {
        if (request.MaxResults < 1) return [];
        var locked = ResolveLocked(request);
        var beam = new List<State> { new(new Dictionary<EquipmentSlot, GlamourItem>(locked), [], 0) };

        foreach (var slot in Order.Where(slot => !locked.ContainsKey(slot)))
        {
            var expanded = new List<State>();
            foreach (var state in beam)
            {
                foreach (var rec in Candidates(request, state.Items, slot)
                             .OrderByDescending(x => x.Breakdown.Total)
                             .ThenBy(x => x.Item.Id)
                             .Take(Math.Max(1, perSlot)))
                {
                    var next = new Dictionary<EquipmentSlot, GlamourItem>(state.Items) { [slot] = rec.Item };
                    var details = new List<Recommendation>(state.Suggestions) { rec };
                    expanded.Add(new State(next, details, state.Sum + rec.Breakdown.Total));
                }
            }
            if (expanded.Count == 0) return [];
            // A small beam avoids evaluating the full Cartesian product of every possible item.
            beam = expanded.OrderByDescending(s => s.Sum / s.Suggestions.Count)
                .ThenBy(s => string.Join("-", Order.Where(s.Items.ContainsKey).Select(x => s.Items[x].Id)))
                .Take(Math.Max(1, beamWidth)).ToList();
        }

        var result = beam.Select(s => new Outfit(s.Items,
                s.Suggestions.Count > 0 ? s.Sum / s.Suggestions.Count : 1.0,
                s.Suggestions))
            .OrderByDescending(o => o.Score)
            .ToList();

        // Avoid returning three near-identical looks if alternative combinations exist.
        var diverse = new List<Outfit>();
        foreach (var outfit in result)
        {
            if (diverse.Any(other =>
                    outfit.Items.Count(kv => other.Items[kv.Key].Id == kv.Value.Id) >= 4))
                continue;
            diverse.Add(outfit);
            if (diverse.Count >= request.MaxResults) break;
        }
        return diverse;
    }

    private Dictionary<EquipmentSlot, GlamourItem> ResolveLocked(OutfitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Style) || string.IsNullOrWhiteSpace(request.Job))
            throw new ArgumentException("Style and job must be set.", nameof(request));
        if (request.LockedIds.Distinct().Count() != request.LockedIds.Count)
            throw new ArgumentException("Duplicate locked item ID.", nameof(request));
        if (request.LockedIds.Count > 2)
            throw new ArgumentException("The MVP supports up to two locked items.", nameof(request));

        var resolved = new Dictionary<EquipmentSlot, GlamourItem>();
        foreach (var id in request.LockedIds)
        {
            if (!byId.TryGetValue(id, out var item))
                throw new ArgumentException($"Locked item {id} not found in catalogue.", nameof(request));
            if (!IsEligible(item, request))
                throw new ArgumentException($"Locked item {item.Name} is not eligible.", nameof(request));
            if (!resolved.TryAdd(item.Slot, item))
                throw new ArgumentException("Two items cannot be locked into the same slot.", nameof(request));
        }
        return resolved;
    }

    private IEnumerable<Recommendation> Candidates(
        OutfitRequest request, IReadOnlyDictionary<EquipmentSlot, GlamourItem> selected,
        EquipmentSlot slot)
    {
        foreach (var item in catalogue)
        {
            if (item.Slot != slot || !IsEligible(item, request)) continue;
            var breakdown = Score(item, selected.Values.ToArray(), request.Style, request.Palette);
            yield return new Recommendation(item, breakdown, Explain(item, selected.Values, request.Style));
        }
    }

    private static bool IsEligible(GlamourItem item, OutfitRequest request) =>
        (!request.OwnedOnly || item.IsOwned)
        && (item.AllowedJobs.Any(j => j.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            || item.AllowedJobs.Any(j => j.Equals(request.Job, StringComparison.OrdinalIgnoreCase)));

    public static MatchBreakdown Score(GlamourItem item, IReadOnlyList<GlamourItem> selected,
        string aesthetic, PaletteMode palette)
    {
        var directAesthetic = item.Styles.Any(s => s.Equals(aesthetic, StringComparison.OrdinalIgnoreCase))
            ? 1.0 : item.Styles.Length == 0 ? 0.50 : 0.20;
        var sharedStyle = selected.Count == 0 ? 0.60 : selected.Average(other =>
            item.Styles.Intersect(other.Styles, StringComparer.OrdinalIgnoreCase).Any() ? 1.0 : 0.40);
        var aestheticScore = 0.80 * directAesthetic + 0.20 * sharedStyle;

        var colour = selected.Count == 0 ? 0.65
            : selected.Average(other => ColourHarmony.Score(item.Color, other.Color, palette));
        var material = selected.Count == 0 ? 0.65
            : selected.Average(other => MaterialScore(item.Material, other.Material));

        var opposite = selected.FirstOrDefault(other =>
            (item.Slot == EquipmentSlot.Body && other.Slot == EquipmentSlot.Legs)
            || (item.Slot == EquipmentSlot.Legs && other.Slot == EquipmentSlot.Body));
        var silhouette = opposite is null ? 0.70 : ShapeScore(item.Silhouette, opposite.Silhouette);

        return new MatchBreakdown(aestheticScore, colour, silhouette, material);
    }

    private static double ShapeScore(Silhouette a, Silhouette b)
    {
        if ((a == Silhouette.Loose && b == Silhouette.Fitted)
            || (a == Silhouette.Fitted && b == Silhouette.Loose)) return 1.0;
        if ((a == Silhouette.Structured && b == Silhouette.Flowing)
            || (a == Silhouette.Flowing && b == Silhouette.Structured)) return 0.90;
        if (a == Silhouette.Loose && b == Silhouette.Loose) return 0.38;
        return 0.70;
    }

    private static double MaterialScore(Material a, Material b)
    {
        if (a == b) return 0.98;
        var pair = new HashSet<Material> { a, b };
        if (pair.SetEquals([Material.Cloth, Material.Knit])
            || pair.SetEquals([Material.Denim, Material.Leather])
            || pair.SetEquals([Material.Leather, Material.Metal])) return 0.90;
        return 0.65;
    }

    private static string Explain(GlamourItem item, IEnumerable<GlamourItem> selected, string style)
    {
        var counterpart = selected.FirstOrDefault(other =>
            (item.Slot == EquipmentSlot.Legs && other.Slot == EquipmentSlot.Body)
            || (item.Slot == EquipmentSlot.Body && other.Slot == EquipmentSlot.Legs));
        if (counterpart is not null && ShapeScore(item.Silhouette, counterpart.Silhouette) >= 0.95)
            return "Balances the silhouette of " + counterpart.Name + ".";
        if (item.Styles.Any(s => s.Equals(style, StringComparison.OrdinalIgnoreCase)))
            return "Fits your chosen " + style + " aesthetic.";
        return "Coordinates with the pieces currently selected.";
    }

    private sealed record State(
        Dictionary<EquipmentSlot, GlamourItem> Items,
        List<Recommendation> Suggestions,
        double Sum);
}
