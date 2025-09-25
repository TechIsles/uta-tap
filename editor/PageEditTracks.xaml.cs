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
    public partial class PageEditTracks : EditorPage
    {
        MainWindow mainWindow;
        public PageEditTracks(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
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
    }
}
