using CsharpJson;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace editor
{
    public class MusicTrack
    {
        PageEditTracks parent;
        Action<MusicTrack, int> onSelected;
        public Grid left;
        public StackPanel right;
        public string comment;
        public bool loop;
        public List<int> notes;
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
            this.left = new Grid()
            {
                Margin = new Thickness(0, 2, 0, 2),
                Height = 20,
                Background = hex(index % 2 == 1 ? "#EAEAEA" : "#EFEFEF")
            };
            this.left.Children.Add(new TextBlock()
            {
                Text = comment,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 0, 0, 0),
            });
            this.right = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(1, 2, 1, 2),
                Height = 20,
                Background = hex(index % 2 == 1 ? "#7B7B7B" : "#808080")
            };
            this.comment = comment;
            this.loop = loop;
            this.notes = notes;

            for (int i = 0; i < notes.Count; i++)
            {
                AddNoteGrid(i, notes[i]);
            }

            left.Children.Insert(index, this.left);
            right.Children.Insert(index, this.right);
        }

        public void AddNoteGrid(int noteIndex, int note)
        {
            var grid = new Grid()
            {
                Tag = noteIndex,
                Width = 25,
                Height = 20,
                Margin = new Thickness(1, 0, 1, 0),
                Background = hex("#AAAAAA"),
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
                SetNote(noteIndex, -1);
                parent.SaveTracks();
                parent.mainWindow.MarkEdit();
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

        public void RefreshBackgroundColor()
        {
            var lastRealNote = notes.Count - 1;
            for (int index = lastRealNote; index >= 0; index--)
            {
                if (notes[index] > -1)
                {
                    lastRealNote = index;
                    break;
                }
            }
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
                        grid.Background = hex("#BFBFBF");
                        continue;
                    }
                }
                grid.Background = hex("#AAAAAA");
            }
        }

        public JsonObject Save()
        {
            var obj = new JsonObject();
            obj["comment"] = comment;
            obj["loop"] = loop;

            var lastRealNote = this.notes.Count - 1;
            for (int index = lastRealNote; index >= 0; index--)
            {
                if (this.notes[index] > -1)
                {
                    lastRealNote = index;
                    break;
                }
            }
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
    /// <summary>
    /// PageEditSinger.xaml 的交互逻辑
    /// </summary>
    public partial class PageEditTracks : EditorPage
    {
        internal List<MusicTrack> tracks = new List<MusicTrack>();
        internal MainWindow mainWindow;
        string selected;
        public PageEditTracks(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
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
            var tracks = mainWindow.json["tracks"].ToArray();
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i].ToObject();
                var comment = track["comment"]?.ToString() ?? ("Track" + (i + 1));
                var loop = track["loop"]?.ToBool() ?? false;
                var notes = track["notes"].ToArray().ToList(v => v.ToInt());
                this.tracks.Add(new MusicTrack(this, ScrollLeftContent, ScrollRightContent, i, comment, loop, notes, (track, noteIndex) =>
                {
                    var note = track.notes[noteIndex];
                    if (note > -1)
                    {
                        var mediaName = note + ".mp3";
                        mainWindow.SelectMedia(mediaName);
                    }
                }));
            }
            FillTracksNotes();
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
        }

        public void FillTracksNotes()
        {
            int max = 32;
            foreach (var item in tracks)
            {
                var realNoteLength = item.notes.Count;
                for (int index = realNoteLength - 1; index >= 0; index--)
                {
                    if (item.notes[index] > -1)
                    {
                        realNoteLength = index + 1;
                        break;
                    }
                }
                if (realNoteLength > max)
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
            if (oldBPM.IsNumber() && oldBPM.ToDouble() == value) return;
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
            foreach (var item in tracks)
            {
                item.selectedIndex = -1;
                item.RefreshBackgroundColor();
            }
        }
    }
}
