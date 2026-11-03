using UnityEngine;

namespace Underfoot
{
    public class UnderfootDetails : MonoBehaviour
    {
        public UnderfootPlayer player;
        public GameObject mossPrefab, tinyMushroomPrefab, leafVeinsPrefab;
        public Transform leaf;
        public float mossThreshold = .5f, fineThreshold = .15f, hysteresis = .025f;
        public int mossCount = 34, fineCount = 22;
        public Transform MossLayer { get; private set; }
        public Transform FineLayer { get; private set; }
        public int CreatedCount { get; private set; }

        void Update() { Refresh(); }
        public void Refresh()
        {
            if (player.Height <= mossThreshold && !MossLayer)
            {
                MossLayer = CreateLayer("Runtime - moss (created once)", mossPrefab, mossCount, 19);
                var veins = Instantiate(leafVeinsPrefab, leaf);
                veins.name = "Runtime - raised leaf veins";
                CreatedCount++;
            }
            if (player.Height <= fineThreshold && !FineLayer)
                FineLayer = CreateLayer("Runtime - tiny mushrooms (created once)", tinyMushroomPrefab, fineCount, 47);
            SetVisibility(MossLayer, mossThreshold);
            SetVisibility(FineLayer, fineThreshold);
            if (leaf.childCount > 2)
                for (int i = 0; i < leaf.childCount; i++)
                    if (leaf.GetChild(i).name.StartsWith("Runtime")) leaf.GetChild(i).gameObject.SetActive(MossLayer && MossLayer.gameObject.activeSelf);
        }

        Transform CreateLayer(string label, GameObject prefab, int count, int seed)
        {
            var layer = new GameObject(label).transform;
            layer.SetParent(transform, false);
            var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                Vector3 position = new Vector3(side * (.14f + (float)random.NextDouble() * .25f), 0, -.43f + (float)random.NextDouble() * 1.5f);
                var detail = Instantiate(prefab, position, Quaternion.Euler(0, (float)random.NextDouble() * 360, 0), layer);
                detail.transform.localScale *= .7f + (float)random.NextDouble() * .65f;
                CreatedCount++;
            }
            return layer;
        }

        void SetVisibility(Transform layer, float threshold)
        {
            if (!layer) return;
            bool visible = layer.gameObject.activeSelf ? player.Height <= threshold + hysteresis : player.Height <= threshold;
            layer.gameObject.SetActive(visible);
        }

        public void ResetDetails()
        {
            // Reuse the bounded collection on reset and on subsequent threshold crossings.
            if (MossLayer) MossLayer.gameObject.SetActive(false);
            if (FineLayer) FineLayer.gameObject.SetActive(false);
            Refresh();
        }
    }
}
