using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.Silverlight.Windows;

namespace SilverlightRemoteController
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            App.Current.InstallStateChanged += CurrentInstallStateChanged;
            InitializeComponent();
        }

        private void CurrentInstallStateChanged(object sender, EventArgs e)
        {
            if (Current.InstallState == InstallState.Installed)
            {
                RootVisual = new AlreadyInstalled();
            }

            if (Current.InstallState == InstallState.NotInstalled)
            {
                RootVisual = new PromptInstall();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (App.Current.InstallState != InstallState.Installed)
            {
                RootVisual = new PromptInstall();
            }
            else
            {
                if (App.Current.InstallState == InstallState.Installed && !App.Current.IsRunningOutOfBrowser)
                {
                    RootVisual = new AlreadyInstalled();
                }
                else
                {
                    if (Installer.CheckNESLInstalled(1, 0))
                    {
                        RootVisual = new MainPage();
                    }
                    else
                    {
                        MessageBox.Show("Native Extensions For Microsoft Silverlight Is Not Installed");
                        Installer.InstallNESL(new Uri("NESLSetup.msi", UriKind.Relative), false);

                        if (Installer.CheckNESLInstalled(1, 0))
                        {
                            RootVisual = new MainPage();
                        }
                        else
                        {
                            MessageBox.Show("Native Extensions For Microsoft Silverlight Could Not Be Installed");
                        }
                    }
                }
            }
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
