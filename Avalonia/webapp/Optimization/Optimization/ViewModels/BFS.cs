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
    private Tuple<Dictionary<CutItem, int>, int, int> CalculateBFS(List<CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        var queue = new Queue<Tuple<int, int, Dictionary<CutItem, int>>>();
        queue.Enqueue(new Tuple<int, int, Dictionary<CutItem, int>>(0, 0, new Dictionary<CutItem, int>()));
        Tuple<Dictionary<CutItem, int>, int, int> best = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int usedLength = current.Item1;
            int totalWeight = current.Item2;
            var combo = current.Item3;

            foreach (var item in items)
            {
                int usedCount = combo.ContainsKey(item) ? combo[item] : 0;
                if (usedCount >= item.MaxCount) continue;
                int extraLoss = (combo.Values.Sum() >= 1) ? cutLoss : 0;
                int nextUsed = usedLength + item.Width + extraLoss;
                if (nextUsed > totalWidth) continue;

                var newCombo = new Dictionary<CutItem, int>(combo);
                if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                newCombo[item]++;

                int newWeight = totalWeight + item.Weight;

                // 只要每個 Preferred 都有被選到（Count > 0），才能當有效解
                bool allPreferredSelected = !mustSelectPreferred ||
                                            preferredItems.All(p => newCombo.TryGetValue(p, out int count) && count > 0);

                if (allPreferredSelected &&
                    (best == null || newWeight > best.Item3 || (newWeight == best.Item3 && nextUsed > best.Item2)))
                {
                    best = new Tuple<Dictionary<CutItem, int>, int, int>(newCombo, nextUsed, newWeight);
                }

                queue.Enqueue(new Tuple<int, int, Dictionary<CutItem, int>>(nextUsed, newWeight, newCombo));
            }
        }
        return best;
    }
}