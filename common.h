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


// ����� ���������
extern double accelH; // ������������ �������������� ��������� �/���^2
extern double accelV; // ������������ ������������ ��������� �/���^2
extern double decelH; // ������������ �������������� ���������� �/���^2
extern double decelV; // ������������ ������������ ���������� �/���^2
extern double speedH; // ������������ �������������� �������� �/���
extern double speedV; // ������������ ������������ �������� �/���
extern double minSpeedDelta; // ����������� ������� ����� ���������� ��� ������� ����� �� ������ ���������
const double epsilonLenH = 0.0000025; // ����������� ����� ��� ������� ����� ������� ��� � �������������� ��������� (����� ���� � ������)
double axisSteps[];

// ��������� �������
template <class T> inline T sqr(T par) { return par * par; }
template <class T> inline T  hypotenuse(T a, T b) { return sqrt(sqr(a) + sqr(b)); }