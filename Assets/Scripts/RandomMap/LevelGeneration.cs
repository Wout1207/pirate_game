using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;
    [SerializeField]
    private GameObject tilePrefab;
    void Start()
    {
        GenerateMap();
    }
    void GenerateMap()
    {
        // get the tile dimensions from the tile Prefab
        Vector2 tileSize = tilePrefab.GetComponent<PlaneGeneration>().planeSize;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.y;

        int waveSize = tilePrefab.GetComponent<TileGeneration>().waves.Length;

        float[] seed = new float[waveSize];

        for (int i = 0; i < seed.Length; i++)
        {
            seed[i] = Random.Range(0, 10000);
        }
        // for each Tile, instantiate a Tile in the correct position
        for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
            {
                // calculate the tile position based on the X and Z indices
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth,
                  this.gameObject.transform.position.y,
                  this.gameObject.transform.position.z + zTileIndex * tileDepth);
                // instantiate a new Tile
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                Wave[] waves = tile.GetComponent<TileGeneration>().waves;
                for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
                {
                    waves[waveIndex].seed = seed[waveIndex];
                }
            }
        }
    }
}
