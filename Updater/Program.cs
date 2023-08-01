using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using ShellProgressBar;

HttpClient httpClient = new();

if (args.Length < 3)
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
    Process.Start(Path.Combine(args[1], "MuseDashModTools.exe"));
}

return;

async Task DownloadUpdates(IReadOnlyList<string> downloadArgs)
{
    var options = new ProgressBarOptions
    {
        ForegroundColorDone = ConsoleColor.DarkGreen,
        ProgressCharacter = '─',
        ProgressBarOnBottom = true
    };
    using var progressBar = new ProgressBar(10000, "Download Progress", options);
    var progress = progressBar.AsProgress<double>();

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
    await using var fs = new FileStream(downloadArgs[1] + ".zip", FileMode.OpenOrCreate);
    var buffer = new byte[5 * 1024];
    var readLength = 0L;
    try
    {
        int length;
        while ((length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
        {
            readLength += length;
            if (totalLength > 0)
                progress.Report(Math.Round((double)readLength / totalLength.Value * 100, 4));

            fs.Write(buffer, 0, length);
        }
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