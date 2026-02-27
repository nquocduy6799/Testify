using Testify.Shared.Interfaces;

namespace Testify.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly Dictionary<string, string> _settings = new()
    {
        { "MaintenanceMode", "false" },
        { "OpenRegistration", "true" }
    };

    public Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false)
    {
        if (_settings.TryGetValue(key, out var value))
            return Task.FromResult(bool.TryParse(value, out var result) ? result : defaultValue);
        return Task.FromResult(defaultValue);
    }

    public Task SetBoolSettingAsync(string key, bool value)
    {
        _settings[key] = value.ToString().ToLower();
        return Task.CompletedTask;
    }

    public Task<string?> GetSettingAsync(string key, string? defaultValue = null)
    {
        return Task.FromResult(_settings.TryGetValue(key, out var value) ? value : defaultValue);
    }

    public Task SetSettingAsync(string key, string value)
    {
        _settings[key] = value;
        return Task.CompletedTask;
    }
}
