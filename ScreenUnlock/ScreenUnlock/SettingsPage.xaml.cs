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

namespace ScreenUnlock
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        MainPage mainPage;

        public SettingsPage()
        {
            InitializeComponent();

            PhoneApplicationFrame frame = Application.Current.RootVisual as PhoneApplicationFrame;
            mainPage = frame.Content as MainPage;

            patternColor.Fill = new SolidColorBrush(mainPage.CenterBucket.Fill.Color);

            mainPage.CenterBucketColorChanged += delegate(Color color)
            {
                patternColor.Fill = new SolidColorBrush(color);
            };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (mainPage.greenYellowBucket.Fill.Color.Equals(Colors.Yellow))
            {
                colorModelListBox.SelectedItem = RYB;
            }
            else if (mainPage.greenYellowBucket.Fill.Color.Equals(Colors.Green))
            {
                colorModelListBox.SelectedItem = RGB;
            }

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainPage != null)
            {
                mainPage.ChangeColorModel((e.AddedItems[0] as ListBoxItem).Name.ToString());
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mainPage != null)
            {
                mainPage.RecordColorPattern();
            }
        }
    }
}