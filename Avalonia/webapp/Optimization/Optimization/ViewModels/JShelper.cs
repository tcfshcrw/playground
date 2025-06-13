using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Optimization.ViewModels;
using System.Runtime.InteropServices.JavaScript;
//#if NET8_0_OR_GREATER && BROWSER
// JS interop 版本
using System.Text.Json.Serialization;
using System.Collections.Generic;

[JsonSerializable(typeof(List<CutItem>))]
public partial class JsonContext : JsonSerializerContext { }
public static partial class LocalStorageHelper
{

    [JSImport("globalThis.localStorageSet")]
    public static partial void LocalStorageSet(string key, string value);
    [JSImport("globalThis.localStorageGet")]
    public static partial string? LocalStorageGet(string key);
    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog(string text);
    [JSImport("globalThis.customLog")]
    public static partial void CustomLog(string message);
    public static void SaveCutItems(List<CutItem> items)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(items, JsonContext.Default.ListCutItem);
            LocalStorageHelper.LocalStorageSet("cutItems", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Serialize or Save Error: " + ex.Message);
            Console.WriteLine("詳細錯誤： " + ex.ToString());
        }
    }

    /*
    public static string LoadCutItems()
    {
        try
        {
            var json = LocalStorageGet("cutItems");
            if (string.IsNullOrEmpty(json))
                return new List<CutItem>();
            




        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

    }
    */
}


//#else
 //桌面 fallback (可寫本機檔案或直接跳過)
//public static class LocalStorageHelper
//{
//    public static void SaveCutItems(List<CutItem> items) { /* 省略或實作本地儲存 */ }
//    public static List<CutItem> LoadCutItems() => new();
//}
//#endif
