using System.Text.Json;
using System.Text.Json.Serialization;
using AetherStylist.Core;

var path = Path.Combine(AppContext.BaseDirectory, "data", "catalogue.json");
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
options.Converters.Add(new JsonStringEnumConverter());
var items = JsonSerializer.Deserialize<List<GlamourItem>>(File.ReadAllText(path), options)
    ?? throw new InvalidDataException("Catalogue was empty.");

var engine = new RecommendationEngine(items);
var request = new OutfitRequest(
    Style: "alt", Job: "ALL", Palette: PaletteMode.Auto,
    LockedIds: [1001], MaxResults: 3);

Console.WriteLine("AetherStylist ♡ — sample catalogue (NOT real FFXIV items)");
Console.WriteLine("Locked: " + items.Single(i => i.Id == 1001).Name);
Console.WriteLine();

Console.WriteLine("Recommended leg pieces:");
foreach (var r in engine.RecommendSlot(request, EquipmentSlot.Legs, 4))
    Console.WriteLine($"  {r.MatchScore,3}/100  {r.Item.Name,-26} {r.Explanation}");

Console.WriteLine("\nComplete outfits:");
foreach (var outfit in engine.BuildOutfits(request))
{
    Console.WriteLine($"\n  Outfit heuristic score: {outfit.MatchScore}/100");
    foreach (var pair in outfit.Items.OrderBy(x => x.Key))
    {
        var suggested = outfit.SuggestedPieces.FirstOrDefault(x => x.Item.Id == pair.Value.Id);
        Console.WriteLine($"  {pair.Key,-6} {pair.Value.Name,-30} " +
                          (suggested is null ? "[LOCKED]" : $"{suggested.MatchScore}/100"));
    }
}
