namespace AudioTranscription.Config
{
    /// <summary>
    /// 服务配置基类
    /// </summary>
    public abstract class ServiceConfig
    {
        public string Name { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string? Model { get; set; }
        public bool IsActive { get; set; } = false;
    }

    /// <summary>
    /// SiliconFlow服务配置
    /// </summary>
    public class SiliconFlowConfig : ServiceConfig
    {
        public SiliconFlowConfig()
        {
            Name = "SiliconFlow";
        }
    }

    /// <summary>
    /// 应用程序总配置
    /// </summary>
    public class AppConfig
    {
        public SiliconFlowConfig? SiliconFlow { get; set; }

        /// <summary>
        /// 当前激活的服务名称
        /// </summary>
        public string ActiveService { get; set; } = "SiliconFlow";
    }
}