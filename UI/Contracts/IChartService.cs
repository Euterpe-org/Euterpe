using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModToolsUI.Contracts;

public interface IChartService
{
    Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts);
}