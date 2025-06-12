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
    private Tuple<Dictionary<CutItem, int>, int, int> CalculateWithSimulatedAnnealing(
        List<CutItem> items, int totalWidth, int cutLoss,
        int maxIterations = 4000, double initialTemp = 300.0, double coolingRate = 0.95)
    {
        var rand = new Random();
        var preferredItems = items.Where(i => i.Preferred).ToList();
        bool mustSelectPreferred = preferredItems.Count > 0;

        // 1. 初始化：先強制每個 Preferred 至少出現一次
        var baseGene = new List<CutItem>();
        foreach (var p in preferredItems) baseGene.Add(p);

        // 其餘隨機補齊
        var bestGene = GenerateRandomGeneWithPreferred(items, baseGene, rand, totalWidth, cutLoss);
        var bestScore = EvaluateUsedLengthWithPreferred(bestGene, totalWidth, cutLoss, preferredItems, out int bestUsed,
            out int bestWeight);

        var currentGene = new List<CutItem>(bestGene);
        double temperature = initialTemp;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var neighbor = MutateGeneWithPreferred(new List<CutItem>(currentGene), preferredItems, rand);
            var score = EvaluateUsedLengthWithPreferred(neighbor, totalWidth, cutLoss, preferredItems, out int used,
                out int weight);

            int delta = score - bestScore;
            if (delta > 0 || rand.NextDouble() < Math.Exp(delta / temperature))
            {
                currentGene = new List<CutItem>(neighbor);
                if (score > bestScore)
                {
                    bestGene = new List<CutItem>(neighbor);
                    bestScore = score;
                    bestUsed = used;
                    bestWeight = weight;
                }
            }

            temperature *= coolingRate;
        }

        // 統計最終結果
        var result = new Dictionary<CutItem, int>(new CutItemComparer());
        int totalCuts = 0, finalUsed = 0;
        foreach (var cut in bestGene)
        {
            int loss = (totalCuts > 0 ? cutLoss : 0);
            int cost = cut.Width + loss;
            if (finalUsed + cost > totalWidth) break;

            finalUsed += cost;
            if (!result.ContainsKey(cut)) result[cut] = 0;
            result[cut]++;
            totalCuts++;
        }

        // 最後再次檢查所有 Preferred 都有
        if (mustSelectPreferred && !preferredItems.All(p => result.ContainsKey(p) && result[p] > 0))
            return null;

        return result.Count > 0 ? Tuple.Create(result, finalUsed, bestWeight) : null;
    }

    private List<CutItem> GenerateRandomGeneWithPreferred(
        List<CutItem> items,
        List<CutItem> preferredSeed,
        Random rand,
        int totalWidth,
        int cutLoss)
    {
        // 先放所有 Preferred
        var gene = new List<CutItem>(preferredSeed);
        var comboCount = preferredSeed
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count(), new CutItemComparer());

        int used = 0;
        int totalCuts = 0;
        foreach (var cut in gene)
        {
            int loss = (totalCuts > 0 ? cutLoss : 0);
            used += cut.Width + loss;
            totalCuts++;
        }

        // 所有候選
        var candidates = items.ToList();

        // 隨機補齊（不限Preferred），不超MaxCount,不超totalWidth
        while (true)
        {
            var available = candidates.Where(i =>
                (!comboCount.ContainsKey(i) || comboCount[i] < i.MaxCount)
                && used + (i.Width + (totalCuts > 0 ? cutLoss : 0)) <= totalWidth
            ).ToList();

            if (available.Count == 0) break;
            var pick = available[rand.Next(available.Count)];

            int loss = (totalCuts > 0 ? cutLoss : 0);
            used += pick.Width + loss;
            totalCuts++;
            gene.Add(pick);
            if (!comboCount.ContainsKey(pick)) comboCount[pick] = 0;
            comboCount[pick]++;
        }

        return gene;
    }

    private List<CutItem> MutateGeneWithPreferred(
        List<CutItem> gene,
        List<CutItem> preferredItems,
        Random rand)
    {
        var result = new List<CutItem>(gene);
        var combo = result.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count(), new CutItemComparer());

        // 1. 隨機挑一種操作
        int action = rand.Next(3);

        if (action == 0 && result.Count > preferredItems.Count)
        {
            // 隨機移除一個非Preferred或非唯一Preferred
            int removeIdx = -1;
            for (int tryCount = 0; tryCount < 10; tryCount++)
            {
                int idx = rand.Next(result.Count);
                var item = result[idx];
                bool isPreferred = preferredItems.Contains(item);

                // 只移除非Preferred，或這個Preferred在gene裡數量大於1
                if (!isPreferred || (combo[item] > 1))
                {
                    removeIdx = idx;
                    break;
                }
            }

            if (removeIdx >= 0)
                result.RemoveAt(removeIdx);
        }
        else if (action == 1)
        {
            // 隨機換一個（非Preferred/Preferred都可）
            int idx = rand.Next(result.Count);
            var oldItem = result[idx];
            // 換成另一個隨機item（不超MaxCount限制）
            // 保證換掉後不會讓任何 Preferred 缺失
            var candidatePool = gene
                .Concat(preferredItems)
                .Distinct(new CutItemComparer())
                .ToList();
            var pick = candidatePool[rand.Next(candidatePool.Count)];
            int count = result.Count(x => new CutItemComparer().Equals(x, pick));
            if (count < pick.MaxCount)
                result[idx] = pick;
        }
        else
        {
            // 隨機新增一個（在MaxCount內）
            var allItems = gene
                .Concat(preferredItems)
                .Concat(preferredItems.SelectMany(x => Enumerable.Repeat(x, 2))) // 提升Preferred被抽中的機率
                .Distinct(new CutItemComparer())
                .ToList();

            var available = allItems.Where(i =>
                result.Count(x => new CutItemComparer().Equals(x, i)) < i.MaxCount
            ).ToList();

            if (available.Count > 0)
            {
                var pick = available[rand.Next(available.Count)];
                result.Add(pick);
            }
        }

        return result;
    }
    
    private int EvaluateUsedLengthWithPreferred(
        List<CutItem> gene,
        int totalWidth,
        int cutLoss,
        List<CutItem> preferredItems,
        out int used,
        out int weight)
    {
        used = 0;
        weight = 0;
        int totalCuts = 0;
        var combo = new Dictionary<CutItem, int>(new CutItemComparer());

        foreach (var cut in gene)
        {
            int loss = (totalCuts > 0 ? cutLoss : 0);
            int cost = cut.Width + loss;
            if (used + cost > totalWidth) break;
            used += cost;
            weight += cut.Weight;
            if (!combo.ContainsKey(cut)) combo[cut] = 0;
            combo[cut]++;
            totalCuts++;
        }
        // 檢查 Preferred
        bool allPreferred = !preferredItems.Any() ||
                            preferredItems.All(p => combo.ContainsKey(p) && combo[p] > 0);

        // 若沒滿足Preferred，給超低分
        return allPreferred ? used : -999999;
    }



}