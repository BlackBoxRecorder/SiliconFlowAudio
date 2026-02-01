using AudioTranscription.Config;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MessageBox = SiliconFlowAudio.MessageBox;

namespace AudioTranscription
{
    public partial class MainWindow : Window
    {
        private readonly string VOICE_PREFIX = "FunAudioLLM/CosyVoice2-0.5B:";
        private readonly string AUDIO_DIR = Path.Combine(Environment.CurrentDirectory, "audios");

        private readonly ObservableCollection<Interfaces.VoiceReference> _referenceVoices = [];

        // 存储UI上显示的复选框集合，方便后续删除操作
        private readonly List<CheckBox> _voiceCheckBoxes = new List<CheckBox>();

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(AUDIO_DIR))
            {
                Directory.CreateDirectory(AUDIO_DIR);
            }
        }

        private async void TranscribeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TranscribeButton.IsEnabled = false;
                TranscribeButton.Content = "正在转录...";

                string transcription = await AudioTranscriber.TranscribeAudioAsync(
                    AudioFilePathText.Text
                );

                TranscriptionTextBox.Text = transcription;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                TranscribeButton.IsEnabled = true;
                TranscribeButton.Content = "转录文字";
            }
        }

        private void LoadAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                AudioFilePathText.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 切换自定义音色和系统预设音色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VoiceMode_Checked(object sender, RoutedEventArgs e)
        {
            if (VoiceComboBox == null)
            {
                return;
            }

            VoiceComboBox.ItemsSource = null;
            VoiceComboBox.Items.Clear();

            if (PresetVoiceRadio?.IsChecked == true)
            {
                VoiceComboBox.ItemsSource = new List<ComboBoxItem>
                {
                    new() { Content = "Anna (女声)", Tag = $"{VOICE_PREFIX}anna" },
                    new() { Content = "Alex (男声)", Tag = $"{VOICE_PREFIX}alex" },
                    new() { Content = "Bella (女声)", Tag = $"{VOICE_PREFIX}bella" },
                    new() { Content = "Benjamin (男声)", Tag = $"{VOICE_PREFIX}benjamin" },
                    new() { Content = "Charles (男声)", Tag = "{VOICE_PREFIX}charles" },
                    new() { Content = "Claire (女声)", Tag = $"{VOICE_PREFIX}claire" },
                    new() { Content = "David (男声)", Tag = $"{VOICE_PREFIX}david" },
                    new() { Content = "Diana (女声)", Tag = $"{VOICE_PREFIX}diana" },
                };
            }
            else if (ReferenceVoiceRadio?.IsChecked == true)
            {
                try
                {
                    var voices = await VoiceEnrollment.GetCustomVoicesAsync();

                    var items = new List<ComboBoxItem>();

                    foreach (var item in voices)
                    {
                        items.Add(new ComboBoxItem() { Content = item.CustomName, Tag = item.Uri });
                    }

                    VoiceComboBox.ItemsSource = items;
                }
                catch (Exception)
                {
                    MessageBox.Show("参考音频获取失败");
                }
            }

            if (VoiceComboBox.Items.Count > 0)
            {
                VoiceComboBox.SelectedIndex = 0;
            }
        }

        private async void ConvertToSpeechButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TextInputBox.Text))
                {
                    MessageBox.Show("请输入要转换的文本", "提示", MessageBoxButton.OK);
                    return;
                }

                if (PresetVoiceRadio.IsChecked == true && VoiceComboBox.SelectedItem == null)
                {
                    MessageBox.Show("请选择预设音色", "提示", MessageBoxButton.OK);
                    return;
                }

                if (ReferenceVoiceRadio.IsChecked == true && VoiceComboBox.SelectedItem == null)
                {
                    MessageBox.Show(
                        "请选择参考音频，或点击刷新按钮。",
                        "提示",
                        MessageBoxButton.OK
                    );
                    return;
                }

                ConvertToSpeechButton.IsEnabled = false;
                ConvertToSpeechButton.Content = "正在转换...";

                byte[] audioData;

                if (PresetVoiceRadio.IsChecked == true)
                {
                    var selectedItem = VoiceComboBox.SelectedItem as ComboBoxItem;
                    var voiceTag =
                        (selectedItem?.Tag?.ToString())
                        ?? throw new InvalidCastException("预设音色 Tag 为空");

                    audioData = await TextToSpeech.ConvertTextToSpeechAsync(
                        TextInputBox.Text,
                        voice: voiceTag
                    );
                }
                else
                {
                    var selectedItem = VoiceComboBox.SelectedItem as ComboBoxItem;
                    var voiceTag =
                        (selectedItem?.Tag?.ToString())
                        ?? throw new InvalidCastException("参考音色 Tag 为空");

                    audioData = await TextToSpeech.ConvertTextToSpeechAsync(
                        TextInputBox.Text,
                        voice: voiceTag
                    );
                }

                var audioFile = Path.Combine(AUDIO_DIR, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.mp3");

                await File.WriteAllBytesAsync(audioFile, audioData);

                TextTTsAudioFile.Content = audioFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "", MessageBoxButton.OK);
            }
            finally
            {
                ConvertToSpeechButton.IsEnabled = true;
                ConvertToSpeechButton.Content = "转换";
            }
        }

        private void SelectReferenceAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                ReferenceAudioPathText.Text = openFileDialog.FileName;
            }
        }

        private async void UploadReferenceAudioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ReferenceAudioPathText.Text))
                {
                    MessageBox.Show("请选择参考音频文件", "提示", MessageBoxButton.OK);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ReferenceAudioTextBox.Text))
                {
                    MessageBox.Show("请输入音频对应的文本", "提示", MessageBoxButton.OK);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CustomVoiceNameText.Text))
                {
                    MessageBox.Show("请输入自定义音色名称", "提示", MessageBoxButton.OK);
                    return;
                }

                UploadReferenceAudioButton.IsEnabled = false;
                UploadReferenceAudioButton.Content = "正在上传...";

                var modelTag =
                    (UploadModelComboBox.SelectedItem as ComboBoxItem)?.Tag as string
                    ?? "FunAudioLLM/CosyVoice2-0.5B";

                string uri = await VoiceEnrollment.CreateVoiceAsync(
                    ReferenceAudioPathText.Text,
                    ReferenceAudioTextBox.Text,
                    modelTag,
                    CustomVoiceNameText.Text
                );

                MessageBox.Show($"创建成功！\nURI: {uri}", "成功", MessageBoxButton.OK);

                await RefreshVoicesList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "", MessageBoxButton.OK);
            }
            finally
            {
                UploadReferenceAudioButton.IsEnabled = true;
                UploadReferenceAudioButton.Content = "上传参考音频";
            }
        }

        // 更新的RefreshVoicesList方法，同时更新ComboBox和WrapPanel
        private async Task RefreshVoicesList()
        {
            try
            {
                var voices = await VoiceEnrollment.GetCustomVoicesAsync();
                _referenceVoices.Clear();
                foreach (var voice in voices)
                {
                    _referenceVoices.Add(voice);
                }

                // 更新ComboBox（如果在参考音频模式下）
                if (ReferenceVoiceRadio?.IsChecked == true)
                {
                    var items = new List<ComboBoxItem>();
                    foreach (var item in voices)
                    {
                        items.Add(new ComboBoxItem() { Content = item.CustomName, Tag = item.Uri });
                    }
                    VoiceComboBox.ItemsSource = items;

                    if (VoiceComboBox.Items.Count > 0)
                    {
                        VoiceComboBox.SelectedIndex = 0;
                    }
                }

                // 更新WrapPanel中的复选框
                UpdateVoicesWrapPanel(voices.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新参考音频列表失败: {ex.Message}", "", MessageBoxButton.OK);
            }
        }

        // 更新WrapPanel中的复选框
        private void UpdateVoicesWrapPanel(List<Interfaces.VoiceReference> voices)
        {
            if (VoicesWrapPanel == null)
                return;

            // 清空现有的复选框
            VoicesWrapPanel.Children.Clear();
            _voiceCheckBoxes.Clear();

            // 为每个参考音频创建复选框
            foreach (var voice in voices)
            {
                var checkBox = new CheckBox
                {
                    Content = voice.CustomName,
                    Tag = voice.Uri, // 使用Tag存储URI，方便删除操作
                    Margin = new Thickness(5),
                    ToolTip = $"URI: {voice.Uri}\n模型: {voice.Model}\n文本: {voice.Text}",
                };

                VoicesWrapPanel.Children.Add(checkBox);
                _voiceCheckBoxes.Add(checkBox);
            }
        }

        // 刷新按钮事件处理
        private async void RefreshVoicesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshVoicesButton.IsEnabled = false;
                RefreshVoicesButton.Content = "刷新中...";

                await RefreshVoicesList();

                MessageBox.Show("参考音频列表已刷新", "提示", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新失败: {ex.Message}", "错误", MessageBoxButton.OK);
            }
            finally
            {
                RefreshVoicesButton.IsEnabled = true;
                RefreshVoicesButton.Content = "刷新列表";
            }
        }

        // 删除选中按钮事件处理
        private async void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取选中的复选框
                var selectedCheckBoxes = _voiceCheckBoxes
                    .Where(cb => cb.IsChecked == true)
                    .ToList();

                if (selectedCheckBoxes.Count == 0)
                {
                    MessageBox.Show("请至少选择一个要删除的参考音频", "提示", MessageBoxButton.OK);
                    return;
                }

                var result = MessageBox.Show(
                    $"确定要删除选中的 {selectedCheckBoxes.Count} 个参考音频吗？",
                    "确认删除",
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                DeleteSelectedButton.IsEnabled = false;
                DeleteSelectedButton.Content = "删除中...";

                // 执行删除操作
                int successCount = 0;
                List<string> failedNames = new List<string>();

                foreach (var checkBox in selectedCheckBoxes)
                {
                    if (checkBox == null)
                    {
                        continue;
                    }

                    var content = checkBox.Content.ToString();

                    try
                    {
                        var uri = checkBox.Tag?.ToString() ?? "";
                        bool success = await VoiceEnrollment.DeleteCustomVoiceAsync(uri);

                        if (success)
                        {
                            successCount++;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(content))
                            {
                                failedNames.Add(content);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (!string.IsNullOrEmpty(content))
                        {
                            failedNames.Add(content);
                        }
                    }
                }

                // 刷新列表以反映删除结果
                await RefreshVoicesList();

                // 显示操作结果
                string message = $"删除完成！成功: {successCount}, 失败: {failedNames.Count}";
                if (failedNames.Any())
                {
                    message += $"\n失败的项: {string.Join(", ", failedNames)}";
                }

                MessageBox.Show(message, "删除结果", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除发生错误: {ex.Message}", "错误", MessageBoxButton.OK);
            }
            finally
            {
                DeleteSelectedButton.IsEnabled = true;
                DeleteSelectedButton.Content = "删除选中";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 检查config.json是否存在，如不存在则弹出设置API Key的窗口
            string configPath = "config.json";
            if (!File.Exists(configPath))
            {
                tab4.IsSelected = true;
            }
            else
            {
                LoadConfig();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void LoadConfig()
        {
            try
            {
                var config = ConfigManager.LoadConfig();

                // 根据当前活动的服务显示相应的API Key
                if (config.SiliconFlow != null)
                {
                    TextApikey.Password = config.SiliconFlow.ApiKey;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败: {ex.Message}", "错误", MessageBoxButton.OK);
            }
        }

        private void BtnSaveApikey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = ConfigManager.LoadConfig();

                config.SiliconFlow ??= new SiliconFlowConfig();
                config.SiliconFlow.ApiKey = TextApikey.Password;

                ConfigManager.SaveConfig(config);

                MessageBox.Show("保存成功", "成功", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButton.OK);
            }
        }
    }
}
