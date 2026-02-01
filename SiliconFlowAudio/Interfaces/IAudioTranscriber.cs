namespace AudioTranscription.Interfaces
{
    /// <summary>
    /// 音频转录服务接口，用于将音频文件转换为文本
    /// </summary>
    public interface IAudioTranscriber
    {
        /// <summary>
        /// 将音频文件转录为文本
        /// </summary>
        /// <param name="audioFilePath">音频文件路径</param>
        /// <param name="model">使用的模型名称</param>
        /// <param name="language">音频语言（可选）</param>
        /// <returns>转录结果文本</returns>
        Task<string> TranscribeAudioAsync(string audioFilePath, string? model = null, string? language = null);

    }
}