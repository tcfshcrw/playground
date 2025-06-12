using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Optimization.ViewModels;
using System.Runtime.InteropServices.JavaScript;
//#if NET8_0_OR_GREATER && BROWSER
// JS interop 版本
public static partial class LocalStorageHelper
{
    [JSImport("window.localStorageSet")]
    internal static partial void LocalStorageSet(string key, string value);
    [JSImport("window.localStorageGet")]
    internal static partial string? LocalStorageGet(string key);

    public static void SaveCutItems(List<CutItem> items)
    {
        try
        {
            var json = JsonSerializer.Serialize(items);
            LocalStorageHelper.LocalStorageSet("cutItems", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Serialize or Save Error: " + ex.Message);
            Console.WriteLine("詳細錯誤： " + ex.ToString());
        }
    }

    public static List<CutItem> LoadCutItems()
    {
        try
        {
            var json = LocalStorageGet("cutItems");
            if (string.IsNullOrEmpty(json))
                return new List<CutItem>();
            return System.Text.Json.JsonSerializer.Deserialize<List<CutItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CutItem>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }
}


//#else
 //桌面 fallback (可寫本機檔案或直接跳過)
//public static class LocalStorageHelper
//{
//    public static void SaveCutItems(List<CutItem> items) { /* 省略或實作本地儲存 */ }
//    public static List<CutItem> LoadCutItems() => new();
//}
//#endif
