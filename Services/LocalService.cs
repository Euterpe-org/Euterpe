using System.Collections.Generic;
using System.IO;
using System.Linq;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class LocalService : ILocalService
{
    public List<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll")
        .ToList();
}