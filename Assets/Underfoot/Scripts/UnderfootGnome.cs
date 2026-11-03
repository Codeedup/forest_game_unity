using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Underfoot
{
    public class UnderfootGnome : MonoBehaviour
    {
        public UnderfootPlayer player;
        public Transform model, throwingArm, hand;
        public GameObject seedPrefab;
        public Vector3[] landingPoints;
        public float interactionRange = .23f, maximumHeight = .19f, cooldown = 4;
        public string Dialogue { get; private set; }
        public bool CanInteract => player.Height <= maximumHeight && Vector3.Distance(player.transform.position, transform.position) < interactionRange;
        public int ActiveSeedCount => seeds.Count;
        public IReadOnlyList<Rigidbody> Seeds => seeds;
        readonly List<Rigidbody> seeds = new List<Rigidbody>();
        float readyAt, dialogueUntil, gesture;
        bool throwing;
        Quaternion idleRotation;

        void Awake() { idleRotation = model.rotation; }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && player.ControlsEnabled) Interact();
            if (Time.time > dialogueUntil) Dialogue = "";
            if (!throwing)
            {
                model.rotation = Quaternion.Slerp(model.rotation, idleRotation, Time.deltaTime * 4);
                gesture = Mathf.MoveTowards(gesture, 0, Time.deltaTime * 100);
                throwingArm.localRotation = Quaternion.Euler(-gesture, 0, -12);
            }
        }

        public bool Interact()
        {
            if (!CanInteract || Time.time < readyAt || throwing) return false;
            readyAt = Time.time + cooldown;
            Dialogue = "A little smaller, a little closer.\nFollow my golden seeds - there's tea beneath the roots.";
            dialogueUntil = Time.time + 11;
            ClearSeeds();
            StartCoroutine(ThrowTrail());
            return true;
        }

        IEnumerator ThrowTrail()
        {
            throwing = true;
            Quaternion pointAtHome = Quaternion.LookRotation(Vector3.ProjectOnPlane(landingPoints[2] - transform.position, Vector3.up));
            for (float t = 0; t < .4f; t += Time.deltaTime)
            {
                model.rotation = Quaternion.Slerp(idleRotation, pointAtHome, t / .4f);
                yield return null;
            }
            model.rotation = pointAtHome;
            for (int i = 0; i < landingPoints.Length; i++)
            {
                for (float t = 0; t < .32f; t += Time.deltaTime)
                {
                    gesture = Mathf.Lerp(15, 105, t / .32f);
                    throwingArm.localRotation = Quaternion.Euler(-gesture, 0, -12);
                    yield return null;
                }
                Vector3 origin = hand.position + Vector3.forward * .035f;
                var seed = Instantiate(seedPrefab, origin, Quaternion.Euler(20, i * 65, 15));
                var rb = seed.GetComponent<Rigidbody>();
                foreach (var own in GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(seed.GetComponent<Collider>(), own);
                Physics.IgnoreCollision(seed.GetComponent<Collider>(), player.Controller);
                // Ballistic launch only. Gravity, contacts, friction and restitution control the flight and landing.
                float flight = .40f + .09f * i;
                Vector3 delta = landingPoints[i] - origin;
                rb.linearVelocity = delta / flight - Physics.gravity * (flight * .5f);
                rb.angularVelocity = new Vector3(3, 2, 4);
                seeds.Add(rb);
                yield return new WaitForSeconds(.3f);
            }
            throwing = false;
        }

        public void ResetGnome()
        {
            StopAllCoroutines();
            throwing = false;
            readyAt = gesture = 0;
            Dialogue = "";
            throwingArm.localRotation = Quaternion.Euler(0, 0, -12);
            model.rotation = idleRotation;
            ClearSeeds();
        }

        void ClearSeeds()
        {
            foreach (var seed in seeds) if (seed) { seed.gameObject.SetActive(false); Destroy(seed.gameObject); }
            seeds.Clear();
        }
    }
}
