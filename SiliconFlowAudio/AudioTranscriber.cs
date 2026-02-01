using AudioTranscription.Interfaces;
using AudioTranscription.Services;

namespace AudioTranscription
{
    /// <summary>
    /// 音频转录服务的统一入口点，支持多种服务提供商
    /// </summary>
    public static class AudioTranscriber
    {
        /// <summary>
        /// 转录音频文件，使用当前配置的服务提供商
        /// </summary>
        /// <param name="audioFilePath">音频文件路径</param>
        /// <param name="model">模型名称（如果为null则使用默认模型）</param>
        /// <param name="language">音频语言（如果为null则使用默认语言）</param>
        /// <returns>转录结果文本</returns>
        public static async Task<string> TranscribeAudioAsync(
            string audioFilePath,
            string? model = null,
            string? language = null
        )
        {
            var service = GetAudioTranscriberService();
            return await service.TranscribeAudioAsync(audioFilePath, model, language);
        }

        /// <summary>
        /// 获取当前配置的音频转录服务实例
        /// </summary>
        /// <returns>音频转录服务实例</returns>
        private static IAudioTranscriber GetAudioTranscriberService()
        {
            return ServiceFactory.CreateAudioTranscriber();
        }
    }
}
