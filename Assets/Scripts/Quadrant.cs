using System;
using UnityEngine;

public class Quadrant : MonoBehaviour {
    [Header("Seed Settings")]
    [SerializeField] string seed = "";
    [SerializeField] bool useRandomSeed = true;

    float left, right, top, bottom;
    const float unitSize = 0.5f;
    MeshRenderer meshRenderer;
    Mesh mesh;

    private void Start() {
        if(useRandomSeed) {
            seed = DateTime.Now.ToString();
        }
        System.Random rng = new System.Random(seed.GetHashCode());

        mesh = GetComponent<MeshFilter>().mesh;
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = true;

        Vector2 size = meshRenderer.bounds.size;

        left = transform.position.x - (size.x / 2);
        right = transform.position.x + (size.x / 2);
        top = transform.position.y + (size.y / 2);
        bottom = transform.position.y - (size.y / 2);

        Generate();
    }

    private void Update() {

    }

    private void Generate() {
        Debug.DrawLine(new Vector3(left, top, -5), new Vector3(right, top, -5), Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(left, bottom, -5), new Vector3(right, bottom, -5), Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(left, top, -5), new Vector3(left, bottom, -5), Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(right, top, -5), new Vector3(right, bottom, -5), Color.green, Mathf.Infinity);



        for(float i = left ; i < right ; i += 0.5f) {
            Debug.DrawLine(new Vector3(i, top, -5), new Vector3(i, bottom, -5), Color.white, Mathf.Infinity);
        }

        for (float i = bottom; i < top; i += 0.5f) {
            Debug.DrawLine(new Vector3(left, i, -5), new Vector3(right, i, -5), Color.white, Mathf.Infinity);
        }
    }

    public bool IsInHole(Vector3 _position) {
        return (_position.x >= left && _position.x <= right && _position.y <= top && _position.y >= bottom);
    }
}
