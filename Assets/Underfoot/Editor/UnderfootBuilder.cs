using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Underfoot.Editor
{
    // Explicit, repeatable authoring command. Nothing generates on import or on entering Play Mode.
    public static partial class UnderfootBuilder
    {
        const string Root = "Assets/Underfoot";
        public const string ScenePath = Root + "/Scenes/UnderfootDemo.unity";
        static readonly Dictionary<string, Material> palette = new Dictionary<string, Material>();
        static Mesh cone, cloak, cap, leafMesh, hoodFold;

        [MenuItem("Underfoot/Build or rebuild demo scene")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string folder in new[] { "Scenes", "Materials", "Meshes", "Prefabs" }) Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            palette.Clear(); modelCache.Clear();
            Colour("Moss", "65774A"); Colour("Sage", "8FA66D"); Colour("DarkMoss", "435C3B");
            Colour("Earth", "766448"); Colour("Path", "B49D73"); Colour("Bark", "634A3C"); Colour("BarkLight", "8A6B52");
            Colour("Plum", "735574"); Colour("PlumLight", "9B7895"); Colour("FaceShadow", "39313B");
            Colour("Skin", "DBB392"); Colour("Boot", "493B35"); Colour("Cream", "E7D9B7");
            Colour("Hat", "B3634C"); Colour("Coat", "59807B"); Colour("Gold", "DDAE53");
            Colour("Leaf", "B5964E"); Colour("LeafVein", "D9BE72"); Colour("Stone", "929386");
            Colour("Wood", "9E7A5A"); Colour("Interior", "C39B6C"); Colour("Rug", "986B63");
            Colour("Amber", "FFCE79", true); Colour("Pollen", "F5DDA1", true);
            Colour("BarkCrease", "513D31"); Colour("Lichen", "A5B48A"); Colour("RoseCap", "BB817B");
            Colour("LeafSpeckle", "9C8547"); Colour("Dew", "C5DCC3");
            Colour("SpringWater", "5FAAAE", true); Colour("WaitingAmber", "806945", true);
            Colour("SquirrelFur", "B77B53"); Colour("SquirrelTail", "8F5E48");
            Colour("Canopy", "5E7A45"); Colour("CanopyLight", "A3A75A"); Colour("ForestFloor", "3A4A30");
            Colour("FarTrunk", "3E3A3A"); Colour("FarCanopy", "34483F"); Colour("FarCanopyDeep", "2A3A3A");
            Colour("Clover", "6E9149"); Colour("MossLight", "7D9150"); Colour("EarthPatch", "806A4C"); Colour("PathWorn", "9C8661");
            Colour("LeafRust", "B4623A"); Colour("LeafGold", "D2A14A"); Colour("LeafAmber", "C47C3C"); Colour("LeafPlum", "8C5A55");
            Colour("ForgetMeNot", "86AEE8"); Colour("Buttercup", "F0C64A");
            Colour("Scarf", "D0703F"); Colour("Rosy", "E2867C"); Colour("SquirrelBelly", "EBD3AE");
            Colour("EyeGloss", "2B211E"); Colour("Glint", "FFF4DC", true); Colour("LanternGlass", "FFC870", true);
            palette["EyeGloss"].SetFloat("_Glossiness", .92f);
            palette["Glint"].SetColor("_EmissionColor", Hex("FFF4DC") * 2.2f);
            palette["LanternGlass"].SetColor("_EmissionColor", Hex("FFC870") * 2.4f);
            Colour("GlowMoss", "8FF5C8", true); palette["GlowMoss"].SetColor("_EmissionColor", Hex("8FF5C8") * 2.2f);
            Colour("DryBed", "5A4636"); Colour("FaceSkin", "E4BE9C", true);
            palette["FaceSkin"].SetColor("_EmissionColor", Hex("E4BE9C") * .22f);
            Colour("RootWood", "7A5236"); Colour("Ember", "FF8A3D", true); Colour("DuskGlass", "8E9AD6", true);
            palette["Ember"].SetColor("_EmissionColor", Hex("FF8A3D") * 2.4f);
            palette["DuskGlass"].SetColor("_EmissionColor", Hex("8E9AD6") * .7f);
            palette["GlowHearth"] = Glow("GlowHearth", "FF9A4A", .55f, 0);
            palette["SpringWater"].shader = Shader.Find("Underfoot/Spring Water");
            palette["SpringWater"].SetColor("_ShimmerColor", Hex("A8FFF0") * 1.6f);
            palette["SpringWater"].SetFloat("_Glossiness", .8f);
            palette["SpringWater"].SetColor("_EmissionColor", Hex("78B9B2") * .18f);
            palette["WaitingAmber"].SetColor("_EmissionColor", Hex("806945") * .04f);
            palette["Dew"].SetFloat("_Glossiness", .9f);
            cone = SaveMesh("Pointed", Lathe(new[] { new Vector2(.5f, 0), new Vector2(.42f, .18f), new Vector2(.22f, .7f), new Vector2(0, 1) }, 12));
            cloak = SaveMesh("Cloak", Lathe(new[] { new Vector2(.31f, 0), new Vector2(.28f, .15f), new Vector2(.14f, .8f), new Vector2(.12f, 1) }, 14));
            cap = SaveMesh("MushroomCap", Lathe(new[] { new Vector2(0, 0), new Vector2(.5f, .08f), new Vector2(.45f, .3f), new Vector2(.28f, .49f), new Vector2(0, .58f) }, 14));
            leafMesh = SaveMesh("FallenLeaf", Leaf());
            hoodFold = SaveMesh("DroopingHood", RootMesh(new[] {
                new Vector3(0, .85f, -.04f), new Vector3(0, .92f, -.09f), new Vector3(-.025f, .94f, -.16f),
                new Vector3(-.06f, .90f, -.23f), new Vector3(-.10f, .83f, -.28f)
            }, new[] { .12f, .09f, .065f, .035f, .001f }));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Lighting();
            var world = Group("Woodland clearing");
            Shape("Earth island", PrimitiveType.Cylinder, world, new Vector3(0, -.18f, 0), new Vector3(12, .14f, 12), "Earth", true);
            Shape("Mossy clearing", PrimitiveType.Cylinder, world, new Vector3(0, -.04f, 0), new Vector3(11.8f, .04f, 11.8f), "Moss", true);
            // A level ground collider is deliberate: close-up decoration never removes the walking surface.
            MeshShape("Meandering earth path", SaveMesh("Footpath", Footpath()), world, Vector3.zero, Vector3.one, "Path");
            Trees(world);
            var home = BuildHome(world);
            var leaf = Group("Fallen ochre leaf - permanent landmark", world);
            leaf.position = new Vector3(.63f, .014f, .03f);
            leaf.rotation = Quaternion.Euler(0, -26, 0);
            MeshShape("Curled leaf blade", leafMesh, leaf, Vector3.zero, Vector3.one, "Leaf");
            Tube("Central leaf rib", leaf, new Vector3(0, .017f, -.57f), new Vector3(0, .035f, .6f), .011f, "LeafVein");
            for (int i = 0; i < 7; i++)
            {
                float z = -.45f + i * .22f;
                Shape("Smooth stepping stone", PrimitiveType.Sphere, world, new Vector3((i % 2 == 0 ? -.035f : .035f), -.008f, z), new Vector3(.12f, .023f, .13f), "Stone");
            }
            Mushroom(world, new Vector3(-.58f, 0, -.08f), .43f, "Hat");
            Mushroom(world, new Vector3(-.81f, 0, .14f), .27f, "Gold");
            Mushroom(world, new Vector3(.89f, 0, .7f), .31f, "Hat");
            Mushroom(world, new Vector3(1.06f, 0, .6f), .19f, "Gold");
            Scatter(world);
            GroundDressing(world);
            LivingLight(world);
            Boundary(world);

            var playerObject = Group("Wanderer - player");
            playerObject.position = new Vector3(-.18f, .004f, -.39f);
            var cc = playerObject.gameObject.AddComponent<CharacterController>();
            cc.slopeLimit = 48;
            var player = playerObject.gameObject.AddComponent<UnderfootPlayer>();
            var body = Character(playerObject, false, out var feet, out var arms, out _);
            player.body = body;
            player.leftFoot = feet[0]; player.rightFoot = feet[1]; player.leftArm = arms[0]; player.rightArm = arms[1];
            body.localScale = Vector3.one * 1.7f;
            cc.height = 1.7f; cc.radius = .306f; cc.center = Vector3.up * .85f;
            cc.skinWidth = .02f; cc.stepOffset = .17f; cc.minMoveDistance = 0;
            var camObj = new GameObject("Third-person camera", typeof(Camera), typeof(AudioListener), typeof(UnderfootCamera));
            camObj.tag = "MainCamera";
            var camera = camObj.GetComponent<Camera>();
            camera.fieldOfView = 55; camera.clearFlags = CameraClearFlags.Skybox; camera.backgroundColor = Hex(FogHex);
            camera.allowHDR = true;
            camObj.AddComponent<UnderfootStorybookGrade>().shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/Materials/StorybookGrade.shader");
            var orbit = camObj.GetComponent<UnderfootCamera>(); orbit.player = player; player.orbit = orbit;
            orbit.Frame();

            var gnomeObject = Group("Pip - springkeeper", world);
            gnomeObject.position = new Vector3(-.15f, 0, .12f);
            var gnomeBody = Character(gnomeObject, true, out _, out var gnomeArms, out var hand);
            gnomeBody.localScale = Vector3.one * .165f;
            gnomeBody.localRotation = Quaternion.Euler(0, 180, 0);
            var gnomeCollider = gnomeObject.gameObject.AddComponent<CapsuleCollider>();
            gnomeCollider.height = .16f; gnomeCollider.radius = .035f; gnomeCollider.center = Vector3.up * .08f;
            var gnome = gnomeObject.gameObject.AddComponent<UnderfootGnome>();
            gnome.player = player; gnome.model = gnomeBody; gnome.pointingArm = gnomeArms[1];
            SeedPrefab(); // Preserve the course prototype's original seed asset.

            var systems = new GameObject("Underfoot demo");
            var details = systems.AddComponent<UnderfootDetails>();
            details.player = player; details.leaf = leaf;
            details.mossPrefab = MossPrefab(); details.tinyMushroomPrefab = TinyPrefab(); details.leafVeinsPrefab = VeinPrefab();
            details.bellcapPrefab = BellcapPrefab(); details.barkPrefab = BarkPrefab(); details.fineLeafPrefab = FineLeafPrefab();
            var demo = systems.AddComponent<UnderfootDemo>();
            demo.player = player; demo.details = details; demo.gnome = gnome; demo.home = home;
            var soundscape = Soundscape(systems, demo);
            demo.soundscape = soundscape;
            BuildSpringRoutes(systems, world, demo, soundscape);
            UI(demo, soundscape);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (var existing in EditorBuildSettings.scenes) if (existing.path != ScenePath) scenes.Add(existing);
            if (!scenes.Exists(s => s.path == "Assets/Scenes/SampleScene.unity")) scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("UNDERFOOT_BUILD_OK: saved scene, materials, meshes, prefabs and references.");
        }

        static void Lighting()
        {
            // Deep dusk: a dimmer amber sun, a darker sky and fog, and a strong cool ambient fill so shadows stay readable and cosy.
            string skyPath = Root + "/Materials/DuskSky.mat";
            var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (!sky) { sky = new Material(Shader.Find("Underfoot/Dusk Sky")); AssetDatabase.CreateAsset(sky, skyPath); }
            sky.shader = Shader.Find("Underfoot/Dusk Sky");
            sky.SetColor("_Zenith", Hex("181C3E")); sky.SetColor("_Upper", Hex("46416E")); sky.SetColor("_Horizon", Hex("C27D68"));
            sky.SetColor("_Ground", Hex(FogHex)); sky.SetColor("_SunGlow", Hex("E39A58"));
            EditorUtility.SetDirty(sky);
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("7F8CC8");
            RenderSettings.ambientEquatorColor = Hex("9C7F9A");
            RenderSettings.ambientGroundColor = Hex("56653F");
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Hex(FogHex); RenderSettings.fogDensity = .072f;
            var sun = new GameObject("Low golden-hour sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = Hex("FFB878"); sun.intensity = .62f;
            sun.shadows = LightShadows.Soft; sun.shadowStrength = .7f; sun.shadowBias = .015f; sun.shadowNormalBias = .025f;
            sun.transform.rotation = Quaternion.Euler(14, 48, 0);
            RenderSettings.sun = sun;
            sun.cullingMask = ~(1 << ShelteredLayer);
        }
        const string FogHex = "37484F";

        static Transform BuildHome(Transform parent)
        {
            var home = Group("Root home - sheltered interior", parent); home.position = new Vector3(0, 0, 1.13f);
            Shape("Left plaster wall", PrimitiveType.Cube, home, new Vector3(-.16f, .115f, .2f), new Vector3(.035f, .23f, .44f), "Interior", true);
            Shape("Right plaster wall", PrimitiveType.Cube, home, new Vector3(.16f, .115f, .2f), new Vector3(.035f, .23f, .44f), "Interior", true);
            Shape("Back wall", PrimitiveType.Cube, home, new Vector3(0, .115f, .41f), new Vector3(.35f, .23f, .035f), "Interior", true);
            Shape("Low timber roof - growth obstruction", PrimitiveType.Cube, home, new Vector3(0, .24f, .2f), new Vector3(.41f, .04f, .5f), "Bark", true);
            for (int side = -1; side <= 1; side += 2)
            {
                Shape("Front timber cheek", PrimitiveType.Cube, home, new Vector3(side * .151f, .1f, 0), new Vector3(.09f, .20f, .035f), "Wood", true);
                Tube("Door jamb", home, new Vector3(side * .106f, .005f, -.025f), new Vector3(side * .106f, .19f, -.025f), .013f, "BarkLight");
            }
            Shape("Door lintel", PrimitiveType.Cube, home, new Vector3(0, .203f, -.01f), new Vector3(.25f, .04f, .05f), "Wood", true);
            for (int i = 0; i < 6; i++)
                Shape("Roof plank", PrimitiveType.Cube, home, new Vector3(-.18f + i * .071f, .269f, .2f), new Vector3(.065f, .022f, .52f), "BarkLight");
            Shape("Welcome mat", PrimitiveType.Cube, home, new Vector3(0, .001f, .07f), new Vector3(.15f, .002f, .12f), "Rug");
            for (int i = 0; i < 7; i++)
                Shape("Honey-coloured floorboard", PrimitiveType.Cube, home, new Vector3(-.126f + i * .042f, .0005f, .235f), new Vector3(.04f, .001f, .335f), "Wood");
            for (int side = -1; side <= 1; side += 2)
                Shape("Interior timber upright", PrimitiveType.Cube, home, new Vector3(side * .135f, .11f, .379f), new Vector3(.015f, .22f, .02f), "BarkLight");
            Shape("Back timber beam", PrimitiveType.Cube, home, new Vector3(0, .19f, .379f), new Vector3(.28f, .015f, .02f), "BarkLight");
            Shape("Round window frame", PrimitiveType.Cylinder, home, new Vector3(.15f, .13f, -.026f), new Vector3(.073f, .006f, .073f), "Bark").localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Amber round window", PrimitiveType.Cylinder, home, new Vector3(.15f, .13f, -.034f), new Vector3(.054f, .004f, .054f), "WaitingAmber").localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Window cross", PrimitiveType.Cube, home, new Vector3(.15f, .13f, -.04f), new Vector3(.004f, .054f, .004f), "Wood");
            Shape("Window cross", PrimitiveType.Cube, home, new Vector3(.15f, .13f, -.041f), new Vector3(.054f, .004f, .004f), "Wood");
            Shape("Little table", PrimitiveType.Cylinder, home, new Vector3(-.085f, .064f, .28f), new Vector3(.085f, .009f, .085f), "Wood");
            Shape("Table leg", PrimitiveType.Cylinder, home, new Vector3(-.085f, .031f, .28f), new Vector3(.016f, .031f, .016f), "Bark");
            Shape("Round cushion", PrimitiveType.Sphere, home, new Vector3(.075f, .018f, .29f), new Vector3(.09f, .035f, .09f), "Plum");
            Tube("Lantern hook", home, new Vector3(-.19f, .15f, -.09f), new Vector3(-.19f, .22f, -.09f), .006f, "Bark");
            Shape("Lantern glow", PrimitiveType.Cylinder, home, new Vector3(-.19f, .16f, -.09f), new Vector3(.035f, .024f, .035f), "WaitingAmber");
            MeshShape("Lantern cap", cap, home, new Vector3(-.19f, .185f, -.09f), new Vector3(.05f, .04f, .05f), "Bark");
            // A soft halo by the door that blooms as the spring restores the warmth (driven with the lantern glass).
            Shape("Doorway glow", PrimitiveType.Quad, home, new Vector3(-.19f, .16f, -.1f), new Vector3(.3f, .3f, 1), "GlowHearth")
                .GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            PointLight("Lantern - warm welcome", home, new Vector3(-.16f, .14f, -.06f), .16f, .5f);
            PointLight("Interior hearth glow", home, new Vector3(.05f, .06f, .32f), .16f, .48f);
            HomeInterior(home);
            return home;
        }

        static void Trees(Transform parent)
        {
            var old = Group("Old guardian tree and roots", parent);
            var trunk = Shape("Ancient fluted trunk", PrimitiveType.Cylinder, old, new Vector3(.12f, 2.5f, 2.24f), new Vector3(1.35f, 2.5f, 1.25f), "Bark", true);
            trunk.rotation = Quaternion.Euler(0, 12, -4);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * .2f;
                Tube("Bark flute", old, new Vector3(.12f + Mathf.Cos(a) * .61f, .16f, 2.24f + Mathf.Sin(a) * .57f), new Vector3(.12f + Mathf.Cos(a) * .47f, 4.1f, 2.24f + Mathf.Sin(a) * .49f), .10f, i % 2 == 0 ? "BarkLight" : "Bark", true);
            }
            // Roots curve around the entrance; the opening and landmarks stay at every scale.
            foreach (int side in new[] { -1, 1 })
            {
                Vector3[] points = { new Vector3(side * .35f, .52f, 1.92f), new Vector3(side * .34f, .36f, 1.4f), new Vector3(side * .45f, .20f, 1.05f), new Vector3(side * .9f, .10f, .65f), new Vector3(side * 1.65f, .015f, .4f) };
                var rootMesh = SaveMesh(side < 0 ? "LeftRoot" : "RightRoot", RootMesh(points));
                var root = MeshShape("Curving tapered guardian root", rootMesh, old, Vector3.zero, Vector3.one, "Bark");
                root.gameObject.AddComponent<MeshCollider>().sharedMesh = rootMesh;
                Tube("Moss along root", old, points[1] + Vector3.up * .12f, points[3] + Vector3.up * .06f, .046f, "DarkMoss");
            }
            Tube("Root over the hidden roof", old, new Vector3(-.30f, .31f, 1.32f), new Vector3(.31f, .33f, 1.37f), .10f, "Bark", true);
            var random = new System.Random(29);
            var canopy = Group("Woodland canopies and trunks (combined)", parent);
            Canopy(canopy, new Vector3(.12f, 4.6f, 2.2f), 1.5f, 16, random, true);
            for (int i = 0; i < 14; i++)
            {
                float a = i * Mathf.PI * 2 / 14;
                Vector3 p = new Vector3(Mathf.Cos(a) * 4.6f, 0, Mathf.Sin(a) * 4.6f);
                float h = 3 + (i % 3) * .6f;
                // The original trunk collider is kept exactly; only its plain cylinder is hidden.
                Shape("Woodland trunk collider", PrimitiveType.Cylinder, parent, p + Vector3.up * h / 2, new Vector3(.35f, h / 2, .42f), "Bark", true)
                    .GetComponent<MeshRenderer>().enabled = false;
                var tree = PlaceModel("Tree", parent, p, h + 1.3f, i * 47f);
                if (tree) { tree.name = "Woodland tree"; continue; }
                TaperedTrunk(canopy, p, h + .3f, .27f, random, "Bark");
                Canopy(canopy, p + Vector3.up * (h + .15f), 1.05f, 8, random, i % 4 == 1);
                for (int j = 0; j < 3; j++)
                    Shape("Soft forest understorey", PrimitiveType.Sphere, canopy, p + new Vector3((j - 1) * .42f, .23f + j * .06f, -.15f),
                        new Vector3(.92f, .62f, .85f), j % 2 == 0 ? "DarkMoss" : "Moss");
            }
            CombineDecoration(canopy, "WoodlandCanopy");
            ForestDepth(parent);
        }

        static void Scatter(Transform parent)
        {
            var random = new System.Random(83);
            for (int i = 0; i < 44; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2;
                float radius = 1.45f + (float)random.NextDouble() * 3.6f;
                Vector3 p = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                if (i % 4 == 0) Shape("Soft woodland stone", PrimitiveType.Sphere, parent, p, new Vector3(.28f, .15f, .22f), "Stone", true);
                else if (PlaceModel("Grass", parent, p, .3f, i * 51f) is Transform grass) grass.name = "Grass tuft";
                else
                    for (int j = 0; j < 3; j++)
                    {
                        var fern = MeshShape("Fern leaf", leafMesh, parent, p + new Vector3(j * .06f, .02f, 0), Vector3.one * .27f, i % 2 == 0 ? "Canopy" : "MossLight");
                        fern.localRotation = Quaternion.Euler(-40 - j * 9, i * 51 + j * 65, 0);
                    }
            }
        }

        static void Boundary(Transform parent)
        {
            var boundary = Group("Invisible clearing safety boundary", parent);
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2 / 24;
                var wall = new GameObject("Clearing edge", typeof(BoxCollider)).transform;
                wall.SetParent(boundary, false); wall.position = new Vector3(Mathf.Sin(a) * 5.3f, 1.8f, Mathf.Cos(a) * 5.3f);
                wall.rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
                wall.GetComponent<BoxCollider>().size = new Vector3(1.5f, 4, .15f);
            }
        }

        static Transform Character(Transform parent, bool gnome, out Transform[] feet, out Transform[] arms, out Transform hand)
        {
            if (TryCharacterModel(parent, gnome, out var model, out feet, out arms, out hand)) return model;
            var body = Group(gnome ? "Friendly gnome model" : "Hooded wanderer model", parent);
            MeshShape("Bell-shaped coat", cloak, body, new Vector3(0, .13f, 0), new Vector3(.84f, .57f, .70f), gnome ? "Coat" : "Plum");
            if (!gnome) MeshShape("Cloak shoulder layer", cloak, body, new Vector3(0, .49f, 0), new Vector3(.79f, .24f, .73f), "PlumLight");
            Shape("Head", PrimitiveType.Sphere, body, new Vector3(0, .77f, .025f), new Vector3(.28f, .29f, .27f), gnome ? "Skin" : "PlumLight");
            if (gnome)
            {
                var hat = MeshShape("Crooked pointed hat", cone, body, new Vector3(0, .85f, 0), new Vector3(.43f, .4f, .43f), "Hat");
                hat.localRotation = Quaternion.Euler(0, 0, -9);
                Shape("Hat brim", PrimitiveType.Sphere, body, new Vector3(0, .86f, 0), new Vector3(.49f, .055f, .4f), "Hat");
                var beard = MeshShape("Tapering cream beard", cone, body, new Vector3(0, .76f, .12f), new Vector3(.26f, .31f, .20f), "Cream");
                beard.localRotation = Quaternion.Euler(180, 0, 0);
                Shape("Bulbous nose", PrimitiveType.Sphere, body, new Vector3(0, .772f, .186f), new Vector3(.125f, .112f, .11f), "Skin");
                for (int side = -1; side <= 1; side += 2)
                {
                    Shape("Kind dark eye", PrimitiveType.Sphere, body, new Vector3(side * .062f, .81f, .146f), Vector3.one * .03f, "EyeGloss");
                    Shape("Eye highlight", PrimitiveType.Sphere, body, new Vector3(side * .056f, .817f, .159f), Vector3.one * .01f, "Glint");
                    Shape("Rosy cheek", PrimitiveType.Sphere, body, new Vector3(side * .078f, .768f, .128f), new Vector3(.05f, .032f, .022f), "Rosy");
                }
                // The hat tip curls over with a little bobble.
                Tube("Curling hat tip", body, new Vector3(.05f, 1.19f, 0), new Vector3(.1f, 1.24f, -.01f), .022f, "Hat");
                Tube("Curling hat tip", body, new Vector3(.1f, 1.24f, -.01f), new Vector3(.145f, 1.215f, -.02f), .016f, "Hat");
                Shape("Hat tip bobble", PrimitiveType.Sphere, body, new Vector3(.152f, 1.205f, -.02f), Vector3.one * .038f, "Cream");
                Shape("Leather belt", PrimitiveType.Cylinder, body, new Vector3(0, .43f, 0), new Vector3(.36f, .016f, .3f), "Boot");
                Shape("Brass buckle", PrimitiveType.Cube, body, new Vector3(0, .43f, .148f), new Vector3(.055f, .045f, .012f), "Gold");
                Shape("Buckle hole", PrimitiveType.Cube, body, new Vector3(0, .43f, .154f), new Vector3(.03f, .022f, .004f), "Boot");
            }
            else
            {
                Shape("Deep hood", PrimitiveType.Sphere, body, new Vector3(0, .805f, -.006f), new Vector3(.38f, .38f, .34f), "Plum");
                MeshShape("Drooping hood fold", hoodFold, body, Vector3.zero, Vector3.one, "Plum");
                Shape("Shadow within hood", PrimitiveType.Sphere, body, new Vector3(0, .79f, .127f), new Vector3(.234f, .248f, .04f), "FaceShadow");
                Shape("Small visible face", PrimitiveType.Sphere, body, new Vector3(0, .778f, .149f), new Vector3(.138f, .13f, .032f), "FaceSkin");
                Shape("Cloak clasp", PrimitiveType.Sphere, body, new Vector3(0, .64f, .17f), Vector3.one * .047f, "Gold");
                Shape("Satchel", PrimitiveType.Sphere, body, new Vector3(.19f, .34f, -.04f), new Vector3(.18f, .22f, .12f), "Wood");
                // A hint of a face: two warm glints in the hood shadow.
                for (int side = -1; side <= 1; side += 2)
                {
                    Shape("Shy eye", PrimitiveType.Sphere, body, new Vector3(side * .03f, .787f, .161f), new Vector3(.024f, .028f, .012f), "EyeGloss");
                    Shape("Eye glint", PrimitiveType.Sphere, body, new Vector3(side * .026f, .794f, .168f), Vector3.one * .012f, "Glint");
                }
                Shape("Warm knitted scarf", PrimitiveType.Sphere, body, new Vector3(0, .662f, .012f), new Vector3(.3f, .075f, .29f), "Scarf");
                Tube("Scarf tail", body, new Vector3(.06f, .65f, .13f), new Vector3(.085f, .5f, .165f), .03f, "Scarf");
                Shape("Scarf tail fringe", PrimitiveType.Sphere, body, new Vector3(.086f, .49f, .166f), new Vector3(.065f, .02f, .03f), "Gold");
                // Running stitches around the cloak hem.
                for (int i = 0; i < 18; i++)
                {
                    float a = i * Mathf.PI * 2 / 18;
                    var stitch = Shape("Hem stitch", PrimitiveType.Sphere, body, new Vector3(Mathf.Cos(a) * .262f, .165f, Mathf.Sin(a) * .219f),
                        new Vector3(.035f, .009f, .01f), "Cream");
                    stitch.localRotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg + 90, 0);
                }
            }
            feet = new Transform[2]; arms = new Transform[2]; hand = null;
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                feet[i] = Group("Walking foot pivot", body); feet[i].localPosition = new Vector3(side * .105f, .17f, 0);
                Shape("Soft boot", PrimitiveType.Capsule, feet[i], new Vector3(0, -.095f, .025f), new Vector3(.13f, .075f, .19f), "Boot");
                arms[i] = Group("Arm gesture pivot", body); arms[i].localPosition = new Vector3(side * .185f, .60f, 0);
                Shape("Sleeve", PrimitiveType.Capsule, arms[i], new Vector3(side * .038f, -.115f, 0), new Vector3(.1f, .12f, .10f), gnome ? "Coat" : "Plum");
                var palm = Shape("Hand", PrimitiveType.Sphere, arms[i], new Vector3(side * .05f, -.24f, .015f), Vector3.one * .085f, "Skin");
                if (i == 1) hand = palm;
                if (!gnome && i == 0)
                {
                    // A small hand lantern: emissive glass and the storybook bloom, no real-time light.
                    var lantern = Group("Hand lantern", arms[i]); lantern.localPosition = new Vector3(side * .05f, -.27f, .03f); lantern.localScale = Vector3.one * 1.3f;
                    Tube("Lantern bail", lantern, new Vector3(0, .035f, 0), new Vector3(0, -.005f, 0), .005f, "Bark");
                    MeshShape("Lantern roof", cone, lantern, new Vector3(0, -.03f, 0), new Vector3(.075f, .035f, .075f), "Bark");
                    Shape("Lantern glow glass", PrimitiveType.Cylinder, lantern, new Vector3(0, -.066f, 0), new Vector3(.05f, .036f, .05f), "LanternGlass");
                    Shape("Lantern base", PrimitiveType.Cylinder, lantern, new Vector3(0, -.104f, 0), new Vector3(.064f, .006f, .064f), "Bark");
                }
            }
            return body;
        }

        static void Mushroom(Transform parent, Vector3 position, float height, string colour)
        {
            var mushroom = PlaceModel("Mushroom", parent, position, height, Mathf.Repeat(position.x * 197 + position.z * 71, 360));
            if (mushroom) mushroom.name = "Woodland mushroom";
            else { mushroom = Group("Woodland mushroom", parent); mushroom.localPosition = position; ProceduralMushroom(mushroom, height, colour); }
            var collider = mushroom.gameObject.AddComponent<CapsuleCollider>();
            collider.height = height; collider.radius = height * .19f; collider.center = Vector3.up * height * .5f;
        }

        static void ProceduralMushroom(Transform mushroom, float height, string colour)
        {
            Shape("Cream stem", PrimitiveType.Capsule, mushroom, new Vector3(0, height * .4f, 0), new Vector3(height * .21f, height * .4f, height * .21f), "Cream");
            MeshShape("Scalloped cap", cap, mushroom, new Vector3(0, height * .67f, 0), new Vector3(height, height * .55f, height), colour);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3;
                Shape("Cream cap spot", PrimitiveType.Sphere, mushroom, new Vector3(Mathf.Cos(a) * height * .27f, height * .91f, Mathf.Sin(a) * height * .27f), new Vector3(height * .12f, height * .025f, height * .09f), "Cream");
            }
        }

        static GameObject MossPrefab()
        {
            var root = Group("Moss star cluster");
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3;
                var stem = MeshShape("Moss spear", cone, root, new Vector3(Mathf.Cos(a) * .012f, 0, Mathf.Sin(a) * .012f), new Vector3(.012f, .026f + i * .003f, .012f), i % 2 == 0 ? "Sage" : "DarkMoss");
                stem.localRotation = Quaternion.Euler(12, i * 60, 12);
            }
            return SavePrefab(root, "MossCluster");
        }
        static GameObject TinyPrefab()
        {
            var root = Group("Tiny luminous inkcaps");
            for (int i = 0; i < 3; i++)
            {
                float h = .018f + i * .004f;
                Vector3 p = new Vector3((i - 1) * .014f, 0, i % 2 * .011f);
                Tube("Fine stem", root, p, p + Vector3.up * h, .0022f, "Cream");
                MeshShape("Little inkcap", cap, root, p + Vector3.up * h, new Vector3(.018f, .018f, .018f), i == 1 ? "Pollen" : "PlumLight");
            }
            return SavePrefab(root, "TinyInkcaps");
        }
        static GameObject VeinPrefab()
        {
            var root = Group("Leaf vein branches");
            for (int i = 0; i < 7; i++)
            {
                float z = -.39f + i * .13f;
                foreach (int side in new[] { -1, 1 })
                    for (int j = 0; j < 5; j++)
                    {
                        float a = j / 5f, b = (j + 1) / 5f;
                        Tube("Raised branching vein", root, LeafPoint(z + a * .14f + a * a * .05f, side * a * .92f) + Vector3.up * .002f,
                            LeafPoint(z + b * .14f + b * b * .05f, side * b * .92f) + Vector3.up * .002f, .0025f * (1 - a * .65f), "LeafVein");
                    }
            }
            CombineDecoration(root, "LeafPrimaryVeins");
            return SavePrefab(root, "LeafVeins");
        }

        static Vector3 LeafPoint(float z, float transverse)
        {
            float ring = Mathf.Clamp01((z + .58f) / 1.18f) * 12;
            int a = Mathf.Min(11, Mathf.FloorToInt(ring)), b = a + 1;
            float w0 = Mathf.Sin(a / 12f * Mathf.PI) * (.38f + a % 2 * .055f);
            float w1 = Mathf.Sin(b / 12f * Mathf.PI) * (.38f + b % 2 * .055f);
            return new Vector3(Mathf.Lerp(w0, w1, ring - a) * transverse,
                Mathf.Lerp(.019f, .055f + Mathf.Abs(z) * .07f, Mathf.Abs(transverse)), z);
        }

        static GameObject FineLeafPrefab()
        {
            var root = Group("Leaf lace and morning dew");
            for (int i = 0; i < 7; i++)
                foreach (int side in new[] { -1, 1 })
                    for (int branch = 1; branch < 4; branch++)
                    {
                        float z = -.39f + i * .13f, f = branch * .2f;
                        Vector3 from = LeafPoint(z + f * .14f, side * f * .92f) + Vector3.up * .003f;
                        Vector3 to = LeafPoint(z + f * .14f + .057f, side * (f + .16f) * .92f) + Vector3.up * .003f;
                        Tube("Fine vein fork", root, from, to, .0008f, "LeafVein");
                    }
            for (int i = 0; i < 14; i++)
            {
                Vector3 p = LeafPoint(-.38f + (i % 7) * .13f, i < 7 ? -.73f : .68f);
                Shape("Leaf freckle", PrimitiveType.Sphere, root, p + Vector3.up * .001f, new Vector3(.01f, .001f, .017f), "LeafSpeckle");
                if (i % 2 == 0)
                    Shape("Morning dew bead", PrimitiveType.Sphere, root, p + Vector3.up * .0055f, new Vector3(.017f, .012f, .017f), "Dew");
            }
            CombineDecoration(root, "LeafLace");
            return SavePrefab(root, "LeafLaceAndDew");
        }

        static GameObject BellcapPrefab()
        {
            var root = Group("Hidden rose bellcap colony");
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = new Vector3(Mathf.Sin(i * 2.4f) * .022f, 0, Mathf.Cos(i * 2.4f) * .022f);
                float h = .024f + (i % 3) * .014f, radius = .014f + i % 2 * .006f;
                Tube("Curved cream stalk", root, p, p + new Vector3(.003f, h, 0), .0025f, "Cream");
                MeshShape("Rose bell", cap, root, p + new Vector3(.003f, h, 0), new Vector3(radius * 2, radius * 2, radius * 2), "RoseCap");
                for (int j = 0; j < 8; j++)
                {
                    float a = j * Mathf.PI / 4;
                    Vector3 edge = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius * .86f;
                    Tube("Visible cream gill", root, p + new Vector3(.003f, h + .001f, 0), p + new Vector3(.003f, h + .001f, 0) + edge, .0006f, "Cream");
                }
                Shape("Tiny cap fleck", PrimitiveType.Sphere, root, p + new Vector3(.003f, h + radius * 1.12f, 0), new Vector3(.0035f, .001f, .004f), "Cream");
            }
            CombineDecoration(root, "Bellcaps");
            return SavePrefab(root, "HiddenBellcaps");
        }

        static GameObject BarkPrefab()
        {
            var root = Group("Hidden bark grain, lichen and shelf fungi");
            foreach (int side in new[] { -1, 1 })
            {
                Vector3[] points = { new Vector3(side * .35f, .52f, 1.92f), new Vector3(side * .34f, .36f, 1.4f), new Vector3(side * .45f, .20f, 1.05f), new Vector3(side * .9f, .10f, .65f), new Vector3(side * 1.65f, .015f, .4f) };
                float[] radii = { .24f, .18f, .13f, .075f, .008f };
                var rotations = new Quaternion[points.Length];
                for (int j = 0; j < points.Length; j++)
                    rotations[j] = Quaternion.FromToRotation(Vector3.up, (points[Mathf.Min(j + 1, 4)] - points[Mathf.Max(0, j - 1)]).normalized);
                for (int line = 0; line < 12; line++)
                {
                    float a = line * Mathf.PI / 6;
                    // Gently wandering ridges, so the roots read as grown wood rather than planks.
                    Vector3 Ridge(float u)
                    {
                        int j = Mathf.Min(3, Mathf.FloorToInt(u)); float f = u - j;
                        float wander = a + .2f * Mathf.Sin(u * 2.7f + line * 1.9f);
                        Vector3 radial = new Vector3(Mathf.Cos(wander), 0, Mathf.Sin(wander));
                        return Vector3.Lerp(points[j], points[j + 1], f) +
                            Quaternion.Slerp(rotations[j], rotations[j + 1], f) * radial * (Mathf.Lerp(radii[j], radii[j + 1], f) + .0018f);
                    }
                    for (int step = 0; step < 12; step++)
                        Tube("Long flowing bark ridge", root, Ridge(step / 3f), Ridge((step + 1) / 3f), .0026f - step / 3f * .0005f,
                            line % 3 == 0 ? "BarkCrease" : "BarkLight");
                }
            }
            Physics.SyncTransforms();
            // Raycast onto the authored wood so the fine details conform to its real silhouette.
            for (int i = 0; i < 34; i++)
            {
                float x = -.65f + (i % 9) * .17f, y = .20f + (i / 9) * .20f;
                if (!Physics.Raycast(new Vector3(x, y, .42f), Vector3.forward, out var hit, 2.4f)) continue;
                if (!hit.transform.IsChildOf(GameObject.Find("Old guardian tree and roots").transform)) continue;
                Vector3 p = hit.point + hit.normal * .002f;
                var patch = Shape("Scalloped sage lichen", PrimitiveType.Sphere, root, p, new Vector3(.027f, .003f, .036f), "Lichen");
                patch.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (i % 3 == 0)
                {
                    Shape("Cream shelf fungus rim", PrimitiveType.Sphere, root, p + hit.normal * .012f, new Vector3(.08f, .011f, .048f), "Cream");
                    Shape("Ochre shelf fungus", PrimitiveType.Sphere, root, p + hit.normal * .012f + Vector3.up * .004f, new Vector3(.076f, .012f, .046f), "Hat");
                }
                if (i % 4 == 0)
                {
                    Vector3 tangent = Vector3.Cross(hit.normal, Vector3.right).normalized;
                    Vector3 cross = Vector3.Cross(hit.normal, tangent).normalized;
                    for (int j = 0; j < 12; j++)
                    {
                        float a = j * Mathf.PI / 6, b = (j + 1) * Mathf.PI / 6;
                        Tube("Oval bark knot", root, p + tangent * Mathf.Cos(a) * .038f + cross * Mathf.Sin(a) * .014f,
                            p + tangent * Mathf.Cos(b) * .038f + cross * Mathf.Sin(b) * .014f, .0016f, "BarkCrease");
                    }
                }
            }
            CombineDecoration(root, "BarkRelief");
            return SavePrefab(root, "BarkRelief");
        }

        static void CombineDecoration(Transform root, string prefix)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var pieces = root.GetComponentsInChildren<MeshFilter>();
            foreach (var piece in pieces)
            {
                var material = piece.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.ContainsKey(material)) groups[material] = new List<CombineInstance>();
                groups[material].Add(new CombineInstance { mesh = piece.sharedMesh, transform = root.worldToLocalMatrix * piece.transform.localToWorldMatrix });
            }
            foreach (var pair in groups)
            {
                string materialName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(pair.Key));
                var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray());
                MeshShape(materialName + " detail", SaveMesh(prefix + materialName, mesh), root, Vector3.zero, Vector3.one, materialName);
            }
            foreach (var piece in pieces) Object.DestroyImmediate(piece.gameObject);
        }

        static UnderfootSoundscape Soundscape(GameObject systems, UnderfootDemo demo)
        {
            var sound = systems.AddComponent<UnderfootSoundscape>(); sound.demo = demo;
            sound.music = AudioTrack(systems.transform, "Original meadow tune", "Underfoot_Meadow_Waltz.wav");
            sound.forest = AudioTrack(systems.transform, "Wind, leaf rustle and woodland birds", "Underfoot_Forest_Air.wav");
            sound.shrinkCue = FeedbackClip("Underfoot_Shrink.wav"); sound.growCue = FeedbackClip("Underfoot_Grow.wav");
            sound.feedback = new GameObject("Size adjustment cues", typeof(AudioSource)).GetComponent<AudioSource>();
            sound.feedback.transform.SetParent(systems.transform, false); sound.feedback.playOnAwake = false;
            sound.water = AudioTrack(systems.transform, "Restored spring water", "Underfoot_Spring.wav");
            sound.water.spatialBlend = .7f; sound.water.rolloffMode = AudioRolloffMode.Linear;
            sound.water.minDistance = .15f; sound.water.maxDistance = 2;
            return sound;
        }
        static AudioClip FeedbackClip(string file)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/" + file);
            if (!clip) throw new FileNotFoundException("Generate feedback with Tools/generate_underfoot_feedback.py first.", file);
            return clip;
        }
        static AudioSource AudioTrack(Transform parent, string name, string file)
        {
            string path = Root + "/Audio/" + file;
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) throw new System.IO.FileNotFoundException("Generate the original audio with Tools/generate_underfoot_audio.py first.", path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad; settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .75f;
            importer.defaultSampleSettings = settings; importer.forceToMono = false; importer.SaveAndReimport();
            var audio = new GameObject(name, typeof(AudioSource)).GetComponent<AudioSource>();
            audio.transform.SetParent(parent, false); audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            audio.playOnAwake = false; audio.loop = true; audio.spatialBlend = 0; audio.volume = 0;
            return audio;
        }
        static GameObject SeedPrefab()
        {
            var root = Group("Golden trail seed");
            Shape("Amber seed husk", PrimitiveType.Sphere, root, Vector3.zero, new Vector3(.017f, .013f, .026f), "Gold");
            Shape("Seed seam", PrimitiveType.Sphere, root, new Vector3(0, .006f, 0), new Vector3(.003f, .002f, .022f), "Cream");
            var collider = root.gameObject.AddComponent<SphereCollider>(); collider.radius = .009f; collider.contactOffset = .001f;
            string physicsPath = Root + "/Materials/SeedFriction.physicMaterial";
            var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(physicsPath);
            if (!friction) { friction = new PhysicsMaterial("Seed friction"); AssetDatabase.CreateAsset(friction, physicsPath); }
            friction.dynamicFriction = .9f; friction.staticFriction = .95f; friction.bounciness = .03f;
            friction.frictionCombine = PhysicsMaterialCombine.Maximum; friction.bounceCombine = PhysicsMaterialCombine.Minimum;
            EditorUtility.SetDirty(friction); collider.sharedMaterial = friction;
            var rb = root.gameObject.AddComponent<Rigidbody>(); rb.mass = .008f; rb.linearDamping = .05f; rb.angularDamping = 1.8f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; rb.interpolation = RigidbodyInterpolation.Interpolate;
            return SavePrefab(root, "GoldenSeed");
        }

        // Storybook interface: small parchment cards with a soft ink border, dark ink text, captions low on screen.
        const string Ink = "2A1A10", SoftInk = "52391F", Parchment = "F3E6C6";
        static void UI(UnderfootDemo demo, UnderfootSoundscape soundscape)
        {
            var canvas = new GameObject("Underfoot interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900); scaler.matchWidthOrHeight = .5f;
            var top = Panel("Title card", canvas.transform, new Vector2(20, -18), new Vector2(286, 62), new Vector2(0, 1));
            TextAt("UNDERFOOT", top, new Vector2(14, -6), new Vector2(260, 32), 25, "5B2E3C").fontStyle = FontStyle.Bold;
            TextAt("A small wanderer. A world worth noticing.", top, new Vector2(15, -37), new Vector2(262, 20), 14, SoftInk).fontStyle = FontStyle.Italic;
            var soundButton = Panel("Sound toggle", canvas.transform, new Vector2(20, -86), new Vector2(118, 26), new Vector2(0, 1));
            var button = soundButton.gameObject.AddComponent<Button>(); button.targetGraphic = soundButton.GetComponent<Image>();
            // Persistent event survives saving/reloading the generated scene.
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, soundscape.ToggleMute);
            soundscape.soundLabel = TextAt("Sound on  [M]", soundButton, new Vector2(10, -3), new Vector2(104, 20), 14, SoftInk);
            var quest = Panel("Spring objective", canvas.transform, new Vector2(20, -118), new Vector2(286, 54), new Vector2(0, 1));
            demo.questLabel = TextAt("", quest, new Vector2(12, -6), new Vector2(264, 44), 15, Ink);
            var size = Panel("Size card", canvas.transform, new Vector2(-20, -18), new Vector2(262, 88), new Vector2(1, 1));
            TextAt("YOUR HEIGHT", size, new Vector2(14, -9), new Vector2(150, 20), 13, SoftInk).fontStyle = FontStyle.Bold;
            demo.heightLabel = TextAt("1.70 m", size, new Vector2(160, -5), new Vector2(90, 26), 19, Ink);
            demo.heightLabel.alignment = TextAnchor.UpperRight;
            TextAt("Tiny", size, new Vector2(14, -65), new Vector2(80, 18), 13, SoftInk);
            TextAt("Human", size, new Vector2(190, -65), new Vector2(60, 18), 13, SoftInk).alignment = TextAnchor.UpperRight;
            var sliderObj = UIObject("Continuous size slider", size, new Vector2(16, -34), new Vector2(230, 28));
            var slider = sliderObj.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            var track = UIObject("Track", sliderObj, new Vector2(0, -11), new Vector2(230, 6));
            var trackImage = track.gameObject.AddComponent<Image>(); trackImage.color = Hex("9C8160");
            trackImage.sprite = BuiltinSprite("UI/Skin/UISprite.psd"); trackImage.type = Image.Type.Sliced; trackImage.pixelsPerUnitMultiplier = 3;
            var handleArea = new GameObject("Handle area", typeof(RectTransform)).GetComponent<RectTransform>(); handleArea.SetParent(sliderObj, false);
            handleArea.anchorMin = Vector2.zero; handleArea.anchorMax = Vector2.one; handleArea.offsetMin = new Vector2(9, 0); handleArea.offsetMax = new Vector2(-9, 0);
            // A tiny acorn handle: a round gold nut under a brown cap with a stalk.
            var handle = new GameObject("Acorn handle", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); handle.SetParent(handleArea, false);
            handle.sizeDelta = new Vector2(20, 2); // the slider stretches it to the 28px track height
            var nut = handle.GetComponent<Image>(); nut.sprite = BuiltinSprite("UI/Skin/Knob.psd"); nut.color = Hex("E0A84E");
            var cap = UIObject("Acorn cap", handle, new Vector2(-2, 1), new Vector2(24, 11));
            var capImage = cap.gameObject.AddComponent<Image>(); capImage.color = Hex("6E4A2E"); capImage.raycastTarget = false;
            capImage.sprite = BuiltinSprite("UI/Skin/UISprite.psd"); capImage.type = Image.Type.Sliced; capImage.pixelsPerUnitMultiplier = 2;
            var stalk = UIObject("Acorn stalk", handle, new Vector2(8.5f, 6), new Vector2(3, 6));
            var stalkImage = stalk.gameObject.AddComponent<Image>(); stalkImage.color = Hex("6E4A2E"); stalkImage.raycastTarget = false;
            slider.handleRect = handle; slider.targetGraphic = nut; slider.navigation = new Navigation { mode = Navigation.Mode.None };
            // The entire 28px track is clickable, in addition to the handle.
            var hit = sliderObj.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0);
            demo.sizeSlider = slider;
            var bottom = Panel("Controls and prompt", canvas.transform, new Vector2(0, 14), new Vector2(640, 52), new Vector2(.5f, 0));
            bottom.GetComponent<Image>().color = WithAlpha(Parchment, .72f);
            demo.promptLabel = TextAt("", bottom, new Vector2(14, -5), new Vector2(612, 24), 17, Ink); demo.promptLabel.alignment = TextAnchor.MiddleCenter;
            var controls = TextAt("WASD  Walk   ·   Right-drag  Look   ·   E  Interact   ·   R  Reset", bottom, new Vector2(14, -29), new Vector2(612, 18), 13, SoftInk);
            controls.alignment = TextAnchor.MiddleCenter;
            var message = Panel("Story message", canvas.transform, new Vector2(0, 76), new Vector2(620, 62), new Vector2(.5f, 0));
            message.GetComponent<Image>().color = WithAlpha(Parchment, .9f);
            demo.messageLabel = TextAt("", message, new Vector2(16, -6), new Vector2(588, 50), 19, Ink); demo.messageLabel.alignment = TextAnchor.MiddleCenter;
            // Empty dialogue should leave the landscape visible.
            var visibility = message.gameObject.AddComponent<UnderfootMessagePanel>(); visibility.message = demo.messageLabel;
            var choices = UIObject("Pip route choices", canvas.transform, new Vector2(0, 148), new Vector2(580, 44));
            choices.anchorMin = choices.anchorMax = choices.pivot = new Vector2(.5f, 0);
            RouteButton(choices, "1  Quiet rootway", new Vector2(0, 0), demo.gnome.ChooseRootway);
            RouteButton(choices, "2  Squirrel clearing", new Vector2(300, 0), demo.gnome.ChooseSquirrel);
            demo.gnome.choicePanel = choices.gameObject; choices.gameObject.SetActive(false);
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        static void RouteButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var rect = Panel(label, parent, position, new Vector2(280, 44), new Vector2(0, 1));
            rect.GetComponent<Image>().color = Hex("E9CF95");
            rect.GetComponent<Outline>().effectColor = WithAlpha("7A4E2C", .9f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            var colours = button.colors; colours.highlightedColor = Hex("FFF1CC"); colours.pressedColor = Hex("D9B672"); button.colors = colours;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
            var text = TextAt(label, rect, new Vector2(8, -4), new Vector2(264, 36), 18, Ink);
            text.alignment = TextAnchor.MiddleCenter; text.fontStyle = FontStyle.Bold;
        }

        static RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var rect = UIObject(name, parent, position, size); rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            var image = rect.gameObject.AddComponent<Image>(); image.color = WithAlpha(Parchment, .86f);
            image.sprite = BuiltinSprite("UI/Skin/UISprite.psd"); image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1.2f;
            var border = rect.gameObject.AddComponent<Outline>(); border.effectColor = WithAlpha("7A5A3A", .55f); border.effectDistance = new Vector2(1.5f, -1.5f);
            return rect;
        }
        static Sprite BuiltinSprite(string path) => AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
        static Color WithAlpha(string hex, float alpha) { var c = Hex(hex); c.a = alpha; return c; }
        static RectTransform UIObject(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static Text TextAt(string text, Transform parent, Vector2 position, Vector2 size, int fontSize, string colour)
        {
            var obj = UIObject(text.Length > 0 ? text : "Context text", parent, position, size);
            var label = obj.gameObject.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text; label.fontSize = fontSize; label.color = Hex(colour); label.raycastTarget = false;
            return label;
        }
        static Transform Group(string name, Transform parent = null)
        {
            var obj = new GameObject(name).transform; if (parent) obj.SetParent(parent, false); return obj;
        }
        static Transform Shape(string name, PrimitiveType primitive, Transform parent, Vector3 p, Vector3 scale, string material, bool collision = false)
        {
            var obj = GameObject.CreatePrimitive(primitive); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = p; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = palette[material];
            if (!collision) Object.DestroyImmediate(obj.GetComponent<Collider>());
            else if (primitive == PrimitiveType.Cylinder)
            {
                // A flattened primitive cylinder's default capsule expands to its largest scale axis.
                // Static mesh contacts follow the authored ground discs and roots exactly.
                Object.DestroyImmediate(obj.GetComponent<Collider>());
                obj.AddComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
            }
            return obj.transform;
        }
        static Transform MeshShape(string name, Mesh mesh, Transform parent, Vector3 p, Vector3 scale, string material)
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)).transform; obj.SetParent(parent, false);
            obj.localPosition = p; obj.localScale = scale; obj.GetComponent<MeshFilter>().sharedMesh = mesh; obj.GetComponent<Renderer>().sharedMaterial = palette[material]; return obj;
        }
        static Transform Tube(string name, Transform parent, Vector3 start, Vector3 end, float radius, string material, bool collision = false)
        {
            var obj = Shape(name, PrimitiveType.Cylinder, parent, (start + end) / 2, new Vector3(radius * 2, Vector3.Distance(start, end) / 2, radius * 2), material, collision);
            obj.localRotation = Quaternion.FromToRotation(Vector3.up, end - start); return obj;
        }
        static void PointLight(string name, Transform parent, Vector3 p, float intensity, float range)
        {
            var obj = new GameObject(name, typeof(Light)).transform; obj.SetParent(parent, false); obj.localPosition = p;
            var light = obj.GetComponent<Light>(); light.type = LightType.Point; light.color = Hex("FFBD68"); light.intensity = intensity; light.range = range;
            light.renderMode = LightRenderMode.ForcePixel;
        }
        static void Colour(string name, string hex, bool emission = false)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
            material.color = Hex(hex); material.SetFloat("_Glossiness", .16f);
            if (emission) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", Hex(hex) * .8f); }
            EditorUtility.SetDirty(material); palette[name] = material;
        }
        static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var colour); return colour; }
        static GameObject SavePrefab(Transform root, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Root + "/Prefabs/" + name + ".prefab"); Object.DestroyImmediate(root.gameObject); return prefab;
        }
        static Mesh SaveMesh(string name, Mesh generated)
        {
            string path = Root + "/Meshes/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path); generated.name = name;
            if (existing) { EditorUtility.CopySerialized(generated, existing); Object.DestroyImmediate(generated); return existing; }
            AssetDatabase.CreateAsset(generated, path); return generated;
        }
        static Mesh Lathe(Vector2[] profile, int segments)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int j = 0; j < profile.Length; j++)
                for (int i = 0; i <= segments; i++)
                {
                    float a = i * Mathf.PI * 2 / segments; vertices.Add(new Vector3(Mathf.Cos(a) * profile[j].x, profile[j].y, Mathf.Sin(a) * profile[j].x));
                    if (j == 0 || i == 0) continue;
                    int n = vertices.Count - 1, stride = segments + 1;
                    triangles.AddRange(new[] { n - stride - 1, n - 1, n, n - stride - 1, n, n - stride });
                }
            var mesh = new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Leaf()
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i <= 12; i++)
            {
                float z = -.58f + i / 12f * 1.18f;
                float w = Mathf.Sin(i / 12f * Mathf.PI) * (.38f + (i % 2) * .055f);
                v.Add(new Vector3(-w, .055f + Mathf.Abs(z) * .07f, z)); v.Add(new Vector3(0, .019f, z)); v.Add(new Vector3(w, .055f + Mathf.Abs(z) * .07f, z));
                if (i == 0) continue;
                int n = i * 3;
                foreach (int s in new[] { 0, 1 })
                {
                    int a = n - 3 + s, b = n + s, c = n + s + 1, d = n - 2 + s;
                    t.AddRange(new[] { a, b, c, a, c, d });
                }
            }
            int frontCount = v.Count;
            v.AddRange(v.ToArray());
            int triangleCount = t.Count;
            for (int i = 0; i < triangleCount; i += 3) t.AddRange(new[] { t[i + 2] + frontCount, t[i + 1] + frontCount, t[i] + frontCount });
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh Footpath()
        {
            var v = new List<Vector3>(); var t = new List<int>();
            // Ends beneath the welcome mat, so it never shows above the home's floorboards.
            for (int i = 0; i < 14; i++)
            {
                float z = -.9f + i * .16f, x = Mathf.Sin(i * .7f) * .025f;
                float w = (.12f + Mathf.Sin(i * .6f) * .018f) * Mathf.Clamp01(.35f + Mathf.Min(i, 13 - i) * .33f); // soft, tapered ends
                v.Add(new Vector3(x - w, .0012f, z)); v.Add(new Vector3(x + w, .0012f, z));
                if (i > 0) { int n = i * 2; t.AddRange(new[] { n - 2, n, n + 1, n - 2, n + 1, n - 1 }); }
            }
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        static Mesh RootMesh(Vector3[] points, float[] radii = null)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            if (radii == null) radii = new[] { .24f, .18f, .13f, .075f, .008f };
            const int segments = 12;
            for (int j = 0; j < points.Length; j++)
            {
                Vector3 tangent = points[Mathf.Min(j + 1, points.Length - 1)] - points[Mathf.Max(0, j - 1)];
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, tangent.normalized);
                for (int i = 0; i <= segments; i++)
                {
                    float a = i * Mathf.PI * 2 / segments;
                    v.Add(points[j] + rotation * new Vector3(Mathf.Cos(a) * radii[j], 0, Mathf.Sin(a) * radii[j]));
                    if (j == 0 || i == 0) continue;
                    int n = v.Count - 1, stride = segments + 1;
                    t.AddRange(new[] { n - stride - 1, n - 1, n, n - stride - 1, n, n - stride });
                }
            }
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
