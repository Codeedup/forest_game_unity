using UnityEngine;

namespace Underfoot
{
    // One saved blockage and one bounded set of feedback objects, reused for every replay.
    public class UnderfootSpring : MonoBehaviour
    {
        public enum SpringState { Blocked, Clearing, Flowing }
        public enum Route { None, Rootway, Squirrel }
        public UnderfootPlayer player;
        public UnderfootGnome gnome;
        public UnderfootSoundscape soundscape;
        public Rigidbody twig;
        public Transform interactionPoint;
        public GameObject rootwaySeeds, squirrelSeeds;
        public Vector3[] rootwayPath, squirrelPath;
        public GameObject[] waterSegments;
        public Light[] homeLights;
        public Renderer[] lanternGlass;
        public float interactionRange = .17f, maximumHeight = .19f;
        public SpringState State { get; private set; }
        public Route SelectedRoute { get; private set; }
        public bool CanClear => State == SpringState.Blocked && SelectedRoute != Route.None &&
            player.Height <= maximumHeight && Vector3.Distance(player.transform.position, interactionPoint.position) <= interactionRange;
        public float Warmth { get; private set; }
        public Vector3 BlockedPosition => startPosition;
        Vector3 startPosition;
        Quaternion startRotation;
        float clearingAt, flowingAt;
        MaterialPropertyBlock glass;

        void Awake()
        {
            glass = new MaterialPropertyBlock();
            startPosition = twig.position; startRotation = twig.rotation;
            ResetSpring();
        }

        void Update()
        {
            if (player.ControlsEnabled && Input.GetKeyDown(KeyCode.E)) ClearTwig();
            if (State == SpringState.Clearing)
            {
                // Progress follows the dynamic body's actual departure from the channel.
                if (Time.time - clearingAt > .45f && Mathf.Abs(twig.position.x - startPosition.x) > .075f)
                {
                    State = SpringState.Flowing; flowingAt = Time.time;
                    soundscape.RestoreSpring(); gnome.AcknowledgeSpring();
                }
                else if (Time.time - clearingAt > 3)
                {
                    // A disturbed body cannot leave the quest stuck halfway through a replay.
                    RestoreTwig(); State = SpringState.Blocked;
                }
            }
            Warmth = Mathf.MoveTowards(Warmth, State == SpringState.Flowing ? 1 : 0, Time.deltaTime * .75f);
            ApplyWarmth();
            for (int i = 0; i < waterSegments.Length; i++)
                waterSegments[i].SetActive(State == SpringState.Flowing && Time.time - flowingAt >= i * .10f);
        }

        public bool SelectRoute(Route route)
        {
            if (State != SpringState.Blocked || (route != Route.Rootway && route != Route.Squirrel)) return false;
            SelectedRoute = route;
            rootwaySeeds.SetActive(route == Route.Rootway);
            squirrelSeeds.SetActive(route == Route.Squirrel);
            return true;
        }

        public bool ClearTwig()
        {
            if (!CanClear) return false;
            State = SpringState.Clearing; clearingAt = Time.time;
            twig.isKinematic = false;
            twig.WakeUp();
            // Real PhysX impulse, gravity and contacts; no transform animation or replacement twig.
            twig.AddForce(new Vector3(.85f, .68f, 0), ForceMode.VelocityChange);
            twig.AddTorque(new Vector3(0, 2, -3), ForceMode.VelocityChange);
            return true;
        }

        void RestoreTwig()
        {
            if (!twig.isKinematic) { twig.linearVelocity = Vector3.zero; twig.angularVelocity = Vector3.zero; }
            twig.isKinematic = true;
            twig.position = startPosition; twig.rotation = startRotation;
            twig.transform.SetPositionAndRotation(startPosition, startRotation);
        }

        void ApplyWarmth()
        {
            // A gentle candle flicker (within about 4%) on top of the warmth the restored spring brings.
            for (int i = 0; i < homeLights.Length; i++)
                homeLights[i].intensity = Mathf.Lerp(.16f, i == 0 ? 1.4f : 1.1f, Warmth) * Flicker(i);
            for (int i = 0; i < lanternGlass.Length; i++)
            {
                Color amber = new Color(1, .68f, .27f);
                glass.SetColor("_Color", amber * Mathf.Lerp(.36f, 1, Warmth));
                // HDR emission once flowing, so the storybook bloom gives the window and lantern a soft halo.
                glass.SetColor("_EmissionColor", amber * Mathf.Lerp(.06f, 2.6f, Warmth) * Flicker(i + 3));
                lanternGlass[i].SetPropertyBlock(glass);
            }
        }

        static float Flicker(int seed) =>
            1 + (Mathf.PerlinNoise(Time.time * 2.7f + seed * 7.31f, seed * 1.7f) - .5f) * .08f;

        public void ResetSpring()
        {
            State = SpringState.Blocked; SelectedRoute = Route.None;
            clearingAt = flowingAt = 0; Warmth = 0;
            RestoreTwig();
            rootwaySeeds.SetActive(false); squirrelSeeds.SetActive(false);
            foreach (var segment in waterSegments) segment.SetActive(false);
            ApplyWarmth();
        }
    }
}
