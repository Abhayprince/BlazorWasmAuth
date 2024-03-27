using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorWasmAuth.Authentication;

public class BrowserStorageService
{
    /*
     * localStorage.setItem('key', 'value');
     * localStorage.getItem('key');
     * localStorage.removeItem('key');
     * localStorage.clear();
     * 
     * sessionStorage.setItem('key', 'value');
     * sessionStorage.getItem('key');
     * sessionStorage.removeItem('key');
     * sessionStorage.clear();
     */
    private const string StorageType = "localStorage";
    private readonly IJSRuntime _jsRuntime;
    
    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveToStorage<TData>(string key, TData value)
    {
        var serializedData = Serialize(value);
        await _jsRuntime.InvokeVoidAsync($"{StorageType}.setItem", key, serializedData);
    }

    public async Task<TData?> GetFromStorage<TData>(string key)
    {
        var serializedData = await _jsRuntime.InvokeAsync<string?>($"{StorageType}.getItem", key);
        return Deserialize<TData?>(serializedData);
    }

    public async Task RemoveFromStorage(string key)
    {
        await _jsRuntime.InvokeVoidAsync($"{StorageType}.removeItem", key);
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new JsonSerializerOptions();

    private static string Serialize<TData>(TData data) =>
        JsonSerializer.Serialize(data, _jsonSerializerOptions);

    private static TData? Deserialize<TData>(string? jsonData)
    {
        if(!string.IsNullOrWhiteSpace(jsonData))
        {
            return JsonSerializer.Deserialize<TData>(jsonData, _jsonSerializerOptions);
        }
        return default;
    }
}
