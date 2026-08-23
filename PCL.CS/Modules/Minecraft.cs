using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mc = XeF4Core.MinecraftCore.Minecraft;
using McGroup = XeF4Core.MinecraftCore.MinecraftGroup;

namespace PCL.CS.Modules
{
    public static class Minecraft
    {
        public static readonly string LocalPath = Path.Combine(Base.Path, ".minecraft");
        public static readonly List<McGroup> McGroups = new();
        public static McGroup Local;
        public static McGroup SelectedGroup;
        public static bool IsSelectedGroupCorrect => SelectedGroup is null || McGroups.Contains(SelectedGroup) || ReferenceEquals(SelectedGroup, Local);
        public static event EventHandler<McGroup> OnSelectGroupChanged;
        public static void SelectMcGroup(McGroup mcGroup)
        {
            if (mcGroup is not null && !McGroups.Contains(mcGroup) && !ReferenceEquals(mcGroup, Local)) throw new ArgumentException($"{mcGroup.Name}必须存在于游戏列表里！");
            SelectedGroup = mcGroup;
            OnSelectGroupChanged?.Invoke(null, mcGroup);
        }
        public static void Init()
        {
            foreach(var group in Config.Current.MinecraftGroups)
            {
                if (!Directory.Exists(group.Value)) continue;
                McGroup _g = McGroup.FromMinecraftPath(group.Value);
                _g.Name = group.Key;
                McGroups.Add(_g);
            }
            if (Directory.Exists(LocalPath))
            {
                Local = McGroup.FromMinecraftPath(LocalPath);
                SelectMcGroup(Local);
            }
            else if (McGroups.Count > 0) SelectMcGroup(McGroups[0]);
            Save();
        }
        public static void Save()
        {
            Config.Current.MinecraftGroups.Clear();
            if (Local is not null)
                Config.Current.MinecraftGroups.Add(new KeyValuePair<string, string>(Local.Name, Local.MinecraftDirectory.FullName));
            foreach(var group in McGroups)
            {
                Config.Current.MinecraftGroups.Add(new KeyValuePair<string, string>(group.Name, group.MinecraftDirectory.FullName));
            }
            Config.Save();
        }
        public static void Refresh()
        {
            if (Local is not null && !Local.IsAvailable) Local = null;
            McGroups.RemoveAll(x => x is null || !x.IsAvailable);
            if (!McGroups.Contains(SelectedGroup) && !ReferenceEquals(Local, SelectedGroup))
            {
                if (Local is not null && Local.IsAvailable)
                    SelectMcGroup(Local);
                else if (McGroups.Count > 0)
                    SelectMcGroup(McGroups[0]);
                else
                    SelectMcGroup(null);
            }
            Save();
        }
        public static void AddMinecraftGroup(McGroup mcGroup)
        {
            if (mcGroup is null) throw new ArgumentNullException(nameof(mcGroup));
            if (!mcGroup.IsAvailable) throw new ArgumentException("必须传入一个可用的游戏文件夹！");
            if (McGroups.Contains(mcGroup)) return;
            if (ReferenceEquals(Local, mcGroup)) return;

            if (mcGroup.MinecraftDirectory.FullName == LocalPath)
            {
                if (Local is null || !Local.IsAvailable)
                    Local = mcGroup;
                else throw new Exception("未知错误");
            }
            else
                McGroups.Add(mcGroup);
            Save();
            if (SelectedGroup is null||!IsSelectedGroupCorrect) SelectMcGroup(mcGroup);
            return;
        }
        public static void RemoveMinecraftGroup(McGroup mcGroup)
        {
            if (mcGroup is null) throw new ArgumentNullException(nameof(mcGroup));
            if (ReferenceEquals(Local, mcGroup)) return;
            if (ReferenceEquals(SelectedGroup, mcGroup)) SelectMcGroup(null);
            McGroups.Remove(mcGroup);
        }
        public static void DeleteMinecraftGroup(McGroup mcGroup)
        {
            if (mcGroup is null) throw new ArgumentNullException(nameof(mcGroup));
            if (!mcGroup.IsAvailable) throw new ArgumentException("必须传入一个可用的游戏文件夹！");
            mcGroup.Delete();
            if (ReferenceEquals(Local, mcGroup)) Local = null;
            else RemoveMinecraftGroup(mcGroup);
        }
    }
}
