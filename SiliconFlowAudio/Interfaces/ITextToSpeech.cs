namespace AudioTranscription.Interfaces
{
    /// <summary>
    /// 文本转语音服务接口，用于将文本转换为语音
    /// </summary>
    public interface ITextToSpeech
    {
        /// <summary>
        /// 将文本转换为语音
        /// </summary>
        /// <param name="text">要转换的文本</param>
        /// <param name="model">使用的模型名称</param>
        /// <param name="voice">语音类型或名称</param>
        /// <param name="speed">语速</param>
        /// <returns>音频字节数组</returns>
        Task<byte[]> TextToSpeechAsync(string text, string? model = null, string? voice = null, float speed = 1.0f);

    }
}