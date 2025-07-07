using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelGeneration : NetworkBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;
    [SerializeField]
    private GameObject tilePrefab;

    private NetworkList<float> seedList;
    private bool mapGenerated = false;

    private void Awake()
    {
        seedList = new NetworkList<float>();
    }

    public override void OnNetworkSpawn()
    {
        seedList.OnListChanged += OnSeedListChanged;

        Debug.Log("OnNetworkSpawn called");

        if (IsOwner)
        {
            Debug.Log("Server generating seed...");
            GenerateSeed();
            GenerateMap(seedList);
            mapGenerated = true;
        }
    }

    void GenerateSeed()
    {
        int waveSize = tilePrefab.GetComponent<TileGeneration>().waves.Length;

        for (int i = 0; i < waveSize; i++)
        {
            seedList.Add(Random.Range(0, 10000f));
        }
    }

    private void OnSeedListChanged(NetworkListEvent<float> changeEvent)
    {
        if (!IsOwner && seedList.Count == tilePrefab.GetComponent<TileGeneration>().waves.Length && !mapGenerated)
        {
            Debug.Log("Client received full seed list, generating map...");
            GenerateMap(seedList);
            mapGenerated = true;
        }
    }

    void GenerateMap(NetworkList<float> seed)
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
    }
}
