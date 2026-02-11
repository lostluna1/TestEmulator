using System.Globalization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using TestEmulator.Activation;
using TestEmulator.Contracts.Services;
using TestEmulator.Core.Contracts.Services;
using TestEmulator.Core.Services;
using TestEmulator.Helpers;
using TestEmulator.Models;
using TestEmulator.Services;
using TestEmulator.ViewModels;
using TestEmulator.Views;

namespace TestEmulator;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();
        // TODO: Replace with your app's default culture code
        var culture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;


        UnhandledException += App_UnhandledException;
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        // Example: Show a dialog that an unhandled exception occurred
        e.Handled = true;

        // Check if XamlRoot is available (window might not be initialized during app startup)
        if (App.MainWindow?.Content?.XamlRoot == null)
        {
            // TODO: Log the exception when XamlRoot is not available
            // For example: Logger.Error(e.Exception, "Unhandled exception during app startup");
            return;
        }

        var errorMessage = string.Format(
            "UnhandledExceptionDialog_ErrorMessage".GetLocalized(),
            e.Message,
            e.Exception.StackTrace);

        var dialog = new ContentDialog()
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Title = "UnhandledExceptionDialog_Title".GetLocalized(),
            Content = new TextBlock()
            {
                Text = errorMessage,
                TextWrapping = TextWrapping.Wrap
            },
            PrimaryButtonText = "UnhandledExceptionDialog_CopyButton".GetLocalized(),
            CloseButtonText = "UnhandledExceptionDialog_CloseButton".GetLocalized(),
            DefaultButton = ContentDialogButton.Primary
        };

        dialog.PrimaryButtonClick += (s, args) =>
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(errorMessage);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        };

        await dialog.ShowAsync();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
