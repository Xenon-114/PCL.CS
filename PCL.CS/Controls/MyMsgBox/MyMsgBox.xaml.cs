using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    /// <summary>
    /// 自定义消息框的实例。尽量复用。
    /// </summary>
    public partial class MyMsgBox : UserControl
    {
        public MyMsgBox()
        {
            InitializeComponent();
            BindingOperations.SetBinding(Shadow, DropShadowEffect.ColorProperty, new Binding("ShadowColor") { Source = this });
            this.SetResourceReference(ShadowColorProperty, "ColorObject1");
            this.Btn1.Click += (s, e) =>
            {
                if (_MyMsg is null) return;
                _MyMsg.ResultTask.SetResult(null);
                BackBtn.Focus();
            };
            this.Btn2.Click += (s, e) =>
            {
                if (_MyMsg is null) return;
                _MyMsg.TryRunFunc2();
                BackBtn.Focus();
            };
            this.Btn3.Click += (s, e) =>
            {
                if (_MyMsg is null) return;
                _MyMsg.TryRunFunc3();
                BackBtn.Focus();
            };
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.Opacity = 0;
            this.KeyUp += (s, e) => OnThisKeyDown(e.Key);
        }
        public void OnThisKeyDown(Key KeyDown)
        {
            if (_MyMsg is null) return;
            if (KeyDown is Key.Enter)
            {
                switch (_MyMsg.BtnCount)
                {
                    case 1:
                        _MyMsg.ResultTask.SetResult(null);
                        BackBtn.Focus();
                        break;
                    case 2:
                        _MyMsg.TryRunFunc2();
                        BackBtn.Focus();
                        break;
                    case 3:
                        _MyMsg.TryRunFunc3();
                        BackBtn.Focus();
                        break;
                }
            }
            if (KeyDown is Key.Escape)
            {
                _MyMsg.ResultTask.SetResult(null);
                BackBtn.Focus();
            }
        }

        private MyMsg _MyMsg;

        public MyMsg Message
        {
            get => _MyMsg;
            set => BindToOther(value);
        }

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShadowColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(MyMsgBox), new PropertyMetadata());



        public void BindToOther(MyMsg msg)
        {
            if (_MyMsg == msg) return;
            _MyMsg = msg;
            SetBinding();
        }
        private void SetBinding()
        {
            if (_MyMsg is null)
            {
                BindingOperations.ClearBinding(PanContent, ContentProperty);
                BindingOperations.ClearBinding(PanExtraContent, ContentProperty);
                BindingOperations.ClearBinding(LabTitle, TextBlock.TextProperty);
                BindingOperations.ClearBinding(Btn1, MyButton.TextProperty);
                BindingOperations.ClearBinding(Btn2, MyButton.TextProperty);
                BindingOperations.ClearBinding(Btn3, MyButton.TextProperty);
                PanContent.Content = null;
                PanExtraContent.Content = null;
                return;
            }
            PanContent.SetBinding(ContentProperty, new Binding("Content") { Source = _MyMsg });
            PanExtraContent.SetBinding(ContentProperty, new Binding("ExtraContent") { Source = _MyMsg });
            LabTitle.SetBinding(TextBlock.TextProperty, new Binding("Title") { Source = _MyMsg });
            Btn1.SetBinding(MyButton.TextProperty, new Binding("Btn1Text") { Source = _MyMsg });
            Btn2.SetBinding(MyButton.TextProperty, new Binding("Btn2Text") { Source = _MyMsg });
            Btn2.SetBinding(UIElement.IsEnabledProperty, new Binding("Btn2Enable") { Source = _MyMsg });
            Btn3.SetBinding(MyButton.TextProperty, new Binding("Btn3Text") { Source = _MyMsg });
            Btn3.SetBinding(UIElement.IsEnabledProperty, new Binding("Btn3Enable") { Source = _MyMsg });

            Btn1.ColorType = MyButton.ColorState.Normal;
            Btn2.ColorType = MyButton.ColorState.Normal;
            Btn3.ColorType = MyButton.ColorState.Normal;

            switch (_MyMsg.BtnCount)
            {
                case 1:
                    Btn1.Visibility = Visibility.Visible;
                    Btn2.Visibility = Visibility.Collapsed;
                    Btn3.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Btn1.Visibility = Visibility.Visible;
                    Btn2.Visibility = Visibility.Visible;
                    if (_MyMsg.FirstBtnHighlight) Btn2.ColorType = _MyMsg.ColorType is MyMsg.ColorState.Normal ? MyButton.ColorState.HighLight : MyButton.ColorState.Red;
                    Btn3.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    Btn1.Visibility = Visibility.Visible;
                    Btn2.Visibility = Visibility.Visible;
                    Btn3.Visibility = Visibility.Visible;
                    if (_MyMsg.FirstBtnHighlight) Btn3.ColorType = _MyMsg.ColorType is MyMsg.ColorState.Normal ? MyButton.ColorState.HighLight : MyButton.ColorState.Red;
                    break;
                default:
                    Btn1.Visibility = Visibility.Visible;
                    Btn2.Visibility = Visibility.Collapsed;
                    Btn3.Visibility = Visibility.Collapsed;
                    break;
            }
            if(_MyMsg.ColorType is MyMsg.ColorState.Red)
            {
                LabTitle.SetResourceReference(ForegroundProperty, "ColorBrushRedLight");
                this.SetResourceReference(ShadowColorProperty, "ColorObjectRedDark");
            }
            else
            {
                LabTitle.SetResourceReference(ForegroundProperty, "ColorBrush2");
                this.SetResourceReference(ShadowColorProperty, "ColorObject1");
            }
        }
        public void Show()
        {
            if (!this.IsLoaded) return;
            Animation.Start(
                new AnimationGroup
                {
                    new DoubleAnimation(Translate,TranslateTransform.YProperty,20,0,300,0,new AniEaseOutBack(2)),
                    new DoubleAnimation(Rotate,RotateTransform.AngleProperty,-6,0,250,0,new AniEaseOutFluent(2)),
                    new DoubleAnimation(this,OpacityProperty,0,1,120,0)
                });
            this.IsHitTestVisible = true;
        }
        public async Task CloseAndWait()
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            this.IsHitTestVisible = false;
            Animation.Start(
                new AnimationGroup
                {
                    new DoubleAnimation(Translate,TranslateTransform.YProperty,Translate.Y,20,150,0,new AniEaseInFluent(2)),
                    new DoubleAnimation(Rotate,RotateTransform.AngleProperty,Rotate.Angle,3,150,0,new AniEaseInFluent(2)),
                    new DoubleAnimation(this,OpacityProperty,Opacity,0,150,0),
                    new EventAnimation(150,()=>{task.SetResult(false); })
                });
            await task.Task;
            return;
        }
    }
}
