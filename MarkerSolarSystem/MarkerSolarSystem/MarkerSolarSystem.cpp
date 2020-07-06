#include "include/GL/glew.h"		
#include "include/GLFW/glfw3.h" 
#include "opencv2/opencv.hpp"  
#include <iostream>

#pragma comment(lib, "OpenGL32.lib")
#pragma comment(lib, "lib/glew32.lib")
#pragma comment(lib, "lib/glfw3.lib")

using namespace cv;
using namespace std;

void window_resized(GLFWwindow* window, int width, int height);

void key_pressed(GLFWwindow* window, int key, int scancode, int action, int mods);
void show_glfw_error(int error, const char* description);

VideoCapture* capture;
Mat img_cam;
int screenW;
int screenH;

GLuint texture_background;

// 카메라 초기화
void cameraInit()
{

	//capture = new VideoCapture(0);
	capture = new VideoCapture("../image/sample2.mp4");
	if (!capture->isOpened())
	{
		cout << "영상파일이 로딩되지 않았습니다." << endl;
		return;
	}
	else
	{
		cout << "영상파일 로딩 성공" << endl;
	}


	if (!capture) {
		printf("Could not capture a camera\n\7");
		return;
	}

	Mat img_frame;

	capture->read(img_frame);

	screenW = img_frame.cols;
	screenH = img_frame.rows;
}

//OpenCV Mat을 OpenGL Texture로 변환 
GLuint MatToTexture(Mat image)
{
	if (image.empty())  return -1;

	//OpenGL 텍스처 생성
	GLuint textureID;
	glGenTextures(1, &textureID);

	//텍스처 ID를 바인딩 -  사용할 텍스처 차원을 지정해준다.
	glBindTexture(GL_TEXTURE_2D, textureID);

	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);

	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, image.cols, image.rows,
		0, GL_RGB, GL_UNSIGNED_BYTE, image.ptr());

	return textureID;
}

void draw_background()
{
	int x = screenW / 100.0;
	int y = screenH / 100.0;

	glBegin(GL_QUADS);
	glTexCoord2f(0.0, 1.0); glVertex3f(-x, -y, 0.0);
	glTexCoord2f(1.0, 1.0); glVertex3f(x, -y, 0.0);
	glTexCoord2f(1.0, 0.0); glVertex3f(x, y, 0.0);
	glTexCoord2f(0.0, 0.0); glVertex3f(-x, y, 0.0);
	glEnd();
}

int main()
{
	glfwSetErrorCallback(show_glfw_error);


	if (!glfwInit()) {
		std::cerr << "GLFW 초기화 실패" << '\n';
		exit(-1);
	}

	cameraInit();

	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);

	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);


	GLFWwindow* window = glfwCreateWindow(
		800, // width
		600, // height
		"OpenGL Example",

		NULL, NULL);
	if (!window)
	{
		std::cerr << "윈도우 생성 실패" << '\n';
		glfwTerminate();
		exit(-1);
	}


	glfwMakeContextCurrent(window);


	glfwSetWindowSizeCallback(window, window_resized);
	glfwSetKeyCallback(window, key_pressed);


	glfwSwapInterval(1);


	glewExperimental = GL_TRUE;
	GLenum err = glewInit();
	if (err != GLEW_OK) {
		std::cerr << "GLEW 초기화 실패 " << glewGetErrorString(err) << '\n';
		glfwTerminate();
		exit(-1);
	}


	std::cout << glGetString(GL_VERSION) << '\n';


	int nr_extensions = 0;
	glGetIntegerv(GL_NUM_EXTENSIONS, &nr_extensions);

	for (int i = 0; i < nr_extensions; ++i) {
		std::cout << glGetStringi(GL_EXTENSIONS, i) << '\n';
	}

	glGenTextures(1, &texture_background);

	//화면 지울때 사용할 색 지정
	glClearColor(0.0, 0.0, 0.0, 0.0);

	//깊이 버퍼 지울 때 사용할 값 지정
	glClearDepth(1.0);

	//깊이 버퍼 활성화
	glEnable(GL_DEPTH_TEST);


	while (!glfwWindowShouldClose(window)) {

		//화면을 지운다. (컬러버퍼와 깊이버퍼)
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		//이후 연산은 ModelView Matirx에 영향을 준다.
		glMatrixMode(GL_MODELVIEW);
		glLoadIdentity();

		//웹캠으로부터 이미지 캡처
		capture->read(img_cam);
		
		texture_background = MatToTexture(img_cam);
		if (texture_background < 0) return 0;

		glEnable(GL_TEXTURE_2D);
		glColor3f(1.0f, 1.0f, 1.0f); //큐브나 좌표축 그릴 때 사용한 색의 영향을 안받을려면 필요
		glBindTexture(GL_TEXTURE_2D, texture_background);
		glPushMatrix();
		glTranslatef(0.0, 0.0, -9.0);
		draw_background(); //배경그림
		glPopMatrix();
		glDisable(GL_TEXTURE_2D);


		glfwSwapBuffers(window);

		glfwPollEvents();
	}


	glfwDestroyWindow(window);

	glfwTerminate();
	return 0;
}

void show_glfw_error(int error, const char* description) {
	std::cerr << "Error: " << description << '\n';
}

void window_resized(GLFWwindow* window, int width, int height) {
	std::cout << "Window resized, new window size: " << width << " x " << height << '\n';

	glClearColor(0, 0, 1, 1);
	glClear(GL_COLOR_BUFFER_BIT);
	glfwSwapBuffers(window);
}

void key_pressed(GLFWwindow* window, int key, int scancode, int action, int mods) {
	if (key == 'Q' && action == GLFW_PRESS) {
		glfwTerminate();
		exit(0);
	}
}