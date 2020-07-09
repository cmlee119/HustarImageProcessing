using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct MarkerTransform
{
    public int marker_id;
    public float x;
    public float y;
}; 

public class DllTest : MonoBehaviour
{
    [DllImport("DllUnityTest 2", EntryPoint = "Init")]
    private static extern void MarkerDetectorInit();
    [DllImport("DllUnityTest 2", EntryPoint = "StartLoop")]
    private static extern void StartLoop();
    [DllImport("DllUnityTest 2", EntryPoint = "CloseLoop")]
    private static extern void CloseLoop();
    [DllImport("DllUnityTest 2", EntryPoint = "GetRawImageBytes")]
    private static extern void GetRawImageBytes(IntPtr data, int width, int height, out IntPtr pVecMarkerTransform, out int itemCount);

    private CanvasRenderer canvasRenderer;

    private Texture2D tex;
    private Color32[] pixel32;

    private GCHandle pixelHandle;
    private IntPtr pixelPtr;

    public GameObject objPlanet;

    private Thread threadMarkerDetector;

    void Start()
    {
        Debug.Log("Start");

        InitTexture();
        canvasRenderer = gameObject.GetComponent<CanvasRenderer>();

        MarkerDetectorInit();

        threadMarkerDetector = new Thread(new ThreadStart(StartLoop));
        threadMarkerDetector.Start();
    }


    void Update()
    {
        MatToTexture2D(); 
    }

    void OnDestroy()
    {
        CloseLoop();
        
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (threadMarkerDetector != null && threadMarkerDetector.IsAlive)
        {
            //threadMarkerDetector.Join();
            threadMarkerDetector = null;
        }
        //threadMarkerDetector.Interrupt();
        //threadMarkerDetector.Abort();
    }

    void InitTexture()
    {
        tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
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

        int structSize = Marshal.SizeOf(typeof(MarkerTransform));

        List<GameObject> listObject = GameObject.FindGameObjectsWithTag("Planets").ToList<GameObject>();

        //Debug.Log(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            MarkerTransform info = (MarkerTransform)Marshal.PtrToStructure(pMarkerTransform, typeof(MarkerTransform));

            string strName = "" + info.marker_id;
            Vector3 position = new Vector3(info.x / (Screen.width/8) - 2, -info.y / (Screen.height/4) + 2, 0);
            Debug.Log("x : " + position.x + ",  y : " + position.y);
            GameObject objGame = GameObject.Find(strName);
            
            if (objGame == null)
            {
                Debug.Log(strName);
                objGame = Instantiate(objPlanet, position, Quaternion.identity);
                objGame.name = strName;
            }
            else
            {
                objGame.GetComponent<Transform>().position = position;
                objGame.GetComponent<Renderer>().enabled = true;
                listObject.Remove(objGame);
            }

            pMarkerTransform = new IntPtr(pMarkerTransform.ToInt64() + structSize);
        }

        foreach(GameObject obj in listObject)
        {
            obj.GetComponent<Renderer>().enabled = false;
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
}