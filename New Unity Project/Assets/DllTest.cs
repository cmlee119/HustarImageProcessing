using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;

public class DllTest : MonoBehaviour
{ 
    [DllImport("DllUnityTest", EntryPoint = "StartLoop")]
    private static extern void StartLoop();
    [DllImport("DllUnityTest", EntryPoint = "GetRawImageBytes")]
    private static extern void GetRawImageBytes(IntPtr data, int width, int height/*, IntPtr dataMarker*/);

    private CanvasRenderer canvasRenderer;

    private Texture2D tex;
    private Color32[] pixel32;

    private GCHandle pixelHandle;
    private IntPtr pixelPtr;

    //private IntPtr markerPtr;

    void Start()
    {
        InitTexture();
        canvasRenderer = gameObject.GetComponent<CanvasRenderer>();
         
        Thread threadMarkerDetector = new Thread(new ThreadStart(StartLoop));
        threadMarkerDetector.Start();
    }


    void Update()
    {
        MatToTexture2D(); 
    }


    void InitTexture()
    {
        tex = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        pixel32 = tex.GetPixels32();
        //Pin pixel32 array
        pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
        //Get the pinned address
        pixelPtr = pixelHandle.AddrOfPinnedObject();
    }

    void MatToTexture2D()
    {
        //int iCountMarker;

        //Convert Mat to Texture2D
        GetRawImageBytes(pixelPtr, tex.width, tex.height/*, markerPtr*/);
        //Update the Texture2D with array updated in C++
        tex.SetPixels32(pixel32);
        tex.Apply();

        

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 vec2;
        vec2.x = Screen.width;
        vec2.y = Screen.height;
        rectTransform.sizeDelta = vec2;

        canvasRenderer.SetTexture(tex);
    }

    void OnApplicationQuit()
    {
        //Free handle
        pixelHandle.Free();
    }
}