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

namespace ScreenUnlock
{
    public partial class PaintBucket : UserControl
    {
        public event RoutedEventHandler Click;

        public PaintBucket()
        {
            InitializeComponent();
        }

        public SolidColorBrush Fill
        {
            get
            {
                return fillRectangle.Fill as SolidColorBrush;
            }
            set
            {
                fillRectangle.Fill = value;
            }
        }

        public void ResetClickCount()
        {
            clickCount.Text = "0";
            clickCount.Opacity = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                clickCount.Text = (Convert.ToInt16(clickCount.Text) + 1).ToString();
                clickCount.Opacity = 1;

                outlineImage.Opacity = 1;

                Storyboard sb = new Storyboard();
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 1;
                animation.To = 0;

                animation.Duration = TimeSpan.FromSeconds(0.25);
                Storyboard.SetTarget(animation, outlineImage);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Image.Opacity)"));

                sb.Children.Add(animation);
                sb.Begin();

                Click(this, e);
            }
        }
    }
}
