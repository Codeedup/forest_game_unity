using UnityEngine;
using UnityEngine.UI;

namespace Underfoot
{
    public class UnderfootDemo : MonoBehaviour
    {
        public UnderfootPlayer player;
        public UnderfootDetails details;
        public UnderfootGnome gnome;
        public UnderfootSpring spring;
        public UnderfootSquirrel squirrel;
        public UnderfootSoundscape soundscape;
        public Slider sizeSlider;
        public Text heightLabel, promptLabel, messageLabel, questLabel;
        public Transform home;
        public bool Discovered { get; private set; }
        float blockedUntil;

        void Start()
        {
            sizeSlider.onValueChanged.AddListener(ChangeSize);
            sizeSlider.SetValueWithoutNotify(1);
            RefreshUI();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && player.ControlsEnabled) ResetDemo();
            Vector3 local = home.InverseTransformPoint(player.transform.position);
            if (Mathf.Abs(local.x) < .105f && local.z > .065f && local.z < .37f && player.Height < .19f) Discovered = true;
            RefreshUI();
        }

        public void ChangeSize(float value)
        {
            float requested = player.minHeight * Mathf.Pow(player.maxHeight / player.minHeight, value);
            float before = player.Height;
            player.RequestHeight(requested);
            soundscape.AcceptSizeChange(before, player.Height, player.GrowthBlocked);
            if (player.GrowthBlocked) blockedUntil = Time.time + 2.4f;
            sizeSlider.SetValueWithoutNotify(Mathf.Log(player.Height / player.minHeight) / Mathf.Log(player.maxHeight / player.minHeight));
            details.Refresh();
            RefreshUI();
        }

        void RefreshUI()
        {
            heightLabel.text = player.Height < 1 ? $"{player.Height * 100:0.0} cm" : $"{player.Height:0.00} m";
            promptLabel.text = spring.CanClear ? "E   Move the twig" : gnome.Choosing ? "Choose a path:  1  Quiet rootway    |    2  Squirrel clearing" :
                gnome.CanInteract ? "E   Talk to Pip, the springkeeper" :
                player.Height > .19f ? "Small wonders wait beneath the roots. Try the size slider." :
                spring.State == UnderfootSpring.SpringState.Clearing ? "The twig is coming free..." :
                spring.State == UnderfootSpring.SpringState.Flowing ? "Water is returning to the roots. The warm home is open." :
                spring.SelectedRoute == UnderfootSpring.Route.None ? "Meet Pip beside the stepping stones. Press E when close." :
                spring.SelectedRoute == UnderfootSpring.Route.Rootway ? "Quiet rootway: follow the seeds under the mossy arch." : "Squirrel clearing: wait by cover, then cross behind her.";
            questLabel.text = spring.State == UnderfootSpring.SpringState.Flowing ? "SPRING RESTORED\nA warm welcome beneath the roots" :
                spring.SelectedRoute == UnderfootSpring.Route.None ? "THE BLOCKED SPRING\nShrink and speak to Pip" :
                "THE BLOCKED SPRING\n" + (spring.SelectedRoute == UnderfootSpring.Route.Rootway ? "Quiet rootway" : "Squirrel clearing") + "  >  Move the twig";
            messageLabel.text = Time.time < blockedUntil ? "Find an open space to grow." : !string.IsNullOrEmpty(squirrel.Reaction) ? squirrel.Reaction : !string.IsNullOrEmpty(gnome.Dialogue) ? gnome.Dialogue :
                Discovered ? "A home underfoot.\nEven the smallest places have room for a guest." : "";
        }

        public void ResetDemo()
        {
            player.ResetPlayer();
            details.ResetDetails();
            gnome.ResetGnome();
            spring.ResetSpring();
            squirrel.ResetPatrol();
            soundscape.ResetFeedback();
            Discovered = false;
            blockedUntil = 0;
            sizeSlider.SetValueWithoutNotify(1);
            RefreshUI();
        }
    }
}
