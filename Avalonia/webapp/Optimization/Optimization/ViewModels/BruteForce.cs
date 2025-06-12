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
   private Tuple<Dictionary<CutItem, int>, int, int> CalculateWithBruteForce(
List<CutItem> items, int totalWidth, int cutLoss)
    {
        var preferredItems = items.Where(i => i.Preferred).ToList();

        var bestCombo = new Dictionary<CutItem, int>(new CutItemComparer());
        int bestUsed = -1;
        int bestWeight = 0;

        int itemCount = items.Count;
        var currentComboIndex = new int[itemCount];
        var maxCounts = items.Select(i => i.MaxCount).ToArray();

        int totalCombinations = 1;
        foreach (int max in maxCounts)
            totalCombinations *= (max + 1);

        for (int comboIndex = 0; comboIndex < totalCombinations; comboIndex++)
        {
            var currentCombo = new Dictionary<CutItem, int>(new CutItemComparer());
            int currentUsed = 0;
            int currentWeight = 0;
            int pieceCount = 0;

            for (int i = 0; i < itemCount; i++)
            {
                int count = currentComboIndex[i];
                if (count > 0)
                {
                    var item = items[i];
                    currentCombo[item] = count;
                    currentUsed += count * item.Width;
                    currentWeight += count * item.Weight;
                    pieceCount += count;
                }
            }

            if (pieceCount > 1)
                currentUsed += (pieceCount - 1) * cutLoss;

            // 核心 Preferred 條件
            bool allPreferredSelected = !preferredItems.Any() ||
                                        preferredItems.All(p => currentCombo.TryGetValue(p, out int cnt) && cnt > 0);

            if (currentUsed <= totalWidth && currentUsed > bestUsed && allPreferredSelected)
            {
                bestUsed = currentUsed;
                bestWeight = currentWeight;
                bestCombo = new Dictionary<CutItem, int>(currentCombo, new CutItemComparer());
            }

            // 進位遞增組合
            for (int i = 0; i < itemCount; i++)
            {
                currentComboIndex[i]++;
                if (currentComboIndex[i] <= maxCounts[i])
                    break;
                currentComboIndex[i] = 0;
            }
        }

        return bestCombo.Count > 0
            ? Tuple.Create(bestCombo, bestUsed, bestWeight)
            : null;
    } 
}