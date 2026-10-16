using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using AetherStylist.Plugin.Windows;

namespace AetherStylist.Plugin;

public sealed class Plugin : IDalamudPlugin
{
    // Dalamud supplies these services before calling the constructor.
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/aetherstylist";
    private readonly WindowSystem windowSystem = new("AetherStylist");
    private readonly MainWindow mainWindow = new();

    public Plugin()
    {
        windowSystem.AddWindow(mainWindow);
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the AetherStylist window."
        });

        // Draw is raised every UI frame; WindowSystem draws only open windows.
        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
    }

    public void Dispose()
    {
        // Undo registrations so disabling or reloading the plugin leaves no callbacks behind.
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        CommandManager.RemoveHandler(CommandName);
        windowSystem.RemoveAllWindows();
    }

    private void OnCommand(string command, string args) => mainWindow.Toggle();

    private void OpenMainUi() => mainWindow.IsOpen = true;
}
