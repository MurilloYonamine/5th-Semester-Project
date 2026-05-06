# The `UI` Directory: Post-Processing Shaders & VFX

The **UI** directory contains **PSX aesthetic shaders and visual effects**—post-processing effects that create the retro PSX look, including pixelation, scan lines, distortion, and VFX particles.

---

## Purpose

UI System provides:
- **PSX visual effects**: Shaders for authentic 90s console aesthetic
- **Post-processing**: Full-screen effects applied after rendering
- **Screen artifacts**: Scan lines, dithering, barrel distortion, VHS effects
- **Vertex warping**: Geometric distortion for character models
- **VFX particles**: Spark and electrical effects
- **Performance**: Optimized shaders for retro look

---

## Directory Structure

```
UI/
├── README.md                    (this file)
├── Shaders/
│   ├── Barrel Distortion/       (curved screen effect)
│   ├── Dithering/               (color reduction)
│   ├── Pixelation/              (pixel block effect)
│   ├── Rolling Bangs/           (scan line glitch)
│   ├── ScanLines/               (CRT scan lines)
│   ├── SMPTE/                   (test pattern/color bars)
│   ├── Vertex Warping/          (geometry distortion)
│   └── VHS/                     (analog tape artifacts)
└── VFX/
    ├── eletricity-spark.vfx
    ├── spark.wav
    └── SparkSound.cs
```

---

## Shader Overview

All shaders are **post-processing effects** applied after main rendering via **Full-Screen Pass** material.

### Architecture

```
Game Rendering (Main Camera)
    ↓
Render to Texture
    ↓
Apply Post-Processing
    ├─ Pixelation Shader
    ├─ ScanLines Shader
    ├─ Dithering Shader
    ├─ VHS Shader
    └─ (mix and match)
    ↓
Display on Screen
```

---

## Individual Shaders

### `Pixelation` (`PSX_Pixelate_Fullscreen`)

Reduces resolution to create blocky pixel look.

```
Input: Full resolution render
    ↓
Group pixels into blocks
    ↓
Output: Pixelated image
```

**Properties:**
- `Pixel Size`: Block size (smaller = more detailed)
- `Screen Width/Height`: Resolution settings

**Effect:**
```
Original:  Smooth curved line
Pixelated: ████▓░ (blocky)
```

**Usage:**
```csharp
// Apply to camera with Full-Screen Pass material
PostProcessVolume volume = camera.gameObject.AddComponent<PostProcessVolume>();
volume.profile.Add<Pixelization>();
```

**Intensity Levels:**
- Low (pixel size 2-4): Subtle retro feel
- Medium (pixel size 6-8): Clear pixel blocks
- High (pixel size 10+): Heavy pixelation, 8-bit look

---

### `ScanLines` (Hard & Soft)

CRT monitor scan line effect.

**Hard Scanlines:**
- Sharp, visible horizontal lines
- Classic arcade feel
- High contrast

**Soft Scanlines:**
- Subtle blur between lines
- More TV-like appearance
- Gentler aesthetic

```
Hard:       ▓░▓░▓░  (sharp lines)
Soft:       ▓▓░░▓▓  (blurred)
```

**Properties:**
- `ScanLine Intensity`: Darkness of lines
- `ScanLine Count`: Horizontal line count
- `ScanLine Speed` (optional): Animation for flicker

**Usage:**
```
Recommended settings:
- Hard: intensity 0.5, count 480 (720p)
- Soft: intensity 0.2, count 480 (gentler)
```

---

### `Dithering` (`PSX_Dither_Fullscreen`)

Color quantization using dither patterns for retro color palette look.

**Dither Patterns:**
- **Bayer 4×4**: Classic checkerboard (most retro)
- **Bayer 8×8**: Finer dither pattern
- **Scanline Retro**: Line-based dithering

```
Input Color:  #FF7733 (orange)
    ↓
Dithering:    ████░░  (mix red/brown in pattern)
    ↓
Output: Retro color banding with dither noise
```

**Properties:**
- `Dither Pattern`: Which texture to use
- `Dither Strength`: How pronounced the effect
- `Color Depth`: How many colors (8-bit, 16-bit simulation)

**Usage:**
```
Best for:
- Reducing color palette to 256 colors
- Creating banding artifacts
- Authentic PSX appearance
```

---

### `ScanLines + Dithering Combo`

Combine both for authentic PSX look.

```
Clean Render
    ↓
Dithering (color reduction)
    ↓
ScanLines (CRT effect)
    ↓
Result: Classic PSX aesthetic
```

---

### `Barrel Distortion` (`PSX_BarrelDistortion`)

Curved screen edge effect simulating old CRT monitors.

```
Before:  ┌─────────┐
         │         │
         └─────────┘

After:   ╭─────────╮
         │ ╱─────╲ │
         │ ╲─────╱ │
         ╰─────────╯
```

**Properties:**
- `Distortion Strength`: Curve intensity
- `Zoom`: Compensation for curved edges

**Effect:**
- Makes flat screen feel curved
- Nostalgic arcade cabinet feel
- Can cause discomfort at high values

---

### `Rolling Bangs` (Scan Line Glitch)

Simulates CRT screen corruption—horizontal scan lines shift and glitch.

```
Normal:   ▓▓▓▓▓▓▓▓
Glitch:   ▓▓  ▓▓▓▓  (shifted lines)
          ▓▓▓  ▓▓   (glitchy offset)
```

**Properties:**
- `Glitch Frequency`: How often glitches occur
- `Glitch Intensity`: How far lines shift
- `Speed`: Animation rate

**Effect:**
- Corrupted screen appearance
- Perfect for horror/scary moments
- Can induce motion sickness—use sparingly

---

### `VHS` (`PSX_VHS_Fullscreen`)

Simulates analog VHS tape artifacts—color separation, noise, tracking errors.

```
Clean:  ███████
VHS:    ██▒░██  (noise, color bleed)
        ▒▒▓▓▒▒  (artifacts)
```

**Properties:**
- `VHS Strength`: How pronounced the effect
- `Color Separation`: RGB channel offset
- `Noise Amount`: Static/grain intensity
- `Tracking Error`: Horizontal glitch frequency

**Effect:**
- Authentic VHS camcorder feel
- 80s/90s nostalgia
- Perfect for cutscenes, replays, security camera footage

---

### `Vertex Warping` (`PSX_Vertex_Warping`)

Geometry deformation on models (not post-processing—applied to materials).

```
Normal Model:  ▓▓▓▓▓
              ▓▓▓▓▓
              ▓▓▓▓▓

Warped:        ▓▓▓
              ▓▓▓▓▓
              ▓▓▓▓
```

**Properties:**
- `Warp Strength`: Deformation amount
- `Warp Frequency`: Wave pattern
- `Wave Speed`: Animation rate

**Usage:**
```csharp
// Apply to character materials or props
Material mat = GetComponent<Renderer>().material;
mat.shader = Shader.Find("PSX/Vertex_Warping");
mat.SetFloat("_WarpStrength", 0.5f);
```

**Effect:**
- Melting/wobbly appearance
- Perfect for cursed characters, corruption effects
- Horror/supernatural moments

---

### `SMPTE` (`PSX_SMPTE`)

Test pattern with color bars and text—for UI debugging or retro overlays.

```
╔════════════════════════════════╗
║ COLOR BAR TEST PATTERN         ║
║ ▓░▓░▓░  ▓░▓░▓░  ▓░▓░▓░       ║
║ SMPTE                          ║
║ 1080p / 60fps / Rec.709        ║
╚════════════════════════════════╝
```

**Usage:**
- Debugging fullscreen pass materials
- Test pattern overlays
- UI mockups
- Not recommended for gameplay

---

## VFX System

### `eletricity-spark.vfx`

Particle system for electrical/spark effects.

**Particle Properties:**
- **Emission**: Spark generation rate
- **Lifetime**: Duration of each spark
- **Velocity**: Speed and direction
- **Scale**: Size over time (fade out)
- **Color**: Blue/white electrical glow

**Usage:**

```csharp
// Spawn spark effect at location
ParticleSystem spark = Resources.Load<ParticleSystem>("VFX/eletricity-spark");
ParticleSystem instance = Instantiate(spark, position, Quaternion.identity);
instance.Play();

// Auto-destroy after duration
Destroy(instance.gameObject, 2f);
```

**Common Applications:**
- Enemy attack effects
- Door opening sparks
- Electrical hazards
- UI interactions

---

### `SparkSound.cs`

Audio component that syncs with spark particle effects.

```csharp
public class SparkSound : MonoBehaviour {
    [SerializeField] private AudioClip _sparkSound;
    [SerializeField] private ParticleSystem _particles;

    public void PlaySparkWithSound() {
        _particles.Play();
        AudioSource.PlayClipAtPoint(_sparkSound, transform.position);
    }
}
```

**Usage:**
```csharp
// In script that triggers spark
var spark = GetComponent<SparkSound>();
spark.PlaySparkWithSound();
```

---

## Material Setup

Each shader has an associated **Material** (`.mat` file):

```
Pixelation/
├── PSX_Pixelate_Fullscreen.shadergraph
└── PSX_Pixelate_Material.mat      ← Apply to Full-Screen Pass
```

### Using Shader Materials

```csharp
// Get the material
Material pixelMat = Resources.Load<Material>("UI/Shaders/Pixelation/PSX_Pixelate_Material");

// Adjust properties
pixelMat.SetFloat("_PixelSize", 4f);

// Apply to post-processing
PostProcessVolume volume = GetComponent<PostProcessVolume>();
// ... configure volume with material
```

---

## Post-Processing Setup

### Using Shader Effects in Scene

1. **Create Post-Process Volume**
   - GameObject → Create → Post-Process Volume
   - Set to Global (covers entire camera view)

2. **Add Effects to Profile**
   - Select volume
   - Inspector → New → Add override
   - Choose effect (Pixelation, ScanLines, etc.)

3. **Configure Intensity**
   ```
   Pixelation:
   - Weight: 1.0
   - Pixel Size: 4
   ```

### Combining Multiple Shaders

```
Volume Order (apply in sequence):
1. Dithering (color reduction)
2. ScanLines (CRT effect)
3. VHS (analog artifacts)
4. Barrel Distortion (final screen warp)
```

---

## Best Practices

### 1. Use Subtly

❌ **Bad:**
```
Pixelation (size 20) + ScanLines (strong) + VHS (full) + Dithering (max)
= Unplayable, nauseating
```

✅ **Good:**
```
Pixelation (size 4) + ScanLines (soft, 30% intensity) + slight Dithering
= Retro feel, still playable
```

### 2. Layer Strategically

✅ **Good:**
```
Story Cutscene:
  - VHS effect (tape feel)
  - Scan lines (CRT monitor)
  - Slight dithering (color depth)

Gameplay:
  - Pixelation only (minimal perf impact)
  - Soft scan lines (subtle atmosphere)

Horror Moments:
  - Rolling Bangs (corruption)
  - Barrel Distortion (unease)
  - VHS (analog chaos)
```

### 3. Performance Optimization

✅ **Good:**
```
Pixelation: Low cost (downscale + upscale)
ScanLines: Very low cost (simple texture lookup)
VHS: Medium cost (color channels + noise)
Barrel Distortion: Medium cost (texture sampling)
```

❌ **Bad:**
```
Using all effects at once = Performance hit
Use selective effects per scene
```

### 4. Context Appropriateness

✅ **Good:**
```
- Menu screens: Barrel distortion + scan lines
- Gameplay: Light pixelation + soft scan lines
- Horror scenes: Heavy effects (rolling bangs, glitch)
- Cutscenes: VHS effect (authentic tape feel)
- Security cameras: Dithering + strong scan lines
```

### 5. Hardware Testing

✅ **Good:**
```
Test on:
- High-end: Full effects enabled
- Mid-range: Selective effects
- Low-end: Minimal effects or disabled
```

---

## Common Patterns

### Menu Screen Effect

```csharp
public class MenuScreenEffects : MonoBehaviour {
    private void OnEnable() {
        // Apply retro feel to menu
        var volume = GetComponent<PostProcessVolume>();
        
        // Barrel + Scan lines
        var barrel = volume.profile.Add<BarrelDistortion>();
        barrel.strength.value = 0.2f;
        
        var scanlines = volume.profile.Add<ScanLines>();
        scanlines.intensity.value = 0.3f;
    }
}
```

### Dynamic Effect Transition

```csharp
public class EffectTransition : MonoBehaviour {
    public void TransitionToHorror() {
        // Gradually add glitch effects
        StartCoroutine(IncreaseGlitch());
    }

    private IEnumerator IncreaseGlitch() {
        for (int i = 0; i < 10; i++) {
            _glitchIntensity += 0.1f;
            _glitchMaterial.SetFloat("_GlitchStrength", _glitchIntensity);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
```

### Conditional Effects

```csharp
public class EnvironmentEffects : MonoBehaviour {
    private void Update() {
        // VHS effect during rainstorm
        if (_isRaining) {
            _vhsMaterial.SetFloat("_VHSStrength", 0.5f);
        } else {
            _vhsMaterial.SetFloat("_VHSStrength", 0f);
        }
    }
}
```

---

## Extending UI

### Custom Shader

```glsl
// Create custom shader based on existing ones
Shader "PSX/Custom_Effect" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _EffectStrength ("Strength", Range(0, 1)) = 0.5
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        
        Pass {
            HLSLPROGRAM
            // ... shader code ...
            ENDHLSL
        }
    }
}
```

### Combined Effect Material

```csharp
public class CombinedEffect : MonoBehaviour {
    private Material _effectMaterial;
    
    private void Start() {
        _effectMaterial = new Material(Shader.Find("PSX/Combined"));
        
        // Mix multiple effects
        _effectMaterial.SetFloat("_Pixelation", 0.5f);
        _effectMaterial.SetFloat("_ScanLines", 0.3f);
        _effectMaterial.SetFloat("_Dithering", 0.2f);
    }
}
```

---

## Performance Tips

### Disable Effects in Build

```csharp
#if !UNITY_EDITOR
// Remove expensive effects in release builds
PostProcessVolume.enabled = false;
#endif
```

### Selective Post-Processing

```csharp
// Only apply to specific cameras
PostProcessLayer layer = mainCamera.GetComponent<PostProcessLayer>();
layer.volumeLayer = LayerMask.GetMask("PostProcessing");
```

### Cache Materials

```csharp
private Material _cachedMaterial;

private void CacheMaterial() {
    _cachedMaterial = Resources.Load<Material>("UI/Shaders/Pixelation/Material");
}

// Reuse instead of loading each time
private void ApplyEffect() {
    GetComponent<Image>().material = _cachedMaterial;
}
```

---

## Debugging

### Visual Isolation

Test each shader independently:
```
1. Enable only Pixelation → verify effect
2. Disable Pixelation, enable ScanLines → verify
3. Enable both → verify combination
```

### Performance Profiling

```csharp
// Use Unity Profiler to measure shader overhead
// Window → Analysis → Profiler
// GPU metrics → Fragment/Vertex count
```

---

## Summary

The **UI** directory provides:
- **8 retro PSX shaders**: Pixelation, scan lines, dithering, distortion, VHS, VH artifacts
- **Post-processing pipeline**: Full-screen effects applied to final render
- **VFX system**: Spark particles with synchronized audio
- **Performance balance**: Subtle effects for playability, strong effects for moments
- **Aesthetic control**: Mix and match effects for desired retro look

By using UI shaders:
- **Authentic PSX feel**: Multiple combined effects create 90s console aesthetic
- **Flexibility**: Enable/disable effects per scene or moment
- **Performance**: Optimized shaders don't tank framerates
- **Atmosphere**: Effects enhance mood and immersion
- **Context**: Different effects for different situations (menus, gameplay, horror)

**See also:**
- [Gameplay/Menu/README.md](../Gameplay/Menu/README.md) - Menu screen implementation
- [Gameplay/README.md](../Gameplay/README.md) - Feature overview
