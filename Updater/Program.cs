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
        result = await httpClient.GetAsync(downloadArgs[0], HttpCompletionOption.ResponseHeadersRead);
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
            {
                Console.WriteLine("Current download progress: " + Math.Round((double)readLength / totalLength.Value * 100, 2) + "%");
            }

            fs.Write(buffer, 0, length);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Download latest version failed\n{ex}");
    }

    Unzip(downloadArgs[1], downloadArgs[2]);
}

void Unzip(string zipPath, string parentFolderPath)
{
    var fastZip = new FastZip();
    try
    {
        fastZip.ExtractZip(zipPath, parentFolderPath, FastZip.Overwrite.Always, null, null, null, true);
    }
    catch (Exception)
    {
        Console.WriteLine($"Unable to unzip the latest version of app in\n{zipPath}\nMaybe try manually unzip?");
    }

    try
    {
        File.Delete(zipPath);
    }
    catch (Exception)
    {
        Console.WriteLine($"Failed to delete zip file in\n{zipPath}Try manually delete");
    }
}