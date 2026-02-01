using System.IO;
using System.Text.Json;

namespace AudioTranscription.Config
{
    /// <summary>
    /// 配置管理器，负责加载和保存服务配置
    /// </summary>
    public static class ConfigManager
    {
        private const string ConfigFileName = "config.json";
        private static AppConfig? _config;

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns>应用程序配置</returns>
        public static AppConfig LoadConfig()
        {
            if (_config != null)
            {
                return _config;
            }

            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string json = File.ReadAllText(ConfigFileName);
                    _config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new AppConfig();
                }
                else
                {
                    // 如果配置文件不存在，创建默认配置
                    _config = CreateDefaultConfig();
                    SaveConfig(_config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
                // 出错时返回默认配置
                _config = CreateDefaultConfig();
            }

            return _config;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config">要保存的配置</param>
        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(ConfigFileName, json);
                _config = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取当前激活的服务配置
        /// </summary>
        /// <returns>当前激活的服务配置</returns>
        public static ServiceConfig GetCurrentServiceConfig()
        {
            var config = LoadConfig();
            return config.ActiveService.ToLower() switch
            {
                "siliconflow" => config.SiliconFlow ?? new SiliconFlowConfig(),
                _ => config.SiliconFlow ?? new SiliconFlowConfig() // 默认使用SiliconFlow
            };
        }

        /// <summary>
        /// 设置当前激活的服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public static void SetActiveService(string serviceName)
        {
            var config = LoadConfig();
            config.ActiveService = serviceName;
            SaveConfig(config);
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认配置</returns>
        private static AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                SiliconFlow = new SiliconFlowConfig
                {
                    ApiKey = "", // 默认为空，需要用户配置
                    IsActive = true
                },

                ActiveService = "SiliconFlow"
            };
        }
    }
}