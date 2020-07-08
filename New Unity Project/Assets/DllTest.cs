using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DllTest : MonoBehaviour
{

    //Lets make our calls from the Plugin
    [DllImport("DllUnityTest2", EntryPoint = "processFrame")]
    private static extern byte[] processFrame(out int witdh, out int height);

    private Texture2D texture;
    private Material material;

    void Start()
    {

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        Debug.Log("Start");
    }

    void Update()
    {
        int witdh = 0;
        int height = 0;
        byte[] data = processFrame(out witdh, out height);
        Debug.Log(witdh + "    " + height);
        if (data == null)
        {
            Debug.Log("ddd"); 
        }    
        texture = new Texture2D(witdh, height, TextureFormat.RGB24, false);
        texture.LoadRawTextureData(data);
        texture.Apply();
        
        Debug.Log("Update");  
    } 
}
 