using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillMe : MonoBehaviour
{
    public int destroyHeight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < destroyHeight) {
            Destroy(gameObject);
        }
    }
}
