# Unity 6 - PSX Effects

This project is a collection of visual effects for Unity, created with the Universal Render Pipeline (URP), to replicate the classic aesthetics of PlayStation 1 (PSX) graphics.

## Included Effects

This package contains a variety of post-processing and vertex manipulation effects to give your project a retro look.

### Post-Processing Effects
*   **Dithering**: Reduces the color palette and applies a dithering pattern to simulate the color limitations of old hardware.
*   **Pixelation**: Reduces the screen resolution to create a pixelated look.
*   **Rolling Bands**: Simulates the horizontal scrolling bands seen on old CRT screens.
*   **ScanLines**: Adds scan lines to mimic the appearance of a CRT monitor.
*   **VHS**: Emulates the visual distortions and artifacts of a VHS tape.
*   **Vertex Warping**: Deforms the vertices of the models, creating a "dancing" or waving effect.

## How to Use

The effects are easy to apply.

1.  **Post-Processing Effects**: For full-screen effects, add the desired effect to your Post-processing Volume in the scene. You can find the pre-configured materials in the `Assets/PSX Effects` folder.
2.  **Vertex Effects**: For material-based effects (`Vertex Jitter` and `Vertex Warping`), simply apply the corresponding material to the Mesh Renderer of the 3D object you want to affect.

## Demo Scenes

The project includes two demo scenes to visualize the effects in action:

*   **Light Scene**: A well-lit scene, ideal for seeing how the effects behave in bright environments.
*   **Dark Scene**: A dark scene that demonstrates the effects in low-light conditions.

You can find them in the `Assets/Scenes` folder.

## Additional Scripts

*   `FPSLock.cs`: A simple script that locks the framerate at 24 FPS to help reinforce the retro feel.
*   `Floating.cs`: A utility script that makes an object float, used in the demo scenes.

## Credits

This project uses third-party assets in the demo scenes and as a project base.

*   **Modular First Person Controller** — by **JeCase**. Obtained from the Unity Asset Store: https://assetstore.unity.com/packages/3d/characters/modular-first-person-controller-189884
*   **Mine** — by **Elbolilloduro**. Obtained from itch.io under **CC0** license: https://elbolilloduro.itch.io/mine
*   **Tacos** — by **Elbolilloduro**. Obtained from itch.io under **CC0** license: https://elbolilloduro.itch.io/tacos
*   **PSX Forest Asset Collection** — by **Stark Crafts**. Obtained from itch.io: https://starkcrafts.itch.io/psx-forest-asset-collection-by-starkcrafts
