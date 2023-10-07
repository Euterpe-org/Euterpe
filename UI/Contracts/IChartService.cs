using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModToolsUI.Contracts;

public interface IChartService
{
    /// <summary>
    ///     Initialize Chart list
    /// </summary>
    /// <param name="sourceCache"></param>
    /// <param name="charts"></param>
    /// <returns></returns>
    Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts);
}