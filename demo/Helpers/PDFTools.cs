using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using O2S.Components.PDF4NET.Core;
using OpenHtmlToPdf;

namespace MyDemo.Helpers
{
    /// <summary>
    /// PDF工具
    /// </summary>
    public class PDFTools
    {
        /// <summary>
        /// html转pdf
        /// </summary>
        /// <param name="html"></param>
        /// <param name="watermarkimg">水印图片</param>
        /// <returns></returns>
        public static string HtmlToPdf(string html, string watermarkimg)
        {
            var document = Pdf.From(html)
                .OfSize(PaperSize.A4)  //大小，这里选A4纸大小
                .WithGlobalSetting("margin.top", "0.4cm");  //设置全局样式

            //此属性的值在32位进程中为4，在64位进程中为8。
            if (IntPtr.Size == 4)
            {
                document = document.WithObjectSetting("load.zoomFactor", "1.5");
            }
            var result = document.Content();
            string filePath = "pdf/" + DateTime.Now.ToString("yyyyMMddHH") + "/";//路径
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + filePath);//保存在当前项目跟路径下
            }
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".pdf";
            //通过stream把html写入到pdf文件中
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + filePath + fileName, FileMode.Create, FileAccess.Write);
            fs.Write(result, 0, result.Length);
            fs.Close();
            //加水印
            string filewatermarkPath = filePath + DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".pdf";
            var res = PDFWatermark(AppDomain.CurrentDomain.BaseDirectory + filePath + fileName, AppDomain.CurrentDomain.BaseDirectory + filewatermarkPath, watermarkimg);
            if (res)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + filePath + fileName))
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + filePath + fileName);
                return filewatermarkPath;
            }
            else
                return filePath + fileName;
        }

        // /// <summary>
        // /// PDF转化为图片
        // /// </summary>
        // /// <param name="filePath">文件路径</param>
        // /// <param name="resolution">分辨率</param>
        // /// <returns></returns>
        // public static List<string> PDFToIMG(string filePath, float resolution)
        // {
        //     string imgPath = "pdf/IMG/" + DateTime.Now.ToString("yyyyMMddHH") + "/";
        //     //如果文件夹不存在，则创建
        //     if (!Directory.Exists(imgPath))
        //     {
        //         Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + imgPath);
        //     }
        //     PDFFile file = PDFFile.(filePath);
        //     int pageCount = file.PageCount;
        //     List<string> imgPathList = new List<string>();
        //     for (int i = 0; i < pageCount; i++)
        //     {
        //         string imgName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".jpg";
        //         Bitmap img = file.GetBWPageImage(i, resolution);
        //         img.Save(AppDomain.CurrentDomain.BaseDirectory + imgPath + imgName, System.Drawing.Imaging.ImageFormat.Jpeg);
        //         imgPathList.Add(imgPath + imgName);
        //     }
        //     return imgPathList;
        // }

        /// <summary>
        /// pdf加水印
        /// </summary>
        /// <param name="inputfilepath">模板路径</param>
        /// <param name="outputfilepath">导出水印背景后的PDF</param>
        /// <param name="ModelPicName">水印图片</param>
        /// <returns></returns>
        public static bool PDFWatermark(string inputfilepath, string outputfilepath, string ModelPicName)
        {
            PdfReader pdfReader = null;
            PdfStamper pdfStamper = null;
            try
            {
                pdfReader = new PdfReader(inputfilepath);
                pdfStamper = new PdfStamper(pdfReader, new FileStream(outputfilepath, FileMode.Create));
                PdfGState gs = new PdfGState();
                gs.FillOpacity = 0.4f;//透明度

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ModelPicName);

                int numberOfPages = pdfReader.NumberOfPages;

                //每一页加2个水印,也可以设置某一页加水印
                for (int i = 1; i <= numberOfPages; i++)
                {
                    PdfContentByte waterMarkContent = pdfStamper.GetOverContent(i);//内容上层加水印
                    //PdfContentByte waterMarkContent = pdfStamper.GetUnderContent(i);//内容下层加水印
                    waterMarkContent.SetGState(gs);
                    image.SetAbsolutePosition(20, 100);//水印定位坐标
                    waterMarkContent.AddImage(image);
                    image.SetAbsolutePosition(20, 300);
                    waterMarkContent.AddImage(image);
                }
                //一定要2个循环才会在每一个加上水印，咱也不知道为啥，反正是试了好久试出来的
                for (int i = 1; i <= numberOfPages; i++)
                {
                    PdfContentByte waterMarkContent = pdfStamper.GetOverContent(i);//内容上层加水印
                    //PdfContentByte waterMarkContent = pdfStamper.GetUnderContent(i);//内容下层加水印
                    waterMarkContent.SetGState(gs);
                    image.SetAbsolutePosition(20, 100);
                    waterMarkContent.AddImage(image);
                    image.SetAbsolutePosition(20, 300);
                    waterMarkContent.AddImage(image);
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Message.Trim();
                return false;
            }
            finally
            {
                if (pdfStamper != null)
                    pdfStamper.Close();

                if (pdfReader != null)
                    pdfReader.Close();
            }
        }
    }
}
