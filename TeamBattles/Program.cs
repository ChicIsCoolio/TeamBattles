using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using TeamBattles.Chic;

namespace TeamBattles
{
    class Program
    {
        public static string Root = "/home/runner/TeamBattles/";

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            Process.Start("node", $"{Root}Server/index.js");

            var now = DateTimeOffset.UtcNow;
            var date = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            Console.WriteLine($"Info will send at {date}");

            Scheduler.Default.Schedule(date, reschedule =>
            {
                DrawInfo();
                reschedule(date.AddHours(1));
            });

            DoCommand(Console.ReadLine());
            
            await Task.Delay(-1);
        }

        private void DoCommand(string command)
        {
            if (command == Environment.GetEnvironmentVariable("DRAWCOMMAND"))
                DrawInfo();
        }

        public void DrawInfo()
        {
            var teams = Battles.GetScoreInfo().Props.PageProps.Teams;
            teams.Sort(TeamComparer.Comparer);

            var teamsDictionary = new Dictionary<string, List<Team>>();

            foreach (var team in teams)
            {
                if (teamsDictionary.ContainsKey(team.Region)) teamsDictionary[team.Region].Add(team);
                else teamsDictionary.Add(team.Region, new List<Team> { team });
            }

            List<BitmapInfo> bitmaps = new List<BitmapInfo>();

            int teamHeight = 50;
            int offset = 20;
            int regionWidth = 300 + offset * 2;

            foreach (var region in teamsDictionary)
            {
                int regionHeight = teamHeight * region.Value.Count + 18 + offset * 2;

                /*using (*/
                var bitmap = new SKBitmap(regionWidth, regionHeight);//)
                //{
                using (var c = new SKCanvas(bitmap))
                {
                    using (var path = new SKPath())
                    {
                        path.MoveTo(15, 15);
                        path.LineTo(regionWidth - 10, 10);
                        path.LineTo(regionWidth - 20, regionHeight - 15);
                        path.LineTo(10, regionHeight - 10);
                        path.Close();

                        c.DrawPath(path, new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            Color = SKColors.White,
                            ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black)
                        });
                        //c.DrawRect(offset, offset, width, height, new SKPaint { Color = SKColors.White });
                    }

                    using (var flag = BitmapFromUrl($"https://teambattles.fortnite.com/image/flag/{region.Value[0].Nationality.ToLower()}.png", region.Value[0].Nationality.ToLower()))
                    {
                        int _x = (regionWidth - flag.Width) / 2;
                        int _y = offset;

                        c.DrawBitmap(flag, _x, _y, new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black)
                        });
                    }

                    int y = 18 + offset;

                    foreach (var team in region.Value)
                    {
                        c.DrawText(team.Name, 10 + offset, y + 36, new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            TextSize = 20,
                            Typeface = ChicTypefaces.BurbankBigRegularBlack,
                            Color = SKColors.Black
                        });

                        using (var paint = new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            TextSize = 20,
                            Typeface = ChicTypefaces.BurbankBigRegularBlack,
                            Color = SKColors.Black
                        })
                        {
                            c.DrawText($"{team.Score.ToString("N0")}",
                                regionWidth - 10 - offset - paint.MeasureText($"{team.Score.ToString("N0")}"), y + 36, paint);
                        }

                        y += teamHeight;
                    }
                }

                bitmaps.Add(SaveToCache(bitmap, $"region_{region.Value[0].Nationality}"));
            }

            int maxRegionsPerRow = 7;
            maxRegionsPerRow = Math.Clamp(bitmaps.Count, 0, maxRegionsPerRow);

            int extraHeight = 800;
            int _regionHeight = bitmaps.Max(bitmap => bitmap.Height);

            int o = 50;

            int width = maxRegionsPerRow * (regionWidth + o) + o;
            int height = (int)Math.Ceiling((decimal)bitmaps.Count / maxRegionsPerRow) * (_regionHeight + o) + o;

            int extraExtraHeight = (int)(width * 0.5625f) - height - extraHeight;
            extraExtraHeight = extraExtraHeight >= 0 ? extraExtraHeight : 0;
            int extraWidth = (int)((height + extraHeight) * 1.77777777778f) - width;
            extraWidth = extraWidth >= 0 ? extraWidth : 0;

            using (var full = new SKBitmap(width + extraWidth, height + extraHeight + extraExtraHeight))
            {
                using (var c = new SKCanvas(full))
                {
                    using (var paint = new SKPaint
                    {
                        IsAntialias = true,
                        FilterQuality = SKFilterQuality.High,
                        Shader = SKShader.CreateLinearGradient(new SKPoint(-100, -200), new SKPoint(width + 300, height + 500),
                        new SKColor[]
                        {
                            new SKColor(56, 33, 50),
                            new SKColor(192, 4, 92),
                            new SKColor(243, 9, 76),
                            new SKColor(249, 96, 83),
                            new SKColor(250, 111, 84),
                            new SKColor(255, 232, 75),
                            new SKColor(255, 239, 74),
                        }, SKShaderTileMode.Clamp)
                    })
                    {
                        c.DrawRect(0, 0, width + extraWidth, extraHeight, paint);
                    }

                    using (var logo = SKBitmap.Decode($"{Root}Resources/logo.png"))
                    {
                        c.DrawBitmap(logo.Resize(new SKSizeI(logo.Width * 2, logo.Height * 2), SKFilterQuality.High), 100, 25);
                    }

                    using (var lovely = SKBitmap.Decode($"{Root}Resources/hero.webp"))
                    {
                        c.DrawBitmap(lovely, width + extraWidth- lovely.Width, 0);
                    }

                    using (var path = new SKPath())
                    {
                        path.MoveTo(0, 695);
                        path.LineTo((width + extraWidth) / 2, 745);
                        path.LineTo(width + extraWidth - (int)((width + extraWidth) / 1.7f), 715);
                        path.LineTo(width + extraWidth, 790);
                        path.LineTo(width + extraWidth, height + extraHeight);
                        path.LineTo(0, height + extraHeight);
                        path.Close();

                        c.DrawPath(path, new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            Color = SKColors.White,
                            ImageFilter = SKImageFilter.CreateDropShadow(0, -2, 5, 5, SKColors.Black)
                        });
                    }

                    int x = o;
                    int y = o + extraHeight;

                    int xi = 1;

                    foreach (var bmp in bitmaps)
                    {
                        c.DrawBitmap(LoadFromCache(bmp), x, y);

                        if (xi == maxRegionsPerRow)
                        {
                            x = o;
                            y += _regionHeight + o;
                            xi = 1;
                        }
                        else
                        {
                            x += regionWidth + o;
                            xi++;
                        }
                    }

                    using (var watermark = SKBitmap.Decode($"{Root}Resources/watermark.png"))
                    {
                        c.DrawBitmap(watermark, width - extraWidth - watermark.Width, height + extraHeight - extraExtraHeight - watermark.Height, new SKPaint
                        {
                            IsAntialias = true,
                            FilterQuality = SKFilterQuality.High,
                            ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black)
                        });
                    }
                }

                using (var image = SKImage.FromBitmap(full))
                {
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        using (var stream = File.OpenWrite($"{Root}Output/output.png"))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }
            }

            TwitterManager.TweetWithMedia($"{Root}Output/output.png", "Fortnite Hearts Wild Team Battles leaderboard update!");

            foreach (var file in Directory.GetFiles($"{Root}Cache").Where(x => x.Contains("region_")))
            {
                File.Delete(file);
            }

            GC.Collect();
            Console.WriteLine("Done");
        }

        public static BitmapInfo SaveToCache(SKBitmap bitmap, string fileName, bool dispose = true)
        {
            var info = new BitmapInfo
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                FileName = fileName
            };

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite($"{Root}Cache/{fileName}.png"))
                data.SaveTo(stream);

            if (dispose) bitmap.Dispose();

            return info;
        }

        public static SKBitmap LoadFromCache(BitmapInfo info) => LoadFromCache(info.FileName);
        public static SKBitmap LoadFromCache(string fileName)
            => SKBitmap.Decode($"{Root}Cache/{fileName}.png");

        public static SKBitmap BitmapFromUrl(string url, string fileName)
        {
            if (File.Exists($"{Root}Cache/{fileName}.png")) return LoadFromCache(fileName);

            using (var client = new HttpClient())
            {
                Console.Write("Downloading bitmap from ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(url);
                Console.ResetColor();

                var bytes = client.GetByteArrayAsync(url).Result;

                using (var stream = new MemoryStream(bytes))
                {
                    SKBitmap bitmap = SKBitmap.Decode(stream);

                    SaveToCache(bitmap, fileName, false);
                    
                    return bitmap;
                }
            }
        }
    }

    public struct BitmapInfo
    {
        public int Width;
        public int Height;
        public string FileName;
    }
}
