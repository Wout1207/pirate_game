using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelGeneration : NetworkBehaviour
{
    [SerializeField] private int mapWidthInTiles, mapDepthInTiles;
    [SerializeField] private GameObject tilePrefab;

    private NetworkList<float> seedList = new NetworkList<float>();
    private bool mapGenerated = false;

    public override void OnNetworkSpawn()
    {
        seedList.OnListChanged += OnSeedListChanged;

        if (IsOwner)
        {
            if (seedList.Count == 0) // Prevents regenerating the map when client reconnects
            {
                GenerateSeed();
            }
        }

        // Regardless of host or client, check if the list is already populated
        if (seedList.Count == tilePrefab.GetComponent<TileGeneration>().waves.Length && !mapGenerated)
        {
            GenerateMap(seedList);
            mapGenerated = true;
        }
    }

    private void GenerateSeed()
    {
        int waveSize = tilePrefab.GetComponent<TileGeneration>().waves.Length;

        for (int i = 0; i < waveSize; i++)
        {
            seedList.Add(Random.Range(0, 10000f));
        }

        GenerateMap(seedList);
        mapGenerated = true;
    }

    private void OnSeedListChanged(NetworkListEvent<float> changeEvent)
    {
        Debug.Log("seed list changed");
        if (!mapGenerated && seedList.Count == tilePrefab.GetComponent<TileGeneration>().waves.Length)
        {
            GenerateMap(seedList);
            mapGenerated = true;
        }
    }

    private void GenerateMap(NetworkList<float> seed)
    {
        Vector2 tileSize = tilePrefab.GetComponent<PlaneGeneration>().planeSize;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.y;

        for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(
                    transform.position.x + xTileIndex * tileWidth,
                    transform.position.y,
                    transform.position.z + zTileIndex * tileDepth
                );

                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.transform.SetParent(transform);

                Wave[] waves = tile.GetComponent<TileGeneration>().waves;
                for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
                {
                    waves[waveIndex].seed = seed[waveIndex];
                }
            }
        }
        Destroy(GameObject.Find("LoadingCanvas"));
    }
}
