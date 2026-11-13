using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Underfoot.Editor
{
    public static partial class UnderfootBuilder
    {
        static void BuildSpringRoutes(GameObject systems, Transform world, UnderfootDemo demo, UnderfootSoundscape sound)
        {
            var spring = systems.AddComponent<UnderfootSpring>();
            demo.spring = spring; demo.gnome.spring = spring;
            spring.player = demo.player; spring.gnome = demo.gnome; spring.soundscape = sound;
            spring.rootwayPath = new[] {
                new Vector3(-.30f, 0, .27f), new Vector3(-.30f, 0, .48f), new Vector3(-.30f, 0, .57f),
                new Vector3(-.17f, 0, .67f), new Vector3(-.17f, 0, .81f), new Vector3(-.04f, 0, .91f) };
            spring.squirrelPath = new[] {
                new Vector3(.20f, 0, .28f), new Vector3(.24f, 0, .43f), new Vector3(.24f, 0, .53f),
                new Vector3(.26f, 0, .83f), new Vector3(.14f, 0, .90f), new Vector3(.02f, 0, .91f) };
            var routes = Group("Two paths to one spring", world);
            spring.rootwaySeeds = RoutePath(routes, "Quiet rootway", spring.rootwayPath);
            spring.squirrelSeeds = RoutePath(routes, "Squirrel clearing", spring.squirrelPath);
            Sign(routes, new Vector3(-.15f, 0, .29f), "< ROOTWAY", .16f);
            Sign(routes, new Vector3(.14f, 0, .27f), "SQUIRREL >", .12f);
            Sign(routes, new Vector3(.13f, 0, .99f), "SPRING", .095f);

            // A little arched root and a leaf detour; actual collision leaves room for an 8 cm walker.
            var archPoints = new Vector3[9]; var archRadii = new float[9];
            for (int i = 0; i < archPoints.Length; i++)
            {
                float angle = Mathf.PI * (1 - i / 8f);
                archPoints[i] = new Vector3(-.30f + Mathf.Cos(angle) * .14f, Mathf.Sin(angle) * .18f, .48f);
                archRadii[i] = .025f + Mathf.Sin(angle) * .017f;
            }
            var archMesh = SaveMesh("RootwayArch", RootMesh(archPoints, archRadii));
            var arch = MeshShape("Mossy root arch - tiny passage", archMesh, routes, Vector3.zero, Vector3.one, "Bark");
            arch.gameObject.AddComponent<MeshCollider>().sharedMesh = archMesh;
            for (int i = 0; i < 7; i++)
            {
                float angle = Mathf.PI * (.22f + i * .09f);
                Vector3 at = new Vector3(-.30f + Mathf.Cos(angle) * .15f, Mathf.Sin(angle) * .18f + .03f, .48f);
                Shape("Rootway moss cushion", PrimitiveType.Sphere, routes, at, new Vector3(.046f, .018f, .05f), "DarkMoss");
                Shape("Rootway lichen", PrimitiveType.Sphere, routes, at + new Vector3(.008f, .009f, -.014f), new Vector3(.022f, .006f, .019f), "Lichen");
            }
            var curl = MeshShape("Curled leaf beside the rootway", leafMesh, routes, new Vector3(-.30f, .018f, .70f), new Vector3(.24f, .5f, .17f), "Leaf");
            curl.localRotation = Quaternion.Euler(0, -10, 25);
            var obstacle = curl.gameObject.AddComponent<BoxCollider>();
            obstacle.center = new Vector3(0, .04f, 0); obstacle.size = new Vector3(.5f, .13f, .95f);
            for (int i = 0; i < archPoints.Length - 1; i++)
                Tube("Close-up root bark seam", routes, archPoints[i] + Vector3.back * .027f,
                    archPoints[i + 1] + Vector3.back * .027f, .002f, "BarkCrease");

            var cover = new List<Collider>();
            cover.Add(Shape("Squirrel crossing - near cover stone", PrimitiveType.Sphere, routes, new Vector3(.36f, .042f, .40f), new Vector3(.14f, .09f, .12f), "Stone", true).GetComponent<Collider>());
            cover.Add(Shape("Squirrel crossing - far cover stone", PrimitiveType.Sphere, routes, new Vector3(.43f, .045f, .86f), new Vector3(.13f, .095f, .13f), "Stone", true).GetComponent<Collider>());
            Mushroom(routes, new Vector3(.14f, 0, .38f), .095f, "Gold");
            var squirrel = Squirrel(routes);
            demo.squirrel = squirrel; squirrel.player = demo.player; squirrel.spring = spring; squirrel.cover = cover.ToArray();
            squirrel.patrolA = new Vector3(.14f, 0, .73f); squirrel.patrolB = new Vector3(.50f, 0, .73f);
            squirrel.recoveryPoint = new Vector3(.18f, .003f, .30f);
            squirrel.transform.SetPositionAndRotation(squirrel.patrolA, Quaternion.Euler(0, 90, 0));

            var channel = Group("Blocked spring beneath the guardian roots", world);
            spring.interactionPoint = Group("Twig interaction point", channel); spring.interactionPoint.position = new Vector3(.015f, 0, .99f);
            var flow = new List<GameObject>();
            Vector3[] points = { new Vector3(.025f, 0, 1.075f), new Vector3(.025f, 0, 1.0f),
                new Vector3(-.045f, 0, .965f), new Vector3(-.135f, 0, 1.0f), new Vector3(-.22f, 0, 1.04f) };
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 delta = points[i + 1] - points[i];
                for (int j = 0; j < 3; j++)
                {
                    Vector3 at = Vector3.Lerp(points[i], points[i + 1], (j + .5f) / 3);
                    var bed = Shape("Shallow dry channel", PrimitiveType.Sphere, channel, at + Vector3.up * .002f,
                        new Vector3(.066f, .005f, delta.magnitude / 3 + .017f), "Earth");
                    bed.rotation = Quaternion.LookRotation(delta);
                    var water = Shape("Returning water", PrimitiveType.Sphere, channel, at + Vector3.up * .006f,
                        new Vector3(.047f, .004f, delta.magnitude / 3 + .014f), "SpringWater");
                    water.rotation = bed.rotation; water.gameObject.SetActive(false); flow.Add(water.gameObject);
                    Shape("Channel pebble", PrimitiveType.Sphere, channel, at + Vector3.Cross(delta.normalized, Vector3.up) * .039f + Vector3.up * .008f,
                        new Vector3(.025f, .018f, .03f), "Stone");
                }
            }
            spring.waterSegments = flow.ToArray();
            Shape("Spring source stone", PrimitiveType.Sphere, channel, new Vector3(.025f, .018f, 1.09f), new Vector3(.09f, .035f, .045f), "Stone");
            for (int i = 0; i < 3; i++)
            {
                var dryLeaf = MeshShape("Dry leaf caught at the blockage", leafMesh, channel, new Vector3(-.025f + i * .034f, .008f, .98f), Vector3.one * .035f, "Leaf");
                dryLeaf.rotation = Quaternion.Euler(0, i * 71, 0);
            }
            var twig = Group("Lodged twig - single reusable physics body", channel); twig.position = new Vector3(.015f, .026f, 1.0f);
            Tube("Twig bark", twig, new Vector3(-.085f, 0, 0), new Vector3(.085f, 0, 0), .009f, "BarkLight");
            Tube("Forked twig tip", twig, new Vector3(.018f, 0, 0), new Vector3(.065f, .003f, .031f), .004f, "BarkLight");
            Shape("Pale broken twig end", PrimitiveType.Sphere, twig, new Vector3(-.085f, 0, 0), new Vector3(.002f, .015f, .015f), "Wood");
            var box = twig.gameObject.AddComponent<BoxCollider>(); box.size = new Vector3(.175f, .018f, .025f); box.contactOffset = .0005f;
            box.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Root + "/Materials/SeedFriction.physicMaterial");
            var body = twig.gameObject.AddComponent<Rigidbody>(); body.mass = .015f; body.isKinematic = true;
            body.linearDamping = .15f; body.angularDamping = 1.8f; body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            spring.twig = body;
            spring.homeLights = demo.home.GetComponentsInChildren<Light>();
            spring.lanternGlass = new[] { demo.home.Find("Lantern glow").GetComponent<Renderer>(), demo.home.Find("Amber round window").GetComponent<Renderer>() };
            sound.water.transform.position = spring.interactionPoint.position;
        }

        static GameObject RoutePath(Transform parent, string name, Vector3[] points)
        {
            var route = Group(name, parent); var seeds = Group(name + " - golden seed trail", route);
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 delta = points[i + 1] - points[i];
                var path = Shape("Soft earth footway", PrimitiveType.Sphere, route, (points[i] + points[i + 1]) * .5f + Vector3.up * .0015f,
                    new Vector3(.13f, .002f, delta.magnitude + .08f), "Path");
                path.rotation = Quaternion.LookRotation(delta);
                for (int j = 0; j < 3; j++)
                {
                    Vector3 at = Vector3.Lerp(points[i], points[i + 1], j / 3f);
                    Shape("Golden wayfinding seed", PrimitiveType.Sphere, seeds, at + Vector3.up * .006f, new Vector3(.012f, .009f, .019f), "Gold");
                    Shape("Seed seam", PrimitiveType.Sphere, seeds, at + Vector3.up * .010f, new Vector3(.002f, .002f, .014f), "Cream");
                }
            }
            seeds.gameObject.SetActive(false);
            return seeds.gameObject;
        }

        static void Sign(Transform parent, Vector3 at, string label, float height)
        {
            var sign = Group(label + " sign", parent); sign.position = at;
            Tube("Little sign post", sign, Vector3.zero, Vector3.up * height, .006f, "Bark");
            Shape("Rounded wooden signboard", PrimitiveType.Cube, sign, Vector3.up * height, new Vector3(.19f, .043f, .012f), "Wood");
            var text = new GameObject(label, typeof(TextMesh)).GetComponent<TextMesh>(); text.transform.SetParent(sign, false);
            text.transform.localPosition = new Vector3(0, height, -.007f);
            text.text = label; text.fontSize = 56; text.characterSize = .0065f; text.anchor = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Hex("FFF0C6");
            Vector3 bounds = text.GetComponent<Renderer>().bounds.size;
            float fit = bounds.x > .001f && bounds.y > .001f ? Mathf.Min(.17f / bounds.x, .030f / bounds.y) : .42f;
            text.transform.localScale = Vector3.one * fit;
            text.gameObject.AddComponent<UnderfootSignText>().depthShader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/Materials/WorldSignText.shader");
        }

        static UnderfootSquirrel Squirrel(Transform parent)
        {
            var animal = Group("Hazel - curious woodland squirrel", parent);
            Shape("Pear shaped russet body", PrimitiveType.Sphere, animal, new Vector3(0, .115f, 0), new Vector3(.15f, .20f, .20f), "SquirrelFur");
            Shape("Warm cream chest", PrimitiveType.Sphere, animal, new Vector3(0, .115f, .077f), new Vector3(.091f, .14f, .045f), "Cream");
            Shape("Round squirrel head", PrimitiveType.Sphere, animal, new Vector3(0, .225f, .066f), new Vector3(.13f, .12f, .13f), "SquirrelFur");
            Shape("Soft muzzle", PrimitiveType.Sphere, animal, new Vector3(0, .205f, .122f), new Vector3(.075f, .048f, .055f), "Cream");
            Shape("Small hazelnut nose", PrimitiveType.Sphere, animal, new Vector3(0, .218f, .148f), new Vector3(.024f, .018f, .014f), "Boot");
            foreach (int side in new[] { -1, 1 })
            {
                Shape("Bright curious eye", PrimitiveType.Sphere, animal, new Vector3(side * .048f, .242f, .109f), new Vector3(.022f, .026f, .015f), "Boot");
                Shape("Eye catchlight", PrimitiveType.Sphere, animal, new Vector3(side * .046f, .247f, .116f), Vector3.one * .006f, "Cream");
                var ear = MeshShape("Tufted squirrel ear", cone, animal, new Vector3(side * .043f, .264f, .058f), new Vector3(.045f, .075f, .032f), "SquirrelFur");
                ear.localRotation = Quaternion.Euler(-7, 0, side * -12);
                Shape("Little hind paw", PrimitiveType.Sphere, animal, new Vector3(side * .056f, .018f, .033f), new Vector3(.049f, .032f, .09f), "SquirrelTail");
                Tube("Tucked forepaw", animal, new Vector3(side * .053f, .153f, .061f), new Vector3(side * .029f, .124f, .112f), .015f, "SquirrelTail");
            }
            Shape("Acorn between paws", PrimitiveType.Sphere, animal, new Vector3(0, .125f, .116f), new Vector3(.035f, .042f, .033f), "Gold");
            var tail = Group("Large curled brush tail", animal);
            var mesh = SaveMesh("SquirrelBrushTail", RootMesh(new[] { new Vector3(0, .06f, -.065f), new Vector3(0, .13f, -.17f),
                new Vector3(0, .29f, -.19f), new Vector3(0, .365f, -.13f), new Vector3(0, .30f, -.075f) }, new[] { .035f, .078f, .092f, .073f, .025f }));
            MeshShape("Soft russet tail silhouette", mesh, tail, Vector3.zero, Vector3.one, "SquirrelTail");
            for (int i = 0; i < 5; i++)
                Tube("Tail fur sweep", tail, new Vector3(-.035f + i * .017f, .19f, -.245f), new Vector3(-.025f + i * .012f, .33f, -.19f), .004f, "SquirrelFur");
            var squirrel = animal.gameObject.AddComponent<UnderfootSquirrel>(); squirrel.tail = tail;
            return squirrel;
        }
    }
}
