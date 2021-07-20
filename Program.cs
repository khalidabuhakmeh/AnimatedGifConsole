using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;

// play around with width to fit gif into your terminal
var maxWidth = args.Any() && int.TryParse(args.First(), out var width) ? width : 25;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) => cts.Cancel();

try
{
    await AnsiConsole.Live(Text.Empty)
        .StartAsync(async ctx =>
        {
            using var gif = await Image.LoadAsync("hallway.gif", new GifDecoder());
            var metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();

            while (!cts.IsCancellationRequested)
            {
                foreach (var frame in gif.Frames.Cast<ImageFrame<Rgba32>>())
                {
                    var bytes = await GetBytesFromFrameAsync(frame, cts);
                    var canvasImage = new CanvasImage(bytes)
                        //.MaxWidth(maxWidth)
                        ;
                    ctx.UpdateTarget(canvasImage);

                    // feels like anything less than 100ms is slow
                    var delay = TimeSpan.FromMilliseconds(Math.Max(100, metadata.FrameDelay));
                    await Task.Delay(delay, cts.Token);
                }
            }
        });
}
catch (Exception e)
{
    // whatever...
}

async Task<byte[]> GetBytesFromFrameAsync(ImageFrame<Rgba32> imageFrame, CancellationTokenSource cancellationTokenSource)
{
    using var image = new Image<Rgba32>(imageFrame.Width, imageFrame.Height);
    for (var y = 0; y < image.Height; y++)
    {
        for (var x = 0; x < image.Width; x++)
        {
            image[x, y] = imageFrame[x, y];
        }
    }

    await using var memoryStream = new MemoryStream();
    await image.SaveAsBmpAsync(memoryStream, cancellationTokenSource.Token);
    return memoryStream.ToArray();
}