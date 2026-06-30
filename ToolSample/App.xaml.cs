using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ToolSample.Services;
using ToolSample.ViewModels;

namespace ToolSample;

/// <summary>
///     애플리케이션 진입점. DI 컨테이너를 구성하고 메인 윈도우를 표시한다.
///     Application entry point. Configures DI container and shows main window.
/// </summary>
public partial class App : Application {
    /// <summary>
    ///     전역 서비스 프로바이더.
    ///     Global service provider.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e) {
        // 기본 시작 처리
        // base startup handling
        base.OnStartup(e);

        // DI 컨테이너 구성
        // configure DI container
        var services = new ServiceCollection();

        // 싱글턴 서비스 등록
        // register singleton services
        services.AddSingleton<HToolService>();
        // 디스패처 서비스 등록
        // register dispatcher service
        services.AddSingleton<IDispatcherService, DispatcherService>();

        // ViewModel 등록 (페이지 전환 시 새 인스턴스)
        // register ViewModels (new instance per navigation)
        services.AddTransient<MainViewModel>();
        // 연결 페이지 ViewModel 등록
        // register connection page ViewModel
        services.AddTransient<ConnectionViewModel>();
        // 레지스터 페이지 ViewModel 등록
        // register register page ViewModel
        services.AddTransient<RegisterViewModel>();
        // 이벤트 페이지 ViewModel 등록
        // register event page ViewModel
        services.AddTransient<EventViewModel>();
        // 그래프 페이지 ViewModel 등록
        // register graph page ViewModel
        services.AddTransient<GraphViewModel>();
        // 로그 페이지 ViewModel 등록
        // register log page ViewModel
        services.AddTransient<LogViewModel>();
        // PRO X 페이지 ViewModel 등록
        // register PRO X page ViewModel
        services.AddTransient<ProViewModel>();

        // 서비스 프로바이더 빌드
        // build service provider
        Services = services.BuildServiceProvider();

        // 메인 윈도우 생성 및 DataContext 설정
        // create main window and set DataContext
        var main = new MainWindow {
            // MainViewModel을 DataContext로 설정
            // set MainViewModel as DataContext
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        // 메인 윈도우 표시
        // show main window
        main.Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e) {
        // HToolService 해제
        // dispose HToolService
        if (Services is IDisposable disposable)
            // ServiceProvider 해제 (모든 싱글턴 포함)
            // dispose ServiceProvider (includes all singletons)
            disposable.Dispose();

        // 기본 종료 처리
        // base exit handling
        base.OnExit(e);
    }
}