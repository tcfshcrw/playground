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
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithMonteCarlo(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss, int iterations = 5000)
    {
        var rand = new Random();
        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        Dictionary<MainViewModel.CutItem, int> bestCombo = null;
        int bestUsed = -1;
        int bestWeight = 0;

        for (int it = 0; it < iterations; it++)
        {
            var combo = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
            int used = 0, weight = 0, cuts = 0;

            // 1. 先強制放入所有 Preferred 各一個
            var selected = new List<MainViewModel.CutItem>();
            bool valid = true;
            foreach (var p in preferredItems)
            {
                if (p.MaxCount < 1)
                {
                    valid = false;
                    break;
                }
                selected.Add(p);
            }
            if (!valid)
                continue; // 有 Preferred 沒法選，直接跳過這次

            // 2. 其餘物件隨機決定選幾個
            foreach (var item in items)
            {
                int already = selected.Count(x => new CutItemComparer().Equals(x, item));
                int count = rand.Next(0, item.MaxCount - already + 1); // 已經先選掉 Preferred
                for (int i = 0; i < count; i++)
                    selected.Add(item);
            }
            // 3. 隨機排列順序
            var shuffled = selected.OrderBy(x => rand.Next()).ToList();

            // 4. 填入直到超過 totalWidth
            foreach (var item in shuffled)
            {
                int loss = (cuts > 0 ? cutLoss : 0);
                int cost = item.Width + loss;

                if (used + cost > totalWidth)
                    break;

                used += cost;
                weight += item.Weight;
                if (!combo.ContainsKey(item)) combo[item] = 0;
                combo[item]++;
                cuts++;
            }

            // 5. 再檢查 Preferred 是否每個都真的有被排進去
            bool allPreferredSelected = !mustSelectPreferred ||
                preferredItems.All(p => combo.ContainsKey(p) && combo[p] > 0);

            if (allPreferredSelected && used > bestUsed)
            {
                bestUsed = used;
                bestWeight = weight;
                bestCombo = new Dictionary<MainViewModel.CutItem, int>(combo, new CutItemComparer());
            }
        }

        return bestCombo != null ? Tuple.Create(bestCombo, bestUsed, bestWeight) : null;
    }
}