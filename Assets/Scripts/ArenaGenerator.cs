using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using CollectObjects = Unity.AI.Navigation.CollectObjects;
using NavMeshSurface = Unity.AI.Navigation.NavMeshSurface;
using Random = UnityEngine.Random;

public class ArenaGenerator : MonoBehaviour
{
    #region VARIABLES
    
    public int gridSize = 15;
    public GameObject wallPrefab;
    public GameObject obstaclePrefab;
    public GameObject floorPrefab;
    public GameObject mudPrefab;
    //public GameObject mudLayerPrefab;
    [FormerlySerializedAs("obstacleInstensity")] public float obstacleIntensity = 0.5f;
    
    private NavMeshSurface navMeshSurface;
    public GameObject playerPrefab;
    
    public GameObject playerSpawnCube;
    private bool playerSpawnCubeCreated;
    
    public GameObject enemySpawnCube;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;
    
    #endregion

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        GenerateArena();
    }
    
    void GenerateArena()
    {
        GenerateWalls();
        GenerateInsideWalls();
        BuildNavMesh();
        ActivatePlayers();
    }

    void ActivatePlayers()
    {
        playerPrefab.SetActive(true);
        enemy1.SetActive(true);
        enemy2.SetActive(true);
        enemy3.SetActive(true);
        enemy4.SetActive(true);
    }

    void GenerateWalls()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (x == 0 || x == gridSize - 1 || z == 0 || z == gridSize - 1)
                {
                    Vector3 position = new Vector3(x + 0.5f, 1f, z + 0.5f);
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    void GenerateInsideWalls()
    {
        int seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);
        float offsetX = Random.Range(0f, 10000f);
        float offsetY = Random.Range(0f, 10000f);
        List<Vector3> floorPositions = new List<Vector3>();

        for (int x = 1; x < gridSize - 1; x++)
        {
            for (int z = 1; z < gridSize - 1; z++)
            {
                //GENERATE PERLIN NOISE VALUE
                float noiseValue = Mathf.PerlinNoise((x + offsetX) * 0.1f, (z + offsetY) * 0.1f);
                //GENERATE OBSTACLES BASED ON NOISE INTENSITY
                if (noiseValue > obstacleIntensity)
                {
                    Vector3 position = new Vector3(x + 0.5f, 1f, z + 0.5f);
                    Instantiate(obstaclePrefab, position, Quaternion.identity);
                }
                //GENERATE FLOOR AROUND THE OBSTACLES
                if (noiseValue - obstacleIntensity > -0.3f)
                {
                    Vector3 position = new Vector3(x + 0.5f, 0f, z + 0.5f);
                    floorPositions.Add(position);
                    Instantiate(floorPrefab, position, Quaternion.identity);
                }
                //GENERATE MUD PUDDLES
                else
                {
                    Vector3 position = new Vector3(x + 0.5f, 0f, z + 0.5f);
                    floorPositions.Add(position);
                    GameObject mudObj = Instantiate(mudPrefab, position, Quaternion.identity);
                }
            }
        }

        if (floorPositions.Count > 0)
        {
            //PICK A RANDOM FLOOR TILE
            int randomIndex = Random.Range(0, floorPositions.Count);
            Vector3 spawnPosition = floorPositions[randomIndex];
            RaycastHit hit;
            //DESTROY IT
            if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out hit, 3f))
            {
                Destroy(hit.collider.gameObject);
            }

            //GENERATE A PLAYER SPAWN CUBE IN THE PLACE
            Instantiate(playerSpawnCube, spawnPosition, Quaternion.identity);
            playerSpawnCubeCreated = true;
            Debug.Log("Player Spawn Point Created");
            //MOVE THE PLAYER INTO ITS SPAWN POSITION
            if (playerSpawnCubeCreated)
            {
                Vector3 playerPos = new Vector3(spawnPosition.x, 1, spawnPosition.z);
                Debug.Log("Player Spawned" + playerPos);
                playerPrefab.transform.position = playerPos;
            }

            //PICK 4 RANDOM TILES FROM THE FLOOR
            for (int i = 0; i < 4; i++)
            {
                int randomFloorIndex = Random.Range(0, floorPositions.Count);
                Vector3 enemySpawnPosition = floorPositions[randomFloorIndex];
                RaycastHit enemyHit;
                //DESTROY IT
                if (Physics.Raycast(enemySpawnPosition + Vector3.up * 2f, Vector3.down, out enemyHit, 3f))
                {
                    Destroy(enemyHit.collider.gameObject);
                }

                //GENERATE ENEMY SPAWN CUBE AND MOVE THE ENEMY INTO THAT POSITION
                if (i == 1)
                {
                    Instantiate(enemySpawnCube, enemySpawnPosition, Quaternion.identity);
                    Vector3 enemyPos = new Vector3(enemySpawnPosition.x, 1, enemySpawnPosition.z);
                    enemy1.transform.position = enemyPos;
                    Debug.Log("Enemy 1 Spawned");
                }
                else if (i == 2)
                {
                    Instantiate(enemySpawnCube, enemySpawnPosition, Quaternion.identity);
                    Vector3 enemyPos = new Vector3(enemySpawnPosition.x, 1, enemySpawnPosition.z);
                    enemy2.transform.position = enemyPos;
                    Debug.Log("Enemy 2 Spawned");
                }
                else if (i == 3)
                {
                    Instantiate(enemySpawnCube, enemySpawnPosition, Quaternion.identity);
                    Vector3 enemyPos = new Vector3(enemySpawnPosition.x, 1, enemySpawnPosition.z);
                    enemy3.transform.position = enemyPos;
                    Debug.Log("Enemy 3 Spawned");
                }
                else
                {
                    Instantiate(enemySpawnCube, enemySpawnPosition, Quaternion.identity);
                    Vector3 enemyPos = new Vector3(enemySpawnPosition.x, 1, enemySpawnPosition.z);
                    enemy4.transform.position = enemyPos;
                    Debug.Log("Enemy 4 Spawned");
                }

                floorPositions.RemoveAt(randomFloorIndex);
            }
        }
    }

    void BuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}