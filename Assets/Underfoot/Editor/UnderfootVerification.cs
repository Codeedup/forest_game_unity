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
            p.ControlsEnabled = false; p.orbit.ControlsEnabled = false;
            var pause = Pause(.6f); while (pause.MoveNext()) yield return null;
            Require(Mathf.Abs(p.Height - 1.7f) < .001f && Mathf.Abs(p.transform.position.y) < .03f, $"Full-size start grounded at 1.70 m ({p.transform.position:F3})");
            foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                foreach (var material in renderer.sharedMaterials)
                    if (!material || !material.shader || !material.shader.isSupported) throw new InvalidOperationException("Missing or unsupported material on " + renderer.name);
            results.Add("PASS: Saved renderer materials and shaders resolve");
            Capture(p.orbit, "01-normal");
            float fullSpeed = p.WalkingSpeed;
            for (int i = 19; i >= 0; i--) { demo.ChangeSize(i / 20f); yield return null; }
            Require(Mathf.Abs(p.Height - .08f) < .001f && p.WalkingSpeed < fullSpeed && Mathf.Abs(p.Controller.height - p.Height) < .001f, "Slider changes body, collider and speed to 8 cm");
            Require(p.orbit.GetComponent<Camera>().nearClipPlane < .003f && Mathf.Abs(p.transform.position.y) < .03f, "Tiny camera clipping and feet remain grounded");
            int count = demo.details.CreatedCount;
            Require(count == demo.details.mossCount + demo.details.fineCount + 3, "Both thresholds instantiate exactly the bounded detail count");
            Require(demo.details.BarkLayer && demo.details.LeafLace && demo.details.BarkLayer.gameObject.activeInHierarchy && demo.details.LeafLace.gameObject.activeInHierarchy,
                "Shrinking reveals bark relief, fine leaf veins and dew");
            for (int i = 0; i < 4; i++) { demo.ChangeSize(1); demo.ChangeSize(0); yield return null; }
            Require(demo.details.CreatedCount == count, "Repeated threshold crossings reuse details");
            demo.ChangeSize(.21f); bool wasVisible = demo.details.FineLayer.gameObject.activeSelf;
            p.RequestHeight(.16f); demo.details.Refresh();
            Require(wasVisible && demo.details.FineLayer.gameObject.activeSelf, "Fine-detail hysteresis keeps the layer stable near the threshold");
            demo.ChangeSize(0);
            var move = MoveTo(p, new Vector3(-.18f, 0, -.055f), 5); while (move.MoveNext()) yield return null;
            Capture(p.orbit, "02-miniature-encounter");
            Require(demo.gnome.CanInteract && demo.gnome.Interact(), "Walk to the gnome and trigger its explicit interaction");
            pause = Pause(.85f); while (pause.MoveNext()) yield return null;
            Require(demo.gnome.ActiveSeedCount > 0 && demo.gnome.Seeds[0].linearVelocity.sqrMagnitude > .01f && !demo.gnome.Seeds[0].isKinematic, "Seeds launch as moving dynamic Rigidbodies");
            Capture(p.orbit, "02b-seed-throw");
            pause = Pause(4); while (pause.MoveNext()) yield return null;
            Require(demo.gnome.ActiveSeedCount == 3, "Exactly three seeds in the trail");
            for (int i = 0; i < 3; i++)
            {
                var seed = demo.gnome.Seeds[i];
                Vector3 error = seed.position - demo.gnome.landingPoints[i]; error.y = 0;
                Require(seed.position.y > -.01f && seed.position.y < .04f && seed.linearVelocity.magnitude < .08f && error.magnitude < .17f,
                    $"Seed {i + 1} collides, settles and stays near its landing area ({seed.position:F3})");
            }
            move = MoveTo(p, new Vector3(0, 0, .02f), 4); while (move.MoveNext()) yield return null;
            Capture(p.orbit, "02c-settled-trail");
            move = MoveTo(p, new Vector3(0, 0, 1.055f), 12); while (move.MoveNext()) yield return null;
            Require(p.transform.position.z > 1.02f, "Miniature controller follows the trail to the entrance");
            Capture(p.orbit, "03-root-doorway");
            move = MoveTo(p, new Vector3(0, 0, 1.33f), 5); while (move.MoveNext()) yield return null;
            yield return null;
            Require(demo.Discovered && p.transform.position.z > 1.25f, "Tiny player physically enters the home and discovers it");
            Vector3 inside = p.transform.position;
            demo.ChangeSize(1);
            Require(p.GrowthBlocked && p.Height < .23f && Vector3.Distance(inside, p.transform.position) < .02f && demo.sizeSlider.value < .6f,
                "Growth is clamped beneath the roof and the slider agrees");
            demo.ChangeSize(0); Capture(p.orbit, "04-home-interior");
            var sound = Object.FindFirstObjectByType<UnderfootSoundscape>();
            Require(sound && sound.music.clip && sound.forest.clip && sound.music.loop && sound.forest.loop &&
                sound.music.isPlaying && sound.forest.isPlaying && sound.music.timeSamples > 0 && sound.forest.timeSamples > 0,
                "Original music and forest ambience are loaded and playing as loops");
            Require(sound.forest.volume < sound.forestVolume * .8f, "Forest ambience softens inside the home");
            sound.ToggleMute(); pause = Pause(1.6f); while (pause.MoveNext()) yield return null;
            Require(sound.music.volume < .001f && sound.forest.volume < .001f && sound.Muted, "Sound toggle fades both tracks to silence");
            sound.ToggleMute(); pause = Pause(1.6f); while (pause.MoveNext()) yield return null;
            Require(sound.music.volume > .1f && sound.forest.volume > .1f && !sound.Muted, "Sound toggle restores both tracks");
            demo.ResetDemo(); yield return null;
            Require(Mathf.Abs(p.Height - 1.7f) < .001f && !demo.Discovered && demo.gnome.ActiveSeedCount == 0 && !demo.details.MossLayer.gameObject.activeSelf,
                "Reset restores player, UI, discovery, details and seeds");
            Require(!demo.details.BarkLayer.gameObject.activeInHierarchy && !demo.details.LeafLace.gameObject.activeInHierarchy,
                "Reset hides the new detail layers without destroying or duplicating them");
            // Full-size approach must hit actual geometry, with no permission-based doorway check.
            move = MoveTo(p, new Vector3(0, 0, .05f), 3); while (move.MoveNext()) yield return null;
            move = MoveTo(p, new Vector3(0, 0, 1.33f), 3); while (move.MoveNext()) yield return null;
            Require(p.transform.position.z < 1.13f, "Full-size controller cannot pass into the home");
            demo.ResetDemo(); demo.ChangeSize(0);
            move = MoveTo(p, new Vector3(-.18f, 0, -.055f), 5); while (move.MoveNext()) yield return null;
            Require(demo.gnome.Interact(), "Encounter can be repeated after reset");
            pause = Pause(4); while (pause.MoveNext()) yield return null;
            Require(demo.gnome.ActiveSeedCount == 3 && demo.details.CreatedCount == count, "Second walkthrough has bounded seed and detail counts");
            Require(demo.gnome.Interact(), "Trail can replay after its cooldown");
            pause = Pause(4); while (pause.MoveNext()) yield return null;
            Require(demo.gnome.ActiveSeedCount == 3, "Replay replaces older seeds");
            demo.ResetDemo();
            Require(!hadError, "No runtime errors during the walkthrough");
            results.Add("Environment: Unity " + Application.unityVersion + "; " + SystemInfo.graphicsDeviceName + "; " + SystemInfo.operatingSystem);
        }
        static void Capture(UnderfootCamera orbit, string name)
        {
            orbit.Frame();
            var camera = orbit.GetComponent<Camera>();
            var rt = RenderTexture.GetTemporary(1440, 900, 24);
            var previous = RenderTexture.active; var oldTarget = camera.targetTexture;
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            var texture = new Texture2D(1440, 900, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0); texture.Apply();
            File.WriteAllBytes(Output + "/" + name + ".png", texture.EncodeToPNG());
            camera.targetTexture = oldTarget; RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt); Object.Destroy(texture);
        }
    }
}
