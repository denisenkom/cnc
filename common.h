#pragma once

struct State
{
	double currSpdX;
	double currSpdY;
	double currSpdZ;
	double posX;
	double posY;
	double posZ;
};


// общие параметры
extern double accelH; // максимальное горизонтальное ускорение м/сек^2
extern double accelV; // максимальное вертикальное ускорение м/сек^2
extern double decelH; // максимальное горизонтальное замедление м/сек^2
extern double decelV; // максимальное вертикальное замедление м/сек^2
extern double speedH; // максимальная горизонтальная скорость м/сек
extern double speedV; // максимальная вертикальная скорость м/сек
extern double minSpeedDelta; // минимальная разница между скоростями при которой можно не делать ускорение
const double epsilonLenH = 0.0000025; // минимальная длина для которой можно сделать шаг в горизонтальной плоскости (длина шага в метрах)
double axisSteps[];

// маленькие утилиты
template <class T> inline T sqr(T par) { return par * par; }
template <class T> inline T  hypotenuse(T a, T b) { return sqrt(sqr(a) + sqr(b)); }