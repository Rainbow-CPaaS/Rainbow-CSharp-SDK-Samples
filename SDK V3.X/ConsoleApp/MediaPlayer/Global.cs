using System.Reflection;

namespace ConsoleMediaPlayer
{
    public class Global
    {
        static readonly String DEFAULT_PRODUCT_NAME = "ConsoleMedias (backup)";
        static readonly String DEFAULT_FILE_VERSION = "0.0.1";

        private static string _fileVersion = "";
        private static string _productName = "";

        private static void GetAssemblyInfo()
        {
            try
            {
                AssemblyName? assemblyName = typeof(Global).Assembly.GetName();
                if (assemblyName is not null)
                {
                    _productName = assemblyName.Name ?? DEFAULT_PRODUCT_NAME;
                    _fileVersion = assemblyName.Version?.ToString() ?? DEFAULT_FILE_VERSION;
                }
            }
            catch
            {
            }

            _productName ??= DEFAULT_PRODUCT_NAME;
            _fileVersion ??= DEFAULT_FILE_VERSION;
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
