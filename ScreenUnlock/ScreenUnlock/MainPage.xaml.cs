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

using Microsoft.Devices.Sensors;
using Microsoft.Devices;
using System.IO;

namespace ScreenUnlock
{
    public partial class MainPage : PhoneApplicationPage
    {
        Accelerometer accelerometer;
        bool canReset = true;

        int rTotal = 0;
        int gTotal = 0;
        int bTotal = 0;
        int totalBuckets = 0;

        private Color colorPattern;
        public Color ColorPattern
        {
            get { return colorPattern; }
            set
            {
                colorPattern = value;
                //colorPatternRectangle.Fill = new SolidColorBrush(value);
            }
        }

        public delegate void ChangedColorHandler(Color color);
        public event ChangedColorHandler CenterBucketColorChanged;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            accelerometer = new Accelerometer();
            accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(accelerometer_ReadingChanged);
            accelerometer.Start();

            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            myDispatcherTimer.Interval = new TimeSpan(0, 0, 1, 0, 0); // 1 Minute
            myDispatcherTimer.Tick += delegate(object o, EventArgs sender)
            {
                UpdateTime(DateTime.Now);
            };
            myDispatcherTimer.Start();
        }

        public void UpdateTime(DateTime dateTime)
        {
            DayWeekTextBlock.Text = dateTime.DayOfWeek.ToString();
            MonthDateTextBlock.Text = dateTime.ToString("MMMM") + " " + dateTime.Day;
            TimeTextBlock.Text = dateTime.ToShortTimeString();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateTime(DateTime.Now);
            ResetCenterBucket();
        }



        void accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => ThreadSafeAccelerometerChanged(e));
        }

        void ThreadSafeAccelerometerChanged(AccelerometerReadingEventArgs e)
        {
            if (e.Z > 0.75)
            {
                if (canReset)
                {
                    VibrateController vibrate = VibrateController.Default;
                    vibrate.Start(TimeSpan.FromSeconds(0.15));

                    ResetCenterBucket();
                    canReset = false;
                }
            }
            else
            {
                canReset = true;
            }
        }

        private void PaintBucket_Click(object sender, RoutedEventArgs e)
        {
            VibrateController vibrate = VibrateController.Default;
            vibrate.Start(TimeSpan.FromSeconds(0.05));

            Color color = (sender as PaintBucket).Fill.Color;
            CenterBucket.Fill = new SolidColorBrush(AddToColorAverage(color));

            if (CenterBucketColorChanged != null)
                CenterBucketColorChanged(CenterBucket.Fill.Color);
        }

        private void CenterBucket_Click(object sender, RoutedEventArgs e)
        {
            CenterBucket.ResetClickCount();

            if (TestPattern(ColorPattern, CenterBucket.Fill.Color))
            {
                UnlockPhone();
            }
            else
            {
                FlashRectangle(new SolidColorBrush(Colors.Red));
            }
        }

        public void FlashRectangle(Brush fillColor)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                flashRectangle.Fill = fillColor;

                Storyboard sb = new Storyboard();
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 0;
                animation.To = 0.75;
                animation.AutoReverse = true;
                animation.Duration = TimeSpan.FromSeconds(0.1);
                Storyboard.SetTarget(animation, flashRectangle);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Rectangle.Opacity)"));

                sb.Children.Add(animation);
                sb.Begin();
            });
        }

        private void UnlockPhone()
        {
            Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(@"Resources\Sound\wp7_lock.wav");
            Microsoft.Xna.Framework.Audio.SoundEffect effect = Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(stream);
            Microsoft.Xna.Framework.FrameworkDispatcher.Update();
            effect.Play();

            VibrateController vibrate = VibrateController.Default;
            vibrate.Start(TimeSpan.FromSeconds(0.4));

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/HomeScreenPage.xaml", UriKind.Relative));
            });
        }

        public bool TestPattern(Color pattern, Color testingColor)
        {
            return pattern.Equals(testingColor);
        }

        private Color AddToColorAverage(Color color)
        {
            totalBuckets += 1;
            rTotal += color.R;
            gTotal += color.G;
            bTotal += color.B;

            byte newR = (byte) (rTotal / totalBuckets);
            byte newG = (byte)(gTotal / totalBuckets);
            byte newB = (byte)(bTotal / totalBuckets);

            return Color.FromArgb(255, newR, newG, newB);
        }

        private void ResetCenterBucket()
        {
            rTotal = 0;
            gTotal = 0;
            bTotal = 0;
            totalBuckets = 0;

            RedBucket.ResetClickCount();
            BlueBucket.ResetClickCount();
            greenYellowBucket.ResetClickCount();

            CenterBucket.Fill = new SolidColorBrush(Colors.White);

            if (CenterBucketColorChanged != null)
                CenterBucketColorChanged(CenterBucket.Fill.Color);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));

        }

        public void ChangeColorModel(string model)
        {
            if (model.Equals("RGB"))
                greenYellowBucket.Fill = new SolidColorBrush(Colors.Green);
            else if (model.Equals("RYB"))
                greenYellowBucket.Fill = new SolidColorBrush(Colors.Yellow);
        }

        public Color RecordColorPattern()
        {
            ColorPattern = CenterBucket.Fill.Color;
            return ColorPattern;
        }
    }
}