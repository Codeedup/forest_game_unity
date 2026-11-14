using System.Collections.Generic;
using UnityEngine;

namespace Underfoot.Editor
{
    // The root home interior: decoration only, inside the unchanged walls, roof and door colliders.
    public static partial class UnderfootBuilder
    {
        // Unnamed layer the low sun skips, so it cannot leak through the walls in coarse shadow-map steps.
        public const int ShelteredLayer = 8;
        const float InnerHalfWidth = .139f, VaultSpring = .085f, VaultApex = .216f, VaultFront = .02f, VaultBack = .39f;

        static void HomeInterior(Transform home)
        {
            var room = Group("Cosy interior - vault, hearth and tea (decoration)", home);
            MeshShape("Curved root-wood vault", SaveMesh("RootVault", Vault()), room, Vector3.zero, Vector3.one, "RootWood");
            foreach (float z in new[] { .06f, .17f, .28f, .37f })
            {
                var rib = new Vector3[9]; var radii = new float[9];
                for (int i = 0; i < 9; i++)
                {
                    rib[i] = VaultPoint(Mathf.PI * i / 8f, z, .992f);
                    radii[i] = Mathf.Lerp(.011f, .006f, Mathf.Sin(Mathf.PI * i / 8f));
                }
                MeshShape("Arched root rib", RootMesh(rib, radii), room, Vector3.zero, Vector3.one, "BarkLight");
            }

            // Round rug in two warm rings.
            var disc = SaveMesh("RoundDisc", Blob(24, 0, 0));
            MeshShape("Round rug border", disc, room, new Vector3(0, .0014f, .17f), new Vector3(.2f, 1, .17f), "Hat");
            MeshShape("Round rug", disc, room, new Vector3(0, .0017f, .17f), new Vector3(.17f, 1, .14f), "Rug");
            MeshShape("Round rug centre", disc, room, new Vector3(0, .002f, .17f), new Vector3(.08f, 1, .065f), "Gold");

            // A tiny stone hearth against the back wall, with glowing embers and two logs.
            for (int i = 0; i < 9; i++)
            {
                float a = Mathf.PI * i / 8f;
                Shape("Hearth stone", PrimitiveType.Sphere, room, new Vector3(.05f + Mathf.Cos(a) * .042f, .006f + Mathf.Sin(a) * .05f, .372f),
                    new Vector3(.022f, .018f, .02f), i % 2 == 0 ? "Stone" : "BarkCrease");
            }
            Shape("Hearth shadow", PrimitiveType.Sphere, room, new Vector3(.05f, .018f, .385f), new Vector3(.07f, .07f, .01f), "FaceShadow");
            Shape("Hearth embers", PrimitiveType.Sphere, room, new Vector3(.05f, .006f, .372f), new Vector3(.05f, .012f, .03f), "Ember");
            Tube("Hearth log", room, new Vector3(.028f, .011f, .366f), new Vector3(.07f, .015f, .374f), .0055f, "Bark");
            Tube("Hearth log", room, new Vector3(.032f, .016f, .378f), new Vector3(.072f, .011f, .364f), .005f, "BarkLight");
            var glow = Shape("Hearth glow", PrimitiveType.Quad, room, new Vector3(.05f, .028f, .36f), new Vector3(.11f, .085f, 1), "GlowHearth");
            glow.localRotation = Quaternion.Euler(0, 180, 0);
            glow.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Teapot and two cups on the little table: Pip promised tea.
            float top = .0735f; Vector3 table = new Vector3(-.085f, top, .28f);
            Shape("Round teapot", PrimitiveType.Sphere, room, table + new Vector3(.005f, .013f, 0), new Vector3(.028f, .024f, .028f), "RoseCap");
            Shape("Teapot lid", PrimitiveType.Sphere, room, table + new Vector3(.005f, .025f, 0), new Vector3(.014f, .006f, .014f), "Cream");
            Shape("Teapot knob", PrimitiveType.Sphere, room, table + new Vector3(.005f, .029f, 0), Vector3.one * .004f, "Gold");
            Tube("Teapot spout", room, table + new Vector3(.016f, .012f, 0), table + new Vector3(.027f, .022f, 0), .0025f, "RoseCap");
            Tube("Teapot handle", room, table + new Vector3(-.01f, .02f, 0), table + new Vector3(-.016f, .008f, 0), .002f, "RoseCap");
            foreach (var cup in new[] { new Vector3(-.03f, 0, .022f), new Vector3(-.02f, 0, -.026f) })
            {
                Shape("Tea cup", PrimitiveType.Cylinder, room, table + cup + Vector3.up * .005f, new Vector3(.011f, .005f, .011f), "Cream");
                Shape("Tea", PrimitiveType.Cylinder, room, table + cup + Vector3.up * .0101f, new Vector3(.0085f, .0003f, .0085f), "Bark");
            }

            // Shelf of books and jars on the left wall, above the table.
            Shape("Wall shelf", PrimitiveType.Cube, room, new Vector3(-.128f, .135f, .24f), new Vector3(.022f, .004f, .13f), "Wood");
            string[] spines = { "Hat", "Coat", "Plum", "Gold", "RoseCap", "Coat" };
            for (int i = 0; i < 6; i++)
            {
                float h = .022f + (i % 3) * .004f;
                var book = Shape("Little book", PrimitiveType.Cube, room, new Vector3(-.128f, .137f + h / 2, .19f + i * .0085f), new Vector3(.016f, h, .007f), spines[i]);
                if (i == 5) book.localRotation = Quaternion.Euler(14, 0, 0);
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 jar = new Vector3(-.128f, .137f, .262f + i * .016f);
                Shape("Glass jar", PrimitiveType.Cylinder, room, jar + Vector3.up * .01f, new Vector3(.012f, .01f, .012f), i == 1 ? "Gold" : "Dew");
                Shape("Jar lid", PrimitiveType.Cylinder, room, jar + Vector3.up * .021f, new Vector3(.013f, .0015f, .013f), "BarkLight");
            }

            // Bundles of dried herbs hanging from the vault.
            for (int i = 0; i < 4; i++)
            {
                float side = i < 2 ? -1 : 1;
                Vector3 hook = new Vector3(side * .085f, .18f, .25f + (i % 2) * .08f);
                Tube("Herb string", room, hook + Vector3.up * .01f, hook, .0006f, "Cream");
                Shape("Herb tie", PrimitiveType.Sphere, room, hook, new Vector3(.005f, .003f, .005f), "Hat");
                for (int k = 0; k < 3; k++)
                {
                    var sprig = MeshShape("Dried herb sprig", cone, room, hook + new Vector3((k - 1) * .0025f, 0, (k % 2) * .002f),
                        new Vector3(.0055f, .02f + k * .003f, .0055f), i % 2 == 0 ? (k == 1 ? "LeafGold" : "Sage") : (k == 1 ? "Lichen" : "LeafAmber"));
                    sprig.localRotation = Quaternion.Euler(180, 0, (k - 1) * 12);
                }
            }

            // A tiny round window in the back wall with a mossy sill, glowing with the dusk outside.
            Shape("Round window frame", PrimitiveType.Cylinder, room, new Vector3(-.065f, .135f, .386f), new Vector3(.05f, .003f, .05f), "Bark")
                .localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Dusk window glass", PrimitiveType.Cylinder, room, new Vector3(-.065f, .135f, .382f), new Vector3(.038f, .002f, .038f), "DuskGlass")
                .localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Window cross", PrimitiveType.Cube, room, new Vector3(-.065f, .135f, .38f), new Vector3(.003f, .038f, .002f), "Wood");
            Shape("Window cross", PrimitiveType.Cube, room, new Vector3(-.065f, .135f, .38f), new Vector3(.038f, .003f, .002f), "Wood");
            Shape("Moss window sill", PrimitiveType.Sphere, room, new Vector3(-.065f, .109f, .382f), new Vector3(.055f, .008f, .016f), "DarkMoss");
            Shape("Sill flower", PrimitiveType.Sphere, room, new Vector3(-.05f, .114f, .38f), Vector3.one * .005f, "ForgetMeNot");
            CombineDecoration(room, "HomeInterior");
            foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.gameObject.layer = ShelteredLayer;
                if (renderer.sharedMaterial == palette["GlowHearth"]) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach (Transform piece in home)
                if (piece.name.StartsWith("Honey-coloured floorboard")) piece.gameObject.layer = ShelteredLayer;
        }

        static Vector3 VaultPoint(float angle, float z, float inset = 1)
        {
            // angle 0 = right springing, PI = left springing; plus a straight wall below.
            return new Vector3(Mathf.Cos(angle) * InnerHalfWidth * inset, VaultSpring + Mathf.Sin(angle) * (VaultApex - VaultSpring) * inset, z);
        }

        static Mesh Vault()
        {
            var profile = new List<Vector3> { new Vector3(InnerHalfWidth, 0, 0) };
            for (int i = 0; i <= 12; i++) profile.Add(VaultPoint(Mathf.PI * i / 12f, 0));
            profile.Add(new Vector3(-InnerHalfWidth, 0, 0));
            var v = new List<Vector3>(); var t = new List<int>();
            const int rings = 8;
            for (int r = 0; r <= rings; r++)
            {
                float z = Mathf.Lerp(VaultFront, VaultBack, r / (float)rings);
                foreach (var p in profile)
                {
                    // A gentle organic wobble, only above head height so walking clearance is untouched.
                    float wobble = 1 - .025f * (1 + Mathf.Sin(z * 55 + p.y * 70)) * Mathf.Clamp01((p.y - VaultSpring) / .05f);
                    v.Add(new Vector3(p.x * wobble, p.y, z));
                }
                if (r == 0) continue;
                int n = profile.Count, a = (r - 1) * n, b = r * n;
                for (int i = 0; i < n - 1; i++) t.AddRange(new[] { a + i, b + i, b + i + 1, a + i, b + i + 1, a + i + 1 });
            }
            // Close the back with a cap of the same shape, on its own vertices so it shades flat.
            int centre = v.Count; v.Add(new Vector3(0, VaultSpring * .8f, VaultBack));
            int last = v.Count;
            foreach (var p in profile) v.Add(new Vector3(p.x, p.y, VaultBack));
            for (int i = 0; i < profile.Count - 1; i++) t.AddRange(new[] { centre, last + i, last + i + 1 });
            t.AddRange(new[] { centre, last + profile.Count - 1, last });
            int frontCount = v.Count;
            v.AddRange(v.ToArray());
            int triangles = t.Count;
            for (int i = 0; i < triangles; i += 3) t.AddRange(new[] { t[i + 2] + frontCount, t[i + 1] + frontCount, t[i] + frontCount });
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
