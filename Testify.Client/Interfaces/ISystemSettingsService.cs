namespace Testify.Shared.Interfaces;

public interface ISystemSettingsService
{
    Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false);
    Task SetBoolSettingAsync(string key, bool value);
    Task<string?> GetSettingAsync(string key, string? defaultValue = null);
    Task SetSettingAsync(string key, string value);
}
