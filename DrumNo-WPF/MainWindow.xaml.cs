using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace DrumNo_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DynamicResourceExtension lp = new DynamicResourceExtension();
        BaseFont bf = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
        BaseFont lf = BaseFont.CreateFont(@"c:\windows\fonts\simhei.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

       
        public MainWindow()
        {
            TJIT.lpi lv = new TJIT.lpi();
            lv.UseDefaultCredentials = true;
            if (lv.DrumNoVerify(System.Environment.UserName))
            {
                InitializeComponent();
                Resources.Add("filesource", "a.pdf");
            }
            else {
                MessageBox.Show("权限不足！");
                Application.Current.Shutdown();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fs = new OpenFileDialog();
            fs.Title = "Select Label";
            fs.Filter = "PDF文档|*.pdf|所有文件|*.*";
            fs.FileName = string.Empty;
            fs.RestoreDirectory = true;
            if ((bool)fs.ShowDialog().GetValueOrDefault()) {
                filesource.Text = fs.FileName;
                
            }
            
        }

        private void NO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
            }
            return true;
        }

        private void NO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }
        private void NO_Pasting(object sender, 
            DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }

        private void NO_LostFocus(object sender, RoutedEventArgs e)
        {
            var a = (TextBox)sender;
            if (!isNumberic(a.Text)) {
               this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
               new Action(() =>
               {
                   a.Focus();
                   a.SelectAll();
               }));               
            }
            if (a.Name == "NOFrom" || a.Name == "NOTo") {
                VerifyToBigerThenFrom(sender);
            }
        }
        private bool VerifyToBigerThenFrom(object sender) {
            var t = (TextBox)sender;
            if (!string.IsNullOrEmpty(NOFrom.Text) && !string.IsNullOrEmpty(NOTo.Text))
            {
                var a = int.Parse(NOFrom.Text);
                var b = int.Parse(NOTo.Text);
                if (a > b)
                {
                    MessageBox.Show(this.FindResource("NumRegionError").ToString());
                    //t.Text = "";
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                        new Action(() =>
                        {                           
                            //t.Focus();
                            //t.SelectAll();
                        }));
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else {
                return true;
            }
        }
        private PdfReader GetPDF()
        {

            if (filesource.Text == "")
            {
                return new PdfReader("a.pdf");
            }
            else
            {
                return new PdfReader(filesource.Text);
            }
        }
        private void HalfA4Label_Click(object sender, RoutedEventArgs e)
        {
            Result.Content = "";
            try
            {
                PdfReader reader;
                //Document document = new Document(PageSize.A4.Rotate());
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream("Finish.pdf", FileMode.Create));
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                
                string pkgcate = PkgCate_Tong.Content.ToString();
                if (!(bool)PkgCate_Tong.IsChecked)
                {
                    pkgcate = PkgCate_Box.Content.ToString();
                }
                reader = GetPDF();
                if ((bool)drumno.IsChecked)
                {
                    if (NOFrom.Text != "" && NOTo.Text != "")
                    {

                        for (int j = Convert.ToInt32(NOFrom.Text); j <= Convert.ToInt32(NOTo.Text); j++)
                        {

                            for (int i = 1; i <= Convert.ToInt32(Copies.Text); i++)
                            {

                                HalfA4Draw(document, writer, reader, cb, pkgcate, i, j);

                            }

                            //j = j + 1;

                        }
                        document.Close();
                        Result.Content = Application.Current.Resources["Complete"];
                        Process.Start("Finish.pdf");
                        writer.Close();
                    }
                    else
                    {
                        Result.Content = Application.Current.Resources["PageNOnull"].ToString();
                        //button1.Enabled = false;
                    }
                }
                else {
                    HalfA4Draw(document, writer, reader, cb, pkgcate, 1, 1);
                    document.Close();
                    Result.Content = Application.Current.Resources["Complete"];
                    Process.Start("Finish.pdf");
                    writer.Close();
                }
            }
            catch (IOException ex) {
                Result.Content = ex.Message;
            }
        }
        private void HalfA4Draw(Document document, PdfWriter writer, PdfReader reader, PdfContentByte cb, string pkgcate,int i,int j)
        {
            PdfImportedPage newPage;
            document.NewPage();
            newPage = writer.GetImportedPage(reader, 1);
            cb.AddTemplate(newPage, 0, 0);
            if ((bool)drumno.IsChecked)
            {
                cb.BeginText();
                BaseFont bf = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                cb.SetFontAndSize(bf, 14);
                cb.SetTextMatrix(200, 705);
                cb.ShowText(pkgcate + "号：" + j);

                cb.SetTextMatrix(200, 293);
                cb.ShowText(pkgcate + "号：" + (Convert.ToInt32(j) + Convert.ToInt32(0)));
                cb.EndText();
            }
            int originX = 241;
            int[] originY = { 239, 649 };
            if ((bool)Mark3C.IsChecked)
            {
                ///3C标签
                iTextSharp.text.Image splitline = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\CCC.jpg");
                splitline.ScaleAbsolute(50, 50);
                splitline.SetAbsolutePosition(originX - splitline.PlainWidth, originY[0] - splitline.PlainHeight);
                cb.AddImage(splitline);
                splitline.SetAbsolutePosition(originX - splitline.PlainWidth, originY[1] - splitline.PlainHeight);
                cb.AddImage(splitline);
                //originX = originX + (int)splitline.PlainHeight;
            }
            if ((bool)MarkRohs.IsChecked)
            {
                iTextSharp.text.Image rohsl = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\rohs.jpg");
                rohsl.ScaleAbsolute(80, 80);
                rohsl.SetAbsolutePosition(originX - rohsl.ScaledWidth, originY[0] - rohsl.ScaledHeight);
                cb.AddImage(rohsl);
                rohsl.SetAbsolutePosition(originX - rohsl.ScaledWidth, originY[1] - rohsl.ScaledHeight);
                cb.AddImage(rohsl);
                //originX = originX + (int)rohsl.PlainHeight;
            }
            if ((bool)MarkQs.IsChecked)
            {
                iTextSharp.text.Image qsl = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\qs.jpg");
                qsl.ScaleAbsolute(45, 60);
                qsl.SetAbsolutePosition(originX - qsl.ScaledWidth, originY[0] - qsl.ScaledHeight);
                cb.AddImage(qsl);
                qsl.SetAbsolutePosition(originX - qsl.ScaledWidth, originY[1] - qsl.ScaledHeight);
                cb.AddImage(qsl);
                //originX = originX + (int)qsl.PlainHeight;
            }
            if ((bool)HideLogo.IsChecked)
            {
                iTextSharp.text.Image bi = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\blank.png");
                bi.ScaleAbsolute(63, 54);
                bi.SetAbsolutePosition(492, 351);
                cb.AddImage(bi);
                bi.SetAbsolutePosition(492, 762);
                cb.AddImage(bi);
            }
        }
        private void A4Label_Click(object sender, RoutedEventArgs e)
        {
            Result.Content = "";
            try
            {
                PdfReader reader;
                //Document document = new Document(PageSize.A4.Rotate());
                Document document = new Document();
                
                
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream("Finish.pdf", FileMode.Create));
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                
                
                string pkgcate = PkgCate_Tong.Content.ToString();
                if (!(bool)PkgCate_Tong.IsChecked) {
                    pkgcate = PkgCate_Box.Content.ToString();
                }
                reader = GetPDF();
                if ((bool)drumno.IsChecked)
                {
                    if (NOFrom.Text != "" && NOTo.Text != "")
                    {

                        for (int j = Convert.ToInt32(NOFrom.Text); j <= Convert.ToInt32(NOTo.Text); j++)
                        {

                            for (int i = 1; i <= Convert.ToInt32(Copies.Text); i++)
                            {
                                A4Draw(document, writer, reader, cb, pkgcate, i, j);
                            }

                        }

                        document.Close();
                        Result.Content = Application.Current.Resources["Complete"];
                        Process.Start("Finish.pdf");
                        writer.Close();

                    }
                    else
                    {
                        Result.Content = Application.Current.Resources["PageNOnull"].ToString();
                    }
                }
                else {
                    A4Draw(document, writer, reader, cb, pkgcate, 1, 1);
                    document.Close();
                    Result.Content = Application.Current.Resources["Complete"];
                    Process.Start("Finish.pdf");
                    writer.Close();
                }
            }
            catch (IOException ex)
            {
                Result.Content = ex.Message;
            }
        }
        private void A4Draw(Document document, PdfWriter writer, PdfReader reader, PdfContentByte cb, string pkgcate, int i, int j)
        {
            PdfImportedPage newPage;
            document.NewPage();
            //PdfStamper stamp = new PdfStamper(reader, new FileStream("Finish.pdf", FileMode.Create));
            newPage = writer.GetImportedPage(reader, 1);
            if (newPage.Height > newPage.Width)
            {
                cb.AddTemplate(newPage, 0, 0);
            }
            else
            {
                cb.AddTemplate(newPage, 0, 1, -1, 0, newPage.Height, 0);
            }
            cb.BeginText();

            cb.SetFontAndSize(bf, 14);
            cb.SetTextMatrix(0, 1, -1, 0, 193, 270);
            //cb.ShowText("桶号：" + j + " " + "to " + Convert.ToInt32(textBox2.Text));
            cb.ShowText(pkgcate + "号：" + j);
            cb.EndText();
            int originX = int.Parse(AsixX.Text), originY = int.Parse(AsixY.Text);
            int MaxX = int.Parse(this.Resources["A4MaxX"].ToString());
            int MinY = int.Parse(this.Resources["A4MinY"].ToString());
            List<int> ActualX = new List<int>();
            List<int> ActualY = new List<int>();
            if ((bool)CustomMark.IsChecked)
            {
                //自定义文本标签外间距，内间距，尺寸
                int x = 0, y = 0, tmx = 8, tmy = 20, w = 170, h = 80;
                if (CustomMarkText.Text.Length < 2)
                {
                    w = w / 2;
                }
                PdfTemplate cut = cb.CreateTemplate(w, h);
                //cut.MoveTo(x, y);
                //cut.LineTo(w - x, 0 + y);
                //cut.LineTo(w - x, h - y);
                //cut.LineTo(x, h - y);
                //cut.LineTo(x, y);
                //cut.Stroke();
                cut.BeginText();
                cut.SetFontAndSize(bf, 90);

                cut.SetTextMatrix(tmx, tmy);

                cut.ShowTextAligned(1, CustomMarkText.Text, w / 2, 10, 0);
                //cut.ShowText(culabeltext.Text);
                cut.EndText();
                float zoom = float.Parse(Zoom.Text) / 100;
                cb.AddTemplate(cut, 0, 1 * zoom, -1 * zoom, 0, originX + h * 1 * zoom, originY - cut.Width * 1 * zoom);
                ActualX.Add((int)(cut.Height * zoom) + 1);
                ActualY.Add((int)(cut.Width * zoom) + 2);
                originX += (int)(cut.Height * zoom) + 1;

            }
            if ((bool)StandardMark.IsChecked)
            {
                int tmx = 4, tmy = 10, w = 170, h=30;
                PdfTemplate cut = cb.CreateTemplate(w, h);
                cut.BeginText();
                cut.SetFontAndSize(bf, 15);
                cut.SetTextMatrix(tmx, tmy);
                cut.ShowTextAligned(1, "企业标准："+StandardCB.Text, w / 2, 10, 0);
                cut.EndText();
                cb.AddTemplate(cut, 0, 1, -1, 0, 500, 40);
            }
            if ((bool)Mark3C.IsChecked)
            {
                ///3C标签
                iTextSharp.text.Image splitline = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\CCC.jpg");
                splitline.RotationDegrees = 90;
                splitline.ScaleAbsolute(50, 50);
                if (originX + splitline.PlainWidth > MaxX)
                {
                    originX = int.Parse(AsixX.Text);
                    originY -= ActualY.Max();
                }
                ActualX.Add((int)splitline.PlainHeight + 1);
                ActualY.Add((int)splitline.PlainWidth + 1);
                splitline.SetAbsolutePosition(originX, originY - splitline.PlainWidth);
                cb.AddImage(splitline);
                //document.Add(splitline);
                originX = originX + (int)splitline.PlainHeight;
            }
            if ((bool)MarkRohs.IsChecked)
            {
                iTextSharp.text.Image rohsl = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\rohs.jpg");
                rohsl.RotationDegrees = 90;
                rohsl.ScaleAbsolute(100, 100);
                if (originX + rohsl.PlainWidth > MaxX)
                {
                    originX = int.Parse(AsixX.Text);
                    originY -= ActualY.Max();
                }
                ActualX.Add((int)rohsl.PlainHeight + 1);
                ActualX.Add((int)rohsl.PlainWidth + 1);
                rohsl.SetAbsolutePosition(originX, originY - rohsl.PlainWidth);
                cb.AddImage(rohsl);
                //document.Add(rohsl);
                originX = originX + (int)rohsl.PlainHeight;
            }
            if ((bool)MarkQs.IsChecked)
            {
                iTextSharp.text.Image qsl = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\qs.jpg");
                qsl.RotationDegrees = 90;
                qsl.ScaleAbsolute(45, 60);
                if (originX + qsl.PlainWidth > MaxX)
                {
                    originX = int.Parse(AsixX.Text);
                    originY -= ActualY.Max();
                }
                ActualX.Add((int)qsl.PlainHeight + 1);
                ActualY.Add((int)qsl.PlainWidth + 1);
                qsl.SetAbsolutePosition(originX, originY - qsl.PlainWidth);
                cb.AddImage(qsl);
                //document.Add(qsl);
                originX = originX + (int)qsl.PlainHeight;
            }
            if ((bool)HideLogo.IsChecked)
            {
                iTextSharp.text.Image bi = iTextSharp.text.Image.GetInstance(Environment.CurrentDirectory + "\\blank.png");
                bi.ScaleAbsolute(57, 76);
                bi.SetAbsolutePosition(26, 656);
                cb.AddImage(bi);
            }
            
        }
        private void CustomMark_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            A4OriginXSlider.Value = 283;
            A4OriginYSliders.Value = 414;
            ZoomSlider.Value = 70;
            rotateslider.Value = 0;            
        }

        private void QuarterA4Label_Click(object sender, RoutedEventArgs e)
        {
            Result.Content = "";
            try
            {
                PdfReader reader;
                //Document document = new Document(PageSize.A4.Rotate());
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream("Finish.pdf", FileMode.Create));
                document.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage newPage;
                string pkgcate = PkgCate_Tong.Content.ToString();
                if (!(bool)PkgCate_Tong.IsChecked)
                {
                    pkgcate = PkgCate_Box.Content.ToString();
                }
                reader = GetPDF();


                if (NOFrom.Text != "" && NOTo.Text != "")
                {

                    for (int j = Convert.ToInt32(NOFrom.Text); j <= Convert.ToInt32(NOTo.Text); j++)
                    {

                        for (int i = 1; i <= Convert.ToInt32(Copies.Text); i++)
                        {

                            document.NewPage();
                            newPage = writer.GetImportedPage(reader, 1);
                            cb.AddTemplate(newPage, 0, 0);

                            cb.BeginText();
                            BaseFont bf = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                            cb.SetFontAndSize(bf, 10);
                            cb.SetTextMatrix(220, 758);
                            cb.ShowText(pkgcate + "号：" + j);

                            cb.SetTextMatrix(220, 559);
                            cb.ShowText(pkgcate + "号：" + (Convert.ToInt32(j) + Convert.ToInt32(0)));

                            cb.SetTextMatrix(220, 360);
                            cb.ShowText(pkgcate + "号：" + (Convert.ToInt32(j) + Convert.ToInt32(1)));

                            cb.SetTextMatrix(220, 163);
                            cb.ShowText(pkgcate + "号：" + (Convert.ToInt32(j) + Convert.ToInt32(1)));

                            cb.EndText();
                        }

                        j = j + 1;

                    }
                    document.Close();
                    Result.Content = Application.Current.Resources["Complete"];
                    Process.Start("Finish.pdf");
                    writer.Close();
                }
                else
                {
                    Result.Content = Application.Current.Resources["PageNOnull"].ToString();
                }
            }
            catch (IOException ex)
            {
                Result.Content = ex.Message;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }



    }
    public class MarkLocation{
        public int AsixX;
        public int AsixY;
        public int Zoom;
        public int Rotate;
    }
}
