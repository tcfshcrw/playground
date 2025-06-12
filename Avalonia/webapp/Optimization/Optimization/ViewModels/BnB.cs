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
    private Tuple<Dictionary<CutItem, int>, int, int> CalculateWithBranchAndBound(
List<CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();

        var queue = new Queue<(int index, int usedLength, int weight, Dictionary<CutItem, int> combo)>();
        queue.Enqueue((0, 0, 0, new Dictionary<CutItem, int>(new CutItemComparer())));

        int bestUsed = -1;
        int bestWeight = 0;
        Dictionary<CutItem, int> bestCombo = null;

        while (queue.Count > 0)
        {
            var (index, used, weight, combo) = queue.Dequeue();

            if (used > totalWidth)
                continue;

            // 剪枝
            int maxUsed = used;
            for (int i = index; i < items.Count; i++)
            {
                var item = items[i];
                if (item.MaxCount > 0)
                {
                    maxUsed += item.MaxCount * item.Width;
                    maxUsed += (item.MaxCount > 1 ? (item.MaxCount - 1) * cutLoss : 0);
                    maxUsed += (used > 0 ? cutLoss : 0);
                }
            }
            if (maxUsed <= bestUsed)
                continue;

            // ---- 核心重點：必須所有 Preferred 都有被選到 ----
            bool allPreferredSelected = preferredItems.All(
                p => combo.TryGetValue(p, out int count) && count > 0
            );

            // 只有全選 Preferred 才能成為最佳解
            if (used > bestUsed && allPreferredSelected)
            {
                bestUsed = used;
                bestCombo = new Dictionary<CutItem, int>(combo, new CutItemComparer());
                bestWeight = weight;
            }

            if (index >= items.Count)
                continue;

            var currentItem = items[index];
            for (int count = 0; count <= currentItem.MaxCount; count++)
            {
                int newUsed = used + count * currentItem.Width;
                if (count > 0)
                    newUsed += (count - 1) * cutLoss;
                if (count > 0 && used > 0)
                    newUsed += cutLoss;

                if (newUsed > totalWidth)
                    break;

                var newCombo = new Dictionary<CutItem, int>(combo, new CutItemComparer());
                if (count > 0)
                    newCombo[currentItem] = count;

                queue.Enqueue((index + 1, newUsed, weight + count * currentItem.Weight, newCombo));
            }
        }

        return bestCombo?.Count > 0 ? Tuple.Create(bestCombo, bestUsed, bestWeight) : null;
    }
}