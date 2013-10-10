#include "stdafx.h"
#include <stdexcept>
#include "common.h"
#include "line.h"

using std::invalid_argument;

// выполняет линейное движение по 3м осям с скоростью
// заданной в виде доли от максимальной скорости
// выполняя ускорение и замедление, если нужно
// параметры:
//  state - исходное состояние машины
//  com - ком порт
//  dx, dy, dz - смещение в метрах
//  spd - доля максимальной скорости (0..1]
//  dodecel - делать замедление, т.е. сделать остановку после прохождения дистанции
/*void Line(State & state, double dx, double dy, double dz, double spd, bool dodecel, LineParams lineParams[3])
{
	LineParams & paramX = lineParams[0];
	LineParams & paramY = lineParams[1];
	LineParams & paramZ = lineParams[2];

	// инициализируем выходные параметры
	paramX.disp = 0;
	paramX.v = 0;
	paramX.acc = 0;
	paramX.dec = 0;
	paramY.disp = 0;
	paramY.v = 0;
	paramY.acc = 0;
	paramY.dec = 0;
	paramZ.disp = 0;
	paramZ.v = 0;
	paramZ.acc = 0;
	paramZ.dec = 0;

	if (dx == 0 && dy == 0 && dz == 0)
	{
		return;
	}

	// определяем скорости и ускорения
	double targetSpeedH = speedH * spd;
	double targetSpeedV = speedV * spd;
	double targetAccH = accelH * spd;
	double targetDecH = decelH * spd;
	double targetAccV = accelV * spd;
	double targetDecV = decelV * spd;

	// определяем направления движения по осям
	paramX.rev = false;
	paramY.rev = false;
	paramZ.rev = false;
	if (dx < 0)
	{
		dx = -dx;
		paramX.rev = true;
	}
	if (dy < 0)
	{
		dy = -dy;
		paramY.rev = true;
	}
	if (dz < 0)
	{
		dz = -dz;
		paramZ.rev = true;
	}
	paramX.disp = dx;
	paramY.disp = dy;
	paramZ.disp = dz;

	// вычисляем проекции скоростей и ускорений по осям
	double sH = 0;
	if (dx != 0 || dy != 0)
	{
		sH = hypotenuse(dx, dy); // горизонтальная гипотенуза
		paramX.v0 = state.currSpdX;
		paramX.v = targetSpeedH * dx / sH; // проекция скорости на X
		paramY.v0 = state.currSpdY;
		paramY.v = targetSpeedH * dy / sH; // проекция скорости на Y

		bool doaccel = fabs(paramX.v - state.currSpdX) > minSpeedDelta ||
			fabs(paramY.v - state.currSpdY) > minSpeedDelta;

		if (doaccel)
		{
			if (state.currSpdX != 0 || state.currSpdY != 0)
			{
				cerr << "Слишком крутой угол поворота в горизонтальной плоскости, нужно сделать остановку потом разгон.";
				exit(1);
			}
			paramX.acc = targetAccH * dx / sH;
			paramY.acc = targetAccH * dy / sH;
		}
		if (dodecel)
		{
			paramX.dec = targetDecH * dx / sH;
			paramY.dec = targetDecH * dy / sH;
			paramX.decLen = sqr(targetSpeedH) / 2.0 / targetDecH;
		}
		if (doaccel && dodecel)
		{
			// проверка случая, когда расстояние слишком мало чтобы набрать полную скорость
			// и сделать полное торможение
			double decelLen = sqr(targetSpeedH) / 2.0 / targetDecH;
			double accelLen = sqr(targetSpeedH - hypotenuse(state.currSpdX, state.currSpdY)) / 2.0 / targetAccH;
			if (accelLen + decelLen > hypotenuse(dx, dy))
			{
				// определяем длину ускорения до момента когда нужно будет делать замедление
				accelLen = targetDecH * hypotenuse(dx, dy) / (targetAccH + targetDecH);
				if (accelLen < epsilonLenH)
				{
					accelLen = 0;
				}
				paramX.decLen = 
			}
		}
	}
	if (dz != 0)
	{
		paramZ.v0 = state.currSpdZ;
		double sV = hypotenuse(sH, dz); // вертикальная гипотенуза
		paramZ.v = targetSpeedV * dz / sV;
		bool doaccel = fabs(paramZ.v - state.currSpdZ) > minSpeedDelta;
		if (doaccel)
		{
			if (state.currSpdZ != 0)
			{
				cerr << "Слишком крутой угол поворота в вертикальной плоскости, нужно сделать остановку потом разгон.";
				exit(1);
			}
			paramZ.acc = targetAccV * dz / sV;
		}
		if (dodecel)
			paramZ.dec = targetDecV * dz / sV;
	}
}*/


void Project2D(double dx, double dy, double startSpeed, double speed, double endSpeed, double accel, double deccel, LineParams & paramX, LineParams & paramY)
{
	if (dx == 0 || dy == 0)
	{
		throw invalid_argument("InvalidDxDy");
	}
	if (deccel != 0 && endSpeed == 0)
	{
		throw invalid_argument("InvalidDeccelEndSpeed");
	}

	double sH = hypotenuse(dx, dy);

	// проекция на X
	double xProj = fabs(dx / sH);
	paramX.disp = dx;
	paramX.vstart = startSpeed * xProj;
	paramX.v = speed * xProj;
	paramX.vend = endSpeed * xProj;
	paramX.acc = accel * xProj;
	paramX.dec = deccel * xProj;

	// проекция на Y
	double yProj = fabs(dy / sH);
	paramY.disp = dy;
	paramY.vstart = startSpeed * yProj;
	paramY.v = speed * yProj;
	paramY.vend = endSpeed * yProj;
	paramY.acc = accel * yProj;
	paramY.dec = deccel * yProj;
}

void ToSteps(const LineParams & param, LineSteps & steps, double stepsPerMeter)
{
	steps.disp = param.disp * stepsPerMeter;
	steps.acc = param.acc * stepsPerMeter;
	steps.vstart = param.vstart * stepsPerMeter;
	steps.v = param.v * stepsPerMeter;
	steps.vend = param.vend * stepsPerMeter;
	steps.dec = param.dec * stepsPerMeter;
}
