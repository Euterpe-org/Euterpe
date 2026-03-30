using System.IO.Pipes;

namespace Euterpe;

internal static class Bootstrapper
{
    private const string PipeName = $"{AppName}-DeepLink";
    private const string BootstrapLogFile = "bootstrap.log";

    private static readonly CancellationTokenSource DeepLinkPipeCts = new();

    internal static void StartDeepLinkPipeServer()
    {
        ListenForDeepLinkPipeAsync(DeepLinkPipeCts.Token).SafeFireAndForget();
    }

    internal static void StopDeepLinkPipeServer()
    {
        DeepLinkPipeCts.Cancel();
        DeepLinkPipeCts.Dispose();
    }

    internal static void CleanupLogFiles()
    {
        try
        {
            if (!Directory.Exists(AppLogsFolder))
            {
                return;
            }

            var logFiles = Directory.EnumerateFiles(AppLogsFolder, "*.log").OrderDescending().Skip(30);
            foreach (var logFile in logFiles)
            {
                File.Delete(logFile);
            }
        }
        catch (Exception ex)
        {
            LogBootstrapException(ex);
        }
    }

    internal static void CleanupBackupFiles()
    {
        try
        {
            var directories = Directory.GetDirectories(".", "backup-*");
            if (directories.Length <= 1)
            {
                return;
            }

            Parallel.ForEach(directories.OrderDescending().Skip(1), (directory, _) => Directory.Delete(directory, true));
        }
        catch (Exception ex)
        {
            LogBootstrapException(ex);
        }
    }

    internal static void SendArgsToPrimaryInstance(string[] args)
    {
        try
        {
            var uri = args[0];
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(3000);
            using var writer = new StreamWriter(client);
            writer.Write(uri);
            writer.Flush();
        }
        catch (Exception ex)
        {
            LogBootstrapException(ex);
        }
    }

    private static async Task ListenForDeepLinkPipeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await using (server.ConfigureAwait(false))
                {
                    await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    using var reader = new StreamReader(server);
                    var uri = await reader.ReadToEndAsync(ct).ConfigureAwait(false);

                    if (!uri.IsNullOrEmpty())
                    {
                        Dispatcher.UIThread.Post(() => IocContainer.Resolve<DeepLinkService>().HandleUri(uri));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                LogBootstrapException(ex);
            }
        }
    }

    private static void LogBootstrapException(Exception ex)
    {
        try
        {
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, BootstrapLogFile), message);
        }
        catch
        {
            // Nothing we can do here
        }
    }
}