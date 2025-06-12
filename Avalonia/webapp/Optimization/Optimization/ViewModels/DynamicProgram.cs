using System;
using System.Collections;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using Avalonia.Media;
namespace Optimization.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Tuple<Dictionary<CutItem, int>, int, int> CalculateWithDp(List<CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        // 狀態：已用長度 → 最佳組合
        var dp = new Dictionary<int, Dictionary<CutItem, int>>();
        dp[0] = new Dictionary<CutItem, int>(new CutItemComparer());

        foreach (var item in items)
        {
            // 最多 item.MaxCount 次，因為每個 item 只能用這麼多
            for (int count = 1; count <= item.MaxCount; count++)
            {
                // item 寬 * count + cut loss
                int addWidth = count * item.Width;
                if (count > 1)
                    addWidth += (count - 1) * cutLoss;

                // 要在遍歷時複製 key list（因為字典會被動態改動）
                var existingStates = dp.Keys.ToList();
                foreach (var usedLength in existingStates)
                {
                    int nextUsed = usedLength + addWidth;
                    if (nextUsed > totalWidth) continue;

                    // 構造新組合
                    var combo = new Dictionary<CutItem, int>(dp[usedLength], new CutItemComparer());
                    if (!combo.ContainsKey(item)) combo[item] = 0;
                    combo[item] += count;

                    // 計算 weight
                    int newWeight = combo.Sum(x => x.Key.Weight * x.Value);

                    // 若已經有該 nextUsed 組合，比較 weight 大小
                    if (!dp.ContainsKey(nextUsed) || 
                        newWeight > dp[nextUsed].Sum(x => x.Key.Weight * x.Value))
                    {
                        dp[nextUsed] = combo;
                    }
                }
            }
        }

        // 找所有 preferred 都有出現的最佳解
        var best = dp
            .Where(kv => !mustSelectPreferred ||
                preferredItems.All(p => kv.Value.TryGetValue(p, out int cnt) && cnt > 0))
            .OrderByDescending(kv => kv.Key)
            .ThenByDescending(kv => kv.Value.Sum(x => x.Key.Weight * x.Value))
            .FirstOrDefault();

        if (best.Value == null || best.Value.Count == 0)
            return null;

        var bestCombo = best.Value;
        int bestUsed = best.Key;
        int bestWeight = bestCombo.Sum(kv => kv.Key.Weight * kv.Value);

        return Tuple.Create(bestCombo, bestUsed, bestWeight);
    }
}