#include "stdafx.h"
#include "Programmer.h"
#include "Common.h"


using std::cerr;
using std::cout;
using std::endl;
using std::invalid_argument;


void Win32Error(const char * message)
{
	DWORD err = GetLastError();
	cerr << message << ", код ошибки " << err;
	exit(1);
}

Programmer::Programmer(void) : simulation(false)
{
}

Programmer::~Programmer(void)
{
}

void Programmer::ClosePort()
{
	if (!CloseHandle(com))
	{
		Win32Error("Ошибка вызова CloseHandle(hCom)");
	}
}

void Programmer::Command(const char * format, ...)
{
	char buffer[256];
	int res;
	va_list va;
	va_start(va, format);
	res = vsprintf_s(buffer, format, va);
	va_end(va);
	DWORD written;
	if (!WriteFile(com, buffer, res, &written, 0))
	{
		Win32Error("Ошибка записи в КОМ порт");
	}
	cout << "Отправлено сообщение " << buffer << endl;

	char lastChar = 0;
	char * pos = buffer;

	//cout << "Ожидаю ответа..." << endl;
	if (simulation)
	{
		strcpy_s(buffer, "E10*");
		pos += strlen(pos);
	}
	else
	{
		// загружаем команду
		while (lastChar != '*')
		{
			DWORD read;
			if (!ReadFile(com, &lastChar, 1, &read, 0))
			{
				Win32Error("Ошибка чтения из КОМ порта");
			}
			cout << lastChar;
			*pos++ = lastChar;
		}
		lastChar = 0;
		pos = buffer;
		// загружаем ответ
		while (lastChar != '*')
		{
			DWORD read;
			if (!ReadFile(com, &lastChar, 1, &read, 0))
			{
				Win32Error("Ошибка чтения из КОМ порта");
			}
			cout << lastChar;
			*pos++ = lastChar;
		}
	}
	*pos = 0;
	cout << endl;

	int errid;
	res = sscanf_s(buffer, "E%d*", &errid);
	if (res == EOF)
	{
		cerr << "Ошибка чтения из КОМ порта, неправильный формат ответа " << buffer;
		exit(1);
	}
	else if (res == 0)
	{
		cerr << "Ошибка чтения из КОМ порта, порт не отвечает";
		exit(1);
	}
	switch(errid % 10)
	{
	case 0: return; // нормальное завершение команды
	case 2:
		cerr << "Срабатывание аварийного выключателя";
		exit(1);
	case 3:
		cerr << "Ошибка кода исполнительной программы";
		exit(1);
	case 4:
		cerr << "Завершение исполнительно";
		exit(1);
	case 5:
		cerr << "Ошибка приёма по ком порту";
		exit(1);
	case 6:
		cerr << "Ошибка команды";
		exit(1);
	case 9:
		cerr << "Ошибка данных команды";
		exit(1);
	default:
		cerr << "Неизвестная ошибка с кодом " << errid;
		exit(1);
	}
}

void Programmer::OpenPort(LPCTSTR port)
{
	// открытие и настройка порта
	com = CreateFile(TEXT("COM1"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
	if (com == INVALID_HANDLE_VALUE)
	{
		Win32Error("Ошибка открытия КОМ порта");
	}

	DCB dcb;
	if (!GetCommState(com, &dcb))
	{
		Win32Error("Ошибка вызова GetCommState");
	}

	dcb.BaudRate = CBR_9600;
	dcb.ByteSize = 8;
	dcb.Parity = EVENPARITY;
	dcb.StopBits = ONESTOPBIT;
	if (!SetCommState(com, &dcb))
	{
		Win32Error("Ошибка вызова SetCommState");
	}
}

void Programmer::SelectChannel(int channel)
{
	currentChannel = channel;
	Command("LD%d*", channel); // выбираем канал
}

void Programmer::Clear()
{
	Command("BG*"); // очищаем программу канала
}

void Programmer::SetMicro(bool micro)
{
	if (micro)
		Command("ON*");
	else
		Command("OF*");
}

void Programmer::SetEnable(bool enable)
{
	if (enable)
		Command("EN*");
	else
		Command("DS*");
}

void Programmer::EndChannel()
{
	Command("ED*"); // заканчиваем работу с каналом
}

void Programmer::Start()
{
	Command("ST*");
}

void Programmer::SetDirection(Direction dir)
{
	if (dir == Left)
		Command("DL*");
	else
		Command("DR*");
}

void Programmer::SetAcceleration(int acc)
{
	Command("AL%d*", acc);
}

void Programmer::SetStartSpeed(int startSpeed)
{
	Command("SS%d*", startSpeed);
}

void Programmer::SetSpeed(int speed)
{
	Command("SD%d*", speed);
}

void Programmer::Move(int steps)
{
	Command("MV%d*", steps);
}


OptimizingProgrammer::OptimizingProgrammer(Programmer & prog)
: currState(0), _prog(&prog)
{
	states[CHANNEL_X - 1].flags = 0;
	states[CHANNEL_X - 1].cleared = false;
	states[CHANNEL_Y - 1].flags = 0;
	states[CHANNEL_Y - 1].cleared = false;
	states[CHANNEL_Z - 1].flags = 0;
	states[CHANNEL_Z - 1].cleared = false;
}

int OptimizingProgrammer::GetCurrentChannel()
{
	return int(currState - states) / sizeof(states[0]) + 1;
}

void OptimizingProgrammer::SelectChannel(int channel)
{
	if (currState != &states[channel - 1])
	{
		if (currState != 0)
		{
			throw invalid_argument("InvalidSelectChannelCall");
		}
		currState = &states[channel - 1];
		_prog->SelectChannel(channel);
	}
}

void OptimizingProgrammer::Clear()
{
	if (!currState->cleared)
	{
		_prog->Clear();
		currState->cleared = true;
	}
}

void OptimizingProgrammer::EndChannel()
{
	if (currState != 0)
	{
		_prog->EndChannel();
		currState = 0;
	}
}

void OptimizingProgrammer::SetEnable(bool enable)
{
	if ((currState->flags & ENABLED_FLAG) == 0 || currState->enabled != enable)
	{
		currState->enabled = enable;
		currState->flags |= ENABLED_FLAG;
		_prog->SetEnable(enable);
	}
}

void OptimizingProgrammer::SetMicro(bool micro)
{
	if ((currState->flags & MICRO_FLAG) == 0 || currState->micro != micro)
	{
		currState->micro = micro;
		currState->flags |= MICRO_FLAG;
		_prog->SetMicro(micro);
	}
}


void OptimizingProgrammer::SetDirection(Direction dir)
{
	if ((currState->flags & DIR_FLAG) == 0 || currState->dir != dir)
	{
		currState->dir = dir;
		currState->flags |= DIR_FLAG;
		_prog->SetDirection(dir);
	}
}

void OptimizingProgrammer::SetAcceleration(int acc)
{
	if ((currState->flags & ACC_FLAG) == 0 || currState->acc != acc)
	{
		currState->acc = acc;
		currState->flags |= ACC_FLAG;
		_prog->SetAcceleration(acc);
	}
}

void OptimizingProgrammer::SetStartSpeed(int startSpeed)
{
	if ((currState->flags & STARTSPEED_FLAG) == 0 || currState->startspeed != startSpeed)
	{
		currState->startspeed = startSpeed;
		currState->flags |= STARTSPEED_FLAG;
		_prog->SetStartSpeed(startSpeed);
	}
}

void OptimizingProgrammer::SetSpeed(int speed)
{
	if ((currState->flags & SPEED_FLAG) == 0 || currState->speed != speed)
	{
		currState->speed = speed;
		currState->flags |= SPEED_FLAG;
		_prog->SetSpeed(speed);
	}
}

void OptimizingProgrammer::Move(int steps)
{
	_prog->Move(steps);
}
