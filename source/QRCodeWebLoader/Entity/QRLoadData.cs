using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeWebLoader.Entity
{
    public class QRLoadData : IEquatable<QRLoadData>
    {
        public string thumb { get; set; }
        public string title { get; set; }
        public string url { get; set; }

        public QRLoadData(string url) {
            this.url = url;
        }

        public override int GetHashCode()
        {
            return url.GetHashCode();
        }


        public bool Equals(QRLoadData other)
        {
            if (other == null) return false;
            return (url == other.url);
        }
    }
}
