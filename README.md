<div align="center">
  <h1>✧ AetherStylist ♡</h1>
  <p><strong>Build a whole glamour around the pieces you love.</strong></p>
  <p><em>A little wardrobe brain for Eorzea.</em> ✨</p>
  <p>
    <img alt="C#" src="https://img.shields.io/badge/C%23-9B74CE?style=for-the-badge&logo=csharp&logoColor=white" />
    <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-BA8DD6?style=for-the-badge&logo=dotnet&logoColor=white" />
    <img alt="FFXIV" src="https://img.shields.io/badge/FFXIV-Dalamud_API_15-E99ABC?style=for-the-badge" />
    <img alt="Status" src="https://img.shields.io/badge/status-milestone_2-E69DBE?style=for-the-badge&labelColor=7F557D" />
  </p>
  <p>
    <a href="#-the-idea">The idea</a> ·
    <a href="#-features">Features</a> ·
    <a href="#-getting-started">Getting started</a> ·
    <a href="#-roadmap">Roadmap</a>
  </p>
</div>
---

## ♡ The idea

Ever found the *perfect* jacket, boots or skirt in **Final Fantasy XIV**, only to spend the next hour trying to make the rest of your glamour work?

**AetherStylist** is an in-development glamour recommendation assistant built around one simple interaction:

> **✧ Build Around This** — choose one or two favourite pieces, lock them in place, and discover the rest of your outfit.

The aim isn't to pick random equipment with matching names. It's to suggest outfits that feel intentional, combining **aesthetic tags** with **visual compatibility** and, eventually, **real character and glamour restrictions**.

### The dream workflow

`Choose a piece` → `Lock it` → `Pick an aesthetic` → `Explore matching pieces` → `Save your glam` ♡

*AetherStylist includes a standalone C# engine prototype and a minimal Dalamud API 15 plugin. The plugin currently shows a welcome window and a fictional placeholder item selector; the workflow above is our goal.*

## ✨ Features

| Available in the prototype | Planned for the Dalamud plugin |
| --- | --- |
| ♡ Lock starting items | 🔎 Search real FFXIV equipment |
| 🎀 Match against aesthetic tags | 🪞 Preview glamour combinations |
| 🎨 Score colour relationships | 🧵 Use richer visual metadata from item renders |
| ✂️ Balance garment silhouettes | 🧍 Validate race, gender, job and level restrictions |
| 🪡 Consider material pairings | 💾 Save and revisit your outfits |
| 🌸 Generate multiple complete looks via beam search | 🎨 Recommend dyes and palettes |
| 📝 Explain why items match | 🔗 Explore Glamourer / Penumbra integrations |

The initial engine uses **manually tagged, fictional clothes**. It does **not** yet read real FFXIV items, access your wardrobe or change your character's appearance.

## 🧠 How the matching works

The recommendation engine is written as a **standalone C# library**, independent of Dalamud, so it's easy to test and improve.

For each candidate piece, it calculates a weighted *heuristic* score:

| Signal | Weight | What it asks |
| --- | ---: | --- |
| 🎀 **Aesthetic** | **40%** | Does this fit the style you've chosen? |
| 🎨 **Colour** | **25%** | Does the palette work with the locked pieces? |
| ✂️ **Silhouette** | **20%** | Do the shapes balance each other? |
| 🪡 **Material** | **15%** | Do the textures and finishes work together? |

It then uses **beam search** to explore several complete outfits instead of choosing each slot independently. Locked pieces stay locked, and suggested items come with a short explanation.

> **A note on scores:** A “92/100 match” is a configurable style heuristic, **not** an AI confidence score or an objective measure of fashion. Real visual analysis is a future milestone.

## 💻 Getting started

### Requirements

- **.NET 10 SDK**
- A C# editor such as **VS Code** or **Visual Studio**
- No FFXIV installation required to run the current demo

Clone the repository (once published), then run the demo and Core checks:

```bash
git clone https://github.com/lilacruntime/AetherStylist.git
cd AetherStylist
```


```bash
# Try the fictional-item outfit builder
dotnet run --project src/AetherStylist.Demo

# Run the current dependency-free executable checks
dotnet run --project tests/AetherStylist.Specs
```

The demo begins with a fictional **Midnight Oversized Jacket** (`ID 1001`) and an `alt` aesthetic. Edit the `OutfitRequest` in `src/AetherStylist.Demo/Program.cs` to experiment with different combinations.

> These are console-based checks, **not** a `dotnet test` test suite yet.

### Dalamud plugin (Milestone 2)

On Windows, with the .NET 10 SDK and Dalamud API 15 development assemblies available:

```powershell
dotnet build AetherStylist.sln -c Debug
```

The plugin DLL is `src/AetherStylist.Plugin/bin/x64/Debug/AetherStylist.Plugin.dll`. Use `/aetherstylist` to toggle **AetherStylist ♡** after enabling it as a dev plugin. See the [build and local loading guide](PluginIntegration/README.md) for prerequisites, exact steps and an explanation of the C# implementation. Compilation does not replace testing inside the game.

## 📁 Project structure

```text
AetherStylist/
├── src/
│   ├── AetherStylist.Core/       # Data models + recommendation engine
│   ├── AetherStylist.Demo/       # Console demo + fictional item catalogue
│   └── AetherStylist.Plugin/     # Dalamud API 15 entry point + ImGui window
├── tests/
│   └── AetherStylist.Specs/      # Executable checks for engine behaviour
├── PluginIntegration/           # Build/loading guide + implementation walkthrough
├── AetherStylist.sln            # Builds all four projects
├── NOTICE.md
└── README.md                    # You're here! ♡
```

The plugin references Core as a separate library. A future adapter will translate FFXIV item data into the core's models; the matching algorithm remains independent of game APIs.

## 🗺️ Roadmap

- [x] Define the item and outfit models
- [x] Implement initial colour, aesthetic, silhouette and material scoring
- [x] Add multi-outfit generation with beam search
- [x] Create a fictional catalogue and console checks
- [x] Compile and run the prototype locally
- [x] Scaffold and compile a real Dalamud plugin with an ImGui window
- [ ] Verify the plugin inside FFXIV
- [ ] Load and search actual FFXIV equipment data with Lumina
- [ ] Validate glamour compatibility and equipment restrictions
- [ ] Support locked pieces and interactive outfit recommendations in-game
- [ ] Save outfits and user preferences
- [ ] Develop real image-derived visual features
- [ ] Explore dye palettes, previews and optional integrations

*Early-stage roadmap — priorities will change as we learn what works.*

## 🧵 Built with

**C# · .NET 10 · JSON · Dalamud / ImGui ·** planned **Lumina**

Useful references: [Dalamud getting started](https://dalamud.dev/plugin-development/getting-started/) · [SamplePlugin](https://github.com/goatcorp/SamplePlugin) · [Dalamud AI use policy](https://dalamud.dev/plugin-publishing/ai-policy/)

## ♡ Notes & credits

This project is a personal learning project and is **not affiliated with Square Enix, Final Fantasy XIV, Dalamud, XIVLauncher, Glamourer or Penumbra**. No FFXIV game assets are included in the starter catalogue.


<div align="center">

---

*Made with love, a lot of glamour plates, and probably too much time spent picking boots.* ♡

</div>
