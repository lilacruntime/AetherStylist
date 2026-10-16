using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace AetherStylist.Plugin.Windows;

internal sealed class MainWindow : Window
{
    // These labels are UI examples only, with no item IDs or game-data lookup.
    private readonly string[] placeholderItems = ["Choose an item...", "Example jacket", "Example boots"];
    private int selectedItemIndex;

    // Everything after ### is an invisible, stable ImGui ID.
    public MainWindow() : base("AetherStylist ♡###AetherStylistMainWindow")
    {
        Size = new Vector2(460, 260);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 220),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.TextWrapped("Welcome to AetherStylist! Build a whole glamour around the pieces you love.");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted("Select a glamour item");
        ImGui.TextWrapped("Try the selector with fictional placeholders. Real item search will come in a later milestone.");
        ImGui.Combo("Item", ref selectedItemIndex, placeholderItems, placeholderItems.Length);

        if (selectedItemIndex > 0)
        {
            ImGui.TextUnformatted($"Selected: {placeholderItems[selectedItemIndex]}");
            if (ImGui.Button("Clear selection"))
                selectedItemIndex = 0;
        }
    }
}
