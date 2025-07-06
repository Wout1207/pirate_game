using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public class TileGeneration : MonoBehaviour
{
    [SerializeField]
    private TerrainType[] terrainTypes;
    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;
    [SerializeField]
    PlaneGeneration planeGeneration;
    [SerializeField]
    private MeshRenderer tileRenderer;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private float mapScale;
    [SerializeField]
    public Wave[] waves;
    [SerializeField]
    public GameObject grassPrefab;
    void Start()
    {
        GenerateTile();
    }
    void GenerateTile()
    {
        planeGeneration.CreatePlane();
        // calculate tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;
        // calculate the offsets based on the tile position
        float offsetX = this.gameObject.transform.position.x;
        float offsetZ = this.gameObject.transform.position.z;
        // generate a heightMap using noise
        float[,] heightMap = this.noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, waves, planeGeneration.planeSize.x, planeGeneration.planeSize.y);
        for (int z = 0; z < tileDepth; z++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                float height = heightMap[z, x];

                // Drastically reduce height below 0.57 with a smooth falloff
                if (height < 0.57f)
                {
                    float t = height / 0.57f;
                    height = Mathf.SmoothStep(0f, 0.57f, Mathf.Pow(t, 3));
                }
                // Smooth raise above 0.7
                else if (height > 0.7f)
                {
                    height = Mathf.Exp(height) - Mathf.Exp(0.7f) + 0.7f;
                }

                heightMap[z, x] = height;
            }
        }
        // build a Texture2D from the height map
        Texture2D tileTexture = BuildTexture(heightMap);
        this.tileRenderer.material.mainTexture = tileTexture;
        // update the tile mesh vertices according to the height map
        UpdateMeshVertices(heightMap);
        if (grassPrefab)
        {
            SpawnGrass(heightMap);
        }
    }
    private Texture2D BuildTexture(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // transform the 2D map index is an Array index
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];
                // choose a terrain type according to the height value
                TerrainType terrainType = ChooseTerrainType(height);
                // assign the color according to the terrain type
                colorMap[colorIndex] = terrainType.color;
            }
        }
        // create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();
        return tileTexture;
    }

    TerrainType ChooseTerrainType(float height)
    {
        // for each terrain type, check if the height is lower than the one for the terrain type
        foreach (TerrainType terrainType in terrainTypes)
        {
            // return the first terrain type whose height is higher than the generated one
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }
        return terrainTypes[terrainTypes.Length - 1];
    }

    [SerializeField]
    private float heightMultiplier;
    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        // iterate through all the heightMap coordinates, updating the vertex index
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];
                Vector3 vertex = meshVertices[vertexIndex];
                // change the vertex Y coordinate, proportional to the height value
                meshVertices[vertexIndex] = new Vector3(vertex.x, height * this.heightMultiplier, vertex.z);
                vertexIndex++;
            }
        }
        // update the vertices in the mesh and update its properties
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        // update the mesh collider
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }
    private void SpawnGrass(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Vector3[] meshVertices = meshFilter.mesh.vertices;

        int vertexIndex = 0;
        for (int z = 0; z < tileDepth; z++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                float height = heightMap[z, x];
                TerrainType terrainType = ChooseTerrainType(height);

                if (terrainType.name.ToLower() == "grass")
                {
                    if (Random.value > 0.1)
                    {
                        vertexIndex++;
                        continue; // Skip this tile
                    }

                    Vector3 localPos = meshVertices[vertexIndex];
                    Vector3 worldPos = transform.TransformPoint(localPos);

                    Vector3 randomOffset = new Vector3(
                        Random.Range(-0.2f, 0.2f),
                        0,
                        Random.Range(-0.2f, 0.2f)
                    );

                    Instantiate(grassPrefab, worldPos + randomOffset, Quaternion.identity, this.transform);
                }

                vertexIndex++;
            }
        }
    }

}
