using Microsoft.Win32;
using QRCodeWebLoader.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using ZXing;
using ZXing.Common;
using System.Media;
using SysIO = System.IO;
using System.IO;
using NAudio.Wave;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using System.ComponentModel;
using QRCodeWebLoader.Reader;

namespace QRCodeWebLoader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 監視
        /// </summary>
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();

        /// <summary>
        /// QRコード読み取り
        /// </summary>
        private QRReader qrReader = new QRReader();

        /// <summary>
        /// 複数イベント発火対応
        /// ※FileSystemWatcherが実質ファイル単位にイベント発生させるので単一オブジェクトで対応
        /// </summary>
        private static ReadFile readFile = new ReadFile();

        class ReadFile
        {
            public string filePath = string.Empty;
            public bool successFlg = false;
        }

        /// <summary>
        /// 起動時の読み取り開始有無
        /// </summary>
        public bool RunWatcher { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            FileDirPath.Text = Properties.Settings.Default.ディレクトリ;
            Volume.Value = Properties.Settings.Default.音量;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (RunWatcher)
            {
                Watcher.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        /// <summary>
        /// 音再生
        /// </summary>
        /// <param name="audio"></param>
        private void PlaySound(UnmanagedMemoryStream audio)
        {
            Volume.Dispatcher.BeginInvoke(new Action(() => {
                if (0 < Volume.Value)
                {
                    WaveFileReader wr = new WaveFileReader(audio);
                    WaveOutEvent output = new WaveOutEvent();
                    float soundVolume = ((float)Volume.Value) / 100;
                    output.Volume = soundVolume;
                    output.Init(wr);
                    output.Play();
                }
            }));
        }

        /// <summary>
        /// フォルダダイアログ表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirDialog_Click(object sender, RoutedEventArgs e)
        {
            // TODO: ライセンス条項を読めてないので、commonの奴は使わない
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = FileDirPath.Text;
            fileDialog.FileName = "フォルダ選択";
            fileDialog.CheckPathExists = false;
            fileDialog.CheckFileExists = false;
            fileDialog.ValidateNames = false;
            fileDialog.Title = "キャプチャ保存先";
            if (fileDialog.ShowDialog() == true)
            {
                FileDirPath.Text = SysIO.Path.GetDirectoryName(fileDialog.FileName);
            }
        }

        /// <summary>
        /// 監視開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Click(object sender, RoutedEventArgs e)
        {
            if (fileWatcher.EnableRaisingEvents)
            {
                fileWatcher.EnableRaisingEvents = false;
                Watcher.Content = "読取開始";
                Status.Content = "読み取り停止中";
                FileDirPath.IsEnabled = true;
                DirDialog.IsEnabled = true;

            }
            else
            {
                fileWatcher.EnableRaisingEvents = false;
                if (!Directory.Exists(FileDirPath.Text))
                {
                    DefaultDialog error = new DefaultDialog("エラー");
                    error.SetMsg("フォルダが存在しません。");
                    error.ShowDialog();
                    return;
                }
                fileWatcher.Path = FileDirPath.Text;
                fileWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Security);
                fileWatcher.Filter = "";
                fileWatcher.IncludeSubdirectories = false;
                fileWatcher.Changed += new FileSystemEventHandler(Watcher_Changed);
                fileWatcher.Created += new FileSystemEventHandler(Watcher_Changed);
                fileWatcher.Renamed += new RenamedEventHandler(Watcher_Renamed);
                fileWatcher.EnableRaisingEvents = true;
                Watcher.Content = "読取停止";
                Status.Content = "読み取り中";
                FileDirPath.IsEnabled = false;
                DirDialog.IsEnabled = false;

            }
        }
        private static void Watcher_Renamed(object source, RenamedEventArgs e)
        {
            //  Show that a file has been renamed.
            WatcherChangeTypes wct = e.ChangeType;
            Console.WriteLine("File {0} {2} to {1}", e.OldFullPath, e.FullPath, wct.ToString());
        }

        /// <summary>
        /// ファイル追加時
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(Object source, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath) && IsRead(e.FullPath))
            {
                string qrText = qrReader.GetUrlText(e.FullPath);
                if (!string.IsNullOrEmpty(qrText))
                {
                    readFile.successFlg = true;
                    PlaySound(Properties.Resources.se_maoudamashii_onepoint07);
                    Process.Start(qrText);
                    return;
                }
            }
        }

        /// <summary>
        /// 複数回のイベント発火対応
        /// ※5秒間同一ファイル名は処理しない
        /// ※正確なスレッド管理はしてない（）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsRead(string filePath)
        {
            if (filePath == readFile.filePath && readFile.successFlg)
            {
                return false;
            }
            readFile.filePath = filePath;
            readFile.successFlg = false;
            return true;
        }

        /// <summary>
        /// 閉じる時に設定内容の保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (fileWatcher != null)
            {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
            }

            Properties.Settings.Default.ディレクトリ = FileDirPath.Text;
            Properties.Settings.Default.音量 = Volume.Value;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// QRコード作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QRCreate_Click(object sender, RoutedEventArgs e)
        { 
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "保存先のファイルを選択してください";
            sfd.RestoreDirectory = true;
            sfd.Filter = "PNGファイル(*.png)|*.png";
            sfd.FileName = DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".png";

            if (string.IsNullOrEmpty(Properties.Settings.Default.QR出力先))
            {
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            else
            {
                sfd.InitialDirectory = Properties.Settings.Default.QR出力先;
            }

            if (sfd.ShowDialog() == true)
            {
                Properties.Settings.Default.QR出力先 = SysIO.Path.GetDirectoryName(sfd.FileName);
                Properties.Settings.Default.Save();
                QrCodeUtil.CreateHBitmap(QRText.Text, sfd.FileName);
            }
        }

        /// <summary>
        /// 同時起動ショートカット作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateLauncher_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "同時起動のショートカットを指定してください。";
            fileDialog.Filter = "URL Files (.url)|*.url|LNK Files (*.lnk)|*.lnk|EXE Files(*.exe)|*.exe";
            if (fileDialog.ShowDialog() == true)
            {
                string createPath = SysIO.Path.Combine(SysIO.Path.GetDirectoryName(fileDialog.FileName), SysIO.Path.GetFileNameWithoutExtension(fileDialog.FileName) + "_QRCodeWebLoader.bat");
                using (StreamWriter writer = new StreamWriter(createPath, false))
                {
                    writer.WriteLine("start " + System.Reflection.Assembly.GetExecutingAssembly().Location + " 1 \"" + fileDialog.FileName + "\"");
                    writer.Close();
                }

                DefaultDialog error = new DefaultDialog("完了");
                error.SetMsg("ショートカットを作成しました。\n["  + createPath + "]");
                error.ShowDialog();
                return;
            }
        }
    }
}
