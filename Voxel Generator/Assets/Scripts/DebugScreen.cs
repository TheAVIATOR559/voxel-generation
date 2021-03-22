using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    World world;
    TMP_Text text;

    float timer;
    float frameRate;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        string debugText = frameRate + " fps";
        debugText += "\n\n";
        debugText += "PLAYER XYZ :: " + Mathf.FloorToInt(world.player.position.x) + "|" + Mathf.FloorToInt(world.player.position.y) + "|" + Mathf.FloorToInt(world.player.position.z);
        debugText += "\nChunk :: " + world.playerChunkCoord.x + "|" + world.playerChunkCoord.y;

        text.text = debugText;

        if(timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }

    }
}
