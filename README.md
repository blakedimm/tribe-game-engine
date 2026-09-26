# ❄️ Frostbound Tribe — Narrative Survival Engine

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-12.x-informational?logo=avaloniaui&logoColor=white)](https://avaloniaui.net/)
[![NAudio](https://img.shields.io/badge/Audio-NAudio-orange)]()

> ⚠️ **Status:** Functional Desktop MVP / Prototype  
> **Frostbound Tribe** is a cross-platform narrative survival engine built with **Avalonia UI** and **C#**, featuring event-driven resource management, dynamic decision timers, and multi-channel audio mixing.

---

## 🎮 Core Mechanics & Features

* **Data-Driven Narrative Graph:** Storyline execution driven by structured JSON nodes with modular branching, state preservation, and flag-based progression (`RequiredFlag`, `HideIfFlag`, `AddFlag`).
* **Survival Resource Management:** Dynamic state tracking for Tribe survival metrics (Food, Wood, Population, Trust factors) directly affected by player choices.
* **Timed Choice System (Telltale-Style):** Real-time async decision countdowns implemented via `CancellationTokenSource` and responsive UI state fading.
* **Multi-Channel Audio Layer:** Custom `AudioManager` managing concurrent audio streams (ambient weather loops, background score, voice cues, and sound effects) powered by `NAudio`.
* **Zero-Dependency Asset Embedding:** Graphics and audio assets packaged as embedded manifest resources for single-binary portability.

---

## 🛠 Tech Stack

* **Platform:** C# / .NET 10
* **GUI Framework:** Avalonia UI (XAML / Fluent Theme / Cross-platform Desktop)
* **Audio Subsystem:** NAudio (WaveOutEvent, Mp3FileReader, multi-stream mixing)
* **Rendering & Text:** SkiaSharp / HarfBuzzSharp
* **Data Serialization:** System.Text.Json

---

## 📂 Project Architecture

```
TribeGameUI/
├── Core/
│   ├── AudioManager.cs       # Multi-channel audio router (BGM/SFX/Ambient)
│   └── StoryManager.cs       # Narrative node graph parser & progression handler
├── Models/
│   ├── Choice.cs             # Decision data contract with stat deltas & flags
│   ├── StoryNode.cs          # Narrative scene schema
│   └── Tribe.cs              # Game state, flags, and survival inventory
├── Audio/                    # Soundscapes, voice cues, and music assets
├── Images/                   # Backgrounds and character sprites
├── day2.json / story.json    # Narrative data scenarios
├── MainWindow.axaml          # Responsive declarative UI layout
└── Program.cs                # Avalonia bootstrap and lifecycle configuration
```

---

## 🚀 Building & Running

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download) or higher.

### Run locally
```bash
# Clone the repository
git clone [https://github.com/blakedimm/tribe-game-engine.git](https://github.com/blakedimm/tribe-game-engine.git)
cd tribe-game-engine

# Build and run
dotnet restore
dotnet run
```

---

## 👨‍💻 Author
* **Developer:** [blakedimm](https://github.com/blakedimm)
* **Focus:** Cross-Platform Desktop Apps, Custom Engines & Systems Programming
