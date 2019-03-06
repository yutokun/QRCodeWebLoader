using Microsoft.Win32;
using QRCodeWebLoader.Dialog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SysIO = System.IO;
using System.IO;
using NAudio.Wave;
using System.ComponentModel;
using QRCodeWebLoader.Reader;
using QRCodeWebLoader.Entity;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Net;
using System.Collections.Generic;
using System.Windows.Input;
using Common;
using System.Reflection;
using System.Text;
using System.Linq;

namespace QRCodeWebLoader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string qrListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRCodeList.csv");


        /// <summary>
        /// アイコン取得用パターン
        /// </summary>
        private Dictionary<string, string> iconPattern = new Dictionary<string, string>() {
            {"github.com", @"(\<link rel\=""fluid-icon"" href\="")(?<imgUrl>.*?)("")"},
            {"booth.pm", @"(\<meta property\=""og:image"" content\="")(?<imgUrl>.*jpg?)("")"},
        };

        /// <summary>
        /// リスト表示用データ
        /// </summary>
        private ObservableCollection<QRLoadData> showDataList { get; set; }

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
            ShowWebView.IsChecked = Properties.Settings.Default.IsWebView;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            showDataList = new ObservableCollection<QRLoadData>();
            ReadQRCodeFile(qrListPath);
            if (RunWatcher)
            {
                Watcher.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            string writeText = string.Empty;
            foreach (var qrData in showDataList)
            {
                writeText += qrData.thumb + "\t";
                writeText += qrData.title + "\t";
                writeText += qrData.url + "\t";
                writeText += "\n";
            }

            // 内容が差があれば複製
            if (File.Exists(qrListPath) && writeText != FileUtil.GetAllText(qrListPath))
            {
                File.Move(qrListPath, FileUtil.ExistsFilePathRename(qrListPath, DateTime.Now.ToString("yyyyMMdd_{0}")));
            }
            FileUtil.WriteText(writeText, qrListPath, Encoding.UTF8, false);

        }

        /// <summary>
        /// ファイルから一覧を読み込み
        /// </summary>
        /// <param name="filePath"></param>
        private void ReadQRCodeFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                foreach (string recode in FileUtil.GetAllText(filePath).Split('\n'))
                {
                    if (string.IsNullOrEmpty(recode)) continue;
                    string[] recodeItem = recode.Split('\t');

                    showDataList.Add(new QRLoadData(recodeItem[2])
                    {
                        thumb = recodeItem[0],
                        title = recodeItem[1]
                    });
                }
            }
            QRLoadList.ItemsSource = showDataList;
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
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.Invoke((Action<object, FileSystemEventArgs>)Watcher_Changed, source, e);
                return;
            }

            if (File.Exists(e.FullPath) && IsRead(e.FullPath))
            {
                string qrText = qrReader.GetUrlText(e.FullPath);
                if (!string.IsNullOrEmpty(qrText))
                {
                    readFile.successFlg = true;
                    PlaySound(Properties.Resources.se_maoudamashii_onepoint07);

                    showDataList.Add(LoadQrCodeData(qrText));
                    Process.Start(qrText);
                    return;
                }
            }
        }

        /// <summary>
        /// アイコン取得処理
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="docText"></param>
        /// <returns></returns>
        private string GetIconUrl(string domain, string docText) {

            if (iconPattern.ContainsKey(domain)) {
                return Regex.Match(docText, iconPattern[domain]).Groups["imgUrl"].Value;
            }

            string imgUrl = string.Empty;
            foreach (var pattern in iconPattern.Values)
            {
                imgUrl = Regex.Match(docText, pattern).Groups["imgUrl"].Value;
                if (!string.IsNullOrEmpty(imgUrl)) {
                    break;
                }
            }
            return imgUrl;
        }

        /// <summary>
        /// QRコードのURLから情報を取得
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private QRLoadData LoadQrCodeData(string url) {
            var loadData = new QRLoadData(url);
            using (var client =  new HttpClient()) {
                string result = client.GetAsync(loadData.url).Result.Content.ReadAsStringAsync().Result;
                string domain = Regex.Match(loadData.url, @"(//)(?<domain>.*?)(/)").Groups["domain"].Value;
                loadData.thumb = GetIconUrl(domain, result);
                loadData.title = Regex.Match(result, @"(\<title\>)(?<title>.*?)(\</title\>)").Groups["title"].Value.Replace("\t", " ").Replace("\r\n", "").Replace("\n", "");
            }
            return loadData;
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
            Properties.Settings.Default.IsWebView = (bool)ShowWebView.IsChecked;
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

        /// <summary>
        /// リストのダブルクリック時に開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QRLoadList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedVal = (QRLoadData)QRLoadList.SelectedItem;
            if (selectedVal != null)
            {
                Process.Start(selectedVal.url);
            }
        }

        /// <summary>
        /// 削除ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (QRLoadList.SelectedItem != null)
            {
                showDataList.Remove((QRLoadData)QRLoadList.SelectedItem);
                
            }
        }

        /// <summary>
        /// タイトルソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleSort_Click(object sender, RoutedEventArgs e)
        {
            showDataList = new ObservableCollection<QRLoadData>(showDataList.OrderBy(x => x.title));
            QRLoadList.ItemsSource = showDataList;
        }

        /// <summary>
        /// URLソート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UrlSort_Click(object sender, RoutedEventArgs e)
        {
            showDataList = new ObservableCollection<QRLoadData>(showDataList.OrderBy(x => x.url));
            QRLoadList.ItemsSource = showDataList;
        }

        /// <summary>
        /// URLベースで重複削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UrlDistinct_Click(object sender, RoutedEventArgs e)
        {
            showDataList = new ObservableCollection<QRLoadData>(showDataList.OrderBy(x => x.url).Distinct().ToArray());
            QRLoadList.ItemsSource = showDataList;
        }

        /// <summary>
        /// ファイルの読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CSVRead_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fileDialog.FileName = "フォルダ選択";
            fileDialog.CheckPathExists = false;
            fileDialog.CheckFileExists = false;
            fileDialog.ValidateNames = false;
            fileDialog.Title = "QRリスト読込";
            if (fileDialog.ShowDialog() == true)
            {
                ReadQRCodeFile(fileDialog.FileName);
                showDataList = new ObservableCollection<QRLoadData>(showDataList.OrderBy(x => x.url));
                showDataList = new ObservableCollection<QRLoadData>(showDataList.OrderBy(x => x.url).Distinct().ToArray());
                QRLoadList.ItemsSource = showDataList;
            }
        }

    }
}
