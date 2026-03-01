namespace Euterpe.Abstractions;

public interface IStatisticsService
{
    Task RecordVisitorAsync();
    Task RecordDownloadAsync(string modName, string modAuthor);
}