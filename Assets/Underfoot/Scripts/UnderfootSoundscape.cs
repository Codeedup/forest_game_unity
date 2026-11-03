using UnityEngine;
using UnityEngine.UI;

namespace Underfoot
{
    public class UnderfootSoundscape : MonoBehaviour
    {
        public AudioSource music, forest;
        public UnderfootDemo demo;
        public Text soundLabel;
        [Range(0, 1)] public float musicVolume = .42f;
        [Range(0, 1)] public float forestVolume = .52f;
        public bool Muted { get; private set; }
        float fade;

        void Start()
        {
            music.volume = forest.volume = 0;
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
            forest.volume = Mathf.MoveTowards(forest.volume, forestVolume * fade * (indoors ? .55f : 1), Time.unscaledDeltaTime * .6f);
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            if (soundLabel) soundLabel.text = Muted ? "Sound off  [M]" : "Sound on  [M]";
        }
    }
}
