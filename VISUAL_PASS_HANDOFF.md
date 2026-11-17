# Underfoot: visual charm pass, handoff for Claude Code

**Goal:** make the whole woodland feel **charming, moody and whimsical**, like a storybook clearing at golden hour turning to dusk, without breaking any gameplay. Dylan has given free rein on visuals.

**Work in:** `C:\Users\dylan\dev\forst_game_blocked_spring` (a git worktree). Create branch `feature/visual-charm` from `feature/blocked-spring-prototype` (HEAD `bc6fec1`, pushed). Commit in small checkpoints. Pushing `feature/visual-charm` to GitHub is fine; never push or merge `main`.

**Read first:** `AGENTS.md` (style rules + the one-visual-reviewer rule), `DEMO_GUIDE.md`, `Assets/Underfoot/Editor/UnderfootBuilder.cs` and `UnderfootSpringBuilder.cs` (the whole scene is procedurally generated here), `UnderfootVerification.cs`.

## Running Unity (no GUI needed)

Unity 6000.6.4f1: `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe`

- Rebuild + full Play Mode check + renders (about 80 s):
  `Unity.exe -batchmode -projectPath <worktree> -logFile <log> -executeMethod Underfoot.Editor.UnderfootVerification.RebuildAndCheck`
  Results: `Logs/Underfoot/walkthrough.txt` (64 PASS lines currently); renders: `Logs/Underfoot/*.png`. Look at the PNGs after every change: that is the feedback loop.
- A Unity Search indexing exception at batch startup is a known editor bug. Ignore it.
- Don't run while the same worktree is open in the Unity editor. A small PowerShell runner (`claude-unity-runner.ps1`) may be idle in a window; it only acts on request files, so leave it alone.
- Batch mode measures live audio RMS as 0 because there is no output device. That's expected.

## Hard constraints

- Every existing verification check must still pass. Don't move route waypoints, colliders, the doorway, the twig, Pip's position or the spring channel unless you update the verification to match and it still passes. Gameplay geometry is the contract; decoration is free.
- Built-in render pipeline, legacy input, Unity 6000.6.4f1. **No new packages.** No URP, no Post Processing Stack, no Asset Store, no downloaded models or textures. Everything stays procedural in the builders, so the scene rebuilds reproducibly.
- Must run comfortably on a recent MacBook (the assessment platform). Keep draw calls and lights modest: combine meshes by material (as `UnderfootDetails` does), use at most a handful of real-time lights, and prefer emissive materials and fake glows.
- Keep the scale relationship readable: the same root, leaf and tree landmarks at 1.7 m and at 8 cm.
- Decorative objects get no colliders.
- Fonts: the built-in `LegacyRuntime.ttf` is safe. An SIL-OFL font may be added under `Assets/Underfoot/Fonts/` with its licence file, if it clearly helps the storybook feel.

## Diagnosis from the current renders (Logs/Underfoot, 10 Nov)

1. **Lighting is flat midday.** The sky is a pale mint fill, ambient light is even, and there's no fog, so nothing feels moody. This is the biggest single fix.
2. **There's no post-processing at all.** That's the second biggest.
3. **Trees are lollipops:** straight trunks with single blob canopies, all the same green, and the forest edge reads as a thin ring.
4. **The ground is a flat green plane** with sparse, identical leaf cut-outs.
5. **The home interior is an empty orange box.** The destination should be the emotional payoff, and it's currently the weakest view.
6. **The squirrel is crude:** a faceted block tail, a slab-like body edge and an odd ear.
7. **The UI is heavy:** big dark rectangles in a default font cover a third of the screen at Pip.
8. **The wanderer is a blob:** a readable silhouette, but no charm or detail.

Pip's silhouette (hat, beard, colours) and the mushroom colonies are the strongest existing elements. Keep and build on them.

## Direction (priority order: stop and verify after each)

1. **Golden hour into dusk.**
   - Low, warm directional sun (amber, about 10–15° elevation) with long soft shadows.
   - Cool blue-violet ambient, using a trilight gradient: sky dusky lavender, equator warm rose, ground deep moss.
   - A gradient sky via `Skybox/Procedural`, or a tiny custom gradient skybox shader with a warm horizon and a few early stars.
   - Exponential fog tinted dusky blue-green, so distant trees fade into atmosphere and the clearing reads as a pocket of light.
2. **One small custom image effect** (`OnRenderImage` on the camera, one shader, built-in pipeline):
   - a gentle vignette
   - warm highlights and cool shadows (split toning)
   - a slight saturation lift
   - a cheap soft bloom for emissive pixels (threshold, two-pass downsample blur), so lanterns, the window, fireflies and restored water glow
   
   Keep it subtle, and expose the strengths in the Inspector.
3. **Living light.**
   - A pooled particle system of fireflies and floating motes around the clearing, densest near the home. Emissive, bounded count.
   - Two or three soft additive light-shaft quads through the canopy.
   - The home window and lantern flicker gently. Brighten them further when the spring flows, building on the existing Warmth.
4. **Trees and forest edge.**
   - Layered canopies built from several overlapping clumps in 2–3 greens with warm rim-lit tops.
   - Tapered, slightly bent trunks with root flares.
   - Hanging moss or vines on a few trees.
   - A darker second and third ring of silhouetted trees, fading into fog, for depth.
5. **Ground.**
   - Large soft colour patches (moss, clover, earth).
   - A worn path tone along the stepping stones.
   - Autumn-tinted fallen leaves in 3–4 hues, clover and tiny flowers (forget-me-not blue and buttercup).
   - Combined meshes, no colliders.
6. **The root home interior: make it the payoff.**
   - Curved root-wood walls instead of a box.
   - A tiny hearth with a flickering ember light.
   - A round rug, a small shelf of books or jars, hanging dried herbs, a teapot and two cups on the table (Pip promised tea), a tiny round window with a moss sill, and a warm glow.
   - Leave walking clearance unchanged. Check `Growth under the home roof remains safe` still passes.
7. **Characters.**
   - **Wanderer:** a hint of a face in the hood shadow (two tiny glints), a scarf in a warm accent colour, a stitched cloak hem, and a small hand lantern with a soft emissive glow. The lantern should be emissive only, not a real-time light, unless performance allows one.
   - **Pip:** rosy cheeks, a bulbous nose, a slight curl at the hat tip, a belt and buckle, and eye highlights.
   - **Squirrel (Hazel):** a smooth curled S-shaped bushy tail made of tapered tubes, cheek fluff, large glossy eyes, small rounded ears with tufts, and a lighter belly. She should be curious and cute, never scary.
8. **UI.**
   - Smaller, parchment-tinted, semi-transparent panels with a soft border, and less screen coverage.
   - Captions near the bottom in a storybook style, and a lighter controls bar.
   - The height slider gets a tiny acorn handle if simple.
   - All text must stay legible; check `02b-Pip-choices-and-captions.png`.
9. **The spring moment.** Water with an emissive turquoise shimmer (scrolling UVs or a vertex wobble), a few ripple rings, and glow-moss lighting up along the channel when it flows.
10. **Sound, optional and only if time allows.** Evening crickets layered into the forest ambience to match the dusk mood, regenerated via `Tools/generate_underfoot_audio.py`. Keep it original.

## Verification and review

- After each priority step: rebuild, run the check, and look at every render.
- At the end, add any new useful render angles to the verification (e.g. a dusk wide shot, the interior with hearth, a squirrel close-up).
- Use **exactly one** read-only visual-style reviewer sub-agent on the final renders, with the prompt in `AGENTS.md`. Fix any "must fix" and "should fix" findings, then re-verify.
- Update `DEMO_GUIDE.md` briefly (what changed visually, what was verified, and that the Mac is unverified).
- Report: what changed, the check count, the commits, and what remains unverified.
