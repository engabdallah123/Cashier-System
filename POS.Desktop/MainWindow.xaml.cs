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

            blazorWebView.BlazorWebViewInitialized += (sender, e) =>
            {
                if (blazorWebView.WebView?.CoreWebView2?.Settings != null)
                {
                    // Disable built-in Chromium browser shortcut keys (Ctrl+P print, Ctrl+S save, Ctrl+F find, etc.)
                    blazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                }
            };
        }
    }
}
