using Microsoft.Extensions.Configuration;

namespace hub.Helpers;

public static class ConfigurationHelper
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string? GetSetting(string key)
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException("Configuration not initialized. Call Initialize first.");
        }

        return _configuration[key];
    }

    public static T? GetSection<T>(string sectionName) where T : class, new()
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException("Configuration not initialized. Call Initialize first.");
        }

        var section = _configuration.GetSection(sectionName);
        return section.Get<T>();
    }
}
