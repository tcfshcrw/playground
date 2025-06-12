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
    private Tuple<Dictionary<CutItem, int>, int, int> CalculateBestFit(List<CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();
        var sorted = items
            .Where(i => i.Width > 0 && i.MaxCount > 0)
            .OrderByDescending(i => i.Width)
            .ToList();

        var combo = new Dictionary<CutItem, int>(new CutItemComparer());
        int used = 0;
        int weight = 0;
        int totalCuts = 0;

        // 先將所有 Preferred 都至少排進去一次
        foreach (var p in preferredItems)
        {
            if (p.MaxCount < 1) return null; // Preferred 但Max=0，無解

            int loss = (totalCuts > 0 ? cutLoss : 0);
            int cost = p.Width + loss;

            if (used + cost > totalWidth)
                return null; // 沒辦法把所有 Preferred 都排進去

            used += cost;
            weight += p.Weight;
            combo[p] = 1;
            totalCuts++;
        }

        // Preferred 強制安排好後，剩餘空間用 BestFit 補滿
        while (true)
        {
            bool added = false;
            foreach (var item in sorted)
            {
                int currentCount = combo.ContainsKey(item) ? combo[item] : 0;
                int max = item.MaxCount;
                // 已經放過 Preferred，其他 item 最高只允許 MaxCount - 現有數量
                if (currentCount >= max) continue;

                int loss = (totalCuts > 0 ? cutLoss : 0);
                int cost = item.Width + loss;

                if (used + cost <= totalWidth)
                {
                    used += cost;
                    weight += item.Weight;
                    if (!combo.ContainsKey(item)) combo[item] = 0;
                    combo[item]++;
                    totalCuts++;
                    added = true;
                    break; // 放入一個後重新排序
                }
            }
            if (!added) break;
        }

        // 最後檢查是否 Preferred 全有出現（理論上會有）
        if (preferredItems.Any() && !preferredItems.All(p => combo.ContainsKey(p) && combo[p] > 0))
            return null;

        return combo.Count > 0 ? Tuple.Create(combo, used, weight) : null;
    }
}