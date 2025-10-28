using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject cubePrefab;
    public int startingHeight;

    [Range(0.1f, 5f)]
    public float spawnInterval = 1f;

    // Start is called before the first frame update
    void Start() {
        Debug.Log("Spawn script has started");

        // One cube only:
        SpawnRandomCube();

        // Spawn lots of cubes:
        InvokeRepeating(nameof(SpawnRandomCube), 2, spawnInterval);
    }

    void SpawnRandomCube() {
        // Position cube somewhere randomly (-5 to 5):
        Instantiate(cubePrefab, new Vector3(Random.Range(-5, 5),
                                            startingHeight,
                                            Random.Range(-5, 5)), cubePrefab.transform.rotation);
    }
}
