#pragma once

#include <Windows.h>
#include "Line.h"

const int NUM_CHANNELS = 3;
const int CHANNEL_X = 1;
const int CHANNEL_Y = 2;
const int CHANNEL_Z = 3;

void Win32Error(const char * message);


enum Direction {Left, Right};


struct IProgrammer
{
	virtual int GetCurrentChannel() = 0;
	virtual void SelectChannel(int channel) = 0;
	virtual void Clear() = 0;
	virtual void EndChannel() = 0;
	virtual void SetEnable(bool enable) = 0;
	virtual void SetMicro(bool micro) = 0;
	virtual void SetDirection(Direction dir) = 0;
	virtual void SetAcceleration(int acc) = 0;
	virtual void SetStartSpeed(int startSpeed) = 0;
	virtual void SetSpeed(int speed) = 0;
	virtual void Move(int steps) = 0;
};


class Programmer : public IProgrammer
{
public:
	Programmer(void);
	virtual ~Programmer(void);

	void OpenPort(LPCTSTR port);
	void ClosePort();

	virtual int GetCurrentChannel() { return currentChannel; }
	virtual void SelectChannel(int channel);
	virtual void Clear();
	virtual void EndChannel();
	virtual void SetEnable(bool enable);
	virtual void SetMicro(bool micro);
	virtual void SetDirection(Direction dir);
	virtual void SetAcceleration(int acc);
	virtual void SetStartSpeed(int startSpeed);
	virtual void SetSpeed(int speed);
	virtual void Move(int steps);

	void Start();

	bool simulation;

private:
	int currentChannel;
	HANDLE com;

	void Command(const char * format, ...);
};


const int DIR_FLAG = 1;
const int SPEED_FLAG = 2;
const int STARTSPEED_FLAG = 4;
const int ENABLED_FLAG = 8;
const int ACC_FLAG = 16;
const int MICRO_FLAG = 32;


struct ChannelState
{
	Direction dir;
	int speed;
	int startspeed;
	bool enabled;
	int acc;
	bool micro;
	int flags;
	bool cleared;
};


class OptimizingProgrammer : public IProgrammer
{
public:
	OptimizingProgrammer(Programmer & prog);
	virtual int GetCurrentChannel();
	virtual void SelectChannel(int channel);
	virtual void Clear();
	virtual void EndChannel();
	virtual void SetEnable(bool enable);
	virtual void SetMicro(bool micro);
	virtual void SetDirection(Direction dir);
	virtual void SetAcceleration(int acc);
	virtual void SetStartSpeed(int startSpeed);
	virtual void SetSpeed(int speed);
	virtual void Move(int steps);

private:
	ChannelState states[NUM_CHANNELS];
	ChannelState * currState;
	Programmer * _prog;
};
