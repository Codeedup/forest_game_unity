using UnityEngine;
using UnityEngine.EventSystems;

namespace Underfoot
{
    [RequireComponent(typeof(CharacterController))]
    public class UnderfootPlayer : MonoBehaviour
    {
        public float minHeight = .08f;
        public float maxHeight = 1.7f;
        public Transform body;
        public Transform leftFoot, rightFoot, leftArm, rightArm;
        public UnderfootCamera orbit;
        public float Height { get; private set; } = 1.7f;
        public bool GrowthBlocked { get; private set; }
        public bool ControlsEnabled { get; set; } = true;
        public float WalkingSpeed => .15f * Mathf.Pow(Height / minHeight, .82f);
        public CharacterController Controller => GetComponent<CharacterController>();
        readonly Collider[] overlaps = new Collider[64];
        float fallSpeed, gait;
        Vector3 startPosition;
        Quaternion startRotation;

        void Awake()
        {
            startPosition = transform.position;
            startRotation = body.rotation;
            ApplyHeight(maxHeight);
        }

        void Update()
        {
            bool usingUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
            Vector2 input = ControlsEnabled && !(usingUI && Input.GetMouseButton(0))
                ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : Vector2.zero;
            Walk(input, Time.deltaTime);
        }

        // The same movement path is used by keyboard input and the focused Play Mode check.
        public void Walk(Vector2 input, float dt)
        {
            input = Vector2.ClampMagnitude(input, 1);
            Vector3 forward = Vector3.ProjectOnPlane(orbit.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = forward * input.y + right * input.x;
            if (Controller.isGrounded && fallSpeed < 0) fallSpeed = -.12f;
            fallSpeed = Mathf.Max(fallSpeed - 9.81f * dt, -12);
            Vector3 before = transform.position;
            Controller.Move((direction * WalkingSpeed + Vector3.up * fallSpeed) * dt);
            bool moving = direction.sqrMagnitude > .000001f &&
                Vector3.ProjectOnPlane(transform.position - before, Vector3.up).sqrMagnitude > .00000001f;
            if (moving)
            {
                body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction), 12 * dt);
                gait += dt * 8;
            }
            float swing = moving ? Mathf.Sin(gait) * 22 : 0;
            leftFoot.localRotation = Quaternion.Euler(swing, 0, 0);
            rightFoot.localRotation = Quaternion.Euler(-swing, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-swing * .6f, 0, -8);
            rightArm.localRotation = Quaternion.Euler(swing * .6f, 0, 8);
        }

        public float RequestHeight(float desired)
        {
            desired = Mathf.Clamp(desired, minHeight, maxHeight);
            GrowthBlocked = false;
            if (desired > Height && !Fits(desired))
            {
                float low = Height, high = desired;
                for (int i = 0; i < 13; i++)
                {
                    float middle = (low + high) * .5f;
                    if (Fits(middle)) low = middle; else high = middle;
                }
                desired = low;
                GrowthBlocked = true;
            }
            ApplyHeight(desired);
            return Height;
        }

        public bool Fits(float height)
        {
            float radius = height * .18f;
            Vector3 bottom = transform.position + Vector3.up * (radius + .002f);
            Vector3 top = transform.position + Vector3.up * (height - radius);
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius * .98f, overlaps, ~0, QueryTriggerInteraction.Ignore);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (overlaps[i] != Controller && !overlaps[i].transform.IsChildOf(transform)) return false;
            return true;
        }

        void ApplyHeight(float value)
        {
            Height = value;
            var cc = Controller;
            cc.height = value;
            cc.radius = value * .18f;
            cc.center = Vector3.up * (value * .5f);
            cc.skinWidth = Mathf.Max(.0008f, value * .012f);
            cc.stepOffset = value * .1f;
            cc.minMoveDistance = 0;
            body.localScale = Vector3.one * value;
        }

        public void ResetPlayer()
        {
            Controller.enabled = false;
            transform.position = startPosition;
            body.rotation = startRotation;
            Controller.enabled = true;
            fallSpeed = gait = 0;
            GrowthBlocked = false;
            ApplyHeight(maxHeight);
            orbit.ResetView();
        }
    }
}
