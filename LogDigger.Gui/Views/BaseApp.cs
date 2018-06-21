using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using LogDigger.Business.Models;
using LogDigger.Business.Services;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogStructure;
using LogDigger.Gui.ViewModels.MainWindow;
using ReactiveUI;
using SimpleLogger;
using SimpleLogger.Logging;
using SimpleLogger.Logging.Formatters;
using SimpleLogger.Logging.Handlers;
using TCD.System.TouchInjection;

namespace LogDigger.Gui.Views
{
    public class BaseApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Resources = new ResourceDictionary
            {
                Source = new Uri("/LogDigger.Gui;component/Views/BaseDictionary.xaml", UriKind.Relative)
            };
            foreach (var rd in BuildResourceDictionaries())
            {
                Resources.MergedDictionaries.Add(rd);
            }

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            TouchInjector.InitializeTouchInjection();

            DispatcherUnhandledException += OnUnhandledException;

            Logger.LoggerHandlerManager
                .AddHandler(new ConsoleLoggerHandler())
                .AddHandler(new FileLoggerHandler(AppUtils.LogFileName))
                .AddHandler(new DebugConsoleLoggerHandler());

            BuildTypeViewsInjection();
            var container = BuildDependencies();

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.DataContext = container.Resolve<MainWindowVm>(); // manually solve dependency
            mainWindow.Show();

            var mainWindowVm = container.Resolve<MainWindowVm>();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var clickOnceArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments?.ActivationData;
                if (clickOnceArgs?.Any() == true)
                {
                    mainWindowVm.LogPath = clickOnceArgs[0];
                    mainWindowVm.ParseCommand.Execute(null);
                }
            }
            else if (e.Args.Any())
            {
                mainWindowVm.LogPath = e.Args[0];
                mainWindowVm.ParseCommand.Execute(null);
            }

            base.OnStartup(e);

            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                MessageBus.Current.SendMessage(new ChangeLogVm());
            }
        }

        protected virtual IEnumerable<ResourceDictionary> BuildResourceDictionaries()
        {
            yield break;
        }

        protected virtual IContainer BuildDependencies()
        {
            var builder = new ContainerBuilder();

            RegisterParser(builder);
            builder.RegisterType<CharSpanParser>().As<ILogParser>().SingleInstance();
            builder.RegisterType<ClosingAppHandler>().As<IClosingAppHandler>().SingleInstance();
            builder.RegisterType<LogExtractionService>().As<ILogExtractionService>().SingleInstance();
            RegisterModuleClassifier(builder);
            RegisterMainWindowVm(builder);
            RegisterMainWindow(builder);
            builder.RegisterType<CharSpanParser>()
                .As<ILogParser>()
                .SingleInstance();
            builder.RegisterType<GuiScheduler>()
                .As<IGuiScheduler>()
                .SingleInstance();
            RegisterLogStructure(builder);

            return builder.Build();
        }

        protected virtual void RegisterLogStructure(ContainerBuilder builder)
        {
            builder.RegisterType<LogStructureVm>().As<ILogStructureVm>().SingleInstance();
        }

        protected virtual void RegisterModuleClassifier(ContainerBuilder builder)
        {
            builder.RegisterType<ModuleClassifier>().As<IModuleClassifier>().SingleInstance();
        }

        protected virtual void RegisterMainWindowVm(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindowVm>()
                .As<MainWindowVm>()
                .As<INavigator>()
                .SingleInstance();
        }

        protected virtual void RegisterParser(ContainerBuilder builder)
        {
            builder.RegisterType<CustomParserSelector>().As<IParserSelector>().SingleInstance();
        }

        protected virtual void RegisterMainWindow(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>()
                .As<MainWindow>()
                .As<IModalHandler>()
                .SingleInstance();
        }

        private void BuildTypeViewsInjection()
        {
            var inject = new AttributeViewInjector(GetLookupAssemblies().ToArray());
            inject.Init();
            Resources.MergedDictionaries.Add(inject.ResourceDictionary);
        }

        protected virtual IEnumerable<Assembly> GetLookupAssemblies()
        {
            yield return typeof(BaseApp).Assembly;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            GlobalMessageQueue.Current.Enqueue($"A crash occured. See logs for more details: {AppUtils.LogFilePath}");
            Logger.Log(e.Exception);
            Logger.Log<BaseApp>(e.Exception);
            e.Handled = true;
        }
    }
}