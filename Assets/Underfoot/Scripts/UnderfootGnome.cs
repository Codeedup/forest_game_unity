using UnityEngine;

namespace Underfoot
{
    public class UnderfootGnome : MonoBehaviour
    {
        public UnderfootPlayer player;
        public UnderfootSpring spring;
        public UnderfootSoundscape sound;
        public Transform model, pointingArm;
        public GameObject choicePanel;
        public float interactionRange = .23f, maximumHeight = .19f;
        public string Dialogue { get; private set; } = "";
        public bool CanInteract => player.Height <= maximumHeight &&
            Vector3.Distance(player.transform.position, transform.position) < interactionRange;
        public bool Choosing => choiceOpen && CanInteract && spring.State == UnderfootSpring.SpringState.Blocked;
        bool choiceOpen;
        float dialogueUntil;
        Quaternion idleRotation;

        void Awake()
        {
            idleRotation = model.rotation;
            if (!sound) sound = FindFirstObjectByType<UnderfootSoundscape>();
        }

        void Update()
        {
            if (player.ControlsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.E)) Interact();
                if (Choosing && Input.GetKeyDown(KeyCode.Alpha1)) ChooseRootway();
                if (Choosing && Input.GetKeyDown(KeyCode.Alpha2)) ChooseSquirrel();
                if (Input.GetKeyDown(KeyCode.Escape)) choiceOpen = false;
            }
            if (!CanInteract) choiceOpen = false;
            if (choicePanel) choicePanel.SetActive(Choosing);
            if (!Choosing && Time.time > dialogueUntil) Dialogue = "";
            Vector3 direction = player.transform.position - transform.position; direction.y = 0;
            Quaternion facing = CanInteract && direction.sqrMagnitude > .00001f ? Quaternion.LookRotation(direction) : idleRotation;
            model.rotation = Quaternion.Slerp(model.rotation, facing, Time.deltaTime * 4);
            pointingArm.localRotation = Quaternion.Slerp(pointingArm.localRotation,
                Quaternion.Euler(Choosing ? -65 : 0, 0, -12), Time.deltaTime * 5);
        }

        public bool Interact()
        {
            if (!CanInteract) return false;
            if (spring.State == UnderfootSpring.SpringState.Flowing) AcknowledgeSpring();
            else if (spring.State == UnderfootSpring.SpringState.Blocked)
            {
                choiceOpen = true;
                Say("Pip: The spring beneath the roots has stopped. Can you help?\nThe quiet rootway, or past the squirrel?", 12);
            }
            return true;
        }

        public void ChooseRootway() { Choose(UnderfootSpring.Route.Rootway); }
        public void ChooseSquirrel() { Choose(UnderfootSpring.Route.Squirrel); }

        public bool Choose(UnderfootSpring.Route route)
        {
            if (!Choosing || !spring.SelectRoute(route)) return false;
            choiceOpen = false;
            if (choicePanel) choicePanel.SetActive(false);
            Say(route == UnderfootSpring.Route.Rootway
                ? "Pip: Under the mossy arch, then around the curled leaf.\nThe golden seeds lead to the spring."
                : "Pip: Past the squirrel! Wait by the stones while she passes.\nShe is only curious. Follow the golden seeds.", 10);
            return true;
        }

        public void AcknowledgeSpring()
        {
            choiceOpen = false;
            Say("Pip: There it goes. The roots can drink again.\nCome in; there is warm tea waiting.", 12);
        }

        void Say(string text, float seconds)
        {
            Dialogue = text; dialogueUntil = Time.time + seconds;
            if (sound) sound.PlayVoice(text);
        }

        public void ResetGnome()
        {
            choiceOpen = false; dialogueUntil = 0; Dialogue = "";
            if (choicePanel) choicePanel.SetActive(false);
            pointingArm.localRotation = Quaternion.Euler(0, 0, -12);
            model.rotation = idleRotation;
        }
    }
}
