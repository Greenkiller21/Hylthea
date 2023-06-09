using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainMeshGenerator : MonoBehaviour {
    private int MESH_SCALE = 5;

    public GameObject waterPlane;
    public float waterLevel;
    public GameObject[] objects;
    public AnimationCurve heightCurve;
    public Gradient gradient;
    public int xSize;
    public int zSize;
    public float scale;
    public int octaves;
    public float lacunarity;
    public int seed;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private Color[] colors;

    private float minTerrainheight;
    private float maxTerrainheight;

    private float lastNoiseHeight;

    void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateNewMap();
    }

    public void CreateNewMap() {
        CreateMeshShape();
        CreateTriangles();
        ColorMap();
        UpdateMesh();
    }

    private void CreateMeshShape() {
        // Creates seed
        Vector2[] octaveOffsets = GetOffsetSeed();

        if (scale <= 0) scale = 0.0001f;

        // Create vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++) {
            for (int x = 0; x <= xSize; x++) {
                // Set height of vertices
                float noiseHeight = GenerateNoiseHeight(z, x, octaveOffsets);
                SetMinMaxHeights(noiseHeight);
                vertices[i] = new Vector3(x, noiseHeight, z);
                i++;
            }
        }
    }

    private Vector2[] GetOffsetSeed() {
        seed = Random.Range(int.MinValue, int.MaxValue);

        // changes area of map
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int o = 0; o < octaves; o++) {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[o] = new Vector2(offsetX, offsetY);
        }
        return octaveOffsets;
    }

    private float GenerateNoiseHeight(int z, int x, Vector2[] octaveOffsets) {
        float amplitude = 20;
        float frequency = 1;
        float persistence = 0.5f;
        float noiseHeight = 0;

        // loop over octaves
        for (int y = 0; y < octaves; y++) {
            float mapZ = z / scale * frequency + octaveOffsets[y].y;
            float mapX = x / scale * frequency + octaveOffsets[y].x;

            //The *2-1 is to create a flat floor level
            float perlinValue = (Mathf.PerlinNoise(mapZ, mapX)) * 2 - 1;
            noiseHeight += heightCurve.Evaluate(perlinValue) * amplitude;
            frequency *= lacunarity;
            amplitude *= persistence;
        }

        return noiseHeight;
    }

    private void SetMinMaxHeights(float noiseHeight) {
        // Set min and max height of map for color gradient
        if (noiseHeight > maxTerrainheight)
            maxTerrainheight = noiseHeight;
        if (noiseHeight < minTerrainheight)
            minTerrainheight = noiseHeight;
    }


    private void CreateTriangles() {
        // Need 6 vertices to create a square (2 triangles)
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;
        // Go to next row
        for (int z = 0; z < xSize; z++) {
            // fill row
            for (int x = 0; x < xSize; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void ColorMap() {
        colors = new Color[vertices.Length];

        // Loop over vertices and apply a color from the depending on height (y axis value)
        for (int i = 0, z = 0; z < vertices.Length; z++) {
            float height = Mathf.InverseLerp(minTerrainheight, maxTerrainheight, vertices[i].y);
            colors[i] = gradient.Evaluate(height);
            i++;
        }
    }

    private void MapEmbellishments() {
        if (objects.Length == 0) return;
        for (int i = 0; i < vertices.Length; i++) {
            // find actual position of vertices in the game
            Vector3 worldPt = transform.TransformPoint(mesh.vertices[i]);
            var noiseHeight = worldPt.y;
            // Stop generation if height difference between 2 vertices is too steep
            if (System.Math.Abs(lastNoiseHeight - worldPt.y) < 25) {
                // min height for object generation
                if (noiseHeight > waterLevel) {
                    // Chance to generate
                    if (Random.Range(1, 5) == 1) {
                        GameObject objectToSpawn = objects[Random.Range(0, objects.Length)];
                        Instantiate(objectToSpawn, new Vector3(mesh.vertices[i].x * MESH_SCALE, noiseHeight, mesh.vertices[i].z * MESH_SCALE), Quaternion.identity);
                    }
                }
            }
            lastNoiseHeight = noiseHeight;
        }
    }

    private void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        gameObject.transform.localScale = new Vector3(MESH_SCALE, MESH_SCALE, MESH_SCALE);
        waterPlane.transform.localScale = new Vector3(xSize / 2.0f, 1f, zSize / 2.0f);
        waterPlane.transform.position = new Vector3(MESH_SCALE * xSize / 2.0f, waterLevel, MESH_SCALE * zSize / 2.0f);

        MapEmbellishments();
    }
}