using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SkiaSharp;

namespace TextureMapBuilder
{
    internal record ImageMetadata
    {
        public int SingleWidth { get; set; }
        public int SingleHeight { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    internal static class ImageProcessor
    {
        public static List<string> BuildList(IEnumerable<string> inputFiles, int skips, bool useLastFile)
        {
            if (!inputFiles.Any())
            {
                return new List<string>();
            }

            var output = new List<string>();
            var skiped = 0;
            for (var i = 0; i < inputFiles.Count(); i++)
            {
                var entry = inputFiles.ElementAt(i);
                skiped = i % (skips + 1);

                if (skiped != 0)
                {
                    continue;
                }

                if (!File.Exists(entry))
                {
                    throw new FileNotFoundException($"文件不存在：{entry}");
                }

                output.Add(entry);
            }

            if (skiped > 0 && useLastFile)
            {
                output.Add(inputFiles.Last());
            }

            return output;
        }

        public static Tuple<byte[], ImageMetadata> Merge(List<string> list, int rows, int cols)
        {
            if (rows == 0 && cols == 0)
            {
                throw new ArgumentException("行列不可皆为 0。");
            }

            if (!list.Any())
            {
                throw new ArgumentNullException("必须指定一张以上的图片。");
            }

            var _rows = rows;
            var _cols = cols;
            var info = new ImageMetadata();

            if (rows == 0)
            {
                _rows = (int)Math.Ceiling((decimal)list.Count / cols);
            }
            else if (cols == 0)
            {
                _cols = (int)Math.Ceiling((decimal)list.Count / rows);
            }

            info.Cols = _cols;
            info.Rows = _rows;

            var image0 = SKImage.FromEncodedData(list.First());
            var singleHeight = image0.Height;
            var singleWidth = image0.Width;

            info.SingleHeight = singleHeight;
            info.SingleWidth = singleWidth;

            var imageInfo = new SKImageInfo(singleWidth * _cols, singleHeight * _rows);

            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            var i = 0;
            foreach (var file in list)
            {
                var x = i % _cols * singleWidth;
                var y = i / _cols * singleHeight;

                var image = SKImage.FromEncodedData(file);
                var bitmap = SKBitmap.FromImage(image);


                if (singleHeight == 0 && singleWidth == 0)
                {
                    singleHeight = image.Height;
                    singleWidth = image.Width;
                }
                else
                {
                    if (image.Height != singleHeight || image.Width != singleWidth)
                    {
                        throw new Exception($"The image#{i}: {image.Width} x {image.Height} not matches profile: {singleWidth} x {singleHeight}.");
                    }
                }

                canvas.DrawBitmap(bitmap, new SKPoint(x, y));

                i++;
            }

            surface.Flush();
            using var snapshot = surface.Snapshot();
            using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();

            info.Height = snapshot.Height;
            info.Width = snapshot.Width;

            data.SaveTo(ms);

            return new (ms.ToArray(), info);
        }
    }
}
