# QRCodeWebLoader
改定：2019年03月06日


## １．前提条件
- 動作確認環境：Windows10
- ＯＳ：Windows7～

※Windows7 の場合、「.NET Framework 4.6.1」以上をインストールする必要があります。<br>
※基本的には「Windows Update」自動でインストールされています。

### １ー１． 使用ライブラリ
- ZXing.Net：https://github.com/micjahn/ZXing.Net/

    Apache License Version 2.0

- NAudio：https://github.com/naudio/NAudio

    Microsoft Public License (Ms-PL)

## ２．イインストール（解凍）
### ２ー１．配布「QRCodeWebLoader.zip」を任意の場所に解凍 
![インストール1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/001_zip.jpg "使用方法")

![インストール2](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/002_zip.jpg "使用方法")



## ３．QRコード作成

### ３－１．作成したいURLを入力して、「QR 作成」をクリック

![QRコード作成1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/100_qrcode.jpg "使用方法")


### ３－２．保存先を指定して、「保存」をクリック 

![QRコード作成2](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/101_qrcode.jpg "使用方法")


指定したフォルダに、QRコードの画像が作成されます。

![QRコード作成3](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/102_qrcode.jpg "使用方法")




## ４．使用方法

### ４－１．解凍先のフォルダ内の「QRCodeWebLoader.exe」をクリック 

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/201_manual.jpg "使用方法")

画面が起動します。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/202_manual.jpg "使用方法")

|||
|---|---|
|URL|QR コード作成用の文字列を設定する。|
|保存先フォルダ|撮影、スクショした画像の保存先フォルダ|
|音量|QR コード読み取り成功時に流れる音の音量を調整します。|
|URL重複削除|読み込んだURL一覧から、重複するURLを削除します。|
|タイトルソート|タイトルで一覧をソートします。|
|URLソート|URLでソートします。|
|ファイル読込|保存されている一覧を読み込みます。<br>※「QRCodeWebLoader.exe」と同じフォルダ出力される「QRCodeList.csv」|
|読込開始|指定した保存先フォルダを監視して、QR コードの読み取りをします。|
|自動的にブラウザで開く|チェックしている場合、QRコード読み取り時に、Webブラウザを開きます。|
|削除|選択している一覧のレコードを削除します。|






### ４－２．「参照」をクリック 

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/203_manual.jpg "使用方法")



### ４－３． 任意のフォルダ内のファイルを選択して、「開く」をクリック
- VRのデフォルトフォルダ：「C:\Users\[ユーザー名]\Pictures\VRChat」
- Steamスクリーンショットのデフォルトフォルダ（デスクトップ）：<br>
「C:\Program Files (x86)\Steam\userdata\[USER_ID]\760\remote\[GAME_ID]\screenshots」<br>
※選択するファイルがない場合、適当なファイル名を入力してください。（デフォルトで入力済み）

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/204_manual.jpg "使用方法")


選択したフォルダが表示されます。

※直接フォルダのパスを貼り付けることも可能です。


![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/205_manual.jpg "使用方法")


### ４－４． 「読取開始」をクリック

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/206_manual.jpg "使用方法")


「読取開始」ボタンが、「読取停止」ボタンに変わり、中央のメッセージが「読み取り中」になります。<br>
この状態で、撮影、スクショした画像にQRコードが含まれている場合、自動的にWebブラウザが起動します。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/207_manual.jpg "使用方法")




### ４－５． 撮影、スクショの注意点
下記のように画面のなるべく中央になるように撮影を行ってください。（縦撮影は可）<br>
読み取り成功時に音がします。無音の場合、読取に失敗しています。<br>
※複数のQRコードを同時に含めないでください。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/208_manual.jpg "使用方法")


読み取りが成功すると下記のように、ブラウザが起動します。<br>
※VRの場合、デスクトップ側で起動してます。（「自動的にブラウザを開く」にチェックを入れている場合）

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/209_manual.jpg "使用方法")


写真の中央にQRコードが写っていれば、縦での撮影、スクショは可能です。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/210_manual.jpg "使用方法")

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/211_manual.jpg "使用方法")

一覧に読み込んだQRコードの情報が表示されます。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/212_manual.jpg "使用方法")


画面を閉じると自動的に、「QRCodeList.csv」作成させれます。<br>
※内容が変更されていると、変更前をバックアップとして、日付単位のファイル名に変更します。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/213_manual.jpg "使用方法")


### ４－６． QRコード読み取り成功時の音量調整
クリックしたまま、左右に移動することで音量が調整できます。

![使用方法1](https://github.com/Himakuma/QRCodeWebLoader/blob/master/manual/img/214_manual.jpg "使用方法")








