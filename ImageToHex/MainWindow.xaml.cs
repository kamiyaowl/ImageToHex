using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageToHex
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFileDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileOk += (sx, ex) => filePath.Text = dialog.FileName;
            dialog.ShowDialog();
        }

        private void convertButton_Click(object sender, RoutedEventArgs e)
        {
            using (var src = new IplImage(filePath.Text, LoadMode.GrayScale))
            {
                using (var resize = new IplImage(new CvSize(400, 240), src.Depth, src.NChannels))
                {
                    using (var threash = new IplImage(resize.Size, BitDepth.U8, 1))
                    {
                        src.Resize(resize);
                        resize.AdaptiveThreshold(threash, 0xff);
                        result.Text = Convert(threash);
                        CvWindow.ShowImages(resize, threash);
                    }
                }
            }
        }
        private static string Convert(IplImage src)
        {
            return convertCol(src).Select(row => row.Aggregate("{ ", (s, x) => s + "0x" + x.ToString("x") + ", ") + "}, ")
                           .Aggregate("uint8 img_src[240][50] = {\r\n", (s, x) => s + x + "\r\n") + "}";
        }
        private static IEnumerable<IEnumerable<byte>> convertCol(IplImage src)
        {
            for (int j = 0; j < 240; ++j)
            {
                yield return convertRow(src, j);
            }
        }

        private static unsafe byte[] convertRow(IplImage src, int j)
        {
            var ptr = (byte*)src.ImageData;
            var datas = new byte[50];
            for (int i = 0; i < 400; i += 8)
            {
                byte data = 0x0;

                if (src[j * src.WidthStep + i + 0] != 0) data |= 0x01;
                if (src[j * src.WidthStep + i + 1] != 0) data |= 0x02;
                if (src[j * src.WidthStep + i + 2] != 0) data |= 0x04;
                if (src[j * src.WidthStep + i + 3] != 0) data |= 0x08;
                if (src[j * src.WidthStep + i + 4] != 0) data |= 0x10;
                if (src[j * src.WidthStep + i + 5] != 0) data |= 0x20;
                if (src[j * src.WidthStep + i + 6] != 0) data |= 0x40;
                if (src[j * src.WidthStep + i + 7] != 0) data |= 0x80;

                datas[i / 8] = data;
            }
            return datas;

        }
    }
}
