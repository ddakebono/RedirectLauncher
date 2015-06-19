using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MetroRadiance;

namespace RedirectLauncherMk2_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeService.Current.Initialize(this, Theme.Dark, Accent.Purple);
        }
    }
}
