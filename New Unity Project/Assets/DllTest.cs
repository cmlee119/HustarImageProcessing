using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct MarkerTransform
{
    public float x;
    public float y;
    public float z;
    public float pitch;
    public float yaw;
    public float roll;
};

public class DllTest : MonoBehaviour
{ 
    [DllImport("DllUnityTest 11", EntryPoint = "StartLoop")]
    private static extern void StartLoop();
    [DllImport("DllUnityTest 11", EntryPoint = "GetRawImageBytes")]
    private static extern void GetRawImageBytes(IntPtr data, int width, int height, out IntPtr pVecMarkerTransform, out int itemCount);

    private CanvasRenderer canvasRenderer;

    private Texture2D tex;
    private Color32[] pixel32;

    private GCHandle pixelHandle;
    private IntPtr pixelPtr;

    private IntPtr markerPtr;

    public GameObject objPlanet;

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
        IntPtr pMarkerTransform = IntPtr.Zero;
        int itemCount = 0;


        //Convert Mat to Texture2D
        GetRawImageBytes(pixelPtr, tex.width, tex.height, out pMarkerTransform, out itemCount);


        List<MarkerTransform> informationList = new List<MarkerTransform>();
        int structSize = Marshal.SizeOf(typeof(MarkerTransform));

        GameObject[] objList = GameObject.FindGameObjectsWithTag("Planets");
        foreach (GameObject obj in objList)
        {
            GameObject.Destroy(obj);
        }

        Debug.Log(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            MarkerTransform info = (MarkerTransform)Marshal.PtrToStructure(pMarkerTransform, typeof(MarkerTransform));
            //informationList.Add(info);
            pMarkerTransform = new IntPtr(pMarkerTransform.ToInt64() + structSize);

            Instantiate(objPlanet, new Vector3(info.x, info.y, info.z), Quaternion.identity);
        }

        //Update the Texture2D with array updated in C++
        tex.SetPixels32(pixel32);
        tex.Apply();



        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        UnityEngine.Vector2 vec2;
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