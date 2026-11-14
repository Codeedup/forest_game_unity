using UnityEngine;

namespace Underfoot
{
    // A curious patrol with a nearby recovery, never damage or a hostile chase.
    public class UnderfootSquirrel : MonoBehaviour
    {
        public UnderfootPlayer player;
        public UnderfootSpring spring;
        public Transform tail;
        public Vector3 patrolA, patrolB, recoveryPoint;
        public Collider[] cover;
        public string Reaction => Time.time < reactionUntil ? "A curious sniff! You are safe beside the stepping stone.\nWait for her to pass, then cross behind her." : "";
        public int RecoveryCount { get; private set; }
        float patrolTime, reactionUntil, readyAt;
        Quaternion tailRest;
        Renderer[] looks;

        void Awake() { tailRest = tail.localRotation; looks = GetComponentsInChildren<Renderer>(); ResetPatrol(); }

        // Visual only: she has no collider, so hide her while the camera is inside her silhouette.
        void LateUpdate()
        {
            var view = Camera.main;
            if (!view || looks.Length == 0) return;
            var bounds = looks[0].bounds;
            foreach (var look in looks) bounds.Encapsulate(look.bounds);
            bounds.Expand(.02f);
            bool inside = bounds.Contains(view.transform.position);
            foreach (var look in looks) look.enabled = !inside;
        }

        void Update()
        {
            if (Time.time >= reactionUntil - 2.4f) patrolTime += Time.deltaTime;
            float along = Mathf.PingPong(patrolTime * .26f, 1);
            transform.position = Vector3.Lerp(patrolA, patrolB, along);
            float direction = Mathf.Repeat(patrolTime * .26f, 2) < 1 ? 90 : -90;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, direction, 0), Time.deltaTime * 4);
            tail.localRotation = tailRest * Quaternion.Euler(0, Mathf.Sin(patrolTime * 2) * 7, 0);
            TryRecoverPlayer();
        }

        public bool TryRecoverPlayer()
        {
            if (Time.time < readyAt || player.Height > .19f || spring.SelectedRoute != UnderfootSpring.Route.Squirrel ||
                spring.State == UnderfootSpring.SpringState.Flowing) return false;
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            if (delta.magnitude > .115f) return false;
            Vector3 eye = transform.position + Vector3.up * .11f;
            Vector3 target = player.transform.position + Vector3.up * (player.Height * .5f);
            foreach (var shelter in cover)
                if (shelter.Raycast(new Ray(eye, (target - eye).normalized), out _, Vector3.Distance(eye, target))) return false;
            player.RecoverAt(recoveryPoint);
            readyAt = Time.time + 3; reactionUntil = Time.time + 3.2f; RecoveryCount++;
            return true;
        }

        public void ResetPatrol()
        {
            patrolTime = 0; readyAt = Time.time + 1; reactionUntil = 0; RecoveryCount = 0;
            transform.SetPositionAndRotation(patrolA, Quaternion.Euler(0, 90, 0));
            tail.localRotation = tailRest;
        }
    }
}
