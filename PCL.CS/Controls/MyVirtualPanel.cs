using PCL.CS.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XeF4Core;

namespace PCL.CS.Controls
{
    public class MyVirtualPanel<T> : FrameworkElement
    {
        public IReadOnlyList<T> Datas { get; set; }
        public MyVirtualPanel() { }
        public double ListItemsHeight { get; set; } = 0;
        public delegate void ToListItem(ref MyClickableItem item, T value);
        public ToListItem Conventer { get; set; }
        /// <summary>
        /// 用于外界注册的Update事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Update(object sender, EventArgs args)
        {
            if (ListItemsHeight is 0) throw new InvalidOperationException("必须完成初始化");
            if (Datas is null) throw new InvalidOperationException("必须完成初始化");
            if (Conventer is null) throw new InvalidOperationException("必须完成初始化");

            var offset = this.TranslatePoint(new Point(0, 0), ClipParent);
            double Up = -offset.Y;
            double Down = -offset.Y + ClipParent.ActualHeight;
            //跳过缓冲更新
            if (UnUpdateUpperRange.Contains(Up) && UnUpdateLowerRange.Contains(Down)) return;
            int MinDatas = RenderMinDatas;
            int MaxDatas = RenderMaxDatas;
            if (!UnUpdateUpperRange.Contains(Up))
            {
                MinDatas = (int)(Up / ListItemsHeight);
                if (MinDatas < 0) MinDatas = 0;
                if (MinDatas >= Datas.Count) MinDatas = Datas.Count - 1;
                UnUpdateUpperRange = new ValueRange<double>(MinDatas > 0 ? MinDatas * ListItemsHeight : null, MinDatas * ListItemsHeight + ListItemsHeight, false, false);
                //Main.Hint($"触发可视上边界更新:{UnUpdateUpperRange}");
            }
            if (!UnUpdateLowerRange.Contains(Down))
            {
                MaxDatas = (int)(Down / ListItemsHeight);
                if (MaxDatas < 0) MaxDatas = 0;
                if (MaxDatas >= Datas.Count) MaxDatas = Datas.Count - 1;
                UnUpdateLowerRange = new ValueRange<double>(MaxDatas * ListItemsHeight, MaxDatas < Datas.Count ? MaxDatas * ListItemsHeight + ListItemsHeight : null, false, false);
                //Main.Hint($"触发可视下边界更新:{UnUpdateLowerRange}");
            }
            if (MinDatas > RenderMinDatas)
            {
                int Target = Math.Min(MinDatas, RenderMaxDatas + 1);
                while (RenderMinDatas < Target)
                {
                    RenderMinDatas++;
                    if (RenderingItems.Count == 0) break;
                    var item = RenderingItems[0];
                    RemoveItem(item);
                }
                RenderMinDatas = MinDatas;
            }
            if (MaxDatas < RenderMaxDatas)
            {
                int Target = Math.Max(MaxDatas, RenderMinDatas - 1);
                while (RenderMaxDatas > Target)
                {
                    RenderMaxDatas--;
                    if (RenderingItems.Count == 0) break;
                    var item = RenderingItems[^1];
                    RemoveItem(item);
                }
                RenderMaxDatas = MaxDatas;
            }
            if (MinDatas < RenderMinDatas)
            {
                RenderMinDatas = Math.Min(RenderMinDatas, MaxDatas + 1);
                while (RenderMinDatas > MinDatas)
                {
                    RenderMinDatas--;
                    AddItem(GetItem(Datas[RenderMinDatas]), 0);
                }
                RenderMinDatas = MinDatas;
            }
            if (MaxDatas > RenderMaxDatas)
            {
                RenderMaxDatas = Math.Max(RenderMaxDatas, MinDatas - 1);
                while (RenderMaxDatas < MaxDatas)
                {
                    RenderMaxDatas++;
                    AddItem(GetItem(Datas[RenderMaxDatas]), -1);
                }
                RenderMaxDatas = MaxDatas;
            }
            InvalidateMeasure();
        }
        //当Datas更改时，提示列表重新加载
        public void NotifyDatasChanged()
        {
            // 清空当前渲染
            foreach (var item in RenderingItems.ToList())
                RemoveItem(item);
            RenderingItems.Clear();

            // 清理缓存中不再存在的 key
            if (Datas is not null)
            {
                var live = new HashSet<T>(Datas);
                var dead = new List<T>();
                foreach (var key in RenderableItems.Keys)
                    if (!live.Contains(key))
                        dead.Add(key);
                foreach (var key in dead)
                    RenderableItems.Remove(key);
            }

            // 重置边界
            RenderMinDatas = 0;
            RenderMaxDatas = -1;
            UnUpdateUpperRange = new ValueRange<double>(0, 0, false, false);
            UnUpdateLowerRange = new ValueRange<double>(0, 0, false, false);
            Update(this, EventArgs.Empty);
        }
        private MyClickableItem GetItem(T Data)
        {
            if(RenderableItems.TryGetValue(Data, out var I)) return I;
            var item = new MyClickableItem();
            Conventer?.Invoke(ref item, Data);
            return RenderableItems[Data] = item;
        }
        private ValueRange<double> UnUpdateUpperRange = new ValueRange<double>(0, 0, false, false);
        private ValueRange<double> UnUpdateLowerRange = new ValueRange<double>(0, 0, false, false);
        public FrameworkElement ClipParent { get; set; }
        private int RenderMinDatas = 0;
        private int RenderMaxDatas = -1;
        private readonly List<MyClickableItem> RenderingItems = new();
        protected override int VisualChildrenCount
            => RenderingItems.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= RenderingItems.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return RenderingItems[index];
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                foreach (var item in RenderingItems)
                    yield return item;
            }
        }
        private readonly Dictionary<T, MyClickableItem> RenderableItems = new Dictionary<T, MyClickableItem>();
        private void AddItem(MyClickableItem item, int Index)
        {
            if (Index >= 0)
                RenderingItems.Insert(Index, item);
            else RenderingItems.Add(item);
            AddLogicalChild(item);
            AddVisualChild(item);
        }
        private void RemoveItem(MyClickableItem item)
        {
            RenderingItems.Remove(item);
            RemoveLogicalChild(item);
            RemoveVisualChild(item);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Size final = new();
            double NeedHeight = Datas.Count * ListItemsHeight;
            final.Height = NeedHeight;
            if (availableSize.Width is double.PositiveInfinity)
                final.Width = 300;
            else
                final.Width = availableSize.Width;
            foreach (var item in RenderingItems)
                item.Measure(new Size(final.Width, ListItemsHeight));
            
            return final;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double start = RenderMinDatas * ListItemsHeight;
            var size = new Size(finalSize.Width, ListItemsHeight);
            foreach (var item in RenderingItems)
            {
                item.Arrange(new Rect(new Point(0, start), size));
                start += ListItemsHeight;
            }
            return finalSize;
        }
    }
}
