#include "pch.h"
//#include "dllmain.h"
#include <iostream>
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

const int ESC = 27;

extern "C"
{
    __declspec(dllexport) void GetRawImageBytes(unsigned char* data, int width, int height)
    {
        VideoCapture inputVideo = VideoCapture("C:/Users/cmlee/source/repos/OpenCv Test/test.mp4");

        Mat input_image;
        inputVideo.read(input_image);

        //Resize Mat to match the array passed to it from C#
        cv::Mat resizedMat(height, width, input_image.type());
        cv::resize(input_image, resizedMat, resizedMat.size(), cv::INTER_CUBIC);

        //You may not need this line. Depends on what you are doing
        cv::imshow("Nicolas", resizedMat);

        //Convert from RGB to ARGB 
        cv::Mat argb_img;
        cv::cvtColor(resizedMat, argb_img, cv::COLOR_RGB2BGRA);
        std::vector<cv::Mat> bgra;
        cv::split(argb_img, bgra);
        std::swap(bgra[0], bgra[3]);
        std::swap(bgra[1], bgra[2]);
        std::memcpy(data, argb_img.data, argb_img.total() * argb_img.elemSize());
    }
}

