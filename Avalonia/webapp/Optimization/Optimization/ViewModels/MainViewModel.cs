using System;
using System.Collections;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
namespace Optimization.ViewModels;
using System.Runtime.InteropServices.JavaScript;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<CutItem> Items { get; } = new();
    public ObservableCollection<RectViewModel> Rect { get; } = new();
    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveLastItemCommand { get; }
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
        /*
        Items.Add(new CutItem { Code = "A", Width = 140, Weight = 1, Preferred = false, MaxCount = 3 });
        Items.Add(new CutItem { Code = "B", Width = 160, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "C", Width = 180, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "D", Width = 200, Weight = 1, Preferred = false, MaxCount = 4 });
        */
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
        //save input data
        try
        {
            SaveCutItemsToLocal();
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
            LogText+=e.Message;
            throw;
        }
        
        //var boardLines = TextBoxRawMaterialWidth.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //var boards = boardLines.Select(line => int.TryParse(line.Trim(), out var w) ? w : 0).Where(w => w > 0).ToList();
        //var boards = textBoxRawMaterialWidth;
        int.TryParse(TextBoxRawMaterialWidth, out int boards);
        _calculationStartTime = DateTime.Now;
        var sb = new StringBuilder();
        int totalWeightAll = 0, totalWasteAll = 0;
        double boardHeight = canvasHeight;
        double canvasWidth = this.canvasWidth;
        //var items = ListBoxInput.Items as IEnumerable<CutItem>
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
    
    

    
   
    
    class CutItemComparer : IEqualityComparer<CutItem>
    {
        public bool Equals(CutItem x, CutItem y)
        {
            return x.Code == y.Code && x.Width == y.Width && x.Weight == y.Weight;
        }

        public int GetHashCode(CutItem obj)
        {
            return obj.Code.GetHashCode() ^ obj.Width ^ obj.Weight;
        }
    }
    
    public void LoadCutItemsFromLocal()
    {
        try
        {
            var loaded = LocalStorageHelper.LoadCutItems();
            Items.Clear();
            foreach (var item in loaded)
                Items.Add(item);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LogText=e.Message;
            throw;
        }

    }

    public void SaveCutItemsToLocal()
    {
        
        LocalStorageHelper.SaveCutItems(Items.ToList());
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

public partial class CutItem : ObservableObject
{
    [ObservableProperty] public string code = "";
    [ObservableProperty] public int width;
    [ObservableProperty] public int weight;
    [ObservableProperty] public bool preferred;
    [ObservableProperty] public int maxCount;

    public CutItem()
    {
        code = "";
        width = 110;
        weight = 1;
        preferred = false;
        maxCount = 3;
    }
}



