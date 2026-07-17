using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.CS.Controls;
using PCL.CS.Modules;

namespace PCL.CS.Pages
{
    public static class PagesContent
    {
        static PagesContent()
        {
            Pages.Add(new MyPageTemplate(new PageLaunchLeft(), new List<MyPageRight> { null }, false, ""));
            Pages.Add(new MyPageTemplate(new PageDownloadLeft(), new List<MyPageRight> { new MyDeveloping() }, false, ""));
            Pages.Add(new MyPageTemplate(null, new List<MyPageRight> { new MyDeveloping() }, false, ""));
            Pages.Add(new MyPageTemplate(null, new List<MyPageRight> { new MyDeveloping() }, false, ""));
            Pages.Add(new MyPageTemplate(null, new List<MyPageRight> { new PageAbout() }, false, ""));
            Pages.Add(new MyPageTemplate(null, new List<MyPageRight> { new MyDeveloping() }, true, "版本选择"));
            PagesStack.Push(0);
        }

        public static List<MyPageTemplate> Pages = new List<MyPageTemplate>();
        private static Stack<int> PagesStack = new Stack<int>();
        public static double PageIndex { get { return PagesStack.Peek(); } }
        /// <summary>
        /// 更改页面
        /// </summary>
        /// <param name="Index">页面编号（从0开始）</param>
        public static void ChangePage(int Index)
        {
            if (Index > Pages.Count) return;
            MyPageTemplate Page = Pages[Index];
            Main.MainWnd.ChangePageLeft(Page.PageLeft);
            //Base.Log($"更改页面，索引：{Index}");
            Main.MainWnd.ChangePageRight(Page.PageRight[Page.PgRightIndex]);
            if (Page.IsSubPage)
            {
                Main.MainWnd.TitleLeftChange(true, Page.Title);
                PagesStack.Push(Index);
            }
            else
            {
                Main.MainWnd.TitleLeftChange(false, "");
                PagesStack.Clear();
                PagesStack.Push(Index);
            }
        }
        /// <summary>
        /// 单独更改右页面（用于切换） 若输入了过大或过小的索引编号，就会引发异常
        /// </summary>
        /// <param name="Index">右页面编号（从0开始）</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void ChangePageRight(int Index)
        {
            if (Index <= 0) throw new IndexOutOfRangeException("页面索引不可为负");
            MyPageTemplate Page = Pages[PagesStack.Peek()];
            if (Index == Page.PgRightIndex) return;
            if (Index >= Page.PageRight.Count) throw new IndexOutOfRangeException("页面索引超出页面数量");
            Page.PgRightIndex = Index;
            Main.MainWnd.ChangePageRight(Page.PageRight[Page.PgRightIndex]);
        }
        /// <summary>
        /// 触发页面刷新动画
        /// </summary>
        /// <param name="RefreshLeft">是否刷新左页面</param>
        /// <param name="RefreshRight">是否刷新右页面</param>
        public static void Refresh(bool RefreshLeft = false, bool RefreshRight = true)
        {
            MyPageTemplate Page = Pages[PagesStack.Peek()];
            if (RefreshLeft) Main.MainWnd.ChangePageLeft(Page.PageLeft);
            if (RefreshRight) Main.MainWnd.ChangePageRight(Page.PageRight[Page.PgRightIndex]);
        }
        /// <summary>
        /// 回退到页面栈里的上一个页面
        /// </summary>
        public static void PageBack()
        {
            PagesStack.Pop();
            if (!PagesStack.Any()) PagesStack.Push(0);
            int Index = PagesStack.Peek();
            ChangePage(Index);
        }
    }
    public class MyPageTemplate
    {
        public MyPageLeft PageLeft { get; set; }
        public List<MyPageRight> PageRight {  get; set; }
        public int PgRightIndex { get; set; } = 0;
        public bool IsSubPage {  get; set; }
        public string Title {  get; set; }
        public MyPageTemplate(MyPageLeft pageLeft, List<MyPageRight> pageRight, bool isSubPage=true, string title="SubPage")
        {
            PageLeft = pageLeft;
            PageRight = pageRight;
            IsSubPage = isSubPage;
            Title = title;
        }
    }
}
