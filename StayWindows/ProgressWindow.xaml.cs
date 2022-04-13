using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StayQL.StayWindows
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : MetroWindow
    {
        private int TotalSteps = 1;
        private int CompletedSteps = 0;
        public ProgressWindow(int ts, string t)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.IsCloseButtonEnabledWithDialog = false;
            this.IsCloseButtonEnabled = false;
            this.ResizeMode = ResizeMode.NoResize;
            CurrentlyWorkingLabel.Content = "";
            this.Title = t;
            this.TotalSteps = ts;

        }

        public void Stop()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
            }));
            
        }

        public static ProgressWindow ShowProgress(String Title, int TotalSteps)
        {
            ProgressWindow pw = null;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                pw = new ProgressWindow(TotalSteps, Title);
                pw.Show();
            }));
            Thread.Sleep(25);
            return pw;
        }

        private void UpdateProgressBar(){

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                float Target = ((CompletedSteps * 1.0f) / (TotalSteps * 1.0f)) * 100.0f;
                DoubleAnimation animation = new DoubleAnimation(Target, TimeSpan.FromMilliseconds(250));
                ProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
                StepsLabel.Content = $"{CompletedSteps}/{TotalSteps}";
            }));
        }

        public void AddProgress(int CompletedSteps)
        {
            this.CompletedSteps+= CompletedSteps;
            UpdateProgressBar();
        }

        public void SetProgress(int CompletedSteps)
        {
            this.CompletedSteps = CompletedSteps;
            UpdateProgressBar();
        }

        public void SetTotalProgress(int TotalSteps)
        {
            this.TotalSteps = TotalSteps;
            UpdateProgressBar();
        }

        public void AddTotalProgress(int TotalSteps)
        {
            this.TotalSteps += TotalSteps;
            UpdateProgressBar();
        }

        public void SetCurrentlyWorkingLabel(string Job)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CurrentlyWorkingLabel.Content = Job;
                UpdateProgressBar();

            }));
        }

    }
}
