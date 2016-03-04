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

using Microsoft.Devices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Devices.Sensors;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Media;


namespace GestureUnlock
{
    public partial class MainPage : PhoneApplicationPage
    {
        public bool record = false;

        public long sensitivityThreshold = 100;

        public List<long> pattern;
        List<long> currentShakes;
        Stopwatch stopwatch;

        Brush CorrectBrush = new SolidColorBrush(Colors.White);
        Brush IncorrectBrush = new SolidColorBrush(Colors.Red);

        Accelerometer accelerometer;
        bool canReset = true;
       
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            accelerometer = new Accelerometer();
            accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(accelerometer_ReadingChanged);
            accelerometer.Start();

            pattern = new List<long>();
            currentShakes = new List<long>();

            stopwatch = new Stopwatch();

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
        }

        public void ShakeEventOccured()
        {
            VibrateController vibrate = VibrateController.Default;
            vibrate.Start(TimeSpan.FromSeconds(0.10));

            FlashRectangle(CorrectBrush);

            if (record)
            {
                pattern.Add(stopwatch.ElapsedMilliseconds);
            }
            else
            {
                currentShakes.Add(stopwatch.ElapsedMilliseconds);

                bool? test = CalculateIfCloseEnough(currentShakes, pattern, sensitivityThreshold);
                if (test.HasValue)
                {
                    if (test.Value == true)
                    {
                        UnlockPhone();
                        currentShakes = new List<long>();
                    }
                    else
                    {
                        //FlashRectangle(IncorrectBrush);
                    }


                }
            }         
        }

        private void UnlockPhone()
        {
            Stream stream = TitleContainer.OpenStream(@"Resources\Sound\wp7_lock.wav");
            SoundEffect effect = SoundEffect.FromStream(stream);
            FrameworkDispatcher.Update();
            effect.Play();

            VibrateController vibrate = VibrateController.Default;
            vibrate.Start(TimeSpan.FromSeconds(0.4));

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/HomeScreenPage.xaml", UriKind.Relative));
            });
        }

        void accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            if (e.Z > 1.3)
            {
                if (canReset)
                {
                    ShakeEventOccured();
                    canReset = false;
                }
            }
            else
            {
                canReset = true;
            }    
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            ShakeEventOccured();
        }

        public void recordButton_Click(object sender, RoutedEventArgs e)
        {
            VibrateController vibrate = VibrateController.Default;
            vibrate.Start(TimeSpan.FromSeconds(0.05));
            record = !record;
            if (record)
            {
                //recordButton.Content = "Stop";
                stopwatch.Start();

                pattern = new List<long>();
                
            }
            else
            {
                //recordButton.Content = "Record";
                stopwatch.Reset();
                stopwatch.Start();

                ReplayPattern(pattern);
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

        public bool? CalculateIfCloseEnough(List<long> list, List<long> pattern, long threshold)
        {
            if (list.Count < pattern.Count || pattern.Count == 0)
                return null;
            else
            {
                List<long> patternDifference = CreateDifferenceList(pattern, pattern.Count);
                List<long> testingDifference = CreateDifferenceList(list, pattern.Count);

                for (int i = 0; i < pattern.Count; i++)
                {
                    if (Math.Abs(patternDifference[i] - testingDifference[i]) < threshold)
                        continue;
                    else
                        return false;
                }

                return true;
            }
        }

        public List<long> CreateDifferenceList(List<long> list, int numberToLookBack)
        {
            List<long> returnList = new List<long>(numberToLookBack);
            returnList.Add(0);

            for (int i = numberToLookBack; i > 1; i--)
            {
                returnList.Add(list[list.Count - i + 1] - list[list.Count - i]);
            }

            return returnList;
        }

        public void ReplayPattern(List<long> pattern)
        {
            if (pattern.Count > 0)
            {
                long startValue = pattern[0];
                VibrateController vibrate = VibrateController.Default;

                Thread thread = new Thread(new ThreadStart(delegate()
                {
                    List<long> differenceList = CreateDifferenceList(pattern, pattern.Count);
                    for (int i = 0; i < pattern.Count; i++)
                    {
                        long sleepTime = differenceList[i];

                        System.Diagnostics.Debug.WriteLine(sleepTime.ToString());
                        Thread.Sleep(TimeSpan.FromMilliseconds(sleepTime));
                        vibrate.Start(TimeSpan.FromSeconds(0.10));
                        FlashRectangle(CorrectBrush);
                    }
                }));

                thread.Start();
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }
    }
}