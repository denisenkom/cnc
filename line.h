#pragma once
#include "common.h"

// параметры линейного движения по оси
struct LineParams
{
	double acc; // ускорение [м/с^2]
	double dec; // замедление [м/с^2]
	double vstart; // начальная скорость [м/с]
	double v; // целевая скорость движения [м/с]
	double vend;
	double disp; // сколько всего нужно сдвинуться [м]
};

struct LineSteps
{
	int acc; // ускорение [шагов/с^2]
	int dec; // замедление [шагов/с^2]
	int vstart; // начальная скорость [шагов/с]
	int v; // целевая скорость движения [шагов/с]
	int vend;
	int disp; // сколько всего нужно сдвинуться [шагов]
};

// выполняет расчёт линейного движения по 3м осям с скоростью
// заданной в виде доли от максимальной скорости
// выполняя ускорение и замедление, если нужно
// параметры:
//  state - исходное состояние машины
//  dx, dy, dz - смещение в метрах
//  spd - доля максимальной скорости (0..1]
//  dodecel - делать замедление, т.е. сделать остановку после прохождения дистанции
//  lineParams - параметры движения для осей 0-х 1-y 2-z
//void Line(State & state, double dx, double dy, double dz, double spd, bool dodecel, LineParams lineParams[3]);

void Project2D(double dx, double dy, double startSpeed, double speed, double endSpeed, double accel, double deccel, LineParams & paramX, LineParams & paramY);

void ToSteps(const LineParams & param, LineSteps & steps, double stepsPerMeter);