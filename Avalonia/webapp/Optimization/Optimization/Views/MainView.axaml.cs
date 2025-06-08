using Avalonia.Controls;
using Avalonia.Interactivity;
using Optimization.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Optimization.Views;

public partial class MainView : UserControl
{
    DateTime _calculationStartTime = DateTime.Now;
    DateTime _calculationEndTime = DateTime.Now;
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
    private void BtnOnCalculate_OnClick(object? sender, RoutedEventArgs e)
    {

        var boardLines = TextBoxRawMaterialWidth.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var boards = boardLines.Select(line => int.TryParse(line.Trim(), out var w) ? w : 0).Where(w => w > 0).ToList();
        _calculationStartTime = DateTime.Now;
        var sb = new StringBuilder();
        int totalWeightAll = 0, totalWasteAll = 0;
        double boardHeight = CanvasVisualSlitting.Height;
        double canvasWidth = CanvasVisualSlitting.Width;
        //var items = ListBoxInput.Items as IEnumerable<MainViewModel.CutItem>;
        if (DataContext is MainViewModel vm)
        {
            var items = vm.Items; // items 就是 ObservableCollection<CutItem>
            var validItems = items
                .Where(i => i.Width > 0 && i.Weight >= 0)
                .ToList();
            // 這就是目前 ListBox 上的所有資料


            //double cutLossValue = 0;
            int.TryParse(TextBoxSlittingWear.Text, out int cutLossValue);
            for (int i = 0; i < boards.Count; i++)
            {
                int boardWidth = boards[i];
                double y = i * (boardHeight+10);
                Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> result = null;

                switch (ComboBoxAlgorithmSelector.SelectedIndex)
                {
                    case 0:
                        result = CalculateBFS(validItems, boardWidth, cutLossValue);
                        //Algorithm_text = "BFS result may get wrong due to the queue explicted";
                        break;

                    case 1:
                        result = CalculateWithDp(validItems, boardWidth, cutLossValue);
                        break;

                    case 2:
                        result = CalculateBestFit(validItems, boardWidth, cutLossValue);
                        break;

                    case 3:
                        result = CalculateWithGenetic_Optimized(validItems, boardWidth, cutLossValue);
                        break;

                    case 4:
                        result = CalculateWithSimulatedAnnealing(validItems, boardWidth, cutLossValue);
                        break;
                    case 5:
                        result = CalculateWithBranchAndBound(validItems, boardWidth, cutLossValue);
                        break;
                    case 6:
                        result = CalculateWithMonteCarlo(validItems, boardWidth, cutLossValue);
                        break;
                    case 7:
                        result = CalculateWithAStar(validItems, boardWidth, cutLossValue);
                        break;
                    case 8:
                        result = CalculateWithBruteForce(validItems, boardWidth, cutLossValue);
                        break;
                    default:
                        result = CalculateBFS(validItems, boardWidth, cutLossValue);
                        break;
                }

                string SelectedAlgorithm = "";
                if (ComboBoxAlgorithmSelector.SelectedItem is ComboBoxItem selectedItem)
                {
                    SelectedAlgorithm = selectedItem.Content?.ToString();
                }
                _calculationEndTime = DateTime.Now;
                TimeSpan diff = _calculationEndTime - _calculationStartTime;
                int millisceonds = (int)diff.TotalMilliseconds;
                TextBoxLog.Text += $"\n{SelectedAlgorithm} calculation time: {millisceonds}ms";
                //add the background rect to the canvas. 
                var boardRect = new Rectangle { Width = canvasWidth, Height = boardHeight, Fill = Brushes.LightGray };
                Canvas.SetTop(boardRect, y); Canvas.SetLeft(boardRect, 0);
                CanvasVisualSlitting.Children.Add(boardRect);
                
                sb.AppendLine($"> Item #{i + 1}(Width {boardWidth}mm)");
                if (result == null)
                {
                    sb.AppendLine("No valid combination found.\n");
                    continue;
                }

                var combo = result.Item1;
                int used = result.Item2;
                int weight = result.Item3;

                int cutCount = combo.Sum(p => p.Value);
                int cutLoss = (cutCount - 1) * cutLossValue;
                double pxPerMM = canvasWidth / boardWidth;
                double x = 0;
                int colorIndex = 0;
                var colors = new[] { Brushes.CadetBlue, Brushes.MediumSeaGreen, Brushes.SteelBlue, Brushes.SandyBrown, Brushes.IndianRed };
                foreach (var kv in combo)
                {
                    for (int j = 0; j < kv.Value; j++)
                    {
                        double segW = kv.Key.Width * pxPerMM ;
                        var rect = new Rectangle { Width = segW, Height = boardHeight, Fill = colors[colorIndex % colors.Length], Stroke = Brushes.Gray, StrokeThickness = 1 };
                        Canvas.SetLeft(rect, x); Canvas.SetTop(rect, y);
                        CanvasVisualSlitting.Children.Add(rect);

                        var text = new TextBlock { Text = $"{kv.Key.Code}\n{kv.Key.Width}mm", FontSize = 8, FontWeight = FontWeight.Bold, Foreground = Brushes.White };
                        Canvas.SetLeft(text, x + 5); Canvas.SetTop(text, y + 5);
                        CanvasVisualSlitting.Children.Add(text);

                        x += segW;
                        if (!(j == kv.Value - 1 && kv.Equals(combo.Last())))
                        {
                            var gap = new Rectangle { Width = cutLossValue * pxPerMM, Height = boardHeight, Fill = Brushes.Red };
                            Canvas.SetLeft(gap, x); Canvas.SetTop(gap, y);
                            CanvasVisualSlitting.Children.Add(gap);
                            x += cutLossValue * pxPerMM;
                        }
                    }
                    colorIndex++;
                }
                foreach (var kv in combo)
                    sb.AppendLine($"[{kv.Key.Code}] Width {kv.Key.Width} x {kv.Value}(Weight {kv.Key.Weight})");
                sb.AppendLine($"Total Usage:{used}mm(including cutting loss {cutLoss}mm)");
                sb.AppendLine($"Waste:{boardWidth - used}mm");
                sb.AppendLine($"Total weight:{weight}\n");

                totalWeightAll += weight;
                totalWasteAll += boardWidth - used;
            }
        }

        sb.AppendLine($"All weight:{totalWeightAll}");
        sb.AppendLine($"All Waste:{totalWasteAll}mm");
        //sb.AppendLine(Algorithm_text);
        TextBoxResult.Text = sb.ToString();
        //TextBoxResult.Text = "result"+"click";

        throw new System.NotImplementedException();
    }



    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateBFS(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var queue = new Queue<Tuple<int, int, Dictionary<MainViewModel.CutItem, int>>>();
        queue.Enqueue(new Tuple<int, int, Dictionary<MainViewModel.CutItem, int>>(0, 0, new Dictionary<MainViewModel.CutItem, int>()));
        Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> best = null;

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

                var newCombo = new Dictionary<MainViewModel.CutItem, int>(combo);
                if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                newCombo[item]++;

                int newWeight = totalWeight + item.Weight;
                if (best == null || newWeight > best.Item3 || (newWeight == best.Item3 && nextUsed > best.Item2))
                    best = new Tuple<Dictionary<MainViewModel.CutItem, int>, int, int>(newCombo, nextUsed, newWeight);

                queue.Enqueue(new Tuple<int, int, Dictionary<MainViewModel.CutItem, int>>(nextUsed, newWeight, newCombo));
            }
        }
        return best;
    }

    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithDP(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        Dictionary<int, Dictionary<MainViewModel.CutItem, int>> combos = new Dictionary<int, Dictionary<MainViewModel.CutItem, int>>();
        combos[0] = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());

        foreach (var item in items)
        {
            var currentCombos = combos.ToList(); // 複製當前所有狀態，避免在內部修改
            foreach (var kv in currentCombos)
            {
                int w = kv.Key;
                var currentCombo = kv.Value;

                int currentCount = currentCombo.ContainsKey(item) ? currentCombo[item] : 0;
                if (currentCount >= item.MaxCount) continue;

                int cutCount = currentCombo.Values.Sum();
                int cost = item.Width + (cutCount > 0 ? cutLoss : 0);
                int nextW = w + cost;

                if (nextW > totalWidth) continue;

                var newCombo = new Dictionary<MainViewModel.CutItem, int>(currentCombo, new CutItemComparer());
                if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                newCombo[item]++;

                // ✅ 只保留最大 nextW 時的任一組合（DP 不比較 weight）
                if (!combos.ContainsKey(nextW))
                    combos[nextW] = newCombo;
            }
        }

        // ✅ 找出最大使用長度
        int bestUsed = combos.Keys.Max();
        var bestCombo = combos[bestUsed];

        int bestWeight = bestCombo.Sum(kv => kv.Key.Weight * kv.Value); // 附帶算出總權重

        return Tuple.Create(bestCombo, bestUsed, bestWeight);
    }

    class CutItemComparer : IEqualityComparer<MainViewModel.CutItem>
    {
        public bool Equals(MainViewModel.CutItem x, MainViewModel.CutItem y)
        {
            return x.Code == y.Code && x.Width == y.Width && x.Weight == y.Weight;
        }

        public int GetHashCode(MainViewModel.CutItem obj)
        {
            return obj.Code.GetHashCode() ^ obj.Width ^ obj.Weight;
        }
    }

    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateBestFit(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        // 依照效率（Weight per mm）排序（可改為 Width 或其他）
        // 改為依照「寬度」排序，大的先填（可改成從小到大看情況）
        var sorted = items
            .Where(i => i.Width > 0 && i.MaxCount > 0)
            .OrderByDescending(i => i.Width)
            .ToList();

        var combo = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
        int used = 0;
        int weight = 0; // 附加資訊
        int totalCuts = 0;

        while (true)
        {
            bool added = false;
            foreach (var item in sorted)
            {
                int currentCount = combo.ContainsKey(item) ? combo[item] : 0;
                if (currentCount >= item.MaxCount) continue;

                int loss = (totalCuts > 0 ? cutLoss : 0);
                int cost = item.Width + loss;

                if (used + cost <= totalWidth)
                {
                    used += cost;
                    weight += item.Weight; // 附加統計
                    if (!combo.ContainsKey(item)) combo[item] = 0;
                    combo[item]++;
                    totalCuts++;
                    added = true;
                    break; // 放入一個後重新排序跑
                }
            }

            if (!added) break; // 無法再加入任何項目
        }

        return combo.Count > 0 ? Tuple.Create(combo, used, weight) : null;
    }

    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithGenetic(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss, int generations = 200, int populationSize = 100)
    {
        var rand = new Random();
        var genePool = new List<List<MainViewModel.CutItem>>();

        // 初始化族群
        for (int i = 0; i < populationSize; i++)
        {
            var gene = new List<MainViewModel.CutItem>();
            foreach (var item in items)
            {
                int count = rand.Next(0, item.MaxCount + 1);
                for (int j = 0; j < count; j++)
                    gene.Add(item);
            }
            gene = gene.OrderBy(_ => rand.Next()).ToList(); // 打亂順序
            genePool.Add(gene);
        }

        List<MainViewModel.CutItem> bestGene = null;
        int bestUsed = -1;

        for (int g = 0; g < generations; g++)
        {
            var scoredGenes = new List<Tuple<List<MainViewModel.CutItem>, int>>();

            foreach (var gene in genePool)
            {
                int totalUsed = 0, cuts = 0;
                foreach (var cut in gene)
                {
                    int loss = (cuts > 0 ? cutLoss : 0);
                    int cost = cut.Width + loss;
                    if (totalUsed + cost > totalWidth) break;

                    totalUsed += cost;
                    cuts++;
                }

                scoredGenes.Add(Tuple.Create(gene, totalUsed));
                if (totalUsed > bestUsed)
                {
                    bestUsed = totalUsed;
                    bestGene = gene;
                }
            }

            // 精英選擇（前 20%）
            var elites = scoredGenes.OrderByDescending(x => x.Item2).Take(populationSize / 5).ToList();
            genePool.Clear();

            while (genePool.Count < populationSize)
            {
                var parent1 = elites[rand.Next(elites.Count)].Item1;
                var parent2 = elites[rand.Next(elites.Count)].Item1;

                var child = new List<MainViewModel.CutItem>();
                for (int i = 0; i < parent1.Count && i < parent2.Count; i++)
                {
                    child.Add(rand.NextDouble() > 0.5 ? parent1[i] : parent2[i]);
                }

                // 突變
                if (rand.NextDouble() < 0.2 && child.Count > 0)
                {
                    int idx = rand.Next(child.Count);
                    child[idx] = items[rand.Next(items.Count)];
                }

                // 去除超過 MaxCount 的項目
                var groupedList = child.GroupBy(c => c, new CutItemComparer())
                                       .Select(grp => new { Item = grp.Key, Count = grp.Count() })
                                       .ToList();

                var grouped = groupedList.ToDictionary(x => x.Item, x => x.Count);
                child = child.Where(c => grouped[c] <= c.MaxCount).ToList();

                genePool.Add(child);
            }
        }

        // 將 bestGene 轉成輸出格式
        var result = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
        int used = 0, totalCuts = 0, weight = 0;
        foreach (var cut in bestGene)
        {
            int loss = (totalCuts > 0 ? cutLoss : 0);
            int cost = cut.Width + loss;
            if (used + cost > totalWidth) break;
            
            used += cost;
            if (!result.ContainsKey(cut)) result[cut] = 0;
            result[cut]++;
            weight += cut.Weight;
            totalCuts++;
        }

        return result.Count > 0 ? Tuple.Create(result, used, weight) : null;
    }
    private List<MainViewModel.CutItem> GenerateRandomGene(List<MainViewModel.CutItem> items, Random rand)
    {
        var gene = new List<MainViewModel.CutItem>();
        foreach (var item in items)
        {
            int count = rand.Next(0, item.MaxCount + 1);
            for (int i = 0; i < count; i++)
                gene.Add(item);
        }
        return gene.OrderBy(_ => rand.Next()).ToList();
    }
    private List<MainViewModel.CutItem> MutateGene(List<MainViewModel.CutItem> gene, Random rand)
    {
        if (gene.Count < 2) return gene;
        int i = rand.Next(gene.Count);
        int j = rand.Next(gene.Count);
        var temp = gene[i];
        gene[i] = gene[j];
        gene[j] = temp;
        return gene;
    }
    private int EvaluateUsedLength(List<MainViewModel.CutItem> gene, int totalWidth, int cutLoss, out int used, out int weight)
    {
        int total = 0, cuts = 0;
        weight = 0;

        foreach (var cut in gene)
        {
            int loss = (cuts > 0 ? cutLoss : 0);
            int cost = cut.Width + loss;
            if (total + cost > totalWidth) break;

            total += cost;
            weight += cut.Weight;
            cuts++;
        }

        used = total;
        return used; // ✅ 回傳作為 score 的就是使用寬度
    }
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithSimulatedAnnealing(
List<MainViewModel.CutItem> items, int totalWidth, int cutLoss,
int maxIterations = 4000, double initialTemp = 300.0, double coolingRate = 0.95)
    {
        var rand = new Random();
        var bestGene = GenerateRandomGene(items, rand);
        var bestScore = EvaluateUsedLength(bestGene, totalWidth, cutLoss, out int bestUsed, out int bestWeight);

        var currentGene = new List<MainViewModel.CutItem>(bestGene);
        double temperature = initialTemp;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            var neighbor = MutateGene(new List<MainViewModel.CutItem>(currentGene), rand);
            var score = EvaluateUsedLength(neighbor, totalWidth, cutLoss, out int used, out int weight);

            int delta = score - bestScore;
            if (delta > 0 || rand.NextDouble() < Math.Exp(delta / temperature))
            {
                currentGene = new List<MainViewModel.CutItem>(neighbor);
                if (score > bestScore)
                {
                    bestGene = new List<MainViewModel.CutItem>(neighbor);
                    bestScore = score;
                    bestUsed = used;
                    bestWeight = weight;
                }
            }

            temperature *= coolingRate;
        }

        // 統計最終結果
        var result = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
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

        return result.Count > 0 ? Tuple.Create(result, finalUsed, bestWeight) : null;
    }
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithBranchAndBound(
List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var queue = new Queue<(int index, int usedLength, int weight, Dictionary<MainViewModel.CutItem, int> combo)>();
        queue.Enqueue((0, 0, 0, new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer())));

        int bestUsed = -1;
        int bestWeight = 0;
        var bestCombo = new Dictionary<MainViewModel.CutItem, int>();

        while (queue.Count > 0)
        {
            var (index, used, weight, combo) = queue.Dequeue();

            if (used > totalWidth)
                continue; // ✂ 剪枝：超出板寬

            // ✂ 剪枝：估算最大可能寬度（理論最大使用長度）
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
                continue; // 無法超越目前最長使用寬度 → 剪枝

            if (used > bestUsed)
            {
                bestUsed = used;
                bestCombo = new Dictionary<MainViewModel.CutItem, int>(combo, new CutItemComparer());
                bestWeight = weight; // 附加統計
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

                var newCombo = new Dictionary<MainViewModel.CutItem, int>(combo, new CutItemComparer());
                if (count > 0)
                    newCombo[currentItem] = count;

                queue.Enqueue((index + 1, newUsed, weight + count * currentItem.Weight, newCombo));
            }
        }

        return bestCombo.Count > 0 ? Tuple.Create(bestCombo, bestUsed, bestWeight) : null;
    }

    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithMonteCarlo(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss, int iterations = 5000)
    {
        var rand = new Random();
        Dictionary<MainViewModel.CutItem, int> bestCombo = null;
        int bestUsed = -1;
        int bestWeight = 0;

        for (int it = 0; it < iterations; it++)
        {
            var combo = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
            int used = 0, weight = 0, cuts = 0;

            // 隨機打亂每項的出現次數與順序
            var shuffled = items.SelectMany(item =>
            {
                int count = rand.Next(0, item.MaxCount + 1);
                return Enumerable.Repeat(item, count);
            }).OrderBy(x => rand.Next()).ToList();

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

            if (used > bestUsed)
            {
                bestUsed = used;
                bestWeight = weight;
                bestCombo = new Dictionary<MainViewModel.CutItem, int>(combo, new CutItemComparer());
            }
        }

        return bestCombo != null ? Tuple.Create(bestCombo, bestUsed, bestWeight) : null;
    }
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithBruteForce(
List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var bestCombo = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
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
            var currentCombo = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
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

            if (currentUsed <= totalWidth && currentUsed > bestUsed)
            {
                bestUsed = currentUsed;
                bestWeight = currentWeight;
                bestCombo = new Dictionary<MainViewModel.CutItem, int>(currentCombo, new CutItemComparer());
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
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithAStar(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var comparer = new CutItemComparer();
        var queue = new PriorityQueue<(int used, int weight, int cuts, Dictionary<MainViewModel.CutItem, int> combo), int>();
        var visited = new HashSet<string>();

        queue.Enqueue((0, 0, 0, new Dictionary<MainViewModel.CutItem, int>(comparer)), 0);

        Dictionary<MainViewModel.CutItem, int> bestCombo = null;
        int bestUsed = -1;
        int bestWeight = 0; // 附帶統計用

        while (queue.Count > 0)
        {
            var (used, weight, cuts, combo) = queue.Dequeue();

            if (used > bestUsed)
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

                // 去重複狀態（狀態 key）
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
    
    public Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithGenetic_Optimized(
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

        Random rand = new Random();
        int maxGeneLength = 0;
        for (int i = 0; i < itemCount; i++)
            maxGeneLength += maxCounts[i];

        int[][] genePool = new int[populationSize][];
        int[] geneLens = new int[populationSize];

        // 初始化族群
        for (int i = 0; i < populationSize; i++)
        {
            int[] geneTmp = new int[maxGeneLength];
            int geneIdx = 0;
            for (int j = 0; j < itemCount; j++)
            {
                int count = rand.Next(0, maxCounts[j] + 1);
                for (int k = 0; k < count; k++)
                    geneTmp[geneIdx++] = j;
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
                int[] filtered = new int[len];
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
                newGenePool[i] = new int[validLen];
                Array.Copy(filtered, newGenePool[i], validLen);
                newGeneLens[i] = validLen;
            }
            genePool = newGenePool;
            geneLens = newGeneLens;
        }

        // 輸出結果
        var result = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());
        int usedFinal = 0, cutsFinal = 0, weightFinal = 0;
        int[] usedCounts = new int[itemCount];
        for (int i = 0; i < bestGene.Length; i++)
        {
            int idx = bestGene[i];
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
    
    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateWithDp(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
    {
        var combos = new Dictionary<int, Dictionary<MainViewModel.CutItem, int>>( );
        combos[0] = new Dictionary<MainViewModel.CutItem, int>(new CutItemComparer());

        foreach (var item in items)
        {
            var currentCombos = combos.ToList();
            foreach (var kv in currentCombos)
            {
                int w = kv.Key;
                var currentCombo = kv.Value;
                int currentCount = currentCombo.TryGetValue(item, out int v) ? v : 0;
                if (currentCount >= item.MaxCount) continue;

                int cutCount = currentCombo.Values.Sum();
                int cost = item.Width + (cutCount > 0 ? cutLoss : 0);
                int nextW = w + cost;
                if (nextW > totalWidth) continue;

                var newCombo = new Dictionary<MainViewModel.CutItem, int>(currentCombo, new CutItemComparer());
                if (!newCombo.ContainsKey(item)) newCombo[item] = 0;
                newCombo[item]++;

                int newPreferred = newCombo.Where(x => x.Key.Preferred).Sum(x => x.Value);
                int newWeight = newCombo.Sum(x => x.Key.Weight * x.Value);

                if (!combos.ContainsKey(nextW))
                {
                    combos[nextW] = newCombo;
                }
                else
                {
                    var oldCombo = combos[nextW];
                    int oldPreferred = oldCombo.Where(x => x.Key.Preferred).Sum(x => x.Value);
                    int oldWeight = oldCombo.Sum(x => x.Key.Weight * x.Value);

                    // preferred 多者勝，preferred 一樣 weight 大者勝
                    if (newPreferred > oldPreferred ||
                        (newPreferred == oldPreferred && newWeight > oldWeight))
                    {
                        combos[nextW] = newCombo;
                    }
                }
            }
        }

        // 選出用量最大的（waste 最小的），preferred 多的
        var best = combos
            .OrderByDescending(kv => kv.Key) // used 長度最大 = waste 最小
            .ThenByDescending(kv => kv.Value.Where(x => x.Key.Preferred).Sum(x => x.Value)) // preferred 多
            .ThenByDescending(kv => kv.Value.Sum(x => x.Key.Weight * x.Value)) // weight 多
            .First();

        var bestCombo = best.Value;
        int bestUsed = best.Key;
        int bestWeight = bestCombo.Sum(kv => kv.Key.Weight * kv.Value);

        return Tuple.Create(bestCombo, bestUsed, bestWeight);
    }

    public double btn_count = 0;
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var rect = new Rectangle { Width = 10, Height = 10, Fill = Brushes.Red, Stroke = Brushes.Black, StrokeThickness = 1 };
        Canvas.SetLeft(rect, 5*btn_count); Canvas.SetTop(rect, 5);
        CanvasVisualSlitting.Children.Add(rect);
        btn_count++;
        throw new NotImplementedException();
    }
}