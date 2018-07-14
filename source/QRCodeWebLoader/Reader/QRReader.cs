using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;

namespace QRCodeWebLoader.Reader
{
    class QRReader
    {
        /// <summary>
        /// QRコードリーダー
        /// </summary>
        private BarcodeReader reader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };

        /// <summary>
        /// QRコード読み取り
        /// </summary>
        public string GetUrlText(string filePath)
        {
            using (Bitmap bmp = FilePathToBitomap(filePath))
            {
                if (bmp != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Result result = reader.Decode(bmp);
                        if (result != null && result.Text.StartsWith("http"))
                        {
                            return result.Text;
                        }
                        // 回転することで読込可能になることもあるので、90度ずつ回転
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// ファイルパスをBitmapオブジェクト可
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Bitmap FilePathToBitomap(string filePath)
        {
            // TODO:画像の判定方法にいい方法があれば切り替える為、切り出し
            // ファイルヘッダ見ればよいか？？
            try
            {
                return new Bitmap(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
