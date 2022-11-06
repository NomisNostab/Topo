using System.Runtime.InteropServices;
using static System.Environment;

namespace Topo
{
    public static class Constants
    {
        public const string Version = "1.34";

        public enum OutputType
        {
            pdf,
            xlsx
        }

        public static string AppLocalPath = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "Topo");
    }
}
