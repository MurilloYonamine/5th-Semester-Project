# The `_Game` Directory: Feature-Based Architecture

Welcome to the core of **PHOTOSSYNC** (5th Semester Project). This directory houses the entire game logic, built upon a clean, scalable, and modular **Feature-Based Architecture**. 

Designed to showcase clean code principles, decoupling, and maintainability, this folder is split into distinct layers to ensure that systems don't become tightly coupled spaghetti code.

---

## Architecture Overview

The `_Game` folder is divided into four main pillars:

1. **`Core/`**: The backbone of the application. Global services, dependency injection, and event routing.
2. **`Framework/`**: Generic, reusable systems (like our custom Behaviour Tree) that don't depend on specific game logic.
3. **`Gameplay/`**: The actual game features (Player, Enemy, Menu). These encapsulate their own scripts, models, and prefabs.
4. **`Shared/` & `UI/`**: Shared interfaces, common utilities, and the visual identity (PSX shaders).

---

## 1. Core (`_Game/Core`)
The `Core` layer provides essential services to the rest of the game. **Features can depend on Core, but Core never depends on Features.**

* **`Services/` & ServiceLocator:** A custom `ServiceLocator` pattern is used instead of Singletons. It registers and provides interfaces (`IAudioService`, `IGameStateService`, `IInputService`), ensuring loose coupling.
* **`Events/` (EventBus):** A centralized `EventBus` allows different features to communicate blindly without direct references (e.g., the Player dies -> EventBus -> UI shows Game Over).
* **`Audio/` & `Input/`:** Wrappers for Unity's new Input System and Audio Mixers, abstracting raw Unity APIs into clean interfaces.

---

## 2. Framework (`_Game/Framework`)
This layer contains highly reusable code architectures that could theoretically be ported to an entirely different game.

* **`Behaviour Trees/`:** Instead of relying on heavy third-party plugins, this project features a **custom-built Behaviour Tree System**.
  * Contains foundational nodes: `Sequence`, `Selector`, `Parallel`, `Blackboard`, and `Node`.
  * Fully modular, allowing designers to snap together AI logic seamlessly.

---

## 3. Gameplay (`_Game/Gameplay`)
This is where the magic happens. By strictly adhering to Feature-Based Architecture, every gameplay mechanic is isolated into its own folder containing its scripts, prefabs, models, and animations.

## 4. Visuals & Polish (`_Game/UI` & `_Game/Shared`)
The game targets a specific **PSX / Retro Horror aesthetic**. This folder contains the custom shaders and render pipelines required to achieve that look.

* **Retro Shaders:**
  * `Dithering`: Adds classic color-banding.
  * `Pixelation`: Downscales the render resolution.
  * `Vertex Warping`: Simulates the lack of floating-point precision in PS1 3D models (jittering polygons).
  * `Scanlines` & `VHS`: Screen-space effects to simulate old CRT monitors.

## 5. Shared (`_Game/Shared`)
* **Shared Logic:** Contains global interfaces like `IInteractable` used by anything the player can look at and click.
