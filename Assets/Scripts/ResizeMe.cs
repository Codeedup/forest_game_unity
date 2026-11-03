using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeMe : MonoBehaviour
{
    public float sizeMin;
    public float sizeMax;
    // Start is called before the first frame update
    void Start() {
        float sizeX = Random.Range(sizeMin, sizeMax);
        float sizeY = Random.Range(sizeMin, sizeMax);
        float sizeZ = Random.Range(sizeMin, sizeMax);
        gameObject.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
    }
}
