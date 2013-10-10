#pragma once
#include "common.h"

// ��������� ��������� �������� �� ���
struct LineParams
{
	double acc; // ��������� [�/�^2]
	double dec; // ���������� [�/�^2]
	double vstart; // ��������� �������� [�/�]
	double v; // ������� �������� �������� [�/�]
	double vend;
	double disp; // ������� ����� ����� ���������� [�]
};

struct LineSteps
{
	int acc; // ��������� [�����/�^2]
	int dec; // ���������� [�����/�^2]
	int vstart; // ��������� �������� [�����/�]
	int v; // ������� �������� �������� [�����/�]
	int vend;
	int disp; // ������� ����� ����� ���������� [�����]
};

// ��������� ������ ��������� �������� �� 3� ���� � ���������
// �������� � ���� ���� �� ������������ ��������
// �������� ��������� � ����������, ���� �����
// ���������:
//  state - �������� ��������� ������
//  dx, dy, dz - �������� � ������
//  spd - ���� ������������ �������� (0..1]
//  dodecel - ������ ����������, �.�. ������� ��������� ����� ����������� ���������
//  lineParams - ��������� �������� ��� ���� 0-� 1-y 2-z
//void Line(State & state, double dx, double dy, double dz, double spd, bool dodecel, LineParams lineParams[3]);

void Project2D(double dx, double dy, double startSpeed, double speed, double endSpeed, double accel, double deccel, LineParams & paramX, LineParams & paramY);

void ToSteps(const LineParams & param, LineSteps & steps, double stepsPerMeter);