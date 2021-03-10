using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 5;
    public static readonly int ChunkHeight = 5;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0,0,0), //VERT 0
        new Vector3(1,0,0), //VERT 1
        new Vector3(1,1,0), //VERT 2
        new Vector3(0,1,0), //VERT 3
        new Vector3(0,0,1), //VERT 4
        new Vector3(1,0,1), //VERT 5
        new Vector3(1,1,1), //VERT 6
        new Vector3(0,1,1) //VERT 7
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0)
    };

    //BACK, FRONT, TOP, BOTTOM, LEFT, RIGHT
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        {0,3,1,2 }, //BACK FACE
        {5,6,4,7 }, //FRONT FACE
        {3,7,2,6 }, //TOP FACE
        {1,5,0,4 }, //BOTTOM FACE
        {4,7,0,3 }, //LEFT FACE
        {1,2,5,6 } //RIGHT FACE
    };

    public static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1)
    };
}
