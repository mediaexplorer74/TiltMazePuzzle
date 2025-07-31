using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TiltMazePuzzle.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.KeyDown += MainPage_KeyDown;
            LoadApplication(new TiltMazePuzzle.App());
        }

        private void MainPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            Xamarin.Forms.MessagingCenter.Send<object, string>(this, "KeyDown", e.Key.ToString());
            System.Diagnostics.Debug.WriteLine($"[DIAG] KeyDown (send via MessagingCenter): {e.Key.ToString()}");
        }
    }
}
