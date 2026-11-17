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
            Sign(routes, new Vector3(-.42f, 0, .30f), "< ROOTWAY", .16f); // clear of Pip's hat in the meeting view
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
                        new Vector3(.066f, .005f, delta.magnitude / 3 + .017f), "DryBed");
                    bed.rotation = Quaternion.LookRotation(delta);
                    // Each segment is an unscaled group: water, ripples and glow-moss appear together, in order.
                    var segment = Group("Returning water", channel);
                    segment.position = at + Vector3.up * .006f; segment.rotation = bed.rotation;
                    Shape("Shimmering water", PrimitiveType.Sphere, segment, Vector3.zero,
                        new Vector3(.047f, .004f, delta.magnitude / 3 + .014f), "SpringWater");
                    Vector3 bank = -Vector3.Cross(delta.normalized, Vector3.up) * .043f;
                    Shape("Channel bank moss", PrimitiveType.Sphere, channel, at + bank + Vector3.up * .004f, new Vector3(.026f, .01f, .03f), "DarkMoss");
                    for (int k = 0; k < 5; k++)
                    {
                        float a = k * 2.4f + i;
                        var tip = Shape("Glow-moss tip", PrimitiveType.Sphere, segment, Vector3.zero, Vector3.one * (.0028f + (k % 3) * .0006f), "GlowMoss");
                        tip.position = at + bank + new Vector3(Mathf.Cos(a) * .009f, .0125f + (k % 2) * .0015f, Mathf.Sin(a) * .011f);
                    }
                    if ((i * 3 + j) % 4 == 0) Ripples(segment, at + Vector3.up * .0105f);
                    segment.gameObject.SetActive(false); flow.Add(segment.gameObject);
                    Shape("Channel pebble", PrimitiveType.Sphere, channel, at + Vector3.Cross(delta.normalized, Vector3.up) * .039f + Vector3.up * .008f,
                        new Vector3(.025f, .018f, .03f), "Stone");
                }
            }
            spring.waterSegments = flow.ToArray();
            // A little pool trapped behind the twig makes the blockage readable before it is cleared.
            Shape("Water trapped behind the twig", PrimitiveType.Sphere, channel, new Vector3(.025f, .0045f, 1.052f), new Vector3(.05f, .003f, .034f), "SpringWater");
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
            // The hearth embers and the doorway halo stay dim until the spring flows.
            spring.lanternGlass = new[] { demo.home.Find("Lantern glow").GetComponent<Renderer>(), demo.home.Find("Amber round window").GetComponent<Renderer>(),
                demo.home.Find("Cosy interior - vault, hearth and tea (decoration)/Ember detail").GetComponent<Renderer>(),
                demo.home.Find("Doorway glow").GetComponent<Renderer>() };
            sound.water.transform.position = spring.interactionPoint.position;
        }

        static void Ripples(Transform parent, Vector3 at)
        {
            var obj = new GameObject("Ripple rings", typeof(ParticleSystem)); obj.transform.SetParent(parent, true);
            obj.transform.SetPositionAndRotation(at, Quaternion.identity); obj.transform.localScale = Vector3.one;
            var ps = obj.GetComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = true; main.playOnAwake = true; main.maxParticles = 4;
            main.startLifetime = 1.4f; main.startSpeed = 0; main.startSize = .028f;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.scalingMode = ParticleSystemScalingMode.Shape;
            var emission = ps.emission; emission.rateOverTime = 1.6f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .006f;
            var growth = ps.sizeOverLifetime; growth.enabled = true; growth.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, .3f, 1, 1.7f));
            var fade = ps.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.9f, .15f), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Glow("GlowRipple", "7FF2E0", .75f, 2); renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
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
            var imported = PlaceModel("Squirrel", parent, Vector3.zero, .38f);
            if (imported)
            {
                imported.name = "Hazel - curious woodland squirrel";
                var custom = imported.gameObject.AddComponent<UnderfootSquirrel>(); custom.tail = Pivot(imported, "Tail");
                return custom;
            }
            var animal = Group("Hazel - curious woodland squirrel", parent);
            // Soft pear-shaped body: a round haunch, a smaller chest and a pale belly.
            Shape("Round russet haunch", PrimitiveType.Sphere, animal, new Vector3(0, .085f, -.012f), new Vector3(.155f, .16f, .175f), "SquirrelFur");
            Shape("Upper russet chest", PrimitiveType.Sphere, animal, new Vector3(0, .155f, .03f), new Vector3(.12f, .15f, .13f), "SquirrelFur");
            Shape("Pale belly", PrimitiveType.Sphere, animal, new Vector3(0, .12f, .068f), new Vector3(.095f, .15f, .06f), "SquirrelBelly");
            Shape("Round squirrel head", PrimitiveType.Sphere, animal, new Vector3(0, .232f, .07f), new Vector3(.135f, .125f, .13f), "SquirrelFur");
            Shape("Soft muzzle", PrimitiveType.Sphere, animal, new Vector3(0, .212f, .122f), new Vector3(.07f, .05f, .055f), "SquirrelBelly");
            Shape("Small hazelnut nose", PrimitiveType.Sphere, animal, new Vector3(0, .224f, .149f), new Vector3(.022f, .016f, .013f), "Boot");
            foreach (int side in new[] { -1, 1 })
            {
                Shape("Cheek fluff", PrimitiveType.Sphere, animal, new Vector3(side * .05f, .2f, .094f), new Vector3(.036f, .028f, .03f), "SquirrelBelly");
                Shape("Large glossy eye", PrimitiveType.Sphere, animal, new Vector3(side * .044f, .246f, .112f), new Vector3(.03f, .035f, .02f), "EyeGloss");
                Shape("Eye catchlight", PrimitiveType.Sphere, animal, new Vector3(side * .04f, .254f, .121f), Vector3.one * .009f, "Glint");
                Shape("Small eye sparkle", PrimitiveType.Sphere, animal, new Vector3(side * .048f, .24f, .12f), Vector3.one * .004f, "Glint");
                var ear = Shape("Small rounded ear", PrimitiveType.Sphere, animal, new Vector3(side * .044f, .286f, .058f), new Vector3(.035f, .047f, .016f), "SquirrelFur");
                ear.localRotation = Quaternion.Euler(-8, side * 18, side * -14);
                Shape("Pink inner ear", PrimitiveType.Sphere, animal, new Vector3(side * .044f, .284f, .065f), new Vector3(.02f, .03f, .006f), "Rosy")
                    .localRotation = ear.localRotation;
                var tuft = MeshShape("Ear tuft", cone, animal, new Vector3(side * .047f, .305f, .056f), new Vector3(.014f, .028f, .012f), "SquirrelTail");
                tuft.localRotation = Quaternion.Euler(-6, 0, side * -16);
                Shape("Little hind paw", PrimitiveType.Sphere, animal, new Vector3(side * .056f, .018f, .033f), new Vector3(.049f, .032f, .09f), "SquirrelTail");
                Shape("Tucked forepaw", PrimitiveType.Sphere, animal, new Vector3(side * .03f, .14f, .1f), new Vector3(.028f, .045f, .03f), "SquirrelTail")
                    .localRotation = Quaternion.Euler(-35, 0, side * 25);
            }
            Shape("Acorn between paws", PrimitiveType.Sphere, animal, new Vector3(0, .128f, .118f), new Vector3(.033f, .04f, .031f), "Gold");
            Shape("Acorn cap", PrimitiveType.Sphere, animal, new Vector3(0, .146f, .118f), new Vector3(.036f, .016f, .034f), "Bark");
            var tail = Group("Large curled brush tail", animal);
            // A smooth S-curve: up the back, over the head, curling forwards and under at the tip.
            var spine = new[] { new Vector3(0, .05f, -.075f), new Vector3(0, .1f, -.15f), new Vector3(0, .18f, -.2f), new Vector3(0, .27f, -.21f),
                new Vector3(0, .345f, -.175f), new Vector3(0, .385f, -.11f), new Vector3(0, .38f, -.05f), new Vector3(0, .345f, -.02f), new Vector3(0, .31f, -.035f) };
            var mesh = SaveMesh("SquirrelBrushTail", RootMesh(spine, new[] { .028f, .055f, .078f, .09f, .088f, .078f, .062f, .044f, .02f }));
            MeshShape("Soft russet tail", mesh, tail, Vector3.zero, Vector3.one, "SquirrelTail");
            for (int i = 1; i < spine.Length - 1; i++)
                Shape("Tail fluff", PrimitiveType.Sphere, tail, spine[i] + new Vector3(0, .012f, -.02f) * Mathf.Sign(.3f - spine[i].y),
                    new Vector3(.11f, .08f, .08f) * (.6f + Mathf.Sin(i / 8f * Mathf.PI) * .55f), "SquirrelFur");
            Shape("Pale tail tip", PrimitiveType.Sphere, tail, spine[spine.Length - 1], new Vector3(.05f, .04f, .04f), "SquirrelBelly");
            var squirrel = animal.gameObject.AddComponent<UnderfootSquirrel>(); squirrel.tail = tail;
            return squirrel;
        }
    }
}
