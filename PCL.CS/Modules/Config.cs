using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.CS.Modules
{
    //================================
    //若需增减配置项，增减实例属性即可
    //================================

    /// <summary>
    /// 配置文件
    /// </summary>
    public class Config
    {

        #region 配置方法
        /// <summary>
        /// 配置文件的实例，参与配置文件的保存或修改
        /// </summary>
        public static Config Current
        {
            get
            {
                if (_Current is null) Read();
                return _Current;
            }
        }
        private static object Locker = new object();
        private static Config _Current;
        private static readonly string ConfigPath = Path.Combine(Base.Path, "PCL", "Setup.json");

        private static void Read()
        {
            lock (Locker)
                if (File.Exists(ConfigPath))
                {
                    using var FileSteam = File.OpenRead(ConfigPath);
                    _Current = XeF4Core.Extensions.DeserializeJson<Config>(FileSteam) ?? new Config();
                }
                else _Current = new Config();
        }
        /// <summary>
        /// 保存配置。
        /// </summary>
        public static void Save()
        {
            lock (Locker)
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Current));
        }
        
        /// <summary>
        /// 强制重载配置文件
        /// </summary>
        public static void Reload() => Read();
        #endregion

        #region 配置属性
        public double WindowWidth { get; set; } = 870;
        public double WindowHeight { get; set; } = 580;
        public double AnimationSpeed { get; set; } = 1.0;
        
        #endregion

    }
}
