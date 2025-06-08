using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace Optimization.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<CutItem> Items { get; } = new();
    

    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveLastItemCommand { get; }
    [ObservableProperty] private string debugtext;

    [ObservableProperty] private string detailResultText;
    [ObservableProperty] private string logText;
    [ObservableProperty] private bool isResultEditable;
    [ObservableProperty] private bool isLogEditable;
    public MainViewModel()
    {
        AddItemCommand = new RelayCommand(AddItem);
        RemoveLastItemCommand = new RelayCommand(RemoveLastItem, CanRemoveLastItem);
        // initialize default data
        Items.Add(new CutItem { Code = "A", Width = 140, Weight = 1, Preferred = false, MaxCount = 3 });
        Items.Add(new CutItem { Code = "B", Width = 160, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "C", Width = 180, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "D", Width = 200, Weight = 1, Preferred = false, MaxCount = 4 });
        //Squares.Add(new Square { X = 20, Y = 20, Size = 100, Fill = Brushes.Blue });
        IsLogEditable = false;
        IsResultEditable = false;
    }

    private void AddItem() => Items.Add(new CutItem());
    void RemoveLastItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
    bool CanRemoveLastItem() => Items.Count > 0;
    
    

    
       
    
    public partial class CutItem : ObservableObject
    {
        [ObservableProperty] private string code = "";
        [ObservableProperty] private int width;
        [ObservableProperty] private int weight;
        [ObservableProperty] private bool preferred;
        [ObservableProperty] private int maxCount;
    }
    
    
}


