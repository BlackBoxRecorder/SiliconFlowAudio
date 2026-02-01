using AudioTranscription.Config;
using AudioTranscription.Interfaces;

namespace AudioTranscription.Services
{
    /// <summary>
    /// 服务工厂类，用于创建各种AI服务的实例
    /// </summary>
    public static class ServiceFactory
    {
        /// <summary>
        /// 创建音频转录服务实例
        /// </summary>
        /// <returns>音频转录服务实例</returns>
        public static IAudioTranscriber CreateAudioTranscriber()
        {
            var config = ConfigManager.GetCurrentServiceConfig();

            return config.Name.ToLower() switch
            {
                "siliconflow" => new SiliconFlowService(config.ApiKey),
                _ => new SiliconFlowService(config.ApiKey) // 默认使用SiliconFlow
            };
        }

        /// <summary>
        /// 创建文本转语音服务实例
        /// </summary>
        /// <returns>文本转语音服务实例</returns>
        public static ITextToSpeech CreateTextToSpeech()
        {
            var config = ConfigManager.GetCurrentServiceConfig();

            return config.Name.ToLower() switch
            {
                "siliconflow" => new SiliconFlowService(config.ApiKey),
                _ => new SiliconFlowService(config.ApiKey) // 默认使用SiliconFlow
            };
        }

        /// <summary>
        /// 创建音色管理服务实例
        /// </summary>
        /// <returns>音色管理服务实例</returns>
        public static IVoiceManager CreateVoiceManager()
        {
            var config = ConfigManager.GetCurrentServiceConfig();

            return config.Name.ToLower() switch
            {
                "siliconflow" => new SiliconFlowService(config.ApiKey),
                // 注意：目前只有SiliconFlow支持音色管理功能
                _ => new SiliconFlowService(config.ApiKey) // 默认使用SiliconFlow（仅当支持音色管理时）
            };
        }
    }
}