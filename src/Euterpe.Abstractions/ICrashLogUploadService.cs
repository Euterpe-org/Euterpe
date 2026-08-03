namespace Euterpe.Abstractions;

public interface ICrashLogUploadService
{
    Task UploadAppLogAsync();
}
