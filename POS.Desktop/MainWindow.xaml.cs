using Microsoft.AspNetCore.Components.WebView.Wpf;
using System.Windows;

namespace POS.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            blazorWebView.Services = App.Services;
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(AppRoutes)
            });
        }
    }
}
