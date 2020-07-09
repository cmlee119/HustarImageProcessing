using System;
using System.Collections;
using System.Collections.Generic;
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

public class testdll
{
    [DllImport("DllUnityTest", EntryPoint = "StartLoop")]
    private static extern void StartLoop();
    [DllImport("DllUnityTest", EntryPoint = "GetRawImageBytes")]
    private static extern void GetRawImageBytes( int width, int height, out IntPtr pVecMarkerTransform, out int itemCount);


    private GCHandle pixelHandle;
    private IntPtr pixelPtr;

    private IntPtr markerPtr;

    static void Main()
    {
        Start();

        while(true)
        {
            Update();
        }
    }
    static void Start()
    {

        Thread threadMarkerDetector = new Thread(new ThreadStart(StartLoop));
        threadMarkerDetector.Start();
    }


    static void Update()
    {
        IntPtr a;
        IntPtr b;
        int i_Count;
        GetRawImageBytes(512,512,out b, out i_Count);//data : frame, 
    }

}