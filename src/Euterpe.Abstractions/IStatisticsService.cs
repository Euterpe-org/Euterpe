namespace Euterpe.Abstractions;

public interface IStatisticsService
{
    void RecordVisitor();
    void RecordDownload(string modName, string modAuthor);
}
