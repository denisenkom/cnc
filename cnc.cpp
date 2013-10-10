// test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "line.h"
#include <iostream>
#include <vector>
#include <algorithm>
#include <string>
#include <sstream>
#include <cmath>
#include <cstdarg>
#include <Windows.h>
#include "Programmer.h"

using std::cerr;
using std::cout;
using std::endl;


// общие параметры
double accelH = 0.00125; // максимальное горизонтальное ускорение м/сек^2
double accelV = 0.00125; // максимальное вертикальное ускорение м/сек^2
double decelH = 0.00125; // максимальное горизонтальное замедление м/сек^2
double decelV = 0.00125; // максимальное вертикальное замедление м/сек^2
double speedH = 0.005; // максимальная горизонтальная скорость м/сек
double speedV = 0.005; // максимальная вертикальная скорость м/сек

// количества шагов в метре по осям
// индексы должны соответствовать номерам каналов - 1
double axisSteps[] = {
	500*400,
	1000*400,
	1000*400,
};

double minSpeedDelta = 0.0005; // минимальная разница между скоростями при которой можно не делать ускорение

void ProgramAxis(IProgrammer & prog, const LineParams & params)
{
	LineSteps steps;
	double stepsPerMeter = axisSteps[prog.GetCurrentChannel() - 1];
	ToSteps(params, steps, stepsPerMeter);
	int decsteps = 0;
	if (steps.dec != 0)
	{
		decsteps = int(params.v*params.v / 2.0 / params.dec * stepsPerMeter);
	}

	// устанавливаем направление вращения
	if (steps.disp < 0)
	{
		prog.SetDirection(Left);
		steps.disp = -steps.disp;
	} 
	else
	{
		prog.SetDirection(Right);
	}

	// устанавливаем ускорение
	if (steps.acc != 0)
	{
		prog.SetAcceleration(steps.acc);
		prog.SetStartSpeed(steps.vstart);
	}
	else
	{
		prog.SetAcceleration(0);
	}

	// устанавливаем скорости
	prog.SetSpeed(steps.v);

	// отсчитываем шаги до конца если нет замедления
	// или до точки замедления если она есть
	prog.Move(steps.disp - decsteps);

	if (steps.dec != 0)
	{
		prog.SetAcceleration(-steps.dec);
		// начальная скорость - текущая
		prog.SetStartSpeed(steps.v);
		// конечная скорость
		prog.SetSpeed(steps.vend);
		// отсчитываем шаги замедления
		prog.Move(decsteps);
	}
}

void Test1(IProgrammer & prog)
{
	LineParams paramX, paramY;
	Project2D(0.010, 0.010, 0/*speedH / 10*/, speedH, 0/*speedH / 10*/, 0/*accelH*/, 0/*decelH*/, paramX, paramY);
	Project2D(0.010, 0.010, 0/*speedH / 10*/, speedH, 0/*speedH / 10*/, 0/*accelH*/, 0/*decelH*/, paramX, paramY);

	prog.SelectChannel(CHANNEL_X);
	prog.Clear();
	//prog.MicroOn();
	prog.SetEnable(true);
	ProgramAxis(prog, paramX);
	prog.SetEnable(false);
	prog.EndChannel();

	prog.SelectChannel(CHANNEL_Y);
	prog.Clear();
	//prog.MicroOn();
	prog.SetEnable(true);
	ProgramAxis(prog, paramX);
	prog.SetEnable(false);
	prog.EndChannel();
}

void Test2(IProgrammer & prog)
{
	double radius = 0.030; // 3 см
	const int segments = 20;
	LineParams paramsX[segments];
	LineParams paramsY[segments];

	double fromx = radius, fromy = 0;
	double segangle = 2 * acos(-1.0) / segments;
	for (int i = 0; i < segments; i++)
	{
		double x = cos((i + 1) * segangle) * radius;
		double y = sin((i + 1) * segangle) * radius;
		Project2D(x - fromx, y - fromy, 0, speedH, 0, 0, 0, paramsX[i], paramsY[i]);
		fromx = x;
		fromy = y;
	}

	prog.SelectChannel(CHANNEL_X);
	prog.Clear();
	prog.SetEnable(true);
	for (int i = 0; i < segments; i++)
		ProgramAxis(prog, paramsX[i]);
	prog.SetEnable(false);
	prog.EndChannel();

	prog.SelectChannel(CHANNEL_Y);
	prog.Clear();
	prog.SetEnable(true);
	for (int i = 0; i < segments; i++)
		ProgramAxis(prog, paramsY[i]);
	prog.SetEnable(false);
	prog.EndChannel();

}

int _tmain(int argc, _TCHAR* argv[])
{
	Programmer prog;
	OptimizingProgrammer oprog(prog);
	if (argc >= 2 && _tcscmp(argv[1], TEXT("sym")) == 0)
	{
		prog.simulation = true;
	}
	setlocale(LC_ALL, "");

	prog.OpenPort(TEXT("COM1"));

	Test2(oprog);

	// запускаем программу
	prog.Start();

	prog.ClosePort();
}

