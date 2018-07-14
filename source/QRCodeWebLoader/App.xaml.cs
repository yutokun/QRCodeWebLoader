using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QRCodeWebLoader
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [System.STAThreadAttribute()]
        static public void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Startup += App_Startup;
            app.Run();
        }
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mw = new MainWindow();
            if (0 < e.Args.Length && "1" == e.Args[0])
            {
                mw.RunWatcher = true;
            }

            if (1 < e.Args.Length)
            {
                Process.Start(e.Args[1]);
              
            }
            mw.Show();
        }
    }
}
