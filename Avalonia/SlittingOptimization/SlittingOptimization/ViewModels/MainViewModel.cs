using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlittingOptimization.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<CutItem> Items { get; } = new();

    public IRelayCommand AddItemCommand { get; }
    public IRelayCommand<CutItem> RemoveItemCommand { get; }

    public MainViewModel()
    {
        AddItemCommand = new RelayCommand(AddItem);
        RemoveItemCommand = new RelayCommand<CutItem>(RemoveItem);

        // 測試資料
        Items.Add(new CutItem { Code = "A", Width = 100, Weight = 2, Preferred = false, MaxCount = 3 });
    }

    private void AddItem() => Items.Add(new CutItem());
    private void RemoveItem(CutItem? item)
    {
        if (item != null)
            Items.Remove(item);
    }
    
    public partial class CutItem : ObservableObject
    {
        [ObservableProperty] private string code = "";
        [ObservableProperty] private int width;
        [ObservableProperty] private int weight;
        [ObservableProperty] private bool preferred;
        [ObservableProperty] private int maxCount;
    }
}


