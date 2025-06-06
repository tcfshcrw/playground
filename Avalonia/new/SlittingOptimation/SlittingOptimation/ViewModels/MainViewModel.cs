using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlittingOptimization.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<CutItem> Items { get; } = new();

    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand RemoveLastItemCommand { get; }

    public MainViewModel()
    {
        AddItemCommand = new RelayCommand(AddItem);
        RemoveLastItemCommand = new RelayCommand(RemoveLastItem, CanRemoveLastItem);

        // initialize default data
        Items.Add(new CutItem { Code = "A", Width = 140, Weight = 1, Preferred = false, MaxCount = 3 });
        Items.Add(new CutItem { Code = "B", Width = 160, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "C", Width = 180, Weight = 1, Preferred = false, MaxCount = 4 });
        Items.Add(new CutItem { Code = "D", Width = 200, Weight = 1, Preferred = false, MaxCount = 4 });
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


