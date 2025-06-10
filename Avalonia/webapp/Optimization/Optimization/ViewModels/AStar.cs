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
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithAStar(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var comparer = new CutItemComparer();
        var queue = new PriorityQueue<(int used, int weight, int cuts, Dictionary<MainViewModel.CutItem, int> combo), int>();
        var visited = new HashSet<string>();

        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        queue.Enqueue((0, 0, 0, new Dictionary<MainViewModel.CutItem, int>(comparer)), 0);

        Dictionary<MainViewModel.CutItem, int> bestCombo = null;
        int bestUsed = -1;
        int bestWeight = 0; // 附帶統計用

        while (queue.Count > 0)
        {
            var (used, weight, cuts, combo) = queue.Dequeue();

            // 檢查是否全部 Preferred 都有選到
            bool allPreferredSelected = !mustSelectPreferred ||
                preferredItems.All(p => combo.TryGetValue(p, out int count) && count > 0);

            // 只有符合條件時才當作可行解
            if (used > bestUsed && allPreferredSelected)
            {
                bestUsed = used;
                bestWeight = weight;
                bestCombo = new Dictionary<MainViewModel.CutItem, int>(combo, comparer);
            }

            foreach (var item in items)
            {
                if (combo.ContainsKey(item) && combo[item] >= item.MaxCount)
                    continue;

                int loss = (cuts > 0 ? cutLoss : 0);
                int nextUsed = used + item.Width + loss;
                if (nextUsed > totalWidth)
                    continue;

                var newCombo = new Dictionary<MainViewModel.CutItem, int>(combo, comparer);
                if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                newCombo[item]++;

                int newCuts = cuts + 1;
                int newWeight = weight + item.Weight;

                // 啟發式：最大剩餘可切的長度估算
                int remaining = totalWidth - nextUsed;
                int h = 0;
                foreach (var rem in items)
                {
                    int fit = remaining / (rem.Width + cutLoss);
                    int remainingCount = rem.MaxCount - (newCombo.ContainsKey(rem) ? newCombo[rem] : 0);
                    h += Math.Min(fit, remainingCount) * (rem.Width + cutLoss);
                }

                // 狀態 key
                string stateKey = string.Join(",", newCombo.OrderBy(k => k.Key.Code).Select(kv => $"{kv.Key.Code}:{kv.Value}"));
                if (visited.Contains(stateKey)) continue;
                visited.Add(stateKey);

                // Priority: 小值優先 → 所以取 -(used + h)
                int priority = -(nextUsed + h);
                queue.Enqueue((nextUsed, newWeight, newCuts, newCombo), priority);
            }
        }

        return bestCombo != null ? Tuple.Create(bestCombo, bestUsed, bestWeight) : null;
    }
}