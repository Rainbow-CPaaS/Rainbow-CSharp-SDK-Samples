using System.Reflection;

namespace BotLibrary
{
    public class Global
    {
        private static string _fileVersion = "";
        private static string _productName = "";

        private static void GetAssemblyInfo()
        {
            try
            {
                AssemblyName? assemblyName = typeof(Global).Assembly.GetName();
                if (assemblyName is not null)
                {
                    _productName = assemblyName.Name ?? _productName;
                    _fileVersion = assemblyName.Version?.ToString() ?? _fileVersion;
                }
            }
            catch 
            {
                _fileVersion = "0.0.0.1";
                _productName = "BotLibrary(backup)";
            }
        }

        public static String FileVersion()
        {
            if (String.IsNullOrEmpty(_fileVersion))
                GetAssemblyInfo();
            return _fileVersion;
        }

        public static String ProductName()
        {
            if (String.IsNullOrEmpty(_productName))
                GetAssemblyInfo();
            return _productName;
        }
    }
}
