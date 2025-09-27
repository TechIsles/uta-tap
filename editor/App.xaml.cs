using System.Configuration;
using System.Data;
using System.Windows;

namespace editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Version VERSION = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        public static string VersionString => $"{VERSION.Major}.{VERSION.Minor}.{VERSION.Build}";
    }

}
