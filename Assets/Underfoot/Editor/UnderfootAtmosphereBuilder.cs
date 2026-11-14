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
    }
}
