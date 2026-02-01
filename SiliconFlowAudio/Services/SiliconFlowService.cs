using AudioTranscription.Interfaces;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AudioTranscription.Services
{
    /// <summary>
    /// SiliconFlow服务的具体实现
    /// </summary>
    public class SiliconFlowService : IAudioTranscriber, ITextToSpeech, IVoiceManager
    {
        private readonly string _baseUrl = "https://api.siliconflow.cn/v1";
        private readonly string _apiKey;

        public SiliconFlowService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        #region IAudioTranscriber Implementation

        /// <summary>
        /// 语音转文本
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="model"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<string> TranscribeAudioAsync(
            string audioFilePath,
            string? model = null,
            string? language = null
        )
        {
            if (string.IsNullOrWhiteSpace(audioFilePath))
                throw new ArgumentException(
                    "Audio file path cannot be null or empty",
                    nameof(audioFilePath)
                );

            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException("Audio file not found", audioFilePath);

            // 验证文件扩展名
            string extension = Path.GetExtension(audioFilePath).ToLower();
            var allowedExtensions = new[] { ".wav", ".mp3" };
            if (!Array.Exists(allowedExtensions, ext => ext == extension))
            {
                throw new NotSupportedException(
                    $"Invalid file type. Only {string.Join(", ", allowedExtensions)} files are allowed."
                );
            }

            using var client = new HttpClient();
            // 设置请求头
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            // 构建 multipart/form-data 请求
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(model ?? "FunAudioLLM/SenseVoiceSmall"), "model");
            formData.Add(
                new StreamContent(File.OpenRead(audioFilePath)),
                "file",
                Path.GetFileName(audioFilePath)
            );

            // 发送请求
            var response = await client.PostAsync($"{_baseUrl}/audio/transcriptions", formData);
            response.EnsureSuccessStatusCode();

            // 解析响应
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SiliconFlowTranscriptionResult>(jsonResponse);
            return result?.text ?? "";
        }

        #endregion

        #region ITextToSpeech Implementation

        /// <summary>
        /// 文本转语音
        /// </summary>
        /// <param name="text"></param>
        /// <param name="model"></param>
        /// <param name="voice"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<byte[]> TextToSpeechAsync(
            string text,
            string? model = null,
            string? voice = null,
            float speed = 1.0f
        )
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            if (text.Length > 4096)
                throw new ArgumentException(
                    "Text is too long. Maximum length is 4096 characters.",
                    nameof(text)
                );

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            object requestBody = new
            {
                model = model ?? "FunAudioLLM/CosyVoice2-0.5B",
                input = text,
                voice = voice ?? "FunAudioLLM/CosyVoice2-0.5B:anna",
                response_format = "mp3",
                speed = speed,
            };

            string jsonString = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                $"{_baseUrl}/audio/speech",
                content
            );

            if (response.IsSuccessStatusCode)
            {
                byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();
                return audioBytes;
            }
            else
            {
                throw new Exception($"API request failed, status code: {response.StatusCode}");
            }
        }

        #endregion

        #region IVoiceManager Implementation

        /// <summary>
        /// 创建自定义音色
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="text"></param>
        /// <param name="model"></param>
        /// <param name="customName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> CreateVoiceAsync(
            string audioFilePath,
            string text,
            string? model = null,
            string? customName = null
        )
        {
            if (string.IsNullOrWhiteSpace(audioFilePath))
                throw new ArgumentException(
                    "Audio file path cannot be null or empty",
                    nameof(audioFilePath)
                );

            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException("Audio file not found", audioFilePath);

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(model ?? "FunAudioLLM/CosyVoice2-0.5B"), "model");
            formData.Add(new StringContent(customName ?? "my-voice"), "customName");
            formData.Add(new StringContent(text), "text");
            formData.Add(
                new StreamContent(File.OpenRead(audioFilePath)),
                "file",
                Path.GetFileName(audioFilePath)
            );

            HttpResponseMessage response = await httpClient.PostAsync(
                $"{_baseUrl}/uploads/audio/voice",
                formData
            );

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SiliconFlowUploadVoiceResult>(jsonResponse);
                return result?.uri ?? "";
            }
            else
            {
                throw new Exception($"API request failed, status code: {response.StatusCode}");
            }
        }

        /// <summary>
        /// 获取所有自定义音色
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<VoiceReference>> GetCustomVoicesAsync()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            HttpResponseMessage response = await httpClient.GetAsync(
                $"{_baseUrl}/audio/voice/list"
            );

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SiliconFlowVoiceListResult>(jsonResponse);

                if (result?.result != null)
                {
                    var voiceReferences = new List<VoiceReference>();
                    foreach (var item in result.result)
                    {
                        voiceReferences.Add(
                            new VoiceReference
                            {
                                Model = item.model,
                                CustomName = item.customName,
                                Text = item.text,
                                Uri = item.uri,
                            }
                        );
                    }
                    return voiceReferences;
                }
                else
                {
                    return new List<VoiceReference>();
                }
            }
            else
            {
                throw new Exception($"API request failed, status code: {response.StatusCode}");
            }
        }

        /// <summary>
        /// 删除自定义音色
        /// </summary>
        /// <param name="voiceUri"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> DeleteCustomVoiceAsync(string voiceUri)
        {
            if (string.IsNullOrWhiteSpace(voiceUri))
                throw new ArgumentException("URI cannot be null or empty", nameof(voiceUri));

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var requestBody = new { uri = voiceUri };
            string jsonString = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                $"{_baseUrl}/audio/voice/deletions",
                content
            );

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception($"API request failed, status code: {response.StatusCode}");
            }
        }

        #endregion

        #region Private Classes

        private class SiliconFlowTranscriptionResult
        {
            public string text { get; set; } = "";
        }

        private class SiliconFlowUploadVoiceResult
        {
            public string uri { get; set; } = "";
        }

        private class SiliconFlowVoiceListResult
        {
            public List<SiliconFlowReferenceVoice> result { get; set; } =
                new List<SiliconFlowReferenceVoice>();
        }

        private class SiliconFlowReferenceVoice
        {
            public string model { get; set; } = "";
            public string customName { get; set; } = "";
            public string text { get; set; } = "";
            public string uri { get; set; } = "";
        }

        #endregion
    }
}
