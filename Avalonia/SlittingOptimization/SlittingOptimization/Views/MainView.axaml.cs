using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SlittingOptimization.ViewModels;

namespace SlittingOptimization.Views;

public partial class MainView : UserControl
{
    DateTime CalculationStartTime = DateTime.Now;
    DateTime CalculationEndTime = DateTime.Now;

    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void BtnOnCalculate_OnClick(object? sender, RoutedEventArgs e)
    {
        
        var boardLines = TextBoxRawMaterialWidth.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var boards = boardLines.Select(line => int.TryParse(line.Trim(), out var w) ? w : 0).Where(w => w > 0).ToList();
        
        var sb = new StringBuilder();
        int totalWeightAll = 0, totalWasteAll = 0;
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
                //double y = i * (boardHeight + 10);
                Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> result = null;

                switch (ComboBoxAlgorithmSelector.SelectedIndex)
                {
                    case 0:
                        result = CalculateBestCombination(validItems, boardWidth, cutLossValue);
                        //Algorithm_text = "BFS result may get wrong due to the queue explicted";
                        break;
                    /*
                    case 1:
                        result = CalculateWithDP(validItems, boardWidth, cutLossValue);
                        break;

                    case 2:
                        result = CalculateBestFit(validItems, boardWidth, cutLossValue);
                        break;
                    case 3:
                        result = CalculateWithGenetic(validItems, boardWidth, cutLossValue);
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
                        result = await RunChatGPTAsAlgorithm(validItems, boardWidth, cutLossValue);
                        Algorithm_text = "Slitting Data provided by ChatGPT\nAlgorithm:" + GPTAlgorithm;
                        break;
                    case 9:
                        result = CalculateWithBruteForce(validItems, boardWidth, cutLossValue);
                        Algorithm_text = "";
                        break;
                        */
                    default:
                        result = CalculateBestCombination(validItems, boardWidth, cutLossValue);
                        break;
                }

                string SelectedAlgorithm = ComboBoxAlgorithmSelector.SelectedItem?.ToString();
                CalculationEndTime = DateTime.Now;
                TimeSpan diff = CalculationEndTime - CalculationStartTime;
                int millisceonds = (int)diff.TotalMilliseconds;
                //LogTextBox.Text += $"\n{SelectedAlgorithm} calculation time: {millisceonds}ms";

                sb.AppendLine($"> 板材 #{i + 1}（寬度 {boardWidth}mm）");
                if (result == null)
                {
                    sb.AppendLine("   找不到有效組合\n");
                    continue;
                }

                var combo = result.Item1;
                int used = result.Item2;
                int weight = result.Item3;

                int cutCount = combo.Sum(p => p.Value);
                int cutLoss = (cutCount - 1) * cutLossValue;

                foreach (var kv in combo)
                    sb.AppendLine($"   [{kv.Key.Code}] 裁切寬度 {kv.Key.Width} × {kv.Value}（權重 {kv.Key.Weight}）");
                sb.AppendLine($"   使用總長：{used}mm（含切割損耗 {cutLoss}mm）");
                sb.AppendLine($"   剩餘浪費：{boardWidth - used}mm");
                sb.AppendLine($"   總權重：{weight}\n");

                totalWeightAll += weight;
                totalWasteAll += boardWidth - used;
            }
        }

        sb.AppendLine($"✅ 所有板材總權重：{totalWeightAll}");
        sb.AppendLine($"✅ 所有剩餘浪費總和：{totalWasteAll}mm");
        //sb.AppendLine(Algorithm_text);
        TextBoxResult.Text = sb.ToString();
        //TextBoxResult.Text = "result"+"click";
        
        throw new System.NotImplementedException();
    }



    private Tuple<Dictionary<MainViewModel.CutItem, int>, int, int> CalculateBestCombination(List<MainViewModel.CutItem> items, int totalWidth, int cutLoss)
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
}