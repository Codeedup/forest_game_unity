# Underfoot

A hooded wanderer changes size to notice a woodland's overlooked details and find a welcoming home beneath its roots.

## Run

Open this repository in **Unity 6000.6.4f1**, open `Assets/Underfoot/Scenes/UnderfootDemo.unity`, and press **Play**. The scene, materials, meshes, prefabs and references are already saved. The original `Assets/Scenes/SampleScene.unity` and its teaching scripts are preserved. Underfoot is the first enabled build scene. The repository is registered in Unity Hub as **forst_game_unity**.

If rebuilding the procedural artwork is necessary, stop Play Mode and choose **Underfoot > Build or rebuild demo scene**. This replaces only the generated Underfoot scene and its named assets. Save any changes to the generated scene elsewhere before rebuilding it. Generation never runs automatically on reload or Play.

## Short demonstration

1. Show the hooded human wanderer, permanent leaf, roots and mushrooms. Walk with **WASD** and orbit with **right mouse drag**.
2. Drag **YOUR HEIGHT** towards Tiny (8 cm). Moss, leaf lace and tiny mushrooms appear; releasing the slider gives one soft descending cue. Growing in open space gives a complementary rising cue. Blocked growth gives a text prompt instead of a successful grow sound.
3. Approach Pip, just left of the stepping stones, and press **E**. Read his captions, then click **Quiet rootway** / **Squirrel clearing**, or press **1** / **2**. Both choices work with sound muted. **Escape** closes choices; stepping away also closes them.
4. Follow the selected golden seed trail. The **quiet rootway** passes under the mossy arch and around a curled leaf. The **squirrel clearing** crosses Hazel's patrol: wait beside the cover stones, then cross behind her. A curious bump returns you to a nearby stone with a short recovery grace period, without damage.
5. Both paths meet at the dry spring just before the home. Approach the visible twig and press **E ? Move the twig**. A dynamic Rigidbody impulse moves it clear; water returns along the channel, the lantern and window brighten, and Pip acknowledges the result in captions.
6. Walk into the warm home and try growing: real geometry limits available height. Return to Tiny to move freely.
7. Press **R** to restore the opening state, including the twig, route choice, patrol, dialogue, water audio and dim lantern. Try the other path.

**M**, or **Sound on/off**, controls music, ambience, size cues and water. The mute preference survives reset. Size feedback waits until a slider adjustment finishes, with a short cooldown and a minimum change to avoid jitter. The route choice can be changed by talking to Pip again before clearing the spring. The two paths remain physically open; they converge at one shared puzzle.

Dialogue is captioned text with buttons and keyboard choices. No recorded/generated speech, microphone capture, speech recognition, paid asset, network service or new package is used. The installed package manifest supplies no speech recognizer; microphone capture alone would not recognize words. Optional push-to-talk recognition remains a future option, with the same visible choices as a complete fallback.

## Formative evidence

| Requirement | Where to see it |
| --- | --- |
| C# object movement | `UnderfootPlayer.cs`: camera-relative CharacterController movement, gravity, turning and procedural walking |
| Continuous mathematical relationship | `UnderfootDemo.cs`: logarithmic height slider; `UnderfootPlayer.cs` and `UnderfootCamera.cs`: height-dependent collision, speed and framing |
| Logical conditions and spawning | `UnderfootDetails.cs`: below 50 cm, 34 moss clusters, raised leaf veins and bark relief appear; below 15 cm, 22 inkcap / rose bellcap colonies plus fine leaf veins and dew appear. All 59 created roots are reused, with hysteresis to prevent flicker |
| Physics and collisions | `UnderfootSpring.cs`: one lodged twig becomes a dynamic Rigidbody, receives an impulse, and settles through gravity and contacts; the original seed prefab is preserved |
| Interaction and discovery | `UnderfootGnome.cs` and `UnderfootDemo.cs`: range and height checks, captioned route choices, spring state, confined growth and home discovery |
| Reproducible authoring | `Editor/UnderfootBuilder.cs`: saved scene, original procedural models and built-in-renderer materials |
| Sound | `UnderfootSoundscape.cs`: original flute and plucked-string tune, wind, leaf rustle, birds and faint insects; looping, mute fade, sheltered home ambience, debounced size cues and restored spring water |

Expand **Underfoot demo > Runtime - moss / tiny mushrooms** in the Play Mode hierarchy to inspect the created objects. They are instantiated on first crossing, hidden when growing, and reused thereafter, including after reset. The leaf details are attached to the permanent fallen leaf. Decorative detail has no colliders, and its meshes are combined by material to keep rendering costs modest. Legacy input and the built-in renderer are retained.

## Checks and limits

The feature branch uses Unity **6000.6.4f1**, built-in rendering and legacy input. Existing course examples and package versions are retained. `Underfoot > Verify saved demo in Play Mode` runs the actual controller and PhysX against the saved scene; results and camera renders are written to ignored `Logs/Underfoot/`. Save any current scene before using the command, which opens the saved demo and exits Play Mode when finished.

The blocked-spring implementation is undergoing its focused acceptance walkthrough and final independent style review. Verification results will be recorded here when complete. Windows is the available test platform; recent-Mac compatibility and standalone builds remain unverified. Automated audio checks establish playback and signal data, not a human listening review.

## Local project and Hub warning

All scene, model, material, script and audio assets are stored in this repository. The inherited `S04_physics-101` / `cassieldotcom` Unity Cloud association caused HTTP 403 errors because this account could not access that organization. Its project and organization IDs have been cleared, Unity services remain disabled, and the unused Unity Version Control package has been removed. Work locally at the repository root and use Git / GitHub for version control. The template's recovery scene is preserved under `Assets/_Recovery/`.

Hub's yellow **Built-In Render Pipeline is deprecated** notice describes Unity's renderer roadmap, rather than a missing pipeline or broken shader. Unity 6.6 still supports this renderer; the notice can be dismissed in Hub. No URP conversion is needed to run this project. See [Unity's render-pipeline announcement](https://unity.com/topics/render-pipelines-strategy-for-2026).

Hub now shows the project as disconnected from Unity Cloud, with its GitHub remote intact. If Play Mode is silent, enable the speaker control in the **Game** view toolbar as well as **Sound on** in the game. Hub can retain its informational deprecated-renderer issue icon after dismissing the banner.

The optional **Underfoot > Verify saved demo in Play Mode** command runs a short acceptance walkthrough using the actual controller and physics, then exits Play Mode. It writes results and runtime camera renders to ignored `Logs/Underfoot/`. It opens the saved scene; save your current scene before running it. The captured images supplement the playable scene and omit the overlay UI.

Both audio loops are original, procedurally composed assets; no soundtrack or field recording was imported. They can be regenerated with `python Tools/generate_underfoot_audio.py` (NumPy required only for regeneration). Music and forest levels are adjustable on **Underfoot demo > Underfoot Soundscape** in the Inspector. Models and animation remain simple procedural artwork, focused on a short route and a sheltered furnished alcove.

## Presentation and next steps

Explain the concept, demonstrate movement / scale / spawning / physics, then show the relevant scripts. Changing body size changes what attracts attention and which spaces are accessible; the detail thresholds are an artistic model of attention, not a biological model of insect vision.

Over the next three weeks, first stabilise navigation and the camera at all sizes; then improve close-up woodland detail, encounters and intentional audio; finally develop and verify the separate WebGL/WebAudio application and prepare documentation and walkthroughs. Confirm this plan against the official assessment brief and deadline. A musical companion involving the gnome is a possible direction, not an agreed web design.

The main challenges are camera obstruction across scales, safe growth under roots, procedural art that holds up close, and a twig impulse that clears and settles reliably. The later final applications still need both visual and sonic elements; this formative prototype does not replace the official assessment requirements.
