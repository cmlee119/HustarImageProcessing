#include "pch.h"
//#include "dllmain.h"
#include <iostream>
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

const int ESC = 27;

extern "C"
{
    __declspec(dllexport) unsigned char* processFrame(int& witdh, int& height) {

        VideoCapture inputVideo = VideoCapture("C:/Users/cmlee/source/repos/OpenCv Test/test.mp4");

        Mat input_image;
        inputVideo.read(input_image);

        cv::Size s = input_image.size();
        witdh = s.height;
        height = s.width;

        imshow("test", input_image);
        return input_image.data;
    }
}
