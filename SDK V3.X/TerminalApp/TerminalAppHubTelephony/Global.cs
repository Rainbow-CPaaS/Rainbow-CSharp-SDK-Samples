using System.Reflection;

internal static class Global
{
    private static string? _fileVersion = null;
    private static string? _productName = null;

    private static void GetAssemblyInfo()
    {
        try
        {
            AssemblyName? assemblyName = typeof(Global).Assembly.GetName();
            if (assemblyName is not null)
            {
                _productName = "" + assemblyName.Name;
                _fileVersion = "" + assemblyName.Version?.ToString();
            }
        }
        catch
        {
        }

        _productName ??= "TerminalAppHubTelephony(backup)";
        _fileVersion ??= "0.0.0.1";
    }

    public static String? FileVersion()
    {
        if (String.IsNullOrEmpty(_fileVersion))
            GetAssemblyInfo();
        return _fileVersion;
    }

    public static String? ProductName()
    {
        if (String.IsNullOrEmpty(_productName))
            GetAssemblyInfo();
        return _productName;
    }
}
