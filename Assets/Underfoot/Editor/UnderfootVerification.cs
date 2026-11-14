using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Underfoot.Editor
{
    // A short acceptance walkthrough, using live controllers and live PhysX in Play Mode.
    // Opt-in only; writes evidence to ignored Logs/Underfoot, never changes the saved scene.
    [InitializeOnLoad]
    public static class UnderfootVerification
    {
        static IEnumerator walkthrough;
        static readonly List<string> results = new List<string>();
        static int frame;
        static bool hadError;
        static string Output => Path.GetFullPath("Logs/Underfoot");
        static UnderfootVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("UnderfootVerify", false))
                {
                    results.Clear(); hadError = false;
                    Application.logMessageReceived += WatchLog;
                    walkthrough = Check();
                    EditorApplication.update += Tick;
                }
                if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("UnderfootVerify", false))
                {
                    SessionState.SetBool("UnderfootVerify", false);
                    if (Application.isBatchMode) EditorApplication.Exit(hadError ? 1 : 0);
                }
            };
        }

        [MenuItem("Underfoot/Verify saved demo in Play Mode")]
        public static void StartCheck()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before running the check.");
            Directory.CreateDirectory(Output);
            EditorSceneManager.OpenScene(UnderfootBuilder.ScenePath);
            foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!behaviour) throw new InvalidOperationException("Missing script in saved scene.");
            SessionState.SetBool("UnderfootVerify", true);
            EditorApplication.isPlaying = true;
        }

        public static void RebuildAndCheck()
        {
            UnderfootBuilder.Build();
            StartCheck();
        }

        public static void OpenDemo()
        {
            EditorSceneManager.OpenScene(UnderfootBuilder.ScenePath);
        }

        [MenuItem("Underfoot/Capture close-up review views (Play Mode)")]
        public static void CaptureDetailViews()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
            Directory.CreateDirectory(Output);
            var demo = Object.FindFirstObjectByType<UnderfootDemo>();
            var player = demo.player;
            Vector3 position = player.transform.position;
            Quaternion bodyRotation = player.body.rotation;
            float height = player.Height, yaw = player.orbit.yaw, pitch = player.orbit.pitch;
            player.Controller.enabled = false;
            try
            {
                demo.ChangeSize(0);
                player.transform.position = new Vector3(.63f, .054f, .08f);
                player.orbit.yaw = -25; player.orbit.pitch = 55;
                Capture(player.orbit, "05-leaf-lace-and-dew");
                player.transform.position = new Vector3(-.45f, .004f, .70f);
                player.orbit.yaw = 0; player.orbit.pitch = 8;
                Capture(player.orbit, "06-bark-and-shelf-fungi");
                var samples = new float[1024];
                AudioListener.GetOutputData(samples, 0);
                float energy = 0;
                foreach (float sample in samples) energy += sample * sample;
                File.WriteAllText(Output + "/audio-output.txt", "Live AudioListener output RMS: " + Mathf.Sqrt(energy / samples.Length));
            }
            finally
            {
                player.transform.position = position; player.body.rotation = bodyRotation;
                player.RequestHeight(height);
                player.Controller.enabled = true;
                demo.ChangeSize(Mathf.Log(player.Height / player.minHeight) / Mathf.Log(player.maxHeight / player.minHeight));
                player.orbit.yaw = yaw; player.orbit.pitch = pitch; player.orbit.Frame();
            }
            Debug.Log("UNDERFOOT_DETAIL_VIEWS_SAVED: close-ups and live audio output in Logs/Underfoot.");
        }

        static void WatchLog(string message, string stack, LogType type)
        {
            if (stack.Contains("UnityEditor.Search.SearchDatabase"))
            {
                results.Add("EDITOR LIMITATION: Unity 6000.6 Search index exception during batch startup (not gameplay).");
                return;
            }
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) hadError = true;
            if (message.Contains("Look rotation viewing vector is zero")) hadError = true;
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || Time.frameCount == frame) return;
            frame = Time.frameCount;
            try { if (walkthrough.MoveNext()) return; }
            catch (Exception error) { hadError = true; results.Add("FAIL: " + error.Message); Debug.LogException(error); }
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= WatchLog;
            File.WriteAllLines(Output + "/walkthrough.txt", results);
            Debug.Log(hadError ? "UNDERFOOT_CHECK_FAILED" : "UNDERFOOT_CHECK_PASSED");
            EditorApplication.isPlaying = false;
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            results.Add("PASS: " + message);
        }
        static IEnumerator Pause(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }
        static IEnumerator MoveTo(UnderfootPlayer player, Vector3 target, float timeout)
        {
            float end = Time.time + timeout;
            while (Time.time < end)
            {
                Vector3 delta = target - player.transform.position; delta.y = 0;
                if (delta.magnitude < .012f) yield break;
                Vector3 forward = Vector3.ProjectOnPlane(player.orbit.transform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                player.Walk(new Vector2(Vector3.Dot(delta.normalized, right), Vector3.Dot(delta.normalized, forward)), Mathf.Min(Time.deltaTime, .033f));
                yield return null;
            }
        }
        static IEnumerator Check()
        {
            var demo = Object.FindFirstObjectByType<UnderfootDemo>(); var p = demo.player;
            var spring = demo.spring; var sound = demo.soundscape; var squirrel = demo.squirrel;
            p.ControlsEnabled = false; p.orbit.ControlsEnabled = false;
            var pause = Pause(.6f); while (pause.MoveNext()) yield return null;
            Require(Mathf.Abs(p.Height - 1.7f) < .001f && Mathf.Abs(p.transform.position.y) < .03f, "Human start is grounded at 1.70 m");
            Require(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null && QualitySettings.renderPipeline == null,
                "Built-in rendering is preserved");
            Require(string.IsNullOrEmpty(Application.cloudProjectId), "No Unity Cloud link");
            foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                foreach (var material in renderer.sharedMaterials)
                    if (!material || !material.shader || !material.shader.isSupported) throw new InvalidOperationException("Unsupported material on " + renderer.name);
            Require(spring.State == UnderfootSpring.SpringState.Blocked && spring.twig.isKinematic && !sound.water.isPlaying && spring.homeLights[0].intensity < .2f,
                "Saved opening has one lodged twig, a dry channel and a dim lantern");
            Require(!demo.gnome.Interact() && !demo.gnome.Choose(UnderfootSpring.Route.Rootway) && !spring.ClearTwig(),
                "Human-scale or distant interaction cannot start the quest or clear the twig");
            Capture(p.orbit, "01-normal");
            float framing = p.orbit.distanceInHeights;
            p.orbit.distanceInHeights = 4.2f; p.orbit.pitch = 9; p.orbit.yaw = -28;
            Capture(p.orbit, "00-dusk-wide");
            p.orbit.distanceInHeights = framing; p.orbit.pitch = 20; p.orbit.yaw = 0;
            p.orbit.Frame();
            float fullSpeed = p.WalkingSpeed;
            for (int i = 19; i >= 0; i--) { demo.ChangeSize(i / 20f); yield return null; }
            pause = Pause(.45f); while (pause.MoveNext()) yield return null;
            Require(Mathf.Abs(p.Height - .08f) < .001f && p.WalkingSpeed < fullSpeed && Mathf.Abs(p.Controller.height - p.Height) < .001f,
                "Continuous slider changes the body, collider, label and walking speed to 8 cm");
            Require(demo.heightLabel.text.Contains("8.0") && sound.ShrinkCueCount == 1 && sound.GrowCueCount == 0,
                "One descending cue follows the completed shrink adjustment, without slider-event spam");
            Require(p.orbit.GetComponent<Camera>().nearClipPlane < .003f && Mathf.Abs(p.transform.position.y) < .03f,
                "Tiny camera clipping and grounded feet are preserved");
            int count = demo.details.CreatedCount;
            Require(count == demo.details.mossCount + demo.details.fineCount + 3 && demo.details.BarkLayer.gameObject.activeInHierarchy && demo.details.LeafLace.gameObject.activeInHierarchy,
                "Both bounded detail layers, bark and leaf lace appear");
            pause = Pause(.8f); while (pause.MoveNext()) yield return null;
            demo.ChangeSize(1); pause = Pause(.8f); while (pause.MoveNext()) yield return null;
            Require(sound.GrowCueCount == 1, "One ascending cue follows accepted growth in open space");
            sound.ToggleMute(); demo.ChangeSize(0);
            pause = Pause(1.4f); while (pause.MoveNext()) yield return null;
            Require(sound.ShrinkCueCount == 1 && sound.feedback.volume == 0 && sound.music.volume < .001f && sound.forest.volume < .001f,
                $"Muted scaling: cue count={sound.ShrinkCueCount}, feedback={sound.feedback.volume:F5}, music={sound.music.volume:F5}, forest={sound.forest.volume:F5}, muted={sound.Muted}");
            sound.ToggleMute(); pause = Pause(1.4f); while (pause.MoveNext()) yield return null;
            for (int i = 0; i < 4; i++) { demo.ChangeSize(1); demo.ChangeSize(0); yield return null; }
            Require(demo.details.CreatedCount == count, "Repeated size thresholds reuse all details");
            demo.ChangeSize(.21f); bool wasVisible = demo.details.FineLayer.gameObject.activeSelf;
            p.RequestHeight(.16f); demo.details.Refresh();
            Require(wasVisible && demo.details.FineLayer.gameObject.activeSelf, "Fine detail hysteresis remains stable");
            demo.ChangeSize(0);
            var move = MoveTo(p, new Vector3(-.18f, 0, -.055f), 5); while (move.MoveNext()) yield return null;
            int voicesBefore = sound.VoiceLineCount;
            Require(demo.gnome.CanInteract && demo.gnome.Interact(), "Miniature walker reaches Pip and opens captioned route choices");
            var babble = UnderfootGnomeVoice.For(demo.gnome.Dialogue);
            Require(sound.VoiceLineCount == voicesBefore + 1 && babble && babble.length > .3f && babble.length <= 1.81f && UnderfootGnomeVoice.For(demo.gnome.Dialogue) == babble,
                $"Pip's line plays one short cached synthesised babble with captions ({(babble ? babble.length : 0):F2} s)");
            yield return null;
            var buttons = demo.gnome.choicePanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            Require(buttons.Length == 2 && demo.gnome.choicePanel.activeSelf && spring.SelectedRoute == UnderfootSpring.Route.None,
                "Two visible choices wait for deliberate selection");
            Capture(p.orbit, "02-miniature-Pip");
            Capture(p.orbit, "02b-Pip-choices-and-captions", true);
            buttons[0].onClick.Invoke(); yield return null;
            Require(spring.SelectedRoute == UnderfootSpring.Route.Rootway && spring.rootwaySeeds.activeSelf && !spring.squirrelSeeds.activeSelf && demo.gnome.Dialogue.Contains("mossy arch"),
                "Persistent rootway button selects and acknowledges its bounded seed trail");
            for (int i = 0; i < spring.rootwayPath.Length; i++)
            {
                move = MoveTo(p, spring.rootwayPath[i], 5); while (move.MoveNext()) yield return null;
                Require(Vector3.ProjectOnPlane(p.transform.position - spring.rootwayPath[i], Vector3.up).magnitude < .025f,
                    "Rootway controller traversal waypoint " + (i + 1));
                if (i == 1)
                {
                    Capture(p.orbit, "03-rootway-arch");
                    int grows = sound.GrowCueCount;
                    demo.ChangeSize(1); pause = Pause(.85f); while (pause.MoveNext()) yield return null;
                    Require(p.GrowthBlocked && p.Height < .16f && sound.GrowCueCount == grows,
                        "The root arch physically blocks growth and suppresses a successful grow cue");
                    demo.ChangeSize(0);
                }
            }
            Require(!demo.gnome.Choose(UnderfootSpring.Route.Squirrel), "Pip's choices cannot be selected from across the scene");
            Capture(p.orbit, "04-blocked-spring");
            Require(spring.CanClear && spring.ClearTwig() && !spring.ClearTwig(), "Only a local deliberate action clears the twig once");
            pause = Pause(.06f); while (pause.MoveNext()) yield return null;
            Require(!spring.twig.isKinematic && (spring.twig.position.x - spring.BlockedPosition.x > .005f || spring.twig.linearVelocity.x > .1f),
                $"Dynamic twig impulse produces motion: displacement={spring.twig.position.x - spring.BlockedPosition.x:F4}, velocity={spring.twig.linearVelocity:F3}");
            pause = Pause(2.2f); while (pause.MoveNext()) yield return null;
            Require(spring.State == UnderfootSpring.SpringState.Flowing && spring.Warmth > .95f && spring.homeLights[0].intensity > 1.3f && sound.water.isPlaying,
                "Twig displacement restores water audio and warms the lantern");
            foreach (var segment in spring.waterSegments) Require(segment.activeSelf, "Water reaches channel segment " + segment.name);
            Require(!spring.ClearTwig() && demo.gnome.Dialogue.Contains("drink again") && spring.twig.linearVelocity.magnitude < .08f,
                "The twig settles, cannot clear twice, and Pip acknowledges success in captions");
            Capture(p.orbit, "05-restored-spring");
            Capture(p.orbit, "05b-success-captions", true);
            move = MoveTo(p, new Vector3(0, 0, 1.055f), 4); while (move.MoveNext()) yield return null;
            move = MoveTo(p, new Vector3(0, 0, 1.33f), 5); while (move.MoveNext()) yield return null;
            Require(demo.Discovered && p.transform.position.z > 1.25f, "The tiny walker physically enters the warm root home");
            Vector3 inside = p.transform.position;
            demo.ChangeSize(1);
            Require(p.GrowthBlocked && p.Height < .23f && Vector3.Distance(inside, p.transform.position) < .02f && demo.sizeSlider.value < .6f,
                "Growth under the home roof remains safe and the slider follows accepted height");
            demo.ChangeSize(0); Capture(p.orbit, "06-warm-home");
            Require(sound.music.isPlaying && sound.forest.isPlaying && sound.music.timeSamples > 0 && sound.forest.timeSamples > 0 && sound.water.timeSamples > 0,
                "Original music, forest and restored water loops advance in Play Mode");
            pause = Pause(1); while (pause.MoveNext()) yield return null;
            Require(sound.forest.volume < sound.forestVolume * .8f, "Home still shelters the forest ambience");
            sound.ToggleMute(); pause = Pause(1.5f); while (pause.MoveNext()) yield return null;
            int voicesMuted = sound.VoiceLineCount;
            Require(sound.music.volume < .001f && sound.forest.volume < .001f && sound.water.volume < .001f && sound.feedback.volume == 0 &&
                !sound.PlayVoice("Pip: Muted check.") && sound.VoiceLineCount == voicesMuted,
                "Mute silences music, ambience, water, interaction cues and Pip's babble");
            sound.ToggleMute(); pause = Pause(1.5f); while (pause.MoveNext()) yield return null;
            Require(sound.music.volume > .1f && sound.forest.volume > .1f && sound.water.volume > .1f, "Unmute restores the soundscape including water");
            CaptureDetailViews();
            demo.ResetDemo(); yield return null;
            Require(Mathf.Abs(p.Height - 1.7f) < .001f && !demo.Discovered && spring.SelectedRoute == UnderfootSpring.Route.None &&
                spring.State == UnderfootSpring.SpringState.Blocked && spring.twig.isKinematic && Vector3.Distance(spring.twig.position, spring.BlockedPosition) < .001f &&
                !sound.water.isPlaying && sound.water.volume == 0 && spring.homeLights[0].intensity < .2f && !demo.gnome.Choosing && demo.gnome.Dialogue == "",
                "Reset restores player, quest, twig, water, lantern and dialogue");
            Require(!spring.rootwaySeeds.activeSelf && !spring.squirrelSeeds.activeSelf && !demo.details.BarkLayer.gameObject.activeInHierarchy && !demo.details.LeafLace.gameObject.activeInHierarchy,
                "Reset hides route seeds and detail layers without destroying them");
            move = MoveTo(p, new Vector3(0, 0, .05f), 3); while (move.MoveNext()) yield return null;
            move = MoveTo(p, new Vector3(0, 0, 1.33f), 3); while (move.MoveNext()) yield return null;
            Require(p.transform.position.z < 1.13f, "Full-size entry is still stopped by physical geometry");
            demo.ResetDemo(); demo.ChangeSize(0);
            move = MoveTo(p, new Vector3(-.18f, 0, -.055f), 5); while (move.MoveNext()) yield return null;
            Require(demo.gnome.Interact(), "Pip's encounter reopens after reset");
            buttons[1].onClick.Invoke(); yield return null;
            Require(spring.SelectedRoute == UnderfootSpring.Route.Squirrel && !spring.rootwaySeeds.activeSelf && spring.squirrelSeeds.activeSelf && demo.gnome.Dialogue.Contains("curious"),
                "Persistent squirrel button selects and acknowledges the alternate route");
            // Deliberately place the controller beside the animal to exercise the harmless contact path.
            p.RecoverAt(squirrel.transform.position + Vector3.forward * .015f);
            Require(squirrel.TryRecoverPlayer() && Vector3.Distance(p.transform.position, squirrel.recoveryPoint) < .015f && squirrel.Reaction.Length > 0 && !squirrel.TryRecoverPlayer(),
                "A curious bump gives a nearby recovery, caption and recovery cooldown");
            for (int i = 0; i < spring.squirrelPath.Length; i++)
            {
                if (i == 3)
                {
                    float until = Time.time + 9;
                    while (squirrel.transform.position.x < .45f && Time.time < until) yield return null;
                    p.orbit.yaw = 65; p.orbit.pitch = -5;
                    Capture(p.orbit, "07-squirrel-crossing");
                    p.orbit.yaw = 0; p.orbit.pitch = 20;
                }
                move = MoveTo(p, spring.squirrelPath[i], 6); while (move.MoveNext()) yield return null;
                Require(Vector3.ProjectOnPlane(p.transform.position - spring.squirrelPath[i], Vector3.up).magnitude < .025f,
                    "Squirrel route controller traversal waypoint " + (i + 1));
            }
            Require(spring.CanClear && spring.ClearTwig(), "Both traversed routes converge at the same local spring interaction");
            pause = Pause(2.5f); while (pause.MoveNext()) yield return null;
            Require(spring.State == UnderfootSpring.SpringState.Flowing && demo.details.CreatedCount == count && Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length == 1,
                "Second route restores the same spring with one twig and bounded reusable details");
            demo.ResetDemo(); demo.ChangeSize(0);
            p.RecoverAt(new Vector3(-.18f, .003f, -.055f));
            demo.gnome.Interact(); demo.gnome.ChooseRootway();
            p.RecoverAt(spring.interactionPoint.position - Vector3.forward * .10f);
            Require(spring.ClearTwig(), "Set up a clearing-state reset");
            yield return null; demo.ResetDemo();
            pause = Pause(.7f); while (pause.MoveNext()) yield return null;
            Require(spring.State == UnderfootSpring.SpringState.Blocked && spring.twig.isKinematic && !sound.water.isPlaying && squirrel.RecoveryCount == 0 && !spring.waterSegments[0].activeSelf,
                "Reset during a moving twig cancels success feedback and restores the patrol");
            Require(!hadError, "No gameplay errors during the walkthrough");
            results.Add("Environment: Unity " + Application.unityVersion + "; " + SystemInfo.graphicsDeviceName + "; " + SystemInfo.operatingSystem);
            results.Add("Audio checks verify clips, playback state and output data; no human listening review. Pip's voice is synthesised babble, not speech; microphone input is not implemented. Windows only; Mac unverified.");
        }
        static void Capture(UnderfootCamera orbit, string name, bool withUI = false)
        {
            orbit.Frame();
            var camera = orbit.GetComponent<Camera>();
            var rt = RenderTexture.GetTemporary(1440, 900, 24);
            var previous = RenderTexture.active; var oldTarget = camera.targetTexture;
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (withUI)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .03f;
                Canvas.ForceUpdateCanvases(); camera.Render();
            }
            var texture = new Texture2D(1440, 900, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0); texture.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", texture.EncodeToPNG());
            camera.targetTexture = oldTarget; RenderTexture.active = previous;
            if (withUI) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
            RenderTexture.ReleaseTemporary(rt); Object.Destroy(texture);
        }
    }
}
