namespace AudioTranscription.Interfaces
{
    /// <summary>
    /// 音色管理接口，用于上传、获取和删除自定义音色
    /// </summary>
    public interface IVoiceManager
    {
        /// <summary>
        /// 上传参考音频以创建自定义音色
        /// </summary>
        /// <param name="audioFilePath">音频文件路径</param>
        /// <param name="text">与音频对应的文本</param>
        /// <param name="model">使用的模型名称</param>
        /// <param name="customName">自定义音色名称</param>
        /// <returns>音色标识符</returns>
        Task<string> CreateVoiceAsync(string audioFilePath, string text, string? model = null, string? customName = null);

        /// <summary>
        /// 获取用户自定义音色列表
        /// </summary>
        /// <returns>音色信息列表</returns>
        Task<IEnumerable<VoiceReference>> GetCustomVoicesAsync();

        /// <summary>
        /// 删除指定的自定义音色
        /// </summary>
        /// <param name="voiceUri">音色标识符</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteCustomVoiceAsync(string voiceUri);
    }

    /// <summary>
    /// 音色引用数据模型
    /// </summary>
    public class VoiceReference
    {
        public string Audio { get; set; } = "";
        public string Text { get; set; } = "";
        public string Model { get; set; } = "";
        public string CustomName { get; set; } = "";
        public string Uri { get; set; } = "";
    }
}