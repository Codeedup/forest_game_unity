using UnityEngine;

namespace Underfoot
{
    [RequireComponent(typeof(TextMesh))]
    public class UnderfootSignText : MonoBehaviour
    {
        public Shader depthShader;
        TextMesh text;
        Material ink;

        void Awake()
        {
            text = GetComponent<TextMesh>();
            // The default legacy font shader draws through scenery. This one respects scene depth.
            ink = new Material(depthShader);
            RefreshAtlas(text.font);
            GetComponent<Renderer>().sharedMaterial = ink;
            Font.textureRebuilt += RefreshAtlas;
        }

        void RefreshAtlas(Font rebuilt)
        {
            if (rebuilt == text.font) ink.mainTexture = text.font.material.mainTexture;
        }

        void OnDestroy()
        {
            Font.textureRebuilt -= RefreshAtlas;
            if (ink) Destroy(ink);
        }
    }
}
