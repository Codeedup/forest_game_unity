using UnityEngine;
using UnityEngine.EventSystems;

namespace Underfoot
{
    [RequireComponent(typeof(Camera))]
    public class UnderfootCamera : MonoBehaviour
    {
        public UnderfootPlayer player;
        public float yaw, pitch = 20;
        public float distanceInHeights = 2.65f;
        public bool ControlsEnabled { get; set; } = true;
        Camera view;
        bool dragging;
        readonly RaycastHit[] hits = new RaycastHit[32];

        void Awake() { view = GetComponent<Camera>(); }
        void Start() { ResetView(); }
        void LateUpdate()
        {
            if (Input.GetMouseButtonDown(1))
                dragging = !(EventSystem.current && EventSystem.current.IsPointerOverGameObject());
            if (Input.GetMouseButtonUp(1)) dragging = false;
            if (ControlsEnabled && dragging && Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * 3;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 3, -8, 67);
            }
            Frame();
        }

        public void ResetView() { yaw = 0; pitch = 20; dragging = false; Frame(); }

        public void Frame()
        {
            if (!view) view = GetComponent<Camera>();
            float h = player.Height;
            float miniature = 1 - Mathf.InverseLerp(.10f, .35f, h);
            Vector3 target = player.transform.position + Vector3.up * (h * Mathf.Lerp(.64f, 1.08f, miniature));
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 back = rotation * Vector3.back;
            float distance = h * Mathf.Lerp(distanceInHeights, 3f, miniature);
            float radius = h * .06f;
            int count = Physics.SphereCastNonAlloc(target, radius, back, hits, distance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (!hits[i].transform.IsChildOf(player.transform)) distance = Mathf.Min(distance, Mathf.Max(h * .23f, hits[i].distance - radius));
            transform.SetPositionAndRotation(target + back * distance, rotation);
            view.nearClipPlane = Mathf.Max(.001f, h * .012f);
            view.farClipPlane = 60;
        }
    }
}
