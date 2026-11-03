using UnityEngine;

public class CollisionColour : MonoBehaviour
{
    private Renderer _renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = Color.black;
    }

    private void OnCollisionEnter(Collision collision) {
        _renderer.material.color = Color.red;
    }
}
