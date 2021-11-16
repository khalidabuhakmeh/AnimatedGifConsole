using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, _) => cts.Cancel();

await AnsiConsole.Live(Text.Empty)
    .StartAsync(async ctx =>
    {
        using var gif = await Image<Rgba32>.LoadAsync("aliens.gif");
        var metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();

        while (!cts.IsCancellationRequested)
        {
            for (int i = 0; i < gif.Frames.Count; i++)
            {
                var delay = gif.Frames[i].Metadata.GetGifMetadata().FrameDelay;
                using var clone = gif.Frames.CloneFrame(i);

                await using var memoryStream = new MemoryStream();
                await clone.SaveAsBmpAsync(memoryStream);
                memoryStream.Position = 0;

                var canvasImage = new CanvasImage(memoryStream).MaxWidth(50);
                ctx.UpdateTarget(canvasImage);

                // FrameDelay is measured in 1/100th second.
                // Let's half the delay since we're encoding per frame.
                await Task.Delay(TimeSpan.FromMilliseconds(delay * 5), cts.Token);
            }
        }
    });