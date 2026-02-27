using Microsoft.JSInterop;
using Testify.Shared.Interfaces;

namespace Testify.Client.Shared.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IJSRuntime _js;
    private const string Prefix = "testify_settings_";

    public SystemSettingsService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false)
    {
        try
        {
            var value = await _js.InvokeAsync<string?>("localStorage.getItem", Prefix + key);
            if (value is null) return defaultValue;
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task SetBoolSettingAsync(string key, bool value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", Prefix + key, value.ToString().ToLower());
        }
        catch { }
    }

    public async Task<string?> GetSettingAsync(string key, string? defaultValue = null)
    {
        try
        {
            var value = await _js.InvokeAsync<string?>("localStorage.getItem", Prefix + key);
            return value ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task SetSettingAsync(string key, string value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", Prefix + key, value);
        }
        catch { }
    }
}
