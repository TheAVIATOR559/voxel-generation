using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector2Int coord;

    GameObject chunkObject;
    MeshRenderer meshRend;
    MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    World m_world;

    public bool IsActive
    {
        get
        {
            return chunkObject.activeSelf;
        }

        set
        {
            chunkObject.SetActive(value);
        }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    // Start is called before the first frame update
    public Chunk(Vector2Int position, World world)
    {
        coord = position;
        m_world = world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRend = chunkObject.AddComponent<MeshRenderer>();

        meshRend.material = m_world.material;

        chunkObject.transform.SetParent(m_world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.y * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk (" + coord.x + ", " + coord.y + ")";

        PopulateVoxelChunk();
        CreateMeshData();
        CreateMesh();
    }

    private void PopulateVoxelChunk()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = m_world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
    }

    private void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (m_world.blockTypes[voxelMap[x, y, z]].isSolid)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
                }
            }
        }
    }

    private void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[i]))
            {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i, 3]]);

                AddTexture(m_world.blockTypes[blockID].GetTextureID(i));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }



    private bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1
            || y < 0 || y > VoxelData.ChunkHeight - 1
            || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return m_world.blockTypes[m_world.GetVoxel(pos + position)].isSolid;
        }

        return m_world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    private void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}
