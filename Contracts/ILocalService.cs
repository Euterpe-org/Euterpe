using System.Collections.Generic;
using System.IO;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    List<string> GetModFiles(string path);
}