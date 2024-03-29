using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModToolsUI.Contracts;

public interface IChartService
{
    /// <summary>
    ///     Download Chart
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task DownloadChartAsync(Chart item);
    
    /// <summary>
    ///     Initialize Chart list
    /// </summary>
    /// <param name="sourceCache"></param>
    /// <param name="charts"></param>
    /// <returns></returns>
    Task InitializeChartListAsync(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts);
}