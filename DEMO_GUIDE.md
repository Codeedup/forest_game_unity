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

The project compiled, generated its saved assets, reopened the saved scene, and completed a Play Mode walkthrough on **Unity 6000.6.4f1 / Windows 11 / Intel Arc Graphics** in an isolated verification copy. Checks covered movement and grounding, 8 cm scaling, both detail thresholds and reuse, dynamic seed launch / collision / settling, physical home entry, blocked growth, reset and replay. The slider was also clicked and dragged in the interactive editor, with stable orbit direction, and keyboard `R` restored the opening state. Runtime images received the required independent style review; the rear hood silhouette and miniature framing were corrected afterward.

**Version limitation:** 6000.2.8f1 was not installed here, so direct validation in that exact version remains outstanding. The repository's Unity version, packages, legacy input and built-in renderer were preserved. The newer editor logged a Search database indexing exception during batch startup; no Underfoot gameplay errors occurred. Editor rendering was responsive in both size views; no standalone performance benchmark was run.

The optional **Underfoot > Verify saved demo in Play Mode** command runs a short acceptance walkthrough using the actual controller and physics, then exits Play Mode. It writes results and runtime camera renders to ignored `Logs/Underfoot/`. It opens the saved scene; save your current scene before running it. The captured images supplement the playable scene and omit the overlay UI.

Both audio loops are original, procedurally composed assets; no soundtrack or field recording was imported. They can be regenerated with `python Tools/generate_underfoot_audio.py` (NumPy required only for regeneration). Music and forest levels are adjustable on **Underfoot demo > Underfoot Soundscape** in the Inspector. Models and animation remain simple procedural artwork, focused on a short route and a sheltered furnished alcove.

## Presentation and next steps

Explain the concept, demonstrate movement / scale / spawning / physics, then show the relevant scripts. Changing body size changes what attracts attention and which spaces are accessible; the detail thresholds are an artistic model of attention, not a biological model of insect vision.

Over the next three weeks, first stabilise navigation and the camera at all sizes; then improve close-up woodland detail, encounters and intentional audio; finally develop and verify the separate WebGL/WebAudio application and prepare documentation and walkthroughs. Confirm this plan against the official assessment brief and deadline. A musical companion involving the gnome is a possible direction, not an agreed web design.

The main challenges are camera obstruction across scales, safe growth under roots, procedural art that holds up close, and seed throws that settle reliably. The later final applications still need both visual and sonic elements; this formative prototype does not replace the official assessment requirements.
