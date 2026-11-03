# Underfoot

A hooded wanderer changes size to notice a woodland's overlooked details and find a welcoming home beneath its roots.

## Run

Open this repository in **Unity 6000.6.4f1**, open `Assets/Underfoot/Scenes/UnderfootDemo.unity`, and press **Play**. The scene, materials, meshes, prefabs and references are already saved. The original `Assets/Scenes/SampleScene.unity` and its teaching scripts are preserved. Underfoot is the first enabled build scene. The repository is registered in Unity Hub as **forst_game_unity**.

If rebuilding the procedural artwork is necessary, stop Play Mode and choose **Underfoot > Build or rebuild demo scene**. This replaces only the generated Underfoot scene and its named assets. Save any changes to the generated scene elsewhere before rebuilding it. Generation never runs automatically on reload or Play.

## One-minute demonstration

1. Show the human wanderer and the large leaf, roots and mushrooms. Walk briefly with **WASD**.
2. Drag the **YOUR HEIGHT** slider left, from Human towards Tiny. Watch the character shrink continuously to **8 cm** and the moss and miniature mushrooms appear.
3. Walk towards the gnome immediately ahead and slightly to the left of the stepping stones. When prompted, press **E**.
4. Watch Pip gesture and throw three golden seeds. Follow the stones and seed trail towards the amber lantern beneath the large tree.
5. Walk through the small doorway. Try growing inside: growth stops at the available space. Return to Tiny to move freely and read the discovery message.
6. Press **R** to restore the opening state and repeat.

**Right mouse drag** orbits the third-person camera. Release it to use the slider. Movement is relative to the camera. **M**, or the **Sound on/off** button, fades music and ambience in or out. The gnome can repeat his throw after a short cooldown; previous seeds are replaced.

## Formative evidence

| Requirement | Where to see it |
| --- | --- |
| C# object movement | `UnderfootPlayer.cs`: camera-relative CharacterController movement, gravity, turning and procedural walking |
| Continuous mathematical relationship | `UnderfootDemo.cs`: logarithmic height slider; `UnderfootPlayer.cs` and `UnderfootCamera.cs`: height-dependent collision, speed and framing |
| Logical conditions and spawning | `UnderfootDetails.cs`: below 50 cm, 34 moss clusters, raised leaf veins and bark relief appear; below 15 cm, 22 inkcap / rose bellcap colonies plus fine leaf veins and dew appear. All 59 created roots are reused, with hysteresis to prevent flicker |
| Physics and collisions | `UnderfootGnome.cs`: ballistic initial velocity, then dynamic Rigidbody flight and contacts; `GoldenSeed.prefab`: collider and friction material |
| Interaction and discovery | `UnderfootGnome.cs` and `UnderfootDemo.cs`: range and height checks, gesture, dialogue, confined growth and home discovery |
| Reproducible authoring | `Editor/UnderfootBuilder.cs`: saved scene, original procedural models and built-in-renderer materials |
| Sound | `UnderfootSoundscape.cs`: original flute and plucked-string tune, wind, leaf rustle, birds and faint insects; seamless looping, mute fade and sheltered home ambience |

Expand **Underfoot demo > Runtime - moss / tiny mushrooms** in the Play Mode hierarchy to inspect the created objects. They are instantiated on first crossing, hidden when growing, and reused thereafter, including after reset. The leaf details are attached to the permanent fallen leaf. Decorative detail has no colliders, and its meshes are combined by material to keep rendering costs modest. Legacy input and the built-in renderer are retained.

## Checks and limits

The actual repository compiled, generated its saved assets, reopened the saved scene, and completed a Play Mode walkthrough on **Unity 6000.6.4f1 / Windows 11 / Intel Arc Graphics**. Checks covered movement and grounding, 8 cm scaling, both detail thresholds and reuse, dynamic seed launch / collision / settling, physical home entry, blocked growth, reset and replay, looping audio, indoor ambience and mute restoration. The slider was also clicked and dragged in the interactive editor, with stable orbit direction, and keyboard `R` restored the opening state. Runtime images received the required independent style review; the rear hood silhouette and miniature framing were corrected afterward.

The project uses the user's installed **6000.6.4f1** editor. Compatibility changes removed the obsolete VR module and updated existing development, Timeline, UI and multiplayer-center packages. Legacy input, the built-in renderer and the course examples are retained. A Search database indexing exception occurred during an earlier batch startup; it was an editor issue, with no Underfoot gameplay errors. No standalone performance benchmark was run.

The resumed verification passed **31 Play Mode checks**, including supported shaders, the active built-in renderer and an empty Unity Cloud project ID. Seven runtime renders, including leaf/dew and bark/fungi close-ups, passed the final independent style review with only optional polish suggestions. The interactive slider reached 8 cm and live audio output was measured after unmuting the Game view toolbar (RMS 0.081). This verifies an output signal; the final mix has not received a human listening review. The idle player's zero-vector rotation warning was also fixed.

## Local project and Hub warning

All scene, model, material, script and audio assets are stored in this repository. The inherited `S04_physics-101` / `cassieldotcom` Unity Cloud association caused HTTP 403 errors because this account could not access that organization. Its project and organization IDs have been cleared, Unity services remain disabled, and the unused Unity Version Control package has been removed. Work locally at the repository root and use Git / GitHub for version control. The template's recovery scene is preserved under `Assets/_Recovery/`.

Hub's yellow **Built-In Render Pipeline is deprecated** notice describes Unity's renderer roadmap, rather than a missing pipeline or broken shader. Unity 6.6 still supports this renderer; the notice can be dismissed in Hub. No URP conversion is needed to run this project. See [Unity's render-pipeline announcement](https://unity.com/topics/render-pipelines-strategy-for-2026).

Hub now shows the project as disconnected from Unity Cloud, with its GitHub remote intact. If Play Mode is silent, enable the speaker control in the **Game** view toolbar as well as **Sound on** in the game. Hub can retain its informational deprecated-renderer issue icon after dismissing the banner.

The optional **Underfoot > Verify saved demo in Play Mode** command runs a short acceptance walkthrough using the actual controller and physics, then exits Play Mode. It writes results and runtime camera renders to ignored `Logs/Underfoot/`. It opens the saved scene; save your current scene before running it. The captured images supplement the playable scene and omit the overlay UI.

Both audio loops are original, procedurally composed assets; no soundtrack or field recording was imported. They can be regenerated with `python Tools/generate_underfoot_audio.py` (NumPy required only for regeneration). Music and forest levels are adjustable on **Underfoot demo > Underfoot Soundscape** in the Inspector. Models and animation remain simple procedural artwork, focused on a short route and a sheltered furnished alcove.

## Presentation and next steps

Explain the concept, demonstrate movement / scale / spawning / physics, then show the relevant scripts. Changing body size changes what attracts attention and which spaces are accessible; the detail thresholds are an artistic model of attention, not a biological model of insect vision.

Over the next three weeks, first stabilise navigation and the camera at all sizes; then improve close-up woodland detail, encounters and intentional audio; finally develop and verify the separate WebGL/WebAudio application and prepare documentation and walkthroughs. Confirm this plan against the official assessment brief and deadline. A musical companion involving the gnome is a possible direction, not an agreed web design.

The main challenges are camera obstruction across scales, safe growth under roots, procedural art that holds up close, and seed throws that settle reliably. The later final applications still need both visual and sonic elements; this formative prototype does not replace the official assessment requirements.
