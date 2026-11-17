using UnityEngine;

namespace Underfoot
{
    // One light built-in-pipeline image effect: soft bloom for emissive glows, warm highlights,
    // cool shadows, a slight saturation lift and a gentle vignette. Four tiny passes in total.
    [RequireComponent(typeof(Camera))]
    public class UnderfootStorybookGrade : MonoBehaviour
    {
        public Shader shader;
        [Header("Bloom (emissive glow)")]
        [Range(0, 3)] public float bloomThreshold = 1.2f;
        [Range(0, 2)] public float bloomIntensity = .55f;
        [Header("Colour")]
        [Range(0, 1)] public float splitToning = .32f;
        public Color highlightTint = new Color(1f, .92f, .8f);
        public Color shadowTint = new Color(.62f, .66f, .95f);
        [Range(-.5f, .8f)] public float saturation = .1f;
        [Range(0, 1)] public float vignette = .48f;
        public Color vignetteColour = new Color(.13f, .08f, .16f);
        Material material;

        public bool Ready => shader && shader.isSupported;

        void OnDisable() { if (material) DestroyImmediate(material); }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!Ready) { Graphics.Blit(source, destination); return; }
            if (!material) material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            material.SetVector("_Bloom", new Vector4(bloomThreshold, bloomIntensity, .5f, 0));
            material.SetVector("_Grade", new Vector4(splitToning, saturation, vignette, 0));
            material.SetColor("_HighlightTint", highlightTint); material.SetColor("_ShadowTint", shadowTint);
            material.SetColor("_VignetteColour", vignetteColour);

            // Two-step downsample (1/2, 1/4, 1/8) and tent upsample: a cheap, wide, soft glow.
            var format = source.format;
            int w = Mathf.Max(1, source.width / 2), h = Mathf.Max(1, source.height / 2);
            var half = RenderTexture.GetTemporary(w, h, 0, format);
            var quarter = RenderTexture.GetTemporary(Mathf.Max(1, w / 2), Mathf.Max(1, h / 2), 0, format);
            var eighth = RenderTexture.GetTemporary(Mathf.Max(1, w / 4), Mathf.Max(1, h / 4), 0, format);
            Graphics.Blit(source, half, material, 0);
            Graphics.Blit(half, quarter, material, 1);
            Graphics.Blit(quarter, eighth, material, 1);
            Graphics.Blit(eighth, quarter, material, 2);
            Graphics.Blit(quarter, half, material, 2);
            material.SetTexture("_BloomTex", half);
            Graphics.Blit(source, destination, material, 3);
            RenderTexture.ReleaseTemporary(half); RenderTexture.ReleaseTemporary(quarter); RenderTexture.ReleaseTemporary(eighth);
        }
    }
}
