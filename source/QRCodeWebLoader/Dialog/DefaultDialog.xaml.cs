using System.Windows;

namespace QRCodeWebLoader.Dialog
{
    /// <summary>
    /// ErrorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DefaultDialog : Window
    {
        public DefaultDialog(string title)
        {
            InitializeComponent();
            this.Title = title;
        }

        public void SetMsg(string msg)
        {
            message.Text = msg;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
