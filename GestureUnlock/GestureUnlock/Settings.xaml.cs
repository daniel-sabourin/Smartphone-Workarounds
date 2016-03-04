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
using Microsoft.Phone.Controls;

namespace GestureUnlock
{
    public partial class Settings : PhoneApplicationPage
    {
        MainPage mainPage;

        public Settings()
        {
            InitializeComponent();

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            mainPage = frame.Content as MainPage;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            long sensitivity = mainPage.sensitivityThreshold;

            switch (sensitivity)
            {
                case 50:
                    lowRadio.IsChecked = true;
                    break;
                case 100:
                    mediumRadio.IsChecked = true;
                    break;
                case 250:
                    highRadio.IsChecked = true;
                    break;
            }

        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();

            if (mainPage.record)
            {
                recordButton.Content = "Record";
                animation.From = 1;
                animation.To = 0;
            }
            else
            {
                recordButton.Content = "Stop";
                animation.From = 0;
                animation.To = 1;
            }

            animation.Duration = TimeSpan.FromSeconds(0.25);
            Storyboard.SetTarget(animation, tapButton);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Button.Opacity)"));

            sb.Children.Add(animation);
            sb.Begin();

            mainPage.recordButton_Click(sender, e);
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.ReplayPattern(mainPage.pattern);
        }

        private void tapButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.ShakeEventOccured();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (mainPage != null)
            {
                if (rb.Content.ToString().Contains("Low"))
                    mainPage.sensitivityThreshold = 50;
                else if (rb.Content.ToString().Contains("Medium"))
                    mainPage.sensitivityThreshold = 100;
                else if (rb.Content.ToString().Contains("High"))
                    mainPage.sensitivityThreshold = 250;
            }
        }
    }
}