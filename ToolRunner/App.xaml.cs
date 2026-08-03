using System.Windows;
using ToolRunner.Services;
using ToolRunner.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ToolRunner;

/// <summary>
///     application entry point. builds the DI container, resolves the single tester window and
///     disposes the container (and with it the tool connection) on exit.
///     애플리케이션 진입점 — DI 구성 및 창 표시
/// </summary>
public partial class App : Application {
    // service provider owning every singleton for the lifetime of the app
    private IServiceProvider? _services;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e) {
        // default startup handling
        base.OnStartup(e);

        // collect the service registrations
        var services = new ServiceCollection();

        // one tool connection for the whole app
        services.AddSingleton<HToolService>();
        // one sequence loop driver
        services.AddSingleton<SequenceRunner>();
        // settings store that keeps the operator input across restarts
        services.AddSingleton<SettingsStore>();
        // UI-thread dispatcher used by every background event handler
        services.AddSingleton<IDispatcherService, DispatcherService>();
        // the single view model behind the window
        services.AddSingleton<MainViewModel>();
        // the window itself, so its dependencies are injected
        services.AddSingleton<MainWindow>();

        // build the container
        _services = services.BuildServiceProvider();

        // resolve and show the tester window
        _services.GetRequiredService<MainWindow>().Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e) {
        // disposing the container closes the socket and cancels a loop still in flight
        if (_services is IDisposable disposable)
            // release every singleton
            disposable.Dispose();

        // default exit handling
        base.OnExit(e);
    }
}
