using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public GameObject debugScreen;
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<Vector2Int> activeChunks = new List<Vector2Int>();
    public Vector2Int playerChunkCoord;
    private Vector2Int playerLastChunkCoord;

    List<Vector2Int> chunksToCreate = new List<Vector2Int>();

    private bool isCreatingChunks;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);

        GenerateWorld();

        player.position = spawnPosition;
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (playerChunkCoord != playerLastChunkCoord)
        {
            CheckViewDistance();
        }

        if(chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            //Debug.Log("Attempting to create chunks");
            StartCoroutine(CreateChunks());
        }

        if(Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    private void GenerateWorld()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for(int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new Vector2Int(x, z), this, true);
                activeChunks.Add(new Vector2Int(x, z));
            }
        }
    }

    IEnumerator CreateChunks()
    {
        //Debug.Log("Started Coroutine");
        isCreatingChunks = true;

        while(chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].y].Init();
            //Debug.Log("Chunk Initialized");
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
        //Debug.Log("Ended Coroutine");
    }

    private Vector2Int GetChunkCoordFromVector3(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth), Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth));
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        return chunks[Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth), Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth)];
    }

    private void CheckViewDistance()
    {
        Vector2Int playerChunk = GetChunkCoordFromVector3(player.position);

        playerLastChunkCoord = playerChunkCoord;

        List<Vector2Int> prevActiveChunks = new List<Vector2Int>(activeChunks);
        activeChunks.Clear();

        for(int x = playerChunk.x - VoxelData.ViewDistanceInChunks; x < playerChunk.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = playerChunk.y - VoxelData.ViewDistanceInChunks; z < playerChunk.y + VoxelData.ViewDistanceInChunks; z++)
            {
                if(IsChunkInWorld(new Vector2Int(x,z)))
                {
                    if(chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new Vector2Int(x, z), this, false);
                        chunksToCreate.Add(new Vector2Int(x, z));
                    }
                    else if(!chunks[x,z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }
                    activeChunks.Add(new Vector2Int(x, z));
                }

                for(int i = 0; i < prevActiveChunks.Count; i++)
                {
                    if(prevActiveChunks[i] == new Vector2Int(x,z))
                    {
                        prevActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(Vector2Int c in prevActiveChunks)
        {
            chunks[c.x, c.y].IsActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        Vector2Int thisChunk = VoxelData.Vector3ToVector2Int(pos);

        if(!IsVoxelInWorld(pos))
        {
            return false;
        }

        if(chunks[thisChunk.x, thisChunk.y] != null && chunks[thisChunk.x, thisChunk.y].IsVoxelMapPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.y].GetVoxelFromGlobalVector3(pos)].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        ///IMMUTABLE PASS
        /// ie must happen regardless of generation
        if(!IsVoxelInWorld(pos))
        {
            return 0;
        }

        if(yPos == 0)
        {
            return 1;
        }

        ///BASIC TERRAIN PASS

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if(yPos == terrainHeight)
        {
            voxelValue = 3;
        }
        else if(yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 5;
        }
        else if(yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 2;
        }

        ///LODES PASS
        
        if(voxelValue == 2)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if(Noise.Get3DPerlinNoise(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        return voxelValue;
    }

    private bool IsChunkInWorld(Vector2Int pos)
    {
        if(pos.x > 0 && pos.x < VoxelData.WorldSizeInChunks - 1
            && pos.y > 0 && pos.y < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels
            && pos.y >= 0 && pos.y < VoxelData.ChunkHeight
            && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
