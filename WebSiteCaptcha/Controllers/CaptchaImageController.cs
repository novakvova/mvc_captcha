using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSiteCaptcha.Helpers;

namespace WebSiteCaptcha.Controllers
{
    public class CaptchaImageController : Controller
    {
        private const int ImageWidth = 200, ImageHight = 70;
        private const string FontFamily = "Arial";
        private readonly static Brush Foreground = Brushes.Navy;
        private readonly static Color Background = Color.Silver;
        //деформация текста
        private const int WarpFactory = 5;
        private const Double xAmp = WarpFactory * ImageWidth / 100;
        private const Double yAmp = WarpFactory * ImageHight / 85;
        private const Double xFreq = 2 * Math.PI / ImageWidth;
        private const Double yFreq = 2 * Math.PI / ImageHight;
        private GraphicsPath DeformPath(GraphicsPath path)
        {
            PointF[] deformed = new PointF[path.PathPoints.Length];
            Random rng = new Random();
            Double xSeed = rng.NextDouble() * 2 * Math.PI;
            Double ySeed = rng.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < path.PathPoints.Length; i++)
            {
                PointF original = path.PathPoints[i];
                Double val = xFreq * original.X + yFreq * original.Y;
                int xOffset = (int)(xAmp * Math.Sin(val + xSeed));
                int yOffset = (int)(yAmp * Math.Sin(val + ySeed));
                deformed[i] = new PointF(original.X + xOffset, original.Y + yOffset);
            }
            return new GraphicsPath(deformed, path.PathTypes);
        }
        // GET: CaptchaImage
        public void Render(string challengeGuid)
        {
            string key = CaptchaHelper
                .SessionKeyPrefix + challengeGuid;
            string solution = (string)HttpContext.Session[key];
            if(solution!=null)
            {
                //Создать пустое полотно для отображения на ней CAPTCHA
                using (Bitmap bmp = new Bitmap(ImageWidth, ImageHight))
                using (Graphics g = Graphics.FromImage(bmp))
                using (Font font = new Font(FontFamily, 1f))
                {
                    g.Clear(Background);
                    //Выполныть пробную визуализацию для определения найлучшего размера шрифта
                    SizeF finalSize;
                    SizeF testSize = g.MeasureString(solution, font);
                    float bestFontSize = Math.Min(ImageWidth / testSize.Width, ImageHight / testSize.Height) * 0.95f;
                    using (Font finalFont = new Font(FontFamily, bestFontSize))
                    {
                        finalSize = g.MeasureString(solution, finalFont);
                    }
                    //получить путь который представляет текст, центрированый на полотне
                    g.PageUnit = GraphicsUnit.Point;
                    PointF textTopLeft = new PointF((ImageWidth - finalSize.Width) / 2, (ImageHight - finalSize.Height) / 2);
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddString(solution, new FontFamily(FontFamily), 0, 
                            bestFontSize, textTopLeft, StringFormat.GenericDefault);
                        //
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.FillPath(Foreground, DeformPath(path));
                        g.Flush();
                        //Отправить изображение в поток ответа в формате GIF
                        Response.ContentType = "image/gif";
                        bmp.Save(Response.OutputStream, ImageFormat.Gif);
                    }
                }
            }
        }
    }
}