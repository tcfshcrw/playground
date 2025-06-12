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
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        if (DataContext is MainViewModel vm)
            vm.LoadCutItemsFromLocal();
    }
    
}