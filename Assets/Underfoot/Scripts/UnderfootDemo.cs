using UnityEngine;
using UnityEngine.UI;

namespace Underfoot
{
    public class UnderfootDemo : MonoBehaviour
    {
        public UnderfootPlayer player;
        public UnderfootDetails details;
        public UnderfootGnome gnome;
        public Slider sizeSlider;
        public Text heightLabel, promptLabel, messageLabel;
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
            player.RequestHeight(requested);
            if (player.GrowthBlocked) blockedUntil = Time.time + 2.4f;
            sizeSlider.SetValueWithoutNotify(Mathf.Log(player.Height / player.minHeight) / Mathf.Log(player.maxHeight / player.minHeight));
            details.Refresh();
            RefreshUI();
        }

        void RefreshUI()
        {
            heightLabel.text = player.Height < 1 ? $"{player.Height * 100:0.0} cm" : $"{player.Height:0.00} m";
            promptLabel.text = gnome.CanInteract ? "E   Talk to Pip, the seedkeeper" :
                player.Height > .19f ? "Small wonders wait beneath the roots. Try the size slider." : "Follow the stepping stones towards the amber lantern.";
            messageLabel.text = Time.time < blockedUntil ? "Find an open space to grow." : !string.IsNullOrEmpty(gnome.Dialogue) ? gnome.Dialogue :
                Discovered ? "A home underfoot.\nEven the smallest places have room for a guest." : "";
        }

        public void ResetDemo()
        {
            player.ResetPlayer();
            details.ResetDetails();
            gnome.ResetGnome();
            Discovered = false;
            blockedUntil = 0;
            sizeSlider.SetValueWithoutNotify(1);
            RefreshUI();
        }
    }
}
