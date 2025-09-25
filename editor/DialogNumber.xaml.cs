using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace editor
{
    /// <summary>
    /// DialogNumber.xaml 的交互逻辑
    /// </summary>
    public partial class DialogNumber : Window
    {
        public int Value => (int)(InputNumber.Value ?? 0);
        Func<string, bool> validate;
        public DialogNumber(string fileName, int def, Func<string, bool> validate)
        {
            InitializeComponent();
            this.validate = validate;
            FileInfo fi = new(fileName);
            if (int.TryParse(fi.Name.AsSpan(0, fi.Name.Length - fi.Extension.Length), out int result))
            {
                InputNumber.Value = result;
            }
            else
            {
                InputNumber.Value = def;
            }
            FileNameText.Inlines.Clear();
            FileNameText.Inlines.Add(new Run(fi.Name));
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (validate.Invoke(Value + ".mp3"))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("同名的资源文件已经存在了", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
