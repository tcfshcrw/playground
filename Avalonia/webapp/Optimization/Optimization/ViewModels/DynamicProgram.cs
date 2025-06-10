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
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithDp(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        // 每個狀態：已用長度/對應一組(組合,用長,總重)
        var states = new List<(int used, int weight, Dictionary<MainViewModel.CutItem, int> combo)>();
        states.Add((0, 0, new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer())));

        for (int round = 0; round < items.Count; round++)
        {
            var newStates = new List<(int used, int weight, Dictionary<MainViewModel.CutItem, int> combo)>();
            foreach (var state in states)
            {
                foreach (var item in items)
                {
                    int usedCount = state.combo.ContainsKey(item) ? state.combo[item] : 0;
                    if (usedCount >= item.MaxCount) continue;

                    int extraLoss = state.combo.Values.Sum() >= 1 ? cutLoss : 0;
                    int nextUsed = state.used + item.Width + extraLoss;
                    if (nextUsed > totalWidth) continue;

                    var newCombo = new Dictionary<MainViewModel.CutItem, int>(state.combo, new CutItemComparer());
                    if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                    newCombo[item]++;

                    int newWeight = state.weight + item.Weight;

                    // 狀態去重：若已經有同組合、同用長，weight較小可略過（可加快）
                    // 但這裡我們暫時全部保留
                    newStates.Add((nextUsed, newWeight, newCombo));
                }
            }
            states.AddRange(newStates);
        }

        // 最後挑選所有Preferred都被選到且用長<=totalWidth，且用量最大、weight最大
        var best = states
            .Where(s =>
                s.used <= totalWidth
                && (!mustSelectPreferred || preferredItems.All(p => s.combo.TryGetValue(p, out int cnt) && cnt > 0)))
            .OrderByDescending(s => s.used)
            .ThenByDescending(s => s.weight)
            .FirstOrDefault();

        if (best.combo == null || best.combo.Count == 0)
            return null;

        return Tuple.Create(best.combo, best.used, best.weight);
    }
}