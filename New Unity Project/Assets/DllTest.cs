﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Text;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct MarkerTransform
{
    public int marker_id;
    public float x;
    public float y;
}; 

public class DllTest : MonoBehaviour
{
    private const string m_strDllName = "DllUnityTest";

    [DllImport(m_strDllName, EntryPoint = "DllMainInit")]
    private static extern bool DllMainInit(bool bCamMode, StringBuilder strVideoFilePath);


    [DllImport(m_strDllName, EntryPoint = "DllMainStartLoop")]
    private static extern void DllMainStartLoop();


    [DllImport(m_strDllName, EntryPoint = "DllMainCloseLoop")]
    private static extern void DllMainCloseLoop();
    

    [DllImport(m_strDllName, EntryPoint = "DllMainGetRawImageBytes")] 
    private static extern void DllMainGetRawImageBytes(IntPtr data, out IntPtr pVecMarkerTransform, out int itemCount);
    
    
    [DllImport(m_strDllName, EntryPoint = "DllMainGetFrameSize")]
    private static extern bool DllMainGetFrameSize(out int iWidth, out int iHeight);


    private CanvasRenderer canvasRenderer;

    private Texture2D tex;
    private Color32[] pixel32;

    private GCHandle pixelHandle;
    private IntPtr pixelPtr;

    public GameObject objPlanet;

    private Thread threadMarkerDetector;

    //비디오, 캠의 프레임 크기
    private int m_iVideoFrameWidth;
    private int m_iVideoFrameHeight;

    public bool m_bCamMode = true;
    public string m_strVideoFilePath = "";

    private bool m_bSpaceMode = false;
    public Texture2D m_textureSpace;

    void Start()
    {
        StringBuilder strBuilder = new StringBuilder(m_strVideoFilePath);

        //Marker Detector를 준비합니다.
        if(false == DllMainInit(m_bCamMode, strBuilder))
        {
            Debug.LogError("DllMainInit 실패");
        }

        //비디오 프레임의 크기를 알아옵니다.
        if(false == DllMainGetFrameSize(out m_iVideoFrameWidth, out m_iVideoFrameHeight))
        {
            Debug.LogError("DllMainGetFrameSize 실패");
        }

        InitTexture();
        canvasRenderer = gameObject.GetComponent<CanvasRenderer>();

        threadMarkerDetector = new Thread(new ThreadStart(DllMainStartLoop));
        threadMarkerDetector.Start();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_bSpaceMode = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            m_bSpaceMode = false;
        }
        
        MatToTexture2D(); 
    }

    void OnDestroy()
    {
        DllMainCloseLoop();
        
    }

    void OnApplicationQuit()
    {
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
        tex = new Texture2D(m_iVideoFrameWidth, m_iVideoFrameHeight, TextureFormat.ARGB32, false);
        pixel32 = tex.GetPixels32();
        //Pin pixel32 array
        pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
        //Get the pinned address
        pixelPtr = pixelHandle.AddrOfPinnedObject();
    }

    void MatToTexture2D()
    {
        //Convert Mat to Texture2D
        DllMainGetRawImageBytes(pixelPtr, out IntPtr pMarkerTransform, out int itemCount);

        int structSize = Marshal.SizeOf(typeof(MarkerTransform));

        List<GameObject> listObject = GameObject.FindGameObjectsWithTag("Planets").ToList<GameObject>();

        for (int i = 0; i < itemCount; ++i)
        {
            MarkerTransform info = (MarkerTransform)Marshal.PtrToStructure(pMarkerTransform, typeof(MarkerTransform));

            string strName = info.marker_id.ToString();

            float iPositionX = (-1.0f + info.x / m_iVideoFrameWidth * 2) * Camera.main.aspect;
            float iPositionY = 1.0f - info.y / m_iVideoFrameHeight * 2;

            Vector3 position = new Vector3(iPositionX, iPositionY, 0);

            GameObject objGame = GameObject.Find(strName);
            
            if (objGame == null)
            {
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

        if(m_bSpaceMode == true)
        {
            if(m_textureSpace != null)
            {
                canvasRenderer.SetTexture(m_textureSpace);
            }
        }
        else
        {
            tex.SetPixels32(pixel32);
            tex.Apply();
            canvasRenderer.SetTexture(tex);
        }

        //Update the Texture2D with array updated in C++
        



        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        UnityEngine.Vector2 vec2 = new Vector2(Screen.width, Screen.height);
        rectTransform.sizeDelta = vec2;

        
    }
}