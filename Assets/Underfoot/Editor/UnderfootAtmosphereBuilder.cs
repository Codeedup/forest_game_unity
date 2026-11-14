using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Underfoot.Editor
{
    // Decoration only: atmosphere, light and set dressing. Nothing here has a collider or touches gameplay geometry.
    public static partial class UnderfootBuilder
    {
        static Material Glow(string name, string hex, float intensity, float shape)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var shader = Shader.Find("Underfoot/Soft Glow");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            var colour = Hex(hex) * intensity; colour.a = 1;
            material.SetColor("_Color", colour); material.SetFloat("_Shape", shape);
            EditorUtility.SetDirty(material); return material;
        }

        static void LivingLight(Transform world)
        {
            var light = Group("Living light - fireflies, motes and canopy shafts", world);
            // Floating dust motes across the clearing: faint, slow and bounded.
            Particles("Floating golden motes", light, new Vector3(0, 1.1f, .2f), 4.2f, 70, new Vector2(.012f, .028f),
                new Vector2(9, 15), Glow("GlowMote", "FFE2B0", 2.4f, 0), .06f, false);
            // Fireflies gather around the root home and blink slowly.
            Particles("Fireflies by the root home", light, new Vector3(0, .3f, .85f), .75f, 50, new Vector2(.008f, .017f),
                new Vector2(5, 9), Glow("GlowFirefly", "E9F08A", 8f, 0), .12f, true);
            Particles("Fireflies under the trees", light, new Vector3(0, .8f, .4f), 3.6f, 30, new Vector2(.02f, .035f),
                new Vector2(5, 9), Glow("GlowFirefly", "E9F08A", 8f, 0), .2f, true);

            // Three soft beams slanting down through the canopy, combined into one draw call.
            var shaftMaterial = Glow("GlowShaft", "FFC27A", .085f, 1);
            Vector3 towardSun = Quaternion.Euler(30, 48, 0) * Vector3.back;
            var parts = new List<CombineInstance>();
            var unit = CrossedQuad(.42f, 1);
            foreach (var shaft in new[] { new Vector4(-1.4f, 1.1f, .75f, 6.5f), new Vector4(1.5f, 1.2f, .55f, 7), new Vector4(.9f, -1.5f, .6f, 6) })
            {
                var ground = new Vector3(shaft.x, -.15f, shaft.y);
                var matrix = Matrix4x4.TRS(ground, Quaternion.FromToRotation(Vector3.up, towardSun), new Vector3(shaft.z, shaft.w, shaft.z));
                parts.Add(new CombineInstance { mesh = unit, transform = matrix });
            }
            var shafts = new Mesh(); shafts.CombineMeshes(parts.ToArray()); Object.DestroyImmediate(unit);
            var beams = new GameObject("Soft canopy light shafts", typeof(MeshFilter), typeof(MeshRenderer));
            beams.transform.SetParent(light, false);
            beams.GetComponent<MeshFilter>().sharedMesh = SaveMesh("LightShafts", shafts);
            var beamRenderer = beams.GetComponent<MeshRenderer>(); beamRenderer.sharedMaterial = shaftMaterial;
            beamRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; beamRenderer.receiveShadows = false;
        }

        // Two perpendicular tapered quads along +Y (wide at the ground end), UV.y runs along the beam.
        static Mesh CrossedQuad(float topWidth, float bottomWidth)
        {
            var v = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
            for (int q = 0; q < 2; q++)
            {
                Vector3 across = q == 0 ? Vector3.right : Vector3.forward;
                int b = v.Count;
                v.Add(-across * bottomWidth * .5f); v.Add(across * bottomWidth * .5f);
                v.Add(across * topWidth * .5f + Vector3.up); v.Add(-across * topWidth * .5f + Vector3.up);
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(1, 0)); uv.Add(new Vector2(1, 1)); uv.Add(new Vector2(0, 1));
                t.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
            }
            var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(t, 0); mesh.RecalculateBounds(); return mesh;
        }

        static void Particles(string name, Transform parent, Vector3 centre, float radius, int max, Vector2 size, Vector2 life,
            Material material, float drift, bool blink)
        {
            var obj = new GameObject(name, typeof(ParticleSystem)); obj.transform.SetParent(parent, false);
            obj.transform.localPosition = centre;
            var ps = obj.GetComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.prewarm = true; main.playOnAwake = true; main.maxParticles = max;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSpeed = 0; main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.gravityModifier = 0;
            var emission = ps.emission; emission.rateOverTime = max / ((life.x + life.y) * .5f);
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = radius;
            shape.scale = new Vector3(1, blink ? .45f : .35f, 1);
            var noise = ps.noise; noise.enabled = true; noise.strength = drift; noise.frequency = .35f; noise.scrollSpeed = .15f;
            noise.quality = ParticleSystemNoiseQuality.Low;
            var colour = ps.colorOverLifetime; colour.enabled = true;
            var gradient = new Gradient();
            var alpha = blink
                ? new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .12f), new GradientAlphaKey(.15f, .3f), new GradientAlphaKey(1, .48f), new GradientAlphaKey(.2f, .66f), new GradientAlphaKey(1, .82f), new GradientAlphaKey(0, 1) }
                : new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.8f, .3f), new GradientAlphaKey(.8f, .7f), new GradientAlphaKey(0, 1) };
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, alpha);
            colour.color = gradient;
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material; renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
            renderer.minParticleSize = 0; renderer.maxParticleSize = .5f;
        }

        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

        // A gently bent, tapering trunk with a flared base and a few surface roots.
        static void TaperedTrunk(Transform parent, Vector3 foot, float height, float radius, System.Random random, string material)
        {
            float lean = Range(random, 0, Mathf.PI * 2);
            Vector3 bend = new Vector3(Mathf.Cos(lean), 0, Mathf.Sin(lean)) * Range(random, .12f, .3f);
            var points = new Vector3[5]; var radii = new float[5];
            for (int i = 0; i < 5; i++)
            {
                float f = i / 4f;
                points[i] = foot + Vector3.up * (height * f - .05f) + bend * Mathf.Sin(f * Mathf.PI * .9f);
                radii[i] = radius * Mathf.Lerp(1.55f, .5f, Mathf.Pow(f, .7f));
            }
            radii[0] = radius * 1.9f;
            MeshShape("Tapered trunk", RootMesh(points, radii), parent, Vector3.zero, Vector3.one, material);
            for (int r = 0; r < 4; r++)
            {
                float a = lean + r * Mathf.PI * .5f + Range(random, -.4f, .4f);
                Vector3 dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Vector3[] root = { foot + dir * radius * .5f + Vector3.up * .35f, foot + dir * radius * 1.6f + Vector3.up * .07f, foot + dir * radius * 2.4f + Vector3.down * .09f };
                MeshShape("Root flare", RootMesh(root, new[] { radius * .5f, radius * .32f, radius * .14f }), parent, Vector3.zero, Vector3.one, material);
            }
        }

        // Layered canopy: dark clumps beneath, mid greens around, warm rim-lit clumps on top.
        static void Canopy(Transform parent, Vector3 centre, float size, int clumps, System.Random random, bool hangingMoss)
        {
            for (int i = 0; i < clumps; i++)
            {
                float a = i * 2.39996f + Range(random, -.3f, .3f), f = (i + .5f) / clumps;
                float ring = Mathf.Sqrt(1 - f) * size * .95f;
                float y = Mathf.Lerp(-.25f, .9f, f) * size + Range(random, -.12f, .12f);
                float s = size * Range(random, .85f, 1.25f) * Mathf.Lerp(1.15f, .7f, f);
                string colour = f < .3f ? "DarkMoss" : f < .72f ? (i % 2 == 0 ? "Sage" : "Canopy") : "CanopyLight";
                Shape("Canopy clump", PrimitiveType.Sphere, parent, centre + new Vector3(Mathf.Cos(a) * ring, y, Mathf.Sin(a) * ring),
                    new Vector3(s * 1.25f, s * .82f, s * 1.2f), colour);
            }
            if (!hangingMoss) return;
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.1f + Range(random, 0, .5f);
                Vector3 top = centre + new Vector3(Mathf.Cos(a) * size * .75f, -size * .35f, Mathf.Sin(a) * size * .75f);
                float length = Range(random, .45f, 1.0f) * size;
                var strand = MeshShape("Hanging moss strand", cone, parent, top, new Vector3(.05f * size, length * .8f, .04f * size), "Moss");
                strand.localRotation = Quaternion.Euler(180 + Range(random, -6, 6), Range(random, 0, 360), 0);
            }
        }

        // Two darker rings of silhouetted trees on a fogged forest floor give the clearing depth.
        static void ForestDepth(Transform parent)
        {
            var depth = Group("Forest depth - outer silhouettes (combined, no shadows)", parent);
            Shape("Fogged forest floor", PrimitiveType.Cylinder, depth, new Vector3(0, -.045f, 0), new Vector3(40, .025f, 40), "ForestFloor");
            var random = new System.Random(57);
            for (int ring = 0; ring < 2; ring++)
            {
                int count = ring == 0 ? 17 : 23;
                for (int i = 0; i < count; i++)
                {
                    float a = (i + (ring == 0 ? .5f : .2f) + Range(random, -.25f, .25f)) * Mathf.PI * 2 / count;
                    float r = (ring == 0 ? 6.8f : 9.4f) + Range(random, -.5f, .7f);
                    Vector3 p = new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
                    float h = Range(random, 3.4f, 5.2f) + ring * .6f;
                    TaperedTrunk(depth, p, h, .24f + ring * .04f, random, ring == 0 ? "FarTrunk" : "FarCanopyDeep");
                    for (int j = 0; j < 6; j++)
                    {
                        float b = j * 2.4f, f = (j + .5f) / 6, s = Range(random, 1.1f, 1.6f) * Mathf.Lerp(1.1f, .75f, f);
                        Shape("Silhouette canopy clump", PrimitiveType.Sphere, depth,
                            p + new Vector3(Mathf.Cos(b) * .8f * (1 - f), h + f * 1.1f, Mathf.Sin(b) * .8f * (1 - f)),
                            new Vector3(s * 1.3f, s * .9f, s * 1.3f), ring == 0 ? (j % 2 == 0 ? "FarCanopy" : "DarkMoss") : "FarCanopyDeep");
                    }
                }
            }
            CombineDecoration(depth, "ForestDepth");
            foreach (var renderer in depth.GetComponentsInChildren<MeshRenderer>())
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
