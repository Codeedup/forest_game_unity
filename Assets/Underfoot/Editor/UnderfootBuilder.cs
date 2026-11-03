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
    public static class UnderfootBuilder
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
            palette.Clear();
            Colour("Moss", "65774A"); Colour("Sage", "8FA66D"); Colour("DarkMoss", "435C3B");
            Colour("Earth", "766448"); Colour("Path", "B49D73"); Colour("Bark", "70503C"); Colour("BarkLight", "94704D");
            Colour("Plum", "735574"); Colour("PlumLight", "9B7895"); Colour("FaceShadow", "39313B");
            Colour("Skin", "DBB392"); Colour("Boot", "493B35"); Colour("Cream", "E7D9B7");
            Colour("Hat", "B3634C"); Colour("Coat", "59807B"); Colour("Gold", "DDAE53");
            Colour("Leaf", "B5964E"); Colour("LeafVein", "D9BE72"); Colour("Stone", "929386");
            Colour("Wood", "A97A4C"); Colour("Interior", "C39B6C"); Colour("Rug", "986B63");
            Colour("Amber", "FFCE79", true); Colour("Pollen", "F5DDA1", true);
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
            camera.fieldOfView = 55; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Hex("B6C8B2");
            camera.allowHDR = true;
            var orbit = camObj.GetComponent<UnderfootCamera>(); orbit.player = player; player.orbit = orbit;
            orbit.Frame();

            var gnomeObject = Group("Pip - seedkeeper", world);
            gnomeObject.position = new Vector3(-.15f, 0, .12f);
            var gnomeBody = Character(gnomeObject, true, out _, out var gnomeArms, out var hand);
            gnomeBody.localScale = Vector3.one * .165f;
            gnomeBody.localRotation = Quaternion.Euler(0, 180, 0);
            var gnomeCollider = gnomeObject.gameObject.AddComponent<CapsuleCollider>();
            gnomeCollider.height = .16f; gnomeCollider.radius = .035f; gnomeCollider.center = Vector3.up * .08f;
            var gnome = gnomeObject.gameObject.AddComponent<UnderfootGnome>();
            gnome.player = player; gnome.model = gnomeBody; gnome.throwingArm = gnomeArms[1]; gnome.hand = hand;
            gnome.seedPrefab = SeedPrefab();
            gnome.landingPoints = new[] { new Vector3(.018f, .008f, .38f), new Vector3(-.018f, .008f, .68f), new Vector3(.018f, .008f, .98f) };

            var systems = new GameObject("Underfoot demo");
            var details = systems.AddComponent<UnderfootDetails>();
            details.player = player; details.leaf = leaf;
            details.mossPrefab = MossPrefab(); details.tinyMushroomPrefab = TinyPrefab(); details.leafVeinsPrefab = VeinPrefab();
            var demo = systems.AddComponent<UnderfootDemo>();
            demo.player = player; demo.details = details; demo.gnome = gnome; demo.home = home;
            UI(demo);
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
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("B5BCA2");
            RenderSettings.ambientEquatorColor = Hex("A6AD8C");
            RenderSettings.ambientGroundColor = Hex("756953");
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("B6C8B2"); RenderSettings.fogStartDistance = 8; RenderSettings.fogEndDistance = 24;
            var sun = new GameObject("Late afternoon sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = Hex("FFE0AA"); sun.intensity = 1.13f;
            sun.shadows = LightShadows.Soft; sun.shadowBias = .015f; sun.shadowNormalBias = .025f;
            sun.transform.rotation = Quaternion.Euler(48, -32, 0);
        }

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
            Shape("Amber round window", PrimitiveType.Cylinder, home, new Vector3(.15f, .13f, -.034f), new Vector3(.054f, .004f, .054f), "Amber").localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Window cross", PrimitiveType.Cube, home, new Vector3(.15f, .13f, -.04f), new Vector3(.004f, .054f, .004f), "Wood");
            Shape("Window cross", PrimitiveType.Cube, home, new Vector3(.15f, .13f, -.041f), new Vector3(.054f, .004f, .004f), "Wood");
            Shape("Little table", PrimitiveType.Cylinder, home, new Vector3(-.085f, .064f, .28f), new Vector3(.085f, .009f, .085f), "Wood");
            Shape("Table leg", PrimitiveType.Cylinder, home, new Vector3(-.085f, .031f, .28f), new Vector3(.016f, .031f, .016f), "Bark");
            Shape("Tea cup", PrimitiveType.Cylinder, home, new Vector3(-.085f, .083f, .28f), new Vector3(.019f, .01f, .019f), "Cream");
            Shape("Tea surface", PrimitiveType.Cylinder, home, new Vector3(-.085f, .094f, .28f), new Vector3(.014f, .001f, .014f), "Bark");
            Shape("Round cushion", PrimitiveType.Sphere, home, new Vector3(.075f, .018f, .29f), new Vector3(.09f, .035f, .09f), "Plum");
            Tube("Lantern hook", home, new Vector3(-.19f, .15f, -.09f), new Vector3(-.19f, .22f, -.09f), .006f, "Bark");
            Shape("Lantern glow", PrimitiveType.Cylinder, home, new Vector3(-.19f, .16f, -.09f), new Vector3(.035f, .024f, .035f), "Amber");
            MeshShape("Lantern cap", cap, home, new Vector3(-.19f, .185f, -.09f), new Vector3(.05f, .04f, .05f), "Bark");
            PointLight("Lantern - warm welcome", home, new Vector3(-.16f, .14f, -.06f), 1.4f, .7f);
            PointLight("Interior hearth glow", home, new Vector3(0, .16f, .28f), 1.1f, .48f);
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
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2.4f;
                Shape("Guardian leaf crown", PrimitiveType.Sphere, old, new Vector3(Mathf.Sin(a) * 1.3f, 4.5f + (i % 3) * .43f, 2.2f + Mathf.Cos(a) * 1.15f), new Vector3(2.7f, 1.7f, 2.6f), i % 2 == 0 ? "Sage" : "DarkMoss");
            }
            for (int i = 0; i < 14; i++)
            {
                float a = i * Mathf.PI * 2 / 14;
                Vector3 p = new Vector3(Mathf.Cos(a) * 4.6f, 0, Mathf.Sin(a) * 4.6f);
                float h = 3 + (i % 3) * .6f;
                Shape("Woodland trunk", PrimitiveType.Cylinder, parent, p + Vector3.up * h / 2, new Vector3(.35f, h / 2, .42f), "Bark", true);
                Shape("Woodland crown", PrimitiveType.Sphere, parent, p + Vector3.up * h, new Vector3(2.8f, 2.1f, 2.7f), i % 2 == 0 ? "Sage" : "DarkMoss");
            }
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
                else
                    for (int j = 0; j < 3; j++)
                    {
                        var fern = MeshShape("Fern leaf", leafMesh, parent, p + new Vector3(j * .06f, .02f, 0), Vector3.one * .27f, i % 2 == 0 ? "Sage" : "DarkMoss");
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
                Shape("Round nose", PrimitiveType.Sphere, body, new Vector3(0, .78f, .18f), new Vector3(.105f, .09f, .1f), "Skin");
                for (int side = -1; side <= 1; side += 2)
                    Shape("Kind dark eye", PrimitiveType.Sphere, body, new Vector3(side * .062f, .81f, .146f), Vector3.one * .027f, "FaceShadow");
            }
            else
            {
                Shape("Deep hood", PrimitiveType.Sphere, body, new Vector3(0, .805f, -.006f), new Vector3(.38f, .38f, .34f), "Plum");
                MeshShape("Drooping hood fold", hoodFold, body, Vector3.zero, Vector3.one, "Plum");
                Shape("Shadow within hood", PrimitiveType.Sphere, body, new Vector3(0, .79f, .127f), new Vector3(.234f, .248f, .04f), "FaceShadow");
                Shape("Small visible face", PrimitiveType.Sphere, body, new Vector3(0, .778f, .149f), new Vector3(.138f, .13f, .032f), "Skin");
                Shape("Cloak clasp", PrimitiveType.Sphere, body, new Vector3(0, .64f, .17f), Vector3.one * .047f, "Gold");
                Shape("Satchel", PrimitiveType.Sphere, body, new Vector3(.19f, .34f, -.04f), new Vector3(.18f, .22f, .12f), "Wood");
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
            }
            return body;
        }

        static void Mushroom(Transform parent, Vector3 position, float height, string colour)
        {
            var mushroom = Group("Woodland mushroom", parent); mushroom.localPosition = position;
            Shape("Cream stem", PrimitiveType.Capsule, mushroom, new Vector3(0, height * .4f, 0), new Vector3(height * .21f, height * .4f, height * .21f), "Cream");
            MeshShape("Scalloped cap", cap, mushroom, new Vector3(0, height * .67f, 0), new Vector3(height, height * .55f, height), colour);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3;
                Shape("Cream cap spot", PrimitiveType.Sphere, mushroom, new Vector3(Mathf.Cos(a) * height * .27f, height * .91f, Mathf.Sin(a) * height * .27f), new Vector3(height * .12f, height * .025f, height * .09f), "Cream");
            }
            var collider = mushroom.gameObject.AddComponent<CapsuleCollider>();
            collider.height = height; collider.radius = height * .19f; collider.center = Vector3.up * height * .5f;
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
                float width = Mathf.Sin((i + 1) / 8f * Mathf.PI) * .37f;
                foreach (int side in new[] { -1, 1 })
                    Tube("Raised vein", root, new Vector3(0, .035f, z), new Vector3(side * width, .065f, z + .15f), .003f, "LeafVein");
            }
            return SavePrefab(root, "LeafVeins");
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

        static void UI(UnderfootDemo demo)
        {
            var canvas = new GameObject("Underfoot interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900); scaler.matchWidthOrHeight = .5f;
            var top = Panel("Title card", canvas.transform, new Vector2(24, -22), new Vector2(390, 105), new Vector2(0, 1));
            TextAt("UNDERFOOT", top, new Vector2(20, -10), new Vector2(350, 44), 32, "F0DEB0");
            TextAt("A small wanderer. A world worth noticing.", top, new Vector2(21, -57), new Vector2(350, 30), 16, "D0D6B7");
            var size = Panel("Size card", canvas.transform, new Vector2(-24, -22), new Vector2(360, 137), new Vector2(1, 1));
            TextAt("YOUR HEIGHT", size, new Vector2(20, -14), new Vector2(210, 26), 15, "D0D6B7");
            demo.heightLabel = TextAt("1.70 m", size, new Vector2(235, -10), new Vector2(110, 32), 24, "F0DEB0");
            TextAt("Tiny", size, new Vector2(20, -96), new Vector2(120, 25), 15, "D0D6B7");
            TextAt("Human", size, new Vector2(282, -96), new Vector2(70, 25), 15, "D0D6B7");
            var sliderObj = UIObject("Continuous size slider", size, new Vector2(26, -60), new Vector2(307, 26));
            var slider = sliderObj.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            var track = UIObject("Track", sliderObj, new Vector2(0, -9), new Vector2(307, 7)); track.gameObject.AddComponent<Image>().color = Hex("69785D");
            var handleArea = new GameObject("Handle area", typeof(RectTransform)).GetComponent<RectTransform>(); handleArea.SetParent(sliderObj, false);
            handleArea.anchorMin = Vector2.zero; handleArea.anchorMax = Vector2.one; handleArea.offsetMin = new Vector2(8, 0); handleArea.offsetMax = new Vector2(-8, 0);
            var handle = new GameObject("Amber handle", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); handle.SetParent(handleArea, false);
            handle.sizeDelta = new Vector2(19, 27); handle.GetComponent<Image>().color = Hex("E9BE70");
            slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>(); slider.navigation = new Navigation { mode = Navigation.Mode.None };
            // The entire 26px track is clickable, in addition to the handle.
            var hit = sliderObj.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0);
            demo.sizeSlider = slider;
            var bottom = Panel("Controls and prompt", canvas.transform, new Vector2(0, 22), new Vector2(760, 86), new Vector2(.5f, 0));
            demo.promptLabel = TextAt("", bottom, new Vector2(18, -12), new Vector2(725, 30), 19, "F0DEB0"); demo.promptLabel.alignment = TextAnchor.MiddleCenter;
            var controls = TextAt("WASD  Walk     |     Right mouse drag  Look     |     E  Talk     |     R  Reset", bottom, new Vector2(18, -46), new Vector2(725, 25), 16, "D0D6B7"); controls.alignment = TextAnchor.MiddleCenter;
            var message = Panel("Story message", canvas.transform, new Vector2(0, 123), new Vector2(650, 76), new Vector2(.5f, 0));
            message.GetComponent<Image>().color = new Color(.12f, .16f, .11f, .72f);
            demo.messageLabel = TextAt("", message, new Vector2(15, -7), new Vector2(620, 62), 21, "F3DBA4"); demo.messageLabel.alignment = TextAnchor.MiddleCenter;
            // Empty dialogue should leave the landscape visible.
            var visibility = message.gameObject.AddComponent<UnderfootMessagePanel>(); visibility.message = demo.messageLabel;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        static RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var rect = UIObject(name, parent, position, size); rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.12f, .16f, .11f, .88f);
            return rect;
        }
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
            for (int i = 0; i < 16; i++)
            {
                float z = -.9f + i * .16f, x = Mathf.Sin(i * .7f) * .025f;
                float w = .12f + Mathf.Sin(i * .6f) * .018f;
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
