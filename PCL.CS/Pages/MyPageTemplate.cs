using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public abstract class MyPageLeft:UserControl
    {
        public abstract AnimationGroup AnimationIn();
        public abstract AnimationGroup AnimationOut();
        public abstract void Reset();
        public virtual void OnLoaded() => Reset();
        public virtual void OnUnloaded() => Reset();
    }
    public abstract class MyPageRight : UserControl
    {
        public abstract AnimationGroup AnimationIn();
        public abstract AnimationGroup AnimationOut();
        public abstract void Reset();
        public virtual void OnLoaded() => Reset();
        public virtual void OnUnloaded() => Reset();
        protected static Animation MyScrollBarAnimationIn(MyScrollViewer viewer)
        {
            TranslateTransform translate = viewer.BarTranslate;
            translate.X = 8;
            Animation animation = new DoubleAnimation(translate, TranslateTransform.XProperty, 8, 0, 300, 0, new AniEaseOutBack(2));
            return animation;
        }
        protected static Animation MyScrollBarAnimationOut(MyScrollViewer viewer)
        {
            TranslateTransform translate = viewer.BarTranslate;
            Animation animation = new DoubleAnimation(translate, TranslateTransform.XProperty, translate.X, 8, 300, 0, new AniEaseInFluent(2));
            return animation;
        }
    }
}
