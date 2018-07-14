using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace QRCodeWebLoader.Reader
{
    class QrCodeUtil
    {
        /// <summary>
        /// QRコード作成
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static void CreateHBitmap(string text, string filePath)
        {
            BarcodeWriter qrcode = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    ErrorCorrection = ErrorCorrectionLevel.H,
                    CharacterSet = "Shift_JIS",
                    Height = 300,
                    Width = 300,
                    Margin = 3
                }
            };
            using (Bitmap qrcodeImg = qrcode.Write(text))
            {
                qrcodeImg.Save(filePath);
            }
        }
    }
}
