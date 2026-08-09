using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace XeF4Core.WPF
{
    public static class EaseFunctions
    {
        static EaseFunctions()
        {
            InFluent = new(3);
            InFluent.Freeze();

            OutFluent = new(3) { EasingMode = EasingMode.EaseOut };
            OutFluent.Freeze();

            InOutFluent = new(3) { EasingMode = EasingMode.EaseInOut };
            InOutFluent.Freeze();

            InSine = new();
            InSine.Freeze();

            OutSine = new() { EasingMode = EasingMode.EaseOut };
            OutSine.Freeze();

            InOutSine = new() { EasingMode=EasingMode.EaseInOut };
            InOutSine.Freeze();
        }
        public static FluentEase InFluent { get; } 
        public static FluentEase OutFluent { get; }
        public static FluentEase InOutFluent { get; }
        public static SineEase InSine { get; }
        public static SineEase OutSine { get; }
        public static SineEase InOutSine { get; }
        
    }
    public class FluentEase(double Power): EasingFunctionBase
    {
        public readonly double Power = Power;
        protected override double EaseInCore(double normalizedTime) =>
            Math.Pow(normalizedTime, Power);
        
        protected override Freezable CreateInstanceCore() => new FluentEase(Power);
    }
    public class FluentSpeedEase(double Power, double Speed) : EasingFunctionBase
    {
        public readonly double Power = Power;
        public readonly double Speed = Speed;
        protected override double EaseInCore(double normalizedTime) =>
            (1 - Speed) * Math.Pow(normalizedTime, Power) + Speed * normalizedTime;
        protected override Freezable CreateInstanceCore() => new FluentSpeedEase(Power, Speed);
    }
    public class StackEase(IEasingFunction easingFunctionA, IEasingFunction easingFunctionB, double EaseARatio = 1, double EaseBRatio = 1) : EasingFunctionBase
    {
        public readonly IEasingFunction EaseA = easingFunctionA;
        public readonly IEasingFunction EaseB = easingFunctionB;
        public readonly double EaseARatio = EaseARatio;
        public readonly double EaseBRatio = EaseBRatio;
        protected override double EaseInCore(double normalizedTime) =>
            (EaseA.Ease(normalizedTime) * EaseARatio + EaseB.Ease(normalizedTime) * EaseBRatio) / (EaseARatio + EaseBRatio);

        protected override Freezable CreateInstanceCore() => new StackEase(EaseA, EaseB, EaseARatio, EaseBRatio);
    }
    
}
