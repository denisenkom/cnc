using System;
using System.Collections.Generic;

namespace dxfloader.Mc1
{
    class Command
    {
    }

    class SetMicro : Command
    {
        public bool Micro;

        SetMicro(bool micro) { Micro = micro; }
    }

    enum DirectionType { Left, Right }
    class SetDirection : Command
    {
        public DirectionType Dir;

        public SetDirection(DirectionType dir) { Dir = dir; }
    }

    class SetAccel : Command
    {
        public int Accel;

        SetAccel(int accel) { Accel = accel; }
    }

    class SetStartSpeed : Command
    {
        public int Speed; // шагов в секунду

        SetStartSpeed(int speed) { Speed = speed; }
    }

    class SetSpeed : Command
    {
        public int Speed; // шагов в секунду

        public SetSpeed(int speed) { Speed = speed; }
    }

    class MakeMove : Command
    {
        public int Steps;

        MakeMove(int steps) { Steps = steps; }
    }

    class Program
    {
        public LinkedList<Command> ChannelX = new LinkedList<Command>();
        public LinkedList<Command> ChannelY = new LinkedList<Command>();
    }

    class Loader
    {
        public static void Upload(Program program)
        {
        }

        public static void Execute()
        {
        }
    }

    class Generator
    {
        const DirectionType PositiveXDir = DirectionType.Left;
        const DirectionType NegativeXDir = DirectionType.Right;
        const DirectionType PositiveYDir = DirectionType.Left;
        const DirectionType NegativeYDir = DirectionType.Right;
        const int Speed = 1000; // шагов в секунду
        const int XStepsPerMeter = 500 * 400;
        const int YStepsPerMeter = 1000 * 400;
        // минимальный угол изменения направления при котором скорость не снижается
        const double MinCollAngle = 3 * Math.PI / 180; // 3 градуса, в радианах

        class IR2Prog
        {
            void Visit(IRLine line)
            {
                if (line.Dx != 0)
                {
                    EmitX(new SetDirection(line.Dx > 0 ? PositiveXDir : NegativeXDir));
                    EmitX(new SetSpeed(Speed));
                }
            }

            void EmitX(Command cmd)
            {
            }

            void EmitY(Command cmd)
            {
            }
        }

        public static Program Generate(LinkedList<IRCommand> irprog)
        {
            return null;
        }
    }
}
