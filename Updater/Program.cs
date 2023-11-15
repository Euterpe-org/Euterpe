using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using Spectre.Console;

HttpClient httpClient = new();

if (args.Length < 2)
{
    Console.WriteLine("Invalid launch!\nLaunch arguments are null!");
    Console.ReadKey();
}
else
{
    await DownloadUpdates(args);
    Console.WriteLine("Download finished. Extracting...");
    Unzip(args[1] + ".zip", args[1]);
    Console.WriteLine("Extracting finished. Pressing any key to launch MuseDashModTools");
    Console.ReadKey();

    if (OperatingSystem.IsWindows())
    {
        Process.Start(Path.Combine(args[1], "MuseDashModTools.exe"));
    }

    if (OperatingSystem.IsLinux())
    {
        Process.Start(Path.Combine(args[1], "MuseDashModTools"));
    }
}

return;

async Task DownloadUpdates(IReadOnlyList<string> downloadArgs)
{
    HttpResponseMessage result;
    try
    {
        result = await httpClient.GetAsync(downloadArgs[0], HttpCompletionOption.ResponseHeadersRead);
    }
    catch
    {
        try
        {
            result = await httpClient.GetAsync("https://hub.gitmirror.com/" + downloadArgs[0], HttpCompletionOption.ResponseHeadersRead);
        }
        catch
        {
            result = await httpClient.GetAsync("https://ghproxy.com/" + downloadArgs[0], HttpCompletionOption.ResponseHeadersRead);
        }
    }

    var totalLength = result.Content.Headers.ContentLength;
    var contentStream = await result.Content.ReadAsStreamAsync();
    var buffer = new byte[5 * 1024];
    int length;
    try
    {
        await AnsiConsole.Progress()
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn(), new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Download Progress[/]");
                await using var fs = new FileStream(downloadArgs[1] + ".zip", FileMode.OpenOrCreate);
                while (!ctx.IsFinished &&
                       (length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
                {
                    task.Increment((double)length / totalLength!.Value * 100);
                    fs.Write(buffer, 0, length);
                }
            });
    }

    catch (Exception ex)
    {
        Console.WriteLine($"Download latest version failed\n{ex}\nYou can download manually from {downloadArgs[0]}");
        Console.ReadKey();
    }
}

void Unzip(string zipPath, string targetPath)
{
    var fastZip = new FastZip();
    try
    {
        fastZip.ExtractZip(zipPath, targetPath, FastZip.Overwrite.Always, null, null, null, true);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        Console.WriteLine($"Unable to unzip the latest version of app in\n{zipPath}\nMaybe try manually unzip?");
        Console.ReadKey();
    }

    try
    {
        File.Delete(zipPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        Console.WriteLine($"Failed to delete zip file in\n{zipPath}Try manually delete");
        Console.ReadKey();
    }
}