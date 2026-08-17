using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using POS.Desktop.Services.Api;
using POS.Desktop.Services.Auth;
using POS.Desktop.Services.Printing;
using POS.Desktop.Services.State;
using System.Net.Http;
using System.Windows;

namespace POS.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();

            serviceCollection.AddSingleton<CustomAuthStateProvider>();
            serviceCollection.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
            serviceCollection.AddAuthorizationCore();

            serviceCollection.AddTransient<BearerTokenHandler>();

            var createHandler = () => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Register untyped HttpClient for pages using @inject HttpClient
            serviceCollection.AddHttpClient("", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7198/");
            })
            .ConfigurePrimaryHttpMessageHandler(createHandler)
            .AddHttpMessageHandler<BearerTokenHandler>();

            serviceCollection.AddHttpClient<PosApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7198/");
            })
            .ConfigurePrimaryHttpMessageHandler(createHandler)
            .AddHttpMessageHandler<BearerTokenHandler>();

            serviceCollection.AddHttpClient<IInvoicePrinterService, QuestPdfInvoicePrinter>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7198/");
            })
            .ConfigurePrimaryHttpMessageHandler(createHandler)
            .AddHttpMessageHandler<BearerTokenHandler>();

            serviceCollection.AddSingleton<ShiftStateContainer>();
            serviceCollection.AddSingleton<CartStateContainer>();
            serviceCollection.AddSingleton<StoreStateContainer>();
            serviceCollection.AddSingleton<CalculatorStateContainer>();

            Services = serviceCollection.BuildServiceProvider();
        }
    }
}
