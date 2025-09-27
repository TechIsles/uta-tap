using CsharpJson;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace editor
{
    public class MusicTrack
    {
        private static Path IconToMute => new()
        {
            Data = Geometry.Parse("M333.1 34.8C344.6 40 352 51.4 352 64l0 384c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L163.8 352 96 352c-35.3 0-64-28.7-64-64l0-64c0-35.3 28.7-64 64-64l67.8 0L298.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3zm172 72.2c43.2 35.2 70.9 88.9 70.9 149s-27.7 113.8-70.9 149c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C507.3 341.3 528 301.1 528 256s-20.7-85.3-53.2-111.8c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zm-60.5 74.5C466.1 199.1 480 225.9 480 256s-13.9 56.9-35.4 74.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C425.1 284.4 432 271 432 256s-6.9-28.4-17.7-37.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5z"),
            Fill = Brushes.Black,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(1),
        };
        private static Path IconToUnMute => new()
        {
            Data = Geometry.Parse("M38.8 5.1C28.4-3.1 13.3-1.2 5.1 9.2S-1.2 34.7 9.2 42.9l592 464c10.4 8.2 25.5 6.3 33.7-4.1s6.3-25.5-4.1-33.7L525.1 386.2C556.7 352 576 306.3 576 256c0-60.1-27.7-113.8-70.9-149c-10.3-8.4-25.4-6.8-33.8 3.5s-6.8 25.4 3.5 33.8C507.3 170.7 528 210.9 528 256c0 39.1-15.6 74.5-40.9 100.5L449 326.6c19-17.5 31-42.7 31-70.6c0-30.1-13.9-56.9-35.4-74.5c-10.3-8.4-25.4-6.8-33.8 3.5s-6.8 25.4 3.5 33.8C425.1 227.6 432 241 432 256s-6.9 28.4-17.7 37.3c-1.3 1-2.4 2.2-3.4 3.4L352 250.6 352 64c0-12.6-7.4-24-18.9-29.2s-25-3.1-34.4 5.3L197.8 129.8 38.8 5.1zM352 373.3L82.9 161.3C53.8 167.4 32 193.1 32 224l0 64c0 35.3 28.7 64 64 64l67.8 0L298.7 471.9c9.4 8.4 22.9 10.4 34.4 5.3S352 460.6 352 448l0-74.7z"),
            Fill = Brushes.Black,
            Stretch = Stretch.Uniform,
        };
        PageEditTracks parent;
        Action<MusicTrack, int> onSelected;
        public Grid left;
        public StackPanel right;
        private TextBlock buttonMute;
        public string comment;
        public bool loop;
        public List<int> notes;
        public bool mute = false;
        internal List<Grid> noteGrids = new List<Grid>();
        internal int selectedIndex = -1;
        private static Brush hex(string s)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(s));
        }
        public MusicTrack(PageEditTracks parent, StackPanel left, StackPanel right, int index, string comment, bool loop, List<int> notes, Action<MusicTrack, int> onSelected)
        {
            this.parent = parent;
            this.onSelected = onSelected;
            this.comment = comment;
            this.loop = loop;
            this.notes = notes;
            this.left = new Grid()
            {
                Height = 24,
            };
            this.left.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.left.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            this.left.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonMute = new TextBlock()
            {
                Width = 18, Height = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 2, 0, 2),
                Cursor = Cursors.Hand,
            };
            buttonMute.MouseLeftButtonDown += (s, e) =>
            {
                mute = !mute;
                UpdateMuteIcon();
                e.Handled = true;
            };
            var textTrackName = new TextBlock()
            {
                Text = comment,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2, 2, 0, 2),
            };
            Grid.SetColumn(buttonMute, 0);
            Grid.SetColumn(textTrackName, 1);
            this.left.Children.Add(buttonMute);
            this.left.Children.Add(textTrackName);
            this.left.Children.Add(GenerateCheckBox(2));
            UpdateMuteIcon();
            SetupLeftContextMenu(this.left, textTrackName);
            this.right = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(1, 0, 1, 0),
                Height = 24,
            };
            UpdateBackgroundByIndex(index);

            for (int i = 0; i < notes.Count; i++)
            {
                AddNoteGrid(i, notes[i]);
            }

            left.Children.Insert(index, this.left);
            right.Children.Insert(index, this.right);
        }

        public void UpdateMuteIcon()
        {
            buttonMute.Inlines.Clear();
            buttonMute.Inlines.Add(mute ? IconToUnMute : IconToMute);
            buttonMute.ToolTip = mute ? "取消静音" : "静音";
        }

        public void UpdateBackgroundByIndex(int index)
        {
            left.Background = hex(index % 2 == 1 ? "#EAEAEA" : "#EFEFEF");
            right.Background = hex(index % 2 == 1 ? "#6B6B6B" : "#808080");
        }

        private void SetupLeftContextMenu(Grid grid, TextBlock textTrackName)
        {
            var ctx = new ContextMenu();
            var itemRename = new MenuItem() { Header = "重命名" };
            itemRename.Click += (s, e) =>
            {
                var dialog = new DialogTrackRename(comment);
                if (dialog.ShowDialog() == true)
                {
                    comment = dialog.TrackName;
                    textTrackName.Text = comment;
                    parent.SaveTracks();
                }
            };
            var itemDelete = new MenuItem() { Header = "删除音轨" };
            itemDelete.Click += (s, e) =>
            {
                var title = "警告";
                var desc = $"你真的要删除音轨 {comment} 吗？\n这个音轨将会永久消失！（真的很久）";
                if (MessageBox.Show(desc, title, MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes)
                {
                    parent.DeleteTrack(this);
                    parent.SaveTracks();
                }
            };
            ctx.Items.Add(itemRename);
            ctx.Items.Add(new Separator());
            ctx.Items.Add(itemDelete);
            grid.ContextMenu = ctx;
        }

        private CheckBox GenerateCheckBox(int column)
        {
            var check = new CheckBox()
            {
                Content = "循环",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                IsChecked = loop,
            };
            Grid.SetColumn(check, column);
            check.Checked += (o, e) =>
            {
                if (!this.loop)
                {
                    this.loop = true;
                    parent.SaveTracks();
                }
            };
            check.Unchecked += (o, e) =>
            {
                if (this.loop)
                {
                    this.loop = false;
                    parent.SaveTracks();
                }
            };
            return check;
        }

        public void AddNoteGrid(int noteIndex, int note)
        {
            var grid = new Grid()
            {
                Tag = noteIndex,
                Width = 25,
                Height = 20,
                Margin = new Thickness(1, 2, 1, 2),
                Background = hex("#60EFEFEF"),
                Cursor = Cursors.Hand,
            };
            grid.MouseLeftButtonDown += (o, e) =>
            {
                foreach (var item in parent.tracks)
                {
                    item.selectedIndex = item == this ? noteIndex : -1;
                    item.RefreshBackgroundColor();
                }
                onSelected.Invoke(this, noteIndex);
                e.Handled = true;
            };
            grid.MouseRightButtonDown += (o, e) =>
            {
                foreach (var item in parent.tracks)
                {
                    item.selectedIndex = -1;
                    item.RefreshBackgroundColor();
                }
                if (notes[noteIndex] != -1)
                {
                    SetNote(noteIndex, -1);
                    RefreshBackgroundColor();
                    parent.SaveTracks();
                    parent.mainWindow.MarkEdit();
                }
                e.Handled = true;
            };
            if (note >= 0)
            {
                grid.Children.Add(new TextBlock()
                {
                    Text = note.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            this.right.Children.Add(grid);
            noteGrids.Add(grid);
        }

        public void SetNote(int index, int note)
        {
            notes[index] = note;
            var grid = noteGrids[index];
            grid.Children.Clear();
            if (note >= 0)
            {
                grid.Children.Add(new TextBlock()
                {
                    Text = note.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
        }

        public int GetLastRealNoteIndex()
        {
            var changed = false;
            var lastRealNote = notes.Count - 1;
            for (int index = lastRealNote; index >= 0; index--)
            {
                if (notes[index] > -1)
                {
                    lastRealNote = index;
                    changed = true;
                    break;
                }
            }
            return changed ? lastRealNote : -1;
        }

        public void RefreshBackgroundColor()
        {
            var lastRealNote = GetLastRealNoteIndex();
            for (int i = 0; i < noteGrids.Count; i++)
            {
                var grid = noteGrids[i];
                if (grid.Tag is int index)
                {
                    if (index == selectedIndex)
                    {
                        grid.Background = hex("#EFEFEF");
                        continue;
                    }
                    if (index <= lastRealNote)
                    {
                        grid.Background = hex("#90EFEFEF");
                        continue;
                    }
                }
                grid.Background = hex("#60EFEFEF");
            }
        }

        public JsonObject Save()
        {
            var obj = new JsonObject();
            obj["comment"] = comment;
            obj["loop"] = loop;

            var lastRealNote = GetLastRealNoteIndex();
            var notes = new JsonArray();
            for (int i = 0; i <= lastRealNote; i++)
            {
                var note = this.notes[i];
                notes.Add(new JsonValue(note));
            }
            obj["notes"] = notes;
            return obj;
        }
    }

    public class TrackData(int[] notes, float[] volumes)
    {
        public readonly int[] notes = notes;
        public readonly float[] volumes = volumes;
    }

    public class TimerWrapper
    {
        private Thread thread;
        private bool stopped = false;
        public TimerWrapper(Action action, Func<bool> running, Func<double> interval)
        {
            thread = new Thread(() =>
            {
                var stopwatch = new Stopwatch();
                while (running() && !stopped)
                {
                    stopwatch.Start();
                    var intervalMills = interval();
                    while (!stopped && stopwatch.ElapsedMilliseconds < intervalMills) ;
                    if (!running() || stopped) break;
                    action();
                    stopwatch.Stop();
                    stopwatch.Reset();
                }
                stopwatch.Stop();
            });
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            stopped = true;
            thread = null;
        }
    }

    /// <summary>
    /// PageEditTracks.xaml 的交互逻辑
    /// </summary>
    public partial class PageEditTracks : EditorPage, INotifyPropertyChanged
    {
        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        // 实现 INotifyPropertyChanged 接口（用于通知UI属性变化）
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal List<MusicTrack> tracks = new List<MusicTrack>();
        internal MainWindow mainWindow;
        string selected;

        private List<TrackData> previewTracks;
        private int previewLength;
        private int previewProgress;
        private double previewGapMills;
        private TimerWrapper previewTimer;

        public PageEditTracks(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            DataContext = this;
            OnSelectedMediaChanged += (sender, selected) =>
            {
                this.selected = selected;
                if (selected != null)
                {
                    TextMediaName.Text = selected;
                    var json = mainWindow.json;
                    var volume = json["volume"].ToObject();
                    if (volume != null && volume.ContainsKey(selected))
                    {
                        InputVolume.Value = Convert.ToDecimal(volume[selected].ToDouble());
                    }
                    else
                    {
                        InputVolume.Value = 1.0m;
                    }
                    InputVolume.IsEnabled = true;
                    ButtonSaveVolume.IsEnabled = true;
                }
                else
                {
                    TextMediaName.Text = "(未选中资源)";
                    InputVolume.Value = 1.0m;
                    InputVolume.IsEnabled = false;
                    ButtonSaveVolume.IsEnabled = false;
                }
            };
            OnMediaClick += (sender, media) =>
            {
                if (int.TryParse(media.Replace(".mp3", ""), out int result))
                {
                    foreach (var item in this.tracks)
                    {
                        if (item.selectedIndex == -1) continue;
                        var old = item.notes[item.selectedIndex];
                        if (old != result)
                        {
                            item.SetNote(item.selectedIndex, result);
                            SaveTracks();
                        }
                        break;
                    }
                }
            };
            OnClose += (sender, e) =>
            {
                IsPlaying = false;
                if (previewTimer != null)
                {
                    previewTimer.Stop();
                    previewTimer = null;
                }
            };
            var tracks = mainWindow.json["tracks"]?.ToArray();
            if (tracks != null)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i].ToObject();
                    var comment = track["comment"]?.ToString() ?? ("Track" + (i + 1));
                    var loop = track["loop"]?.ToBool() ?? false;
                    var notes = track["notes"].ToArray().ToList(v => v.ToInt());
                    AddTrack(i, comment, loop, notes);
                }
            }
            FillTracksNotes();
            BuildPreviewData();
        }

        public void BuildPreviewData()
        {
            int length = 0;
            foreach (var track in this.tracks)
            {
                int lastRealNote = track.GetLastRealNoteIndex() + 1;
                if (lastRealNote > length) length = lastRealNote;
            }
            var volumeJson = (mainWindow.json["volume"] ?? new JsonObject()).ToObject();
            List<TrackData> tracks = new List<TrackData>();
            foreach (var track in this.tracks)
            {
                int[] notes = new int[length];
                float[] volumes = new float[length];
                for (int i = 0; i < length; i++)
                {
                    var notesLength = track.GetLastRealNoteIndex() + 1;
                    var note = track.loop
                        ? track.notes[i % notesLength]
                        : i >= notesLength ? -1 : track.notes[i];
                    var volume = volumeJson == null ? 1.0f
                        : (float) (volumeJson[note + ".mp3"]?.ToDouble() ?? 1.0d);
                    notes[i] = note;
                    volumes[i] = volume;
                }
                tracks.Add(new TrackData(notes, volumes));
            }
            previewLength = length;
            previewTracks = tracks;
            SliderProgress.Maximum = previewLength;
            SliderProgress.Value = previewProgress % previewLength;
            UpdatePreviewTime();
        }

        public void PlayPreviewFrame(int index)
        {
            for (int i = 0; i < previewTracks.Count; i++)
            {
                var track = previewTracks[i];
                if (tracks[i].mute) continue;
                int note = track.notes[index];
                float volume = track.volumes[index];
                if (note == -1) continue;
                mainWindow.PlayAudio(note + ".mp3", volume);
            }
            UpdatePreviewTime();
        }

        public void UpdatePreviewTime()
        {
            static string ToString(double time)
            {
                var minute = (int)Math.Round(time / 60.0);
                var second = (int)Math.Round(time % 60);
                return $"{minute:D2}:{second:D2}";
            }
            var progress = previewGapMills * previewProgress / 1000.0;
            var length = previewGapMills * previewLength / 1000.0;
            var time = $"{ToString(progress)}/{ToString(length)}";
            if (TextProgress != null)
            {
                TextProgress.Text = time;
            }
        }

        public void DeleteTrack(MusicTrack targetTrack)
        {
            tracks.Remove(targetTrack);
            ScrollLeftContent.Children.Remove(targetTrack.left);
            ScrollRightContent.Children.Remove(targetTrack.right);
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                track.UpdateBackgroundByIndex(i);
            }
        }

        public void AddTrack(int index, string comment, bool loop, List<int> notes)
        {
            this.tracks.Add(new MusicTrack(this, ScrollLeftContent, ScrollRightContent, index, comment, loop, notes, (track, noteIndex) =>
            {
                var note = track.notes[noteIndex];
                if (note > -1)
                {
                    var mediaName = note + ".mp3";
                    mainWindow.SelectMedia(mediaName);
                }
            }));
        }

        public void SaveTracks()
        {
            var tracks = new JsonArray();
            foreach (var item in this.tracks)
            {
                tracks.Add(item.Save());
            }
            mainWindow.json["tracks"] = tracks;
            mainWindow.MarkEdit();
            BuildPreviewData();
        }

        public void FillTracksNotes()
        {
            int max = 32;
            foreach (var item in tracks)
            {
                var changed = false;
                var realNoteLength = item.notes.Count;
                for (int index = realNoteLength - 1; index >= 0; index--)
                {
                    if (item.notes[index] > -1)
                    {
                        realNoteLength = index + 1;
                        changed = true;
                        break;
                    }
                }
                if (changed && realNoteLength > max)
                {
                    max = realNoteLength;
                }
            }
            int length = max * 2;
            foreach (var track in tracks)
            {
                int start = track.noteGrids.Count;
                int needCount = length - track.noteGrids.Count;
                if (needCount > 0)
                {
                    for (int i = 0; i < needCount; i++)
                    {
                        int index = start + i;
                        track.notes.Add(-1);
                        track.AddNoteGrid(index, -1);
                    }
                    track.RefreshBackgroundColor();
                }
            }
        }

        private void ScrollLeft_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset != ScrollRight.VerticalOffset)
            {
                ScrollRight.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }
        private void ScrollRight_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset != ScrollLeft.VerticalOffset)
            {
                ScrollLeft.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

        private void ButtonSaveVolume_Click(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {
                var json = mainWindow.json;
                var volume = (json.ContainsKey("volume") ? json["volume"] : (json["volume"] = new JsonObject())).ToObject();
                var value = Convert.ToDouble(InputVolume.Value ?? 1.0m);
                if (value == 1.0)
                {
                    volume.Remove(selected);
                    mainWindow.MarkEdit();
                }
                else
                {
                    volume[selected] = value;
                    mainWindow.MarkEdit();
                }
            }
        }

        private void InputBPM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var inputValue = InputBPM.Value;
            if (inputValue == null) return;
            var value = Convert.ToDouble(inputValue);
            var oldBPM = mainWindow.json["bpm"];
            previewGapMills = 60000.0 / value / 2.0;
            UpdatePreviewTime();
            if (oldBPM?.IsNumber() == true && oldBPM.ToDouble() == value) return;
            if (IsPlaying)
            {
                // 如果 BPM 急剧变小，则停止播放。
                // 否则可能会出现一分钟播放一次的情况 (BPM=1)
                double oldValue = e.OldValue == null ? 0 : Convert.ToDouble(e.OldValue);
                double newValue = e.NewValue == null ? 120 : Convert.ToDouble(e.NewValue);
                if (Math.Abs(newValue - oldValue) >= 10 && newValue < 60)
                {
                    IsPlaying = false;
                    if (previewTimer != null)
                    {
                        previewTimer.Stop();
                        previewTimer = null;
                    }
                }
            }
            mainWindow.json["bpm"] = value;
            mainWindow.MarkEdit();
        }

        private void ScrollRight_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                // 当按住 Shift 键时，或者纵向无法滚动时，执行横向滚动
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) || scrollViewer.ScrollableHeight == 0)
                {
                    double newHorizontalOffset = scrollViewer.HorizontalOffset - e.Delta;
                    newHorizontalOffset = Math.Max(0, Math.Min(newHorizontalOffset, scrollViewer.ScrollableWidth));
                    scrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
                    e.Handled = true;
                }
            }
        }

        private void ScrollRightContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 点击空白处取消选择
            foreach (var track in tracks)
            {
                track.selectedIndex = -1;
                track.RefreshBackgroundColor();
            }
        }

        private void ButtonAddTrack_Click(object sender, RoutedEventArgs e)
        {
            var max = 0;
            foreach (var track in tracks)
            {
                if (track.notes.Count > max) max = track.notes.Count;
            }
            var i = tracks.Count;
            var comment = "Track" + (i + 1);
            var loop = false;
            var notes = new List<int>();
            for (int j = 0; j < max; j++)
            {
                notes.Add(-1);
            }
            AddTrack(i, comment, loop, notes);
            foreach (var track in tracks)
            {
                track.selectedIndex = -1;
                track.RefreshBackgroundColor();
            }
            SaveTracks();
        }

        public void ButtonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying)
            {
                PlayPreviewFrame(previewProgress % previewLength);
                previewTimer = new TimerWrapper(() =>
                {
                    previewProgress = (previewProgress + 1) % previewLength;
                    Dispatcher.Invoke(() => SliderProgress.Value = previewProgress);
                    Dispatcher.InvokeAsync(() => PlayPreviewFrame(previewProgress));
                }, () => IsPlaying, () => previewGapMills);
                previewTimer.Start();
            }
            else
            {
                if (previewTimer != null)
                {
                    previewTimer.Stop();
                    previewTimer = null;
                }
            }
        }

        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (previewTracks == null || previewLength == 0) return;
            if (!IsPlaying)
            {
                if (e.NewValue >= previewLength)
                {
                    SliderProgress.Value = 0;
                    return;
                }
                previewProgress = ((int)e.NewValue) % previewLength;
                PlayPreviewFrame(previewProgress);
            }
            else
            {
                int progress = ((int)e.NewValue) % previewLength;
                if (progress != previewProgress)
                {
                    previewProgress = progress;
                }
            }
        }
    }
}
