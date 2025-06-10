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
        public Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithGenetic_Preferr(
    List<MainViewModel.CutItem> items, int totalWidth, int cutLoss,
    int generations = 200, int populationSize = 100)
    {
        int itemCount = items.Count;
        int[] maxCounts = new int[itemCount];
        int[] itemWidths = new int[itemCount];
        int[] itemWeights = new int[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            maxCounts[i] = items[i].MaxCount;
            itemWidths[i] = items[i].Width;
            itemWeights[i] = items[i].Weight;
        }

        // Preferred item index list
        var preferredIdx = items
            .Select((item, idx) => (item, idx))
            .Where(t => t.item.Preferred)
            .Select(t => t.idx)
            .ToArray();

        Random rand = new Random();
        int maxGeneLength = 0;
        for (int i = 0; i < itemCount; i++)
            maxGeneLength += maxCounts[i];

        int[][] genePool = new int[populationSize][];
        int[] geneLens = new int[populationSize];

        // 初始化族群 (每個個體都必須包含所有 preferred)
        for (int i = 0; i < populationSize; i++)
        {
            int[] geneTmp = new int[maxGeneLength];
            int geneIdx = 0;

            // 先放 preferred，每個至少一個
            foreach (var pIdx in preferredIdx)
            {
                geneTmp[geneIdx++] = pIdx;
            }

            // 計算剩餘 maxCount
            int[] remain = (int[])maxCounts.Clone();
            foreach (var pIdx in preferredIdx)
                remain[pIdx]--;

            for (int j = 0; j < itemCount; j++)
            {
                if (remain[j] > 0)
                {
                    int count = rand.Next(0, remain[j] + 1);
                    for (int k = 0; k < count; k++)
                        geneTmp[geneIdx++] = j;
                }
            }

            // 洗牌
            for (int k = geneIdx - 1; k > 0; k--)
            {
                int n = rand.Next(k + 1);
                int t = geneTmp[k]; geneTmp[k] = geneTmp[n]; geneTmp[n] = t;
            }
            genePool[i] = new int[geneIdx];
            geneLens[i] = geneIdx;
            Array.Copy(geneTmp, genePool[i], geneIdx);
        }

        int[] bestGene = null;
        int bestUsed = -1;
        int[] fitness = new int[populationSize];

        for (int gen = 0; gen < generations; gen++)
        {
            // 計算分數
            for (int i = 0; i < populationSize; i++)
            {
                int used = 0, cuts = 0;
                int[] gene = genePool[i];
                int len = geneLens[i];
                for (int j = 0; j < len; j++)
                {
                    int idx = gene[j];
                    int loss = (cuts > 0 ? cutLoss : 0);
                    int cost = itemWidths[idx] + loss;
                    if (used + cost > totalWidth)
                        break;
                    used += cost;
                    cuts++;
                }
                fitness[i] = used;
                if (used > bestUsed)
                {
                    bestUsed = used;
                    bestGene = new int[len];
                    Array.Copy(gene, bestGene, len);
                }
            }

            // 精英選前 20%
            int eliteCount = populationSize / 5;
            int[] eliteIdx = new int[eliteCount];
            for (int i = 0; i < eliteCount; i++)
            {
                int maxIdx = -1, maxFit = -1;
                for (int j = 0; j < populationSize; j++)
                {
                    if (fitness[j] > maxFit && Array.IndexOf(eliteIdx, j, 0, i) < 0)
                    {
                        maxFit = fitness[j];
                        maxIdx = j;
                    }
                }
                eliteIdx[i] = maxIdx;
            }

            // 下一代
            int[][] newGenePool = new int[populationSize][];
            int[] newGeneLens = new int[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                int p1 = eliteIdx[rand.Next(eliteCount)];
                int p2 = eliteIdx[rand.Next(eliteCount)];
                int len = Math.Min(geneLens[p1], geneLens[p2]);
                int[] child = new int[len];
                for (int j = 0; j < len; j++)
                    child[j] = (rand.NextDouble() > 0.5) ? genePool[p1][j] : genePool[p2][j];

                // 突變
                if (rand.NextDouble() < 0.2 && len > 0)
                {
                    int midx = rand.Next(len);
                    child[midx] = rand.Next(itemCount);
                }

                // 過濾 MaxCount
                int[] counts = new int[itemCount];
                int[] filtered = new int[len + preferredIdx.Length]; // 保留空間以防補齊 preferred
                int validLen = 0;
                for (int j = 0; j < len; j++)
                {
                    int idx = child[j];
                    if (counts[idx] < maxCounts[idx])
                    {
                        filtered[validLen++] = idx;
                        counts[idx]++;
                    }
                }
                // 強制補齊 preferred
                foreach (var pIdx in preferredIdx)
                {
                    if (counts[pIdx] == 0 && maxCounts[pIdx] > 0)
                    {
                        filtered[validLen++] = pIdx;
                        counts[pIdx]++;
                    }
                }
                newGenePool[i] = new int[validLen];
                Array.Copy(filtered, newGenePool[i], validLen);
                newGeneLens[i] = validLen;
            }
            genePool = newGenePool;
            geneLens = newGeneLens;
        }

        // === 最終輸出結果，優先放 preferred ===
        var result = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
        int usedFinal = 0, cutsFinal = 0, weightFinal = 0;
        int[] usedCounts = new int[itemCount];

        // 1. 先放所有 preferred
        foreach (var pIdx in preferredIdx)
        {
            int loss = (cutsFinal > 0 ? cutLoss : 0);
            int cost = itemWidths[pIdx] + loss;
            if (usedFinal + cost > totalWidth)
                return null; // 無法組出含所有 preferred 的合法組合
            usedFinal += cost;
            usedCounts[pIdx]++;
            weightFinal += itemWeights[pIdx];
            cutsFinal++;
        }

        // 2. 放 gene 內其他項目
        for (int i = 0; i < bestGene.Length; i++)
        {
            int idx = bestGene[i];
            // 已經放過的 preferred 跳過
            if (items[idx].Preferred && usedCounts[idx] > 0)
                continue;

            int loss = (cutsFinal > 0 ? cutLoss : 0);
            int cost = itemWidths[idx] + loss;
            if (usedFinal + cost > totalWidth) break;
            usedFinal += cost;
            usedCounts[idx]++;
            weightFinal += itemWeights[idx];
            cutsFinal++;
        }
        for (int i = 0; i < itemCount; i++)
            if (usedCounts[i] > 0)
                result[items[i]] = usedCounts[i];

        return result.Count > 0 ? Tuple.Create(result, usedFinal, weightFinal) : null;
    }
}