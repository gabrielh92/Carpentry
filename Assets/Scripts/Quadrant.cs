using UnityEngine;

public class Quadrant : MonoBehaviour {
    [Header("Seed Settings")]
    [SerializeField] string seed = "";
    [SerializeField] bool useRandomSeed = false;

    int[,] region;

    private void Start() {
        if(useRandomSeed) {
            seed = Time.time.ToString();
        }
        System.Random rng = new System.Random(seed.GetHashCode());

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Vector2 size = meshRenderer.bounds.size;
        print(size);
        Generate();
    }

    private void Generate() {

    }
}
