using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;

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
    Unzip(args[1], args[2]);
    Console.WriteLine("Extracting finished. Launching MuseDashModTools...");
    Process.Start(Path.Combine(args[2], "MuseDashModTools.exe"));
}

async Task DownloadUpdates(IReadOnlyList<string> downloadArgs)
{
    HttpResponseMessage result;
    try
    {
        result = await httpClient.GetAsync(downloadArgs[0], HttpCompletionOption.ResponseHeadersRead);
    }
    catch (Exception)
    {
        try
        {
            result = await httpClient.GetAsync(downloadArgs[0].Replace("github.com", "download.fastgit.org"),
                HttpCompletionOption.ResponseHeadersRead);
        }
        catch
        {
            result = await httpClient.GetAsync("https://hub.gitmirror.com/" + downloadArgs[0],
                HttpCompletionOption.ResponseContentRead);
        }
    }

    var totalLength = result.Content.Headers.ContentLength;
    var contentStream = await result.Content.ReadAsStreamAsync();
    await using var fs = new FileStream(downloadArgs[1], FileMode.OpenOrCreate);
    var buffer = new byte[5 * 1024];
    var readLength = 0L;
    try
    {
        int length;
        while ((length = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            readLength += length;
            if (totalLength > 0)
                Console.WriteLine("Current download progress: " + Math.Round((double)readLength / totalLength.Value * 100, 2) + "%");

            fs.Write(buffer, 0, length);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Download latest version failed\n{ex}");
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