using CsharpJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace editor
{
    /// <summary>
    /// PageEditSinger.xaml 的交互逻辑
    /// </summary>
    public partial class PageEditSinger : EditorPage
    {
        MainWindow mainWindow;
        string selected;
        public PageEditSinger(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            OnSelectedMediaChanged += (sender, selected) =>
            {
                this.selected = selected;
                if (selected != null && selected != "-1")
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
    }
}
