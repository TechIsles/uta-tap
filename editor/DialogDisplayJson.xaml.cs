using System.Windows;

namespace editor
{
    /// <summary>
    /// DialogDisplayJson.xaml 的交互逻辑
    /// </summary>
    public partial class DialogDisplayJson : Window
    {
        public DialogDisplayJson(string text)
        {
            InitializeComponent();
            TextJson.AppendText(text);
            TextJson.IsReadOnly = true;
        }
    }
}
