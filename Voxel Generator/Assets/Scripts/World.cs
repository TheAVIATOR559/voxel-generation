using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance;
    public Material material;
    public BlockType[] blockTypes;

    private void Awake()
    {
        Instance = this;
    }
}
