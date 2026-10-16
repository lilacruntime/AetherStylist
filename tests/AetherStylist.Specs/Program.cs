using AetherStylist.Core;

static GlamourItem Item(uint id, EquipmentSlot slot, string[]? styles = null,
    Silhouette shape = Silhouette.Neutral, string[]? jobs = null, bool owned = true) =>
    new(id, $"Item{id}", slot, styles ?? ["alt"], new RgbColor(20, 20, 25),
        shape, Material.Cloth, jobs ?? ["ALL"], owned);

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
}

var items = new[]
{
    Item(1, EquipmentSlot.Body, ["alt"], Silhouette.Loose),
    Item(2, EquipmentSlot.Legs, ["alt"], Silhouette.Fitted),
    Item(3, EquipmentSlot.Legs, ["alt"], Silhouette.Loose),
    Item(4, EquipmentSlot.Head),
    Item(5, EquipmentSlot.Hands),
    Item(6, EquipmentSlot.Feet),
    Item(7, EquipmentSlot.Legs, ["alt"], Silhouette.Fitted, ["DRG"]),
    Item(8, EquipmentSlot.Feet, owned: false),
};

var engine = new RecommendationEngine(items);
var request = new OutfitRequest("alt", "WHM", PaletteMode.Auto, [1], false, 3);
var legs = engine.RecommendSlot(request, EquipmentSlot.Legs);
Check(legs.Count == 2, "Job-ineligible items are excluded");
Check(legs[0].Item.Id == 2, "Fitted legs are preferred with an oversized top");
var outfits = engine.BuildOutfits(request);
Check(outfits.Count > 0, "A complete outfit can be generated");
Check(outfits.All(x => x.Items[EquipmentSlot.Body].Id == 1), "Locked item is preserved");
Check(outfits.All(x => x.Items.Count == 5), "An outfit contains all five slots");
Check(engine.BuildOutfits(request with { OwnedOnly = true })
      .All(o => o.Items[EquipmentSlot.Feet].Id == 6), "Owned-only filter is enforced");
Check(engine.BuildOutfits(request with { LockedIds = [1, 2] })
      .All(o => o.Items[EquipmentSlot.Legs].Id == 2), "Two locked pieces remain locked");

var threw = false;
try { engine.BuildOutfits(request with { LockedIds = [2, 3] }); }
catch (ArgumentException) { threw = true; }
Check(threw, "Conflicting locked slots are rejected");

var red = new RgbColor(255, 0, 0);
var cyan = new RgbColor(0, 255, 255);
Check(ColourHarmony.Score(red, cyan, PaletteMode.Complementary) >
      ColourHarmony.Score(red, cyan, PaletteMode.Monochrome),
      "Complementary palette favours opposing hues");
Check(ColourHarmony.Score(red, cyan, PaletteMode.Auto) ==
      ColourHarmony.Score(cyan, red, PaletteMode.Auto), "Colour score is symmetrical");

Console.WriteLine("All starter checks passed.");
