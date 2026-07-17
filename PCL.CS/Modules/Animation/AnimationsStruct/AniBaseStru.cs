using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PCL.CS.Modules
{
    /// <summary>
    /// 缓动函数基类
    /// </summary>
    public abstract class AniEase
    {
        /// <summary>
        /// 获取动画值
        /// </summary>
        /// <param name="t">时间进度，必须在0~1之间</param>
        public abstract double GetValue(double t);
    }
    

    /// <summary>
    /// 在动画播放时发生的事件
    /// </summary>
    public class EventAnimation : Animation
    {
        //public override TimeSpan TotalTime { get => TimeSpan.Zero; set => base.TotalTime = value; }
        public Action Action;
        public EventAnimation(TimeSpan After, Action action)
        {
            this.After = After;
            this.TotalTime = TimeSpan.Zero;
            this.Action = action;
        }
        public EventAnimation(double After, Action action):this(TimeSpan.FromMilliseconds(After),action)
        {}
        public override object GetValue(double t)
        {
            return null;
        }
        public override void SetValue(object Value)
        {
            Action();
        }
    }

    /// <summary>
    /// 动画组
    /// </summary>
    //public class AnimationGroup
    //{
    //    public List<Animation> Animations = new List<Animation>();
    //    /// <summary>
    //    /// 严格的持续时间
    //    /// </summary>
    //    public TimeSpan TotalTime;
    //    /// <summary>
    //    /// 重复次数。为0时代表永远循环直到移除。
    //    /// </summary>
    //    public int repeat = 1;
    //    public override string ToString()
    //    {
    //        string AniInfo = "";
    //        for(int i = 0; i < Animations.Count; i++)
    //        {
    //            AniInfo += Animations[i].ToString();
    //            AniInfo += "\n";
    //        }
    //        return $"{Animations.Count},{TotalTime.TotalMilliseconds}ms\nAnimations:\n{AniInfo}";
    //    }
    //}
}
