using AudioTranscription.Interfaces;
using AudioTranscription.Services;

namespace AudioTranscription
{
    /// <summary>
    /// 文本转语音和音色管理服务的统一入口点
    /// </summary>
    public static class TextToSpeech
    {
        public static async Task<byte[]> ConvertTextToSpeechAsync(
            string text,
            string model = "FunAudioLLM/CosyVoice2-0.5B",
            string? voice = null,
            float speed = 1.0f
        )
        {
            var service = GetTextToSpeechService();
            return await service.TextToSpeechAsync(text, model, voice, speed);
        }

        /// <summary>
        /// 获取当前配置的文本转语音服务实例
        /// </summary>
        /// <returns>文本转语音服务实例</returns>
        private static ITextToSpeech GetTextToSpeechService()
        {
            return ServiceFactory.CreateTextToSpeech();
        }
    }
}
