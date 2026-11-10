using UnityEngine;
using UnityEngine.UI;

namespace Underfoot
{
    public class UnderfootSoundscape : MonoBehaviour
    {
        public AudioSource music, forest, feedback, water;
        public AudioClip shrinkCue, growCue;
        public UnderfootDemo demo;
        public Text soundLabel;
        [Range(0, 1)] public float musicVolume = .42f;
        [Range(0, 1)] public float forestVolume = .52f;
        public bool Muted { get; private set; }
        public bool SpringRestored { get; private set; }
        public int ShrinkCueCount { get; private set; }
        public int GrowCueCount { get; private set; }
        float fade, waterFade, changedAt, cueReadyAt, sizeFrom, sizeTo;
        bool pendingSize;
        int changedFrame;

        void Start()
        {
            music.volume = forest.volume = 0;
            feedback.volume = .22f; water.volume = 0;
            double start = AudioSettings.dspTime + .1;
            music.PlayScheduled(start);
            forest.PlayScheduled(start);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) ToggleMute();
            fade = Mathf.MoveTowards(fade, Muted ? 0 : 1, Time.unscaledDeltaTime / 1.2f);
            music.volume = musicVolume * fade;
            // The tiny home shelters the listener from the canopy without muting the scene.
            Vector3 local = demo.home.InverseTransformPoint(demo.player.transform.position);
            bool indoors = Mathf.Abs(local.x) < .14f && local.z > .04f && local.z < .4f;
            forest.volume = Mathf.MoveTowards(forest.volume, forestVolume * fade * (indoors ? .55f : 1) * (SpringRestored ? 1 : .76f), Time.unscaledDeltaTime * .6f);
            feedback.volume = Muted ? 0 : .22f;
            waterFade = Mathf.MoveTowards(waterFade, SpringRestored ? 1 : 0, Time.unscaledDeltaTime * .8f);
            water.volume = .28f * fade * waterFade;
            // One cue after a deliberate adjustment finishes, including keyboard slider input.
            if (pendingSize && !Input.GetMouseButton(0) && Time.frameCount - changedFrame >= 3 &&
                Time.unscaledTime - changedAt > .18f && Time.unscaledTime >= cueReadyAt)
            {
                pendingSize = false;
                float change = Mathf.Log(sizeTo / sizeFrom);
                if (!Muted && Mathf.Abs(change) >= .05f)
                {
                    bool shrinking = change < 0;
                    feedback.PlayOneShot(shrinking ? shrinkCue : growCue);
                    if (shrinking) ShrinkCueCount++; else GrowCueCount++;
                    cueReadyAt = Time.unscaledTime + .7f;
                }
            }
        }

        public void AcceptSizeChange(float before, float accepted, bool blocked)
        {
            if (blocked || Mathf.Abs(accepted - before) < .00001f) return;
            if (!pendingSize) sizeFrom = before;
            sizeTo = accepted; changedAt = Time.unscaledTime; changedFrame = Time.frameCount; pendingSize = true;
        }

        public void RestoreSpring()
        {
            if (SpringRestored) return;
            SpringRestored = true;
            water.Play();
        }

        public void ResetFeedback()
        {
            pendingSize = false; cueReadyAt = changedAt = 0;
            ShrinkCueCount = GrowCueCount = 0;
            SpringRestored = false; waterFade = 0;
            water.Stop(); water.volume = 0; feedback.Stop();
            // Preserve the listener's mute preference across reset.
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            feedback.volume = Muted ? 0 : .22f;
            if (soundLabel) soundLabel.text = Muted ? "Sound off  [M]" : "Sound on  [M]";
        }
    }
}
