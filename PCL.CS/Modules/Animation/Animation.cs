using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PCL.CS.Modules
{
    /// <summary>
    /// 动画引擎
    /// </summary>
    public static class Animator
    {
        /// <summary>
        /// 动画帧数
        /// </summary>
        public static int AniFPS = 60;
        /// <summary>
        /// 动画速度。当它=0时代表不进行动画
        /// </summary>
        public static double AniSpeed
        {
            get => Config.Current.AnimationSpeed;
            set
            {
                Config.Current.AnimationSpeed = value;
                Config.Save();
            }
        }
        //private static readonly object _animationLock = new object();
        //private static Thread AnimationThread;
        public static bool IsRunning = false;
        private static Dispatcher Dispatcher { get; set; }

        public static event Action<Animation> AddAnimationAction;
        public static event Action<Animation> RemoveAnimationAction;

        #region 私有类定义
        private class AniInfo
        {
            public TimeSpan RunTime;
            public int RepeatCounter = 1;
            public bool HasStarted = false;
            public Animation Animation;
            //public List<bool> BrushAniFinished = new List<bool>();
            public AniInfo(Animation Animation)
            {
                this.Animation = Animation;
                this.RunTime = TimeSpan.Zero;
            }
            public void AddRepeadCounter()
            {
                RunTime -= Animation.TotalTime;
                RepeatCounter++;
            }
        }
        private struct Command
        {
            public enum CommandType
            {
                Add,
                Remove,
                EndThread
            }
            public CommandType Type;
            public Animation Animation;
        }
        #endregion

        #region 私有成员

        private static readonly List<AniInfo> AnimationRunTime = new List<AniInfo>();

        private static readonly Dictionary<Animation, AniInfo> AnimationRunTimeDictionary = new Dictionary<Animation, AniInfo>();

        private static readonly Queue<Command> AnimationCommand = new Queue<Command>();
        //private static Dictionary<AniInfo, Animation> InfoToAnim = new Dictionary<AniInfo, Animation>();

        #endregion
        public static void StartThread()
        {
            Base.Log("[Thread]动画主进程已启动");
            //AnimationThread = Base.RunInNewThread(AniMainThread, "Animation", ThreadPriority.AboveNormal, true);
            Dispatcher = Dispatcher.CurrentDispatcher;
            IsRunning = true;
            Base.RunInNewThread(AniThread, "Animation", ThreadPriority.AboveNormal);
        }

        public static void StopThread()
        {
            IsRunning = false;
        }

        private static void AniThread()
        {
            Stopwatch Watch = Stopwatch.StartNew();
            TimeSpan RunTime = TimeSpan.Zero;
            TimeSpan TickTime;
            while (IsRunning)
            {
                var Time = Watch.Elapsed;
                TickTime = Time - RunTime;
                RunTime = Time;
                Task taska = Task.Delay(1000 / AniFPS);
                AniLoop(TickTime);
                taska.Wait();
            }
        }
        #region 动画主逻辑
        private static DispatcherOperation DispatcherOperation { get; set; }

        private static int Counter = 0;
        private static bool AniLoop(TimeSpan Tick)
        {
            if (Counter >= 40)
            {
                Base.Log($"当前运行动画数量：{AnimationRunTime.Count}");
                Counter = 0;
            }
            Counter++;

            try
            {
                DispatcherOperation?.Wait();
            }
            catch (Exception Ex)
            {
                Base.Log(Ex);
            }

            if (Tick.TotalMilliseconds > 1000 / AniFPS * 5.0) Base.Log($"[Animation]动画延迟过高 动画帧间隔{Tick.TotalMilliseconds.ToString()}ms");

            lock (AnimationCommand)
            {
                while (AnimationCommand.Any())
                {
                    Command command = AnimationCommand.Dequeue();
                    switch (command.Type)
                    {
                        case Command.CommandType.Add:
                            {
                                if (AnimationRunTimeDictionary.TryGetValue(command.Animation, out var aniInfo))
                                {
                                    AnimationRunTime.Remove(aniInfo);
                                    command.Animation.OnStop();
                                    RemoveAnimationAction?.Invoke(command.Animation);
                                }
                            }
                            {
                                AniInfo NewAniInfo = new AniInfo(command.Animation);
                                AnimationRunTimeDictionary[command.Animation] = NewAniInfo;
                                AnimationRunTime.Add(NewAniInfo);
                                AddAnimationAction?.Invoke(command.Animation);
                            }
                            //command.Animation.OnStart();
                            break;
                        case Command.CommandType.Remove:
                            {
                                if (AnimationRunTimeDictionary.TryGetValue(command.Animation, out var aniInfo))
                                {
                                    AnimationRunTime.Remove(aniInfo);
                                    command.Animation.OnStop();
                                    RemoveAnimationAction?.Invoke(command.Animation);
                                }
                            }
                            break;
                        case Command.CommandType.EndThread:
                            IsRunning = false;
                            return false;
                    }
                }
                foreach (var aniInfo in AnimationRunTime)
                {
                    var Anim = aniInfo.Animation;
                    aniInfo.RunTime += TimeSpan.FromMilliseconds(
                        Tick.TotalMilliseconds * AniSpeed);
                    var RunTime = aniInfo.RunTime;
                    if (RunTime < Anim.After) continue;
                    var ActRunTime = RunTime - Anim.After;
                    object Value;
                    if (!aniInfo.HasStarted)
                    {
                        aniInfo.HasStarted = true;
                        Anim.OnStart();
                    }
                    if (Anim.TotalTime == TimeSpan.Zero)
                    {
                        Value = Anim.GetValue(1.0);
                        AniRun(Anim, Value);
                        //Anim.OnStop();
                        Stop(Anim);
                        continue;
                    }
                    int ARepeat = Anim.Repeat;
                    if (ARepeat == 0) ARepeat = 1;
                    int AnimRepeat = (int)(ActRunTime.TotalMilliseconds / Anim.TotalTime.TotalMilliseconds);
                    if (AnimRepeat >= ARepeat && ARepeat > 0)
                    {
                        Value = Anim.GetValue(1.0);
                        Stop(Anim);
                    }
                    else
                    {
                        double Time = ActRunTime.TotalMilliseconds % Anim.TotalTime.TotalMilliseconds;
                        double t = Time / Anim.TotalTime.TotalMilliseconds;
                        if (t < 0) t = 0;
                        if (t > 1 || t is double.NaN) t = 1;
                        Value = Anim.GetValue(t);
                    }
                    AniRun(Anim, Value);
                }
                if (AnimRunActions.Count > 0)
                    DispatcherOperation = Dispatcher.BeginInvoke(_RunCommands_AnimRun, null);
            }
            return true;
        }


        private static Action _RunCommands_AnimRun = RunCommands_AnimRun;
        private static void RunCommands_AnimRun()
        {
            foreach (var i in AnimRunActions)
            {
                i.Anim.SetValue(i.Value);
            }
            AnimRunActions.Clear();
        }

        private static List<AnimRunAction> AnimRunActions = new List<AnimRunAction>();
        private delegate void AnimAction(Animation Anim, object Value);
        private struct AnimRunAction
        {
            public Animation Anim { get; set; }
            public object Value { get; set; }
        }

        /// <summary>
        /// 运行动画组
        /// </summary>
        /// <param name="AniGroup">目标动画组</param>
        /// <param name="Run">运行时间</param>
        /// <returns>返回true证明需要移除</returns>
        private static bool AniRun(Animation ThisAnimation, object Value)
        {

            AnimRunActions.Add(new AnimRunAction
            {
                Anim = ThisAnimation,
                Value = Value,
            });

            //if (ThisAnimation.TotalTime + ThisAnimation.After <= Run) Stop(ThisAnimation);

            return false;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始动画。动画将在下一帧开始。
        /// </summary>
        /// <param name="Animation">要开始的动画</param>
        public static void Start(Animation Animation)
        {
            lock (AnimationCommand)
            {
                if (Animation is null) return;

                AniInfo AniInfo = new AniInfo(Animation);
                AniInfo.RepeatCounter = 0;
                AnimationCommand.Enqueue(new Command { Type = Command.CommandType.Add, Animation = Animation });
            }
        }
        /// <summary>
        /// 获取运行时长
        /// </summary>
        /// <param name="Animation">指定的动画</param>
        /// <returns>运行时长</returns>
        public static TimeSpan GetTime(Animation Animation)
        {
            TimeSpan RunTime;
            lock (AnimationCommand)
            {
                if (Animation is null) return TimeSpan.Zero;
                
                if (AnimationRunTimeDictionary.TryGetValue(Animation, out var aniInfo))
                {
                    double RunT = (aniInfo.RunTime - Animation.After).TotalMilliseconds % Animation.TotalTime.TotalMilliseconds;
                    if (Animation.TotalTime == TimeSpan.Zero) RunT = 0;
                    RunTime =
                        TimeSpan.FromMilliseconds(RunT);
                }
                else
                    RunTime = Animation.TotalTime;
                //AnimationCommand.Enqueue(new Command { Type = Command.CommandType.Remove, Animation = Animation });
            }
            return RunTime;
        }
        /// <summary>
        /// 移除指定的动画
        /// </summary>
        /// <param name="Animation">指定的动画</param>
        /// <returns>TimeSpan，表示被移除的动画已经运行的时间。</returns>
        public static TimeSpan Stop(Animation Animation)
        {
            TimeSpan RunTime;
            lock (AnimationCommand)
            {
                if (Animation is null) return TimeSpan.Zero;
                
                if (AnimationRunTimeDictionary.TryGetValue(Animation, out var aniInfo))
                {
                    double RunT = (aniInfo.RunTime - Animation.After).TotalMilliseconds % Animation.TotalTime.TotalMilliseconds;
                    if (Animation.TotalTime == TimeSpan.Zero) RunT = 0;
                    RunTime =
                        TimeSpan.FromMilliseconds(RunT);
                }
                else
                    RunTime = Animation.TotalTime;
                AnimationCommand.Enqueue(new Command { Type = Command.CommandType.Remove, Animation = Animation });
            }
            return RunTime;
        }
        #endregion

    }

    #region 动画定义

    public abstract class Animation
    {
        public virtual TimeSpan TotalTime { get; set; }
        public virtual TimeSpan After { get; set; }
        public virtual int Repeat { get; set; }
        public abstract object GetValue(double t);
        public abstract void SetValue(object value);
        public virtual void OnStart() { }
        public virtual void OnStop() { }
        public static void Start(Animation animation) => Animator.Start(animation);
        public static TimeSpan Stop(Animation animation) => Animator.Stop(animation);
        public static TimeSpan GetTime(Animation animation) => Animator.GetTime(animation);
        public void StartAnimation()
        {
            Start(this);
        }
        public void StopAnimation()
        {
            Stop(this);
        }
    }

    public abstract class PropertyAnimation : Animation
    {
        /// <summary>
        /// 拥有方式
        /// </summary>
        public enum PropertyOwnType
        {
            /// <summary>
            /// 不使用属性
            /// </summary>
            None,
            /// <summary>
            /// 共享
            /// </summary>
            Shared,
            /// <summary>
            /// 拥有
            /// </summary>
            Owned
        }
        /// <summary>
        /// 拥有方式
        /// </summary>
        public PropertyOwnType OwnType { get; set; } = PropertyOwnType.Owned;
        /// <summary>
        /// 动画目标
        /// </summary>
        public DependencyObject Object { get; set; }
        /// <summary>
        /// 动画属性
        /// </summary>
        public DependencyProperty Property { get; set; }
        /// <summary>
        /// 属性所有者字典
        /// </summary>
        private static Dictionary<(DependencyObject, DependencyProperty), PropertyAnimation> PropertyOwner { get; } = new Dictionary<(DependencyObject, DependencyProperty), PropertyAnimation>();
        public override void SetValue(object value)
        {
            //Base.Log($"[Animation]动画设置值运算，值：{value}");
            if (Object == null || Property == null) return;
            switch (OwnType)
            {
                case PropertyOwnType.Shared:
                    {
                        if (!PropertyOwner.TryGetValue((Object, Property), out var Parent) || Parent == null) SetPropertyValue(value);
                    }
                    break;
                case PropertyOwnType.Owned:
                    {
                        if (PropertyOwner.TryGetValue((Object, Property), out var Parent) && Parent == this) SetPropertyValue(value);
                    }
                    break;
                default:
                    break;
            }
        }
        public void SetPropertyValue(object value)
        {
            Object.SetValue(Property, value);
        }

        public override void OnStart()
        {
            base.OnStart();
            if (OwnType == PropertyOwnType.None || OwnType == PropertyOwnType.Shared) return;
            if (!PropertyOwner.TryGetValue((Object, Property), out var Parent)) PropertyOwner.Add((Object, Property), this);
            else if (Parent != this) PropertyOwner[(Object, Property)] = this;
        }
        public override void OnStop()
        {
            base.OnStop();
            if (OwnType == PropertyOwnType.None || OwnType == PropertyOwnType.Shared) return;
            if (PropertyOwner.TryGetValue((Object, Property), out var Owner) && Owner == this) PropertyOwner.Remove((Object, Property));
        }
    }

    public class DoubleAnimation : PropertyAnimation
    {
        public DoubleAnimation(DependencyObject Obj, DependencyProperty dp, double StartValue, double EndValue, double Time = 400, double After = 0, AniEase Ease = null)
        {
            this.Object = Obj;
            this.Property = dp;
            this.EndValue = EndValue;
            this.StartValue = StartValue;
            this.Ease = Ease is null ? new AniEaseLinear() : Ease;
            this.TotalTime = TimeSpan.FromMilliseconds(Time);
            this.After = TimeSpan.FromMilliseconds(After);
        }
        public double StartValue { get; set; }
        public double EndValue { get; set; }
        public AniEase Ease { get; set; }
        public override object GetValue(double t)
        {
            return (EndValue - StartValue) * Ease.GetValue(t) + StartValue;
        }
        public override void OnStart()
        {
            base.OnStart();
            //if (this.StartValue is double.NaN)
            //    this.StartValue = (double)Object.GetValue(Property);
        }
        //public override void SetPropertyValue(object value)
        //{
        //    Object.SetValue(Property, value);
        //}
    }

    public class ColorAnimation : PropertyAnimation
    {
        public ColorAnimation(DependencyObject Obj, DependencyProperty dp, Color StartValue, Color EndValue, double Time = 400, double After = 0, AniEase Ease = null)
        {
            this.Object = Obj;
            this.Property = dp;
            this.EndValue = EndValue;
            this.StartValue = StartValue ;
            //if (StartValue is null) IsAutoStart = true;
            this.TotalTime = TimeSpan.FromMilliseconds(Time);
            this.After = TimeSpan.FromMilliseconds(After);
            this.Ease = Ease is null ? new AniEaseLinear() : Ease;
        }
        //private readonly bool IsAutoStart = false;
        public Color StartValue { get; set; }
        public Color EndValue { get; set; }
        public AniEase Ease { get; set; }
        public override object GetValue(double t)
        {
            return Color.FromArgb(
                (byte)((EndValue.A - StartValue.A) * Ease.GetValue(t) + StartValue.A),
                (byte)((EndValue.R - StartValue.R) * Ease.GetValue(t) + StartValue.R),
                (byte)((EndValue.G - StartValue.G) * Ease.GetValue(t) + StartValue.G),
                (byte)((EndValue.B - StartValue.B) * Ease.GetValue(t) + StartValue.B));
        }
        public override void OnStart()
        {
            base.OnStart();
            //if (IsAutoStart)
            //    StartValue = (Color)Object.GetValue(Property);
        }
        //public override void SetPropertyValue(object value)
        //{
        //    Object.SetValue(Property, value);
        //}
    }

    public class ThicknessAnimation : PropertyAnimation
    {
        public ThicknessAnimation(DependencyObject Obj, DependencyProperty dp, Thickness StartValue, Thickness EndValue, double Time = 400, double After = 0, AniEase Ease = null)
        {
            this.Object = Obj;
            this.Property = dp;
            this.EndValue = EndValue;
            this.StartValue = StartValue;
            this.TotalTime = TimeSpan.FromMilliseconds(Time);
            this.After = TimeSpan.FromMilliseconds(After);
            this.Ease = Ease is null ? new AniEaseLinear() : Ease;
        }
        public Thickness StartValue { get; set; }
        public Thickness EndValue { get; set; }
        public AniEase Ease { get; set; }

        public override object GetValue(double t)
        {
            return new Thickness(
                (EndValue.Left - StartValue.Left) * Ease.GetValue(t) + StartValue.Left,
                (EndValue.Top - StartValue.Top) * Ease.GetValue(t) + StartValue.Top,
                (EndValue.Right - StartValue.Right) * Ease.GetValue(t) + StartValue.Right,
                (EndValue.Bottom - StartValue.Bottom) * Ease.GetValue(t) + StartValue.Bottom);
        }

    }

    public class AnimationGroup : Animation, IList<Animation>
    {
        public override TimeSpan TotalTime
        {
            get
            {
                //Base.Log($"[Animation]动画时长取值运算，MaxTime={MaxTime()},TTime={TTime}");
                if (TTime is double.NaN) return TimeSpan.FromMilliseconds(MaxTime());
                else return TimeSpan.FromMilliseconds(TTime);
            }
            set
            {
                //Base.Log($"[Animation]动画时长设置值运算，Value={value}");
                TTime = value.TotalMilliseconds;
                //Base.Log($"[Animation]动画时长设置值运算,，Value={value},TTime={TTime}");
            }
        }
        private double TTime = double.NaN;
        private double _MaxTime = double.NaN;
        private double MaxTime()
        {
            //Console.WriteLine($"[Animation]动画取值运算，_MaxTime={_MaxTime}");
            if (!(_MaxTime is double.NaN)) return _MaxTime;
            double Max = 0;
            foreach (var i in this)
            {
                if (i.Repeat >= 0)
                    Max = Math.Max(Max, i.TotalTime.TotalMilliseconds * (i.Repeat == 0 ? 1 : i.Repeat) + i.After.TotalMilliseconds);
                else if (i.TotalTime == TimeSpan.Zero)
                    Max = Math.Max(Max, i.After.TotalMilliseconds);
                else
                    Max = TimeSpan.FromHours(1).TotalMilliseconds;
                //if (Max is double.NaN) Max = 0;
                //Base.Log($"[AnimGroup]动画时长计算，Max={Max}");
            }
            Max = Math.Min(Max, TimeSpan.FromHours(1).TotalMilliseconds);
            _MaxTime = Max;
            return Max;
        }
        public override object GetValue(double t)
        {
            TimeSpan RunTime = TimeSpan.FromMilliseconds(t * TotalTime.TotalMilliseconds);
            ValuePairs.Clear();
            foreach (var Anim in this)
            {
                if (!Parent.TryGetValue(Anim, out var group) || group != this) continue;

                var RunningState = AnimRunState.TryGetValue(Anim, out var state) ? state : AnimationRunState.Ended;

                var ThisTickState = AnimationRunState.Waiting;
                //判断目标运行状态
                {
                    //等待状态，停止
                    if (RunTime < Anim.After)
                    {
                        ThisTickState = AnimationRunState.Waiting;
                    }
                    else if ((Anim.Repeat >= 0 && RunTime.TotalMilliseconds >= Anim.After.TotalMilliseconds + Anim.TotalTime.TotalMilliseconds * Math.Max(1, Anim.Repeat)) || Anim.TotalTime == TimeSpan.Zero)
                    {
                        ThisTickState = AnimationRunState.Ended;
                    }
                    else
                    {
                        ThisTickState = AnimationRunState.Running;
                    }

                    switch (ThisTickState)
                    {
                        case AnimationRunState.Waiting:
                            {
                                if (RunningState == AnimationRunState.Running)
                                {
                                    Anim.OnStop();
                                    RepeatCounter[Anim] = 0;
                                }
                            }
                            break;
                        case AnimationRunState.Running:
                            {
                                if (RunningState == AnimationRunState.Waiting || RunningState == AnimationRunState.Ended)
                                {
                                    Anim.OnStart();
                                }
                                TimeSpan ActRunTime = RunTime - Anim.After;
                                int Repeat = (int)(ActRunTime.TotalMilliseconds / Anim.TotalTime.TotalMilliseconds);
                                while (Repeat > RepeatCounter[Anim])
                                {
                                    RepeatCounter[Anim]++;
                                    Anim.OnStop();
                                    Anim.OnStart();
                                }
                                {
                                    double Time2 = ActRunTime.TotalMilliseconds - Anim.TotalTime.TotalMilliseconds * Repeat;
                                    ValuePairs[Anim] = (Anim.GetValue(Time2 / Anim.TotalTime.TotalMilliseconds));
                                }
                            }
                            break;
                        case AnimationRunState.Ended:
                            {
                                if (RunningState == AnimationRunState.Ending)
                                {
                                    Anim.OnStop();
                                }
                                else if (RunningState == AnimationRunState.Waiting)
                                {
                                    Anim.OnStart();
                                    ValuePairs[Anim] = (Anim.GetValue(1.0));
                                    ThisTickState = AnimationRunState.Ending;
                                }
                                else if (RunningState == AnimationRunState.Running)
                                {
                                    ValuePairs[Anim] = (Anim.GetValue(1.0));
                                    ThisTickState = AnimationRunState.Ending;
                                }

                            }
                            break;
                    }
                }
                AnimRunState[Anim] = ThisTickState;
            }
            return ValuePairs;
        }
        private Dictionary<Animation, object> ValuePairs { get; } = new Dictionary<Animation, object>();
        public override void SetValue(object value)
        {
            //Base.Log($"[Animation]动画设置值运算，值：{value}");
            //TimeSpan RunTime = (TimeSpan)value;
            var ValuePairs = value as Dictionary<Animation, object>;
            foreach (var Anim in this)
            {
                if (ValuePairs.TryGetValue(Anim, out var Value))
                    Anim.SetValue(Value);
            }
        }
        private List<Animation> Animations = new List<Animation>();
        #region 集合操作
        public int IndexOf(Animation Item)
        {
            return Animations.IndexOf(Item);
        }
        public void Insert(int Index, Animation Item)
        {
            Animations.Insert(Index, Item);
            _MaxTime = double.NaN;
            return;
        }
        public void RemoveAt(int Index)
        {
            Animations.RemoveAt(Index);
            _MaxTime = double.NaN;
            return;
        }
        public Animation this[int Index]
        {
            get => Animations[Index];
            set
            {
                Animations[Index] = value;
                _MaxTime = double.NaN;
            }
        }
        public void Add(Animation Item)
        {
            Animations.Add(Item);
            _MaxTime = double.NaN;
        }
        public void Clear()
        {
            Animations.Clear();
            _MaxTime = double.NaN;
        }
        public bool Contains(Animation Item)
        {
            return Animations.Contains(Item);
        }
        public void CopyTo(Animation[] array, int arrayIndex)
        {
            Animations.CopyTo(array, arrayIndex);
        }
        public bool Remove(Animation Item)
        {
            _MaxTime = double.NaN;
            return Animations.Remove(Item);
        }
        public int Count => Animations.Count;
        public bool IsReadOnly => false;
        public IEnumerator<Animation> GetEnumerator()
        {
            return Animations.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();  // 直接调用泛型版本
        }
        #endregion
        /// <summary>
        /// 所有者字典
        /// </summary>
        private static readonly Dictionary<Animation, AnimationGroup> Parent = new Dictionary<Animation, AnimationGroup>();

        /// <summary>
        /// 动画状态
        /// </summary>
        private enum AnimationRunState
        {
            Waiting,
            Running,
            Ending,
            Ended
        }

        private readonly Dictionary<Animation, AnimationRunState> AnimRunState = new Dictionary<Animation, AnimationRunState>();
        private readonly Dictionary<Animation, int> RepeatCounter = new Dictionary<Animation, int>();
        public override void OnStart()
        {
            foreach (var Anim in this)
            {
                if (Anim is null) continue;
                if (Parent.ContainsKey(Anim)) Parent[Anim] = this;
                else Parent.Add(Anim, this);
                //Anim.OnStart();
                AnimRunState[Anim] = AnimationRunState.Waiting;
                RepeatCounter[Anim] = 0;
            }
        }
        public override void OnStop()
        {
            foreach (var Anim in this)
            {
                if (Anim is null) continue;
                if (Parent.ContainsKey(Anim)) Parent.Remove(Anim);
                if (AnimRunState.TryGetValue(Anim, out var state) && state == AnimationRunState.Running) Anim.OnStop();
            }
            //Base.Log($"动画已停止，动画组长度：{this.Count}");
        }
        static AnimationGroup()
        {
            Animator.AddAnimationAction += (Animation Anim) => {
                Parent[Anim] = null;
            };
            Animator.RemoveAnimationAction += (Animation Anim) => {
                if (Parent.TryGetValue(Anim, out var P) && P == null) Parent.Remove(Anim);
            };
        }
    }

    #endregion

}
