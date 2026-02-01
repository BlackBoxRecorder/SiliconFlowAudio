using AudioTranscription.Interfaces;
using AudioTranscription.Services;

namespace AudioTranscription
{
    public class VoiceEnrollment
    {
        /// <summary>
        /// 获取当前配置的音色管理服务实例
        /// </summary>
        /// <returns>音色管理服务实例</returns>
        private static IVoiceManager GetVoiceManagerService()
        {
            return ServiceFactory.CreateVoiceManager();
        }

        public static async Task<string> CreateVoiceAsync(
            string audioFilePath,
            string text,
            string model = "FunAudioLLM/CosyVoice2-0.5B",
            string customName = "my-voice"
        )
        {
            var service = GetVoiceManagerService();
            return await service.CreateVoiceAsync(audioFilePath, text, model, customName);
        }

        public static async Task<List<Interfaces.VoiceReference>> GetCustomVoicesAsync()
        {
            var service = GetVoiceManagerService();
            var voices = await service.GetCustomVoicesAsync();
            return new List<Interfaces.VoiceReference>(voices);
        }

        public static async Task<bool> DeleteCustomVoiceAsync(string uri)
        {
            var service = GetVoiceManagerService();
            return await service.DeleteCustomVoiceAsync(uri);
        }
    }
}
