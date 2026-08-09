using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.CS.Modules
{
    public class AniEaseLinear : AniEase
    {
        public AniEaseLinear() { }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return t;
        }
    }
    public class AniEaseInFluent : AniEase
    {
        private readonly int _power;
        public AniEaseInFluent(int power = 2)
        {
            _power = power;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return Math.Pow(t, _power);
        }
    }
    public class AniEaseOutFluent : AniEase
    {
        private readonly int _power;
        public AniEaseOutFluent(int power = 2)
        {
            _power = power;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return 1.0-Math.Pow(1.0-t, _power);
        }
    }
    public class AniEaseInBack : AniEase
    {
        private readonly double _power;
        public AniEaseInBack(double Power=2)
        {
            _power = 3 - Power * 0.5;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return Math.Pow(t, _power) * Math.Cos(1.5 * Math.PI * (1 - t));
        }
    }
    public class AniEaseOutBack : AniEase
    {
        private readonly double _power;
        public AniEaseOutBack(double Power = 2)
        {
            _power = 3 - Power * 0.5;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return 1 - Math.Pow(1 - t,_power) * Math.Cos(1.5 * Math.PI * t);
        }
    }
    public class AniEaseInElastic : AniEase
    {
        private readonly double _power;
        public AniEaseInElastic(double Power=2)
        {
            _power = Power;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return Math.Cos((1 - t) * _power * Math.PI) * t;
        }
    }
    public class AniEaseOutElastic : AniEase
    {
        private readonly double _power;
        public AniEaseOutElastic(double Power = 2)
        {
            _power = Power;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return -Math.Cos(t * _power * Math.PI) * (1 - t) + 1;
        }
    }
    public class AniEaseFluentSpeed : AniEase
    {
        private readonly double _power;
        private readonly double _speed;
        public AniEaseFluentSpeed(double Power = 2, double Speed = 2)
        {
            _power = Power;
            _speed = Speed;
        }
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return (1 - _speed) * Math.Pow(t, _power) + _speed * t;
        }
    }
    public class AniEaseInSine:AniEase
    {
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return 1 - Math.Cos(0.5*t * Math.PI);
        }
    }
    public class AniEaseOutSine : AniEase
    {
        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            return Math.Sin(0.5*t * Math.PI);
        }
    }
    public class AniEaseAdd : AniEase
    {
        public readonly AniEase Ease1;
        public readonly AniEase Ease2;
        public readonly double Add1;
        public readonly double Add2;
        public AniEaseAdd(AniEase ease1, AniEase ease2, double add1 = 1, double add2 = 1)
        {
            Ease1 = ease1;
            Ease2 = ease2;
            Add1 = add1 / (add1 + add2);
            Add2 = add2 / (add1 + add2);
        }
        public override double GetValue(double t)
        {
            return Ease1.GetValue(t)*Add1 + Ease2.GetValue(t)*Add2;
        }
    }
    public class AniEaseInOut : AniEase
    {
        private readonly AniEase _easeIn;
        private readonly AniEase _easeOut;
        private readonly double _middle;
        private readonly double _midValue;
        public AniEaseInOut(AniEase EaseIn, AniEase EaseOut, double middle = 0.5,double midValue=0.5)
        {
            _easeIn = EaseIn;
            _easeOut = EaseOut;
            _middle = middle;
            _midValue = midValue;
        }

        public override double GetValue(double t)
        {
            t = MathHelper.Clamp(t, 0, 1);
            if (t < _middle)
            {
                return _midValue * _easeIn.GetValue(t / _middle);
            }
            else
            {
                return (1- _midValue) * _easeOut.GetValue((t - _middle) / (1 - _middle)) + _middle;
            }
        }
    }
}
