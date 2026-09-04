# FoxDock Experiments & Prototypes

This directory preserves alternative exploratory branches, UI redesign attempts, and architectural experiments developed alongside the main FoxDock project between 2019 and 2020:

## 📁 `foxdock-fluent` & `fluent-dock` (UWP / Windows Fluent Design Experiment)
- **Concept**: An attempt to rebuild the dock as a modern Windows 10 UWP / Fluent Design application utilizing official acrylic and reveal highlight effects.
- **Why it was shelved**: The contemporary Windows UWP app model and restricted desktop sandboxing APIs in Windows 10 did not provide low-level Win32 window management hooks, global shell hook notifications (`WH_SHELL`), or unrestricted taskbar manipulation required for a true always-on-top desktop dock.

## 📁 `foxdock-neo` (Architectural Refactor Prototype)
- **Concept**: A rewritten, decoupled prototype aimed at rethinking the state loop and icon cache pipeline.
- **Why it was shelved**: The critical performance bottlenecks, thread contention, and icon hover physics were successfully refactored and integrated directly into the core FoxDock codebase, making an isolated "Neo" fork redundant.
