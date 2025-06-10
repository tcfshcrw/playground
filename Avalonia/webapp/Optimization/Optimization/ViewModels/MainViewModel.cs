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
    public ObservableCollection<CutItem> Items { get; } = new();
    public ObservableCollection<RectViewModel> Rect { get; } = new();
    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveLastItemCommand { get; }
    public IRelayCommand AddCircleCommand { get; }
    public IRelayCommand CalculateCommand { get; }
    [ObservableProperty] private string debugtext;

    [ObservableProperty] private string detailResultText;
    [ObservableProperty] private string logText;
    [ObservableProperty] private string textBoxRawMaterialWidth;
    [ObservableProperty] private string textBoxSlittingWear;
    [ObservableProperty] private bool isResultEditable;
    [ObservableProperty] private bool isLogEditable;
    
    [ObservableProperty] private int selectedAlgorithmIndex;
    [ObservableProperty] private int canvasWidth;
    [ObservableProperty] private int canvasHeight;
    private DateTime _calculationStartTime = DateTime.Now;
    private DateTime _calculationEndTime = DateTime.Now;
    private string SelectedAlgorithm;
    public MainViewModel()
    {
        AddItemCommand = new RelayCommand(AddItem);
        RemoveLastItemCommand = new RelayCommand(RemoveLastItem, CanRemoveLastItem);
        CalculateCommand = new RelayCommand(calculate);
        // initialize default data
        Items.Add(new CutItem { Code = "A", Width = 140, Weight = 1, Preferred = false, MaxCount = 3 });
        Items.Add(new CutItem { Code = "B", Width = 160, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "C", Width = 180, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "D", Width = 200, Weight = 1, Preferred = false, MaxCount = 4 });
        //Squares.Add(new Square { X = 20, Y = 20, Size = 100, Fill = Brushes.Blue });
        IsLogEditable = false;
        IsResultEditable = false;
        selectedAlgorithmIndex = 0;
        canvasWidth = 350;
        canvasHeight = 40;
        SelectedAlgorithm = "";
        selectedAlgorithmIndex = 0;
        textBoxRawMaterialWidth = "1200";
        textBoxSlittingWear = "0";
    }

    private void AddItem() => Items.Add(new CutItem());
    void RemoveLastItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
    bool CanRemoveLastItem() => Items.Count > 0;
    
    
    private void calculate()
    {

        //var boardLines = TextBoxRawMaterialWidth.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //var boards = boardLines.Select(line => int.TryParse(line.Trim(), out var w) ? w : 0).Where(w => w > 0).ToList();
        //var boards = textBoxRawMaterialWidth;
        int.TryParse(TextBoxRawMaterialWidth, out int boards);
        _calculationStartTime = DateTime.Now;
        var sb = new StringBuilder();
        int totalWeightAll = 0, totalWasteAll = 0;
        double boardHeight = canvasHeight;
        double canvasWidth = this.canvasWidth;
        //var items = ListBoxInput.Items as IEnumerable<MainViewModel.CutItem>
         var items = Items; // items 就是 ObservableCollection<CutItem>
         var validItems = items
                .Where(i => i.Width > 0 && i.Weight >= 0)
                .ToList();
            // 這就是目前 ListBox 上的所有資料


            //double cutLossValue = 0;
            int.TryParse(TextBoxSlittingWear, out int cutLossValue);

                int boardWidth = boards;
                double y = 0;
                Tuple<Dictionary<CutItem, int>, int, int> result = null;

                switch (selectedAlgorithmIndex)
                {
                    case 0:
                        result = CalculateBFS(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "BFS";
                        //Algorithm_text = "BFS result may get wrong due to the queue explicted";
                        break;
                    
                    case 1:
                        result = CalculateWithDp(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "DP";
                        break;

                    case 2:
                        result = CalculateBestFit(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "BestFit";
                        break;

                    case 3:
                        result = CalculateWithGenetic_Preferr(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "GeneticAlgorithm";
                        break;

                    case 4:
                        result = CalculateWithSimulatedAnnealing(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "SimulatedAnnealing";
                        break;
                    case 5:
                        result = CalculateWithBranchAndBound(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "BranchAndBound";
                        break;
                    case 6:
                        result = CalculateWithMonteCarlo(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "MonteCarlo";
                        break;
                    case 7:
                        result = CalculateWithAStar(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "A Star";
                        break;
                    case 8:
                        result = CalculateWithBruteForce(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "BruteForce";
                        break;
                        
                    default:
                        result = CalculateBFS(validItems, boardWidth, cutLossValue);
                        this.SelectedAlgorithm = "Default BFS";
                        break;
                }

                //string SelectedAlgorithm = "";
                _calculationEndTime = DateTime.Now;
                TimeSpan diff = _calculationEndTime - _calculationStartTime;
                int millisceonds = (int)diff.TotalMilliseconds;
                LogText += $"{this.SelectedAlgorithm} calculation time: {millisceonds}ms\n";
                //add the background rect to the canvas. 

                Rect.Clear();
                sb.AppendLine($"> Raw Coil Width {boardWidth}mm");
                if (result == null)
                {
                    sb.AppendLine("No valid combination found.\n");
                    DetailResultText=sb.ToString();
                    return;
                }
                var boardRect = new RectViewModel {X=0,Y=0, Width = canvasWidth, Height = boardHeight ,Fill=Brushes.WhiteSmoke, Text = ""};
                Rect.Add(boardRect);
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
                        var rectangle = new RectViewModel{ Width = segW, Height = boardHeight, Fill = colors[colorIndex % colors.Length], X=x, Y=y ,Text = $"{kv.Key.Code}\n{kv.Key.Width}mm"};
                        Rect.Add(rectangle);
                        
                        x += segW;
                        if (!(j == kv.Value - 1 && kv.Equals(combo.Last())))
                        {
                            var gap = new RectViewModel { Width = cutLossValue * pxPerMM, Height = boardHeight, Fill = Brushes.Red, X=x, Y=y ,Text = ""};
                            Rect.Add(gap);
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
            
        

        sb.AppendLine($"All weight:{totalWeightAll}");
        sb.AppendLine($"All Waste:{totalWasteAll}mm");
        DetailResultText=sb.ToString();
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
    
   
    
    
    

    
    


       
    
    public partial class CutItem : ObservableObject
    {
        [ObservableProperty] private string code = "";
        [ObservableProperty] private int width;
        [ObservableProperty] private int weight;
        [ObservableProperty] private bool preferred;
        [ObservableProperty] private int maxCount;

        public CutItem()
        {
            code = "";
            width = 110;
            weight = 1;
            preferred = false;
            maxCount = 3;
        }
    }
    
    
}



public class RectViewModel : ObservableObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public IBrush Fill { get; set; }
    public string Text { get; set; }
}




