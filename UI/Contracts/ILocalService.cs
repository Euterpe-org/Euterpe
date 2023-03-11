using System.Collections.Generic;
using UI.Models;

namespace UI.Contracts;

public interface ILocalService
{
    List<string> GetModFiles(string path);
    Mod? LoadMod(string filePath);
}