using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Underfoot.Editor
{
    // Optional imported-model slots. If Assets/Underfoot/Models/<Slot>.(fbx|glb|prefab) exists, the builder uses it
    // in place of the procedural version; with the folder empty the scene builds exactly as before.
    // Slots: Wanderer, Gnome, Squirrel, Mushroom, Tree, Grass. See Models/README.md for the import rules.
    public static partial class UnderfootBuilder
    {
        const string ModelFolder = Root + "/Models";
        static readonly Dictionary<string, GameObject> modelCache = new Dictionary<string, GameObject>();

        static GameObject FindModel(string slot)
        {
            if (modelCache.TryGetValue(slot, out var cached)) return cached;
            GameObject found = null;
            if (AssetDatabase.IsValidFolder(ModelFolder))
                foreach (string guid in AssetDatabase.FindAssets(slot, new[] { ModelFolder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) != slot) continue;
                    found = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (found) { Debug.Log("UNDERFOOT_MODEL: slot '" + slot + "' uses " + path); break; }
                }
            modelCache[slot] = found;
            return found;
        }

        // Instances the slot's model with its feet at the holder's origin, facing +Z, scaled to the target height.
        // Returns the holder (the object the procedural version would have been), or null when the slot is empty.
        static Transform PlaceModel(string slot, Transform parent, Vector3 position, float height, float yaw = 0)
        {
            var asset = FindModel(slot);
            if (!asset) return null;
            var holder = Group(slot + " model");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            // The builder owns collision; imported colliders would fight the CharacterController and the physics routes.
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            instance.transform.SetParent(holder, false);
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning("UNDERFOOT_MODEL: slot '" + slot + "' has no renderers; using the procedural version.");
                Object.DestroyImmediate(holder.gameObject); modelCache[slot] = null; return null;
            }
            Bounds Measure() { var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds); return b; }
            var size = Measure().size;
            if (size.y > 1e-4f) instance.transform.localScale *= height / size.y;
            var fitted = Measure();
            instance.transform.position += new Vector3(-fitted.center.x, -fitted.min.y, -fitted.center.z);
            holder.SetParent(parent, false);
            holder.localPosition = position; holder.localRotation = Quaternion.Euler(0, yaw, 0);
            return holder;
        }

        // A named bone/pivot the gameplay scripts rotate; an empty stand-in keeps them safe when the model has none.
        static Transform Pivot(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t != root && string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase)) return t;
            var stand = Group(name); stand.SetParent(root, false); return stand;
        }

        static bool TryCharacterModel(Transform parent, bool gnome, out Transform body, out Transform[] feet, out Transform[] arms, out Transform hand)
        {
            body = PlaceModel(gnome ? "Gnome" : "Wanderer", parent, Vector3.zero, gnome ? 1.25f : 1f);
            feet = null; arms = null; hand = null;
            if (!body) return false;
            body.name = gnome ? "Friendly gnome model" : "Hooded wanderer model";
            feet = new[] { Pivot(body, "LeftFoot"), Pivot(body, "RightFoot") };
            arms = new[] { Pivot(body, "LeftArm"), Pivot(body, "RightArm") };
            hand = Pivot(body, "RightHand");
            return true;
        }
    }
}
