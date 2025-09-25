using CsharpJson;
using Microsoft.Win32;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace editor
{
    public class EditorPage : UserControl
    {
        public event EventHandler<string> OnSelectedMediaChanged;
    }
    public partial class MainWindow : Window
    {
        private System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
        };
        private string windowTitle = "[ uta-tap Editor ]";
        EditorPage page;
        Encoding UTF8 = new UTF8Encoding(false);
        string openFileName;
        JsonObject json = new JsonObject();
        Dictionary<string, byte[]> loadedMedias = new Dictionary<string, byte[]>();
        bool hasEdit = false;
        public MainWindow()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/editor;component/favicon.ico", UriKind.Absolute));
            Title = windowTitle;
            json["media"] = new JsonObject();
            json["volume"] = new JsonObject();
        }

        private async void ReplaceMedia_Click(object sender, RoutedEventArgs e)
        {
            if (MediaList.SelectedItem is ListBoxItem item)
            {
                var selected = item.Tag?.ToString();
                if (selected == null) return;
                var ofd = new OpenFileDialog()
                {
                    Title = "选择一个音频文件进行替换",
                    Filter = "音频文件 (*.mp3)|*.mp3",
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                };
                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        byte[] audio = await File.ReadAllBytesAsync(ofd.FileName);
                        string base64Audio = "data:audio/mp3;base64," + Convert.ToBase64String(audio);
                        json["media"].ToObject()[selected] = base64Audio;
                        loadedMedias[selected] = audio;
                        MessageBox.Show("已替换资源文件 " + selected);
                    }
                    catch (Exception ex)
                    {
                        var title = "错误";
                        var desc = "打开文件失败：" + ex.Message;
                        MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                var title = "错误";
                var desc = "请先选择一个待替换的资源文件";
                MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void AddMedia_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Title = "选择一个要添加到资源中的音频文件",
                Filter = "音频文件 (*.mp3)|*.mp3",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    byte[] audio = await File.ReadAllBytesAsync(ofd.FileName);
                    using (var memoryStream = new MemoryStream(audio))
                    {
                        using var reader = new Mp3FileReader(memoryStream);
                        if (reader.TotalTime.Seconds > 10)
                        {
                            var title = "提示";
                            var desc = $"你选择的文件 {new FileInfo(ofd.FileName).Name} 时长超过了 10 秒，是否继续？";
                            if (MessageBox.Show(desc, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }
                    }
                    string base64Audio = "data:audio/mp3;base64," + Convert.ToBase64String(audio);
                    int def = loadedMedias.Count + 1;
                    bool validate(string name)
                    {
                        return !loadedMedias.ContainsKey(name) && !json["media"].ToObject().ContainsKey(name);
                    }
                    while (validate(def + ".mp3") == false) def++;
                    var dialog = new DialogNumber(ofd.FileName, def, validate);
                    if (dialog.ShowDialog() == true)
                    {
                        var id = dialog.Value + ".mp3";
                        json["media"].ToObject()[id] = base64Audio;
                        AddMedia(id, audio);
                        MessageBox.Show("已添加资源文件 " + id);
                    }
                }
                catch (Exception ex)
                {
                    var title = "错误";
                    var desc = "打开文件失败：" + ex.Message;
                    MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DelMedia_Click(object sender, RoutedEventArgs e)
        {
            if (MediaList.SelectedItem is ListBoxItem item)
            {
                var selected = item.Tag?.ToString();
                if (selected == null) return;
                var title = "提示";
                var desc = "你确定要删除 " + selected + " 吗？";
                if (MessageBox.Show(desc, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    MediaList.Items.Remove(item);
                    json["media"].ToObject().Remove(selected);
                    loadedMedias.Remove(selected);
                    MessageBox.Show("已删除资源文件 " + selected);
                }
            }
            else
            {
                var title = "错误";
                var desc = "请先选择一个待删除的资源文件";
                MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void MenuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog() {
                Title = "打开音轨/歌姬文件",
                Filter = "JSON文件 (*.json)|*.json",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    openFileName = ofd.FileName;
                    string jsonText = await File.ReadAllTextAsync(openFileName, UTF8);
                    json = JsonDocument.FromString(jsonText).Object;
                    if (json == null)
                    {
                        MessageBox.Show("文件格式错误，无法解析为 JsonObject", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (!json.ContainsKey("media") || !json["media"].IsObject())
                    {
                        MessageBox.Show("文件格式错误，无法在 JSON 中找到 media", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MediaList_SelectionChanged(null, null);
                    loadedMedias.Clear();
                    MediaList.Items.Clear();
                    var media = json["media"].ToObject();
                    foreach (var item in media.Keys)
                    {
                        byte[] audio = ReadBase64Audio(media[item].ToString());
                        AddMedia(item, audio);
                    }
                    Title = windowTitle + " - " + new FileInfo(openFileName).Name;
                    PageContainer.Children.Clear();
                    if (json.ContainsKey("tracks"))
                    {
                        page = new PageEditTracks(this);
                    }
                    else
                    {
                        page = new PageEditSinger(this);
                    }
                    PageContainer.Children.Add(page);
                }
                catch (Exception ex)
                {
                    var title = "错误";
                    var desc = "打开文件失败：" + ex.Message;
                    MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AddMedia(string name, byte[] audio)
        {
            var item = new ListViewItem()
            {
                Content = name,
                Tag = name,
            };
            item.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                if (item.IsSelected) OnClickItem(item);
            };
            MediaList.Items.Add(item);
            loadedMedias[name] = audio;
        }
        private void MenuFileSave_Click(object sender, RoutedEventArgs e)
        {
            SaveTo(openFileName);
        }
        private void MenuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Title = "音轨/歌姬文件另存为",
                Filter = "JSON文件 (*.json)|*.json",
                OverwritePrompt = true,
                AddExtension = true,
                DefaultExt = ".json",
            };
            if (sfd.ShowDialog() == true)
            {
                SaveTo(sfd.FileName);
            }
        }
        private void MenuFileJson_Click(object sender, RoutedEventArgs e)
        {
            new DialogDisplayJson(SaveToJson()).ShowDialog();
        }
        private string SaveToJson()
        {
            var doc = JsonDocument.ToJsonString(json);
            var obj = System.Text.Json.JsonSerializer.Deserialize<object>(doc);
            return System.Text.Json.JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }
        private void SaveTo(string fileName)
        {
            try
            {
                File.WriteAllTextAsync(fileName, SaveToJson(), UTF8).Start();
            }
            catch (Exception ex)
            {
                var title = "错误";
                var desc = "保存文件失败：" + ex.Message;
                MessageBox.Show(desc, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSafeToExit())
            {
                Close();
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var title = "关于";
            var url = "https://github.com/MrXiaoM/uta-tap";
            var desc = string.Join("\n", [
                "uta-tap Editor",
                $"版本 - {App.VERSION}",
                "作者 - MrXiaoM",
                "",
                "是否查看开源地址？",
                url
            ]);
            if (MessageBox.Show(desc, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start("explorer", url);
            }
        }

        private bool CheckSafeToExit()
        {
            if (hasEdit)
            {
                var title = "提示";
                var desc = "你的项目已编辑，是否保存项目再退出？";
                var state = MessageBox.Show(desc, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (state == MessageBoxResult.Cancel) return false;
                if (state == MessageBoxResult.Yes)
                {
                    MenuFileSave_Click(null, null);
                }
            }
            return true;
        }

        private void MediaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MediaList.SelectedItem is ListViewItem item)
            {
                ButtonReplaceMedia.IsEnabled = true;
                OnClickItem(item);
            }
            else
            {
                ButtonReplaceMedia.IsEnabled = false;
            }
        }

        private void OnClickItem(ListViewItem item)
        {
            var selected = item.Tag?.ToString();
            if (selected != null)
            {
                PlayAudio(selected);
            }
        }

        public void PlayAudio(string name)
        {
            if (loadedMedias.TryGetValue(name, out byte[] audio))
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    using var memoryStream = new MemoryStream(audio);
                    using var reader = new Mp3FileReader(memoryStream);
                    using var waveOut = new WaveOutEvent();
                    waveOut.Init(reader);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                    }
                });
            }
        }

        public byte[] ReadBase64Audio(string base64Audio)
        {
            if (string.IsNullOrEmpty(base64Audio))
                throw new ArgumentNullException(nameof(base64Audio));
            int index = base64Audio.IndexOf("base64,");
            if (index == -1 || !base64Audio.StartsWith("data:audio"))
                throw new FormatException("无效的 base64 音频格式，未找到 base64 标识");
            string base64Data = base64Audio.Substring(index + 7);
            return Convert.FromBase64String(base64Data);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                MenuFileSave_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.O && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                MenuFileOpen_Click(null, null);
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckSafeToExit())
            {
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
        }

        private void MediaList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Right || e.Key == Key.Left)
            {
                if (MediaList.SelectedItem is ListViewItem item)
                {
                    OnClickItem(item);
                }
            }
        }
    }
}
