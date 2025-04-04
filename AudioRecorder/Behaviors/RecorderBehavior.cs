using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using AudioRecorder.Views;
using AudioRecorder.Service;


namespace AudioRecorder.Behaviors
{
    public class RecorderBehavior : Behavior<RecorderView>
    {
        private RecorderView _recorder;
        private Window _window;

        public double CurrentPosition
        {
            get { return (double)GetValue(CurrentPositionProperty); }
            set { SetValue(CurrentPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register("CurrentPosition", typeof(double), typeof(RecorderBehavior), new PropertyMetadata(0d, OnCurrentPositionChanged));

        private static void OnCurrentPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RecorderBehavior)d;
            var recorder = ((RecorderBehavior)d).AssociatedObject;
            

            recorder.Dispatcher.Invoke(() =>
            {
                var position = (double)e.NewValue;
                recorder.CurrentPositionLine.X1 = recorder.ManipulationCanvas.ActualWidth * position;
                recorder.CurrentPositionLine.X2 = recorder.ManipulationCanvas.ActualWidth * position;
            });
        }

        public AudioState State
        {
            get { return (AudioState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(AudioState), typeof(RecorderBehavior), new PropertyMetadata(AudioState.Default, OnStateChanged));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var recorder = ((RecorderBehavior)d).AssociatedObject;  
            var state = (AudioState)e.NewValue;

            if (state == AudioState.Default)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Collapsed;

                recorder.SaveToFileButton.IsEnabled = false;
                recorder.OpenFileButton.IsEnabled = true;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Collapsed;
                recorder.ManipulationCanvas.IsHitTestVisible = false;

                recorder.TrimLeftButton.IsEnabled = false;
                recorder.TrimRightButton.IsEnabled = false;

                recorder.NormalizeAudioButton.IsEnabled = false;

                recorder.RecordButton.Visibility = Visibility.Visible;
                recorder.RecordButton.IsEnabled = true;

                recorder.StopRecordButton.Visibility = Visibility.Hidden;
                recorder.StopRecordButton.IsEnabled = false;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = false;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = false;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = false;
              

                recorder.LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (state == AudioState.Record)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Collapsed;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Collapsed;
                recorder.ManipulationCanvas.IsHitTestVisible = false;

                recorder.SaveToFileButton.IsEnabled = false;
                recorder.OpenFileButton.IsEnabled = false;

                recorder.TrimLeftButton.IsEnabled = false;
                recorder.TrimRightButton.IsEnabled = false;

                recorder.NormalizeAudioButton.IsEnabled = false;

                recorder.RecordButton.Visibility = Visibility.Hidden;
                recorder.RecordButton.IsEnabled = false;

                recorder.StopRecordButton.Visibility = Visibility.Visible;
                recorder.StopRecordButton.IsEnabled = true;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = false;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = false;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = false;

                recorder.LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (state == AudioState.Pause)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Visible;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Visible;
                recorder.ManipulationCanvas.IsHitTestVisible = true;

                recorder.SaveToFileButton.IsEnabled = true;
                recorder.OpenFileButton.IsEnabled = true;

                recorder.TrimLeftButton.IsEnabled = true;
                recorder.TrimRightButton.IsEnabled = true;

                recorder.NormalizeAudioButton.IsEnabled = true;

                recorder.RecordButton.Visibility = Visibility.Visible;
                recorder.RecordButton.IsEnabled = true;

                recorder.StopRecordButton.Visibility = Visibility.Hidden;
                recorder.StopRecordButton.IsEnabled = false;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = true;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = true;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = true;

                recorder.LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (state == AudioState.Play)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Visible;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Visible;
                recorder.ManipulationCanvas.IsHitTestVisible = true;

                recorder.SaveToFileButton.IsEnabled = false;
                recorder.OpenFileButton.IsEnabled = true;

                recorder.TrimLeftButton.IsEnabled = false;
                recorder.TrimRightButton.IsEnabled = false;

                recorder.NormalizeAudioButton.IsEnabled = false;

                recorder.RecordButton.Visibility = Visibility.Visible;
                recorder.RecordButton.IsEnabled = false;

                recorder.StopRecordButton.Visibility = Visibility.Hidden;
                recorder.StopRecordButton.IsEnabled = false;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = true;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = true;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = true;

                recorder.LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (state == AudioState.Stop)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Collapsed;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Visible;
                recorder.ManipulationCanvas.IsHitTestVisible = true;

                recorder.SaveToFileButton.IsEnabled = true;
                recorder.OpenFileButton.IsEnabled = true;

                recorder.TrimLeftButton.IsEnabled = true;
                recorder.TrimRightButton.IsEnabled = true;

                recorder.NormalizeAudioButton.IsEnabled = true;

                recorder.RecordButton.Visibility = Visibility.Visible;
                recorder.RecordButton.IsEnabled = true;

                recorder.StopRecordButton.Visibility = Visibility.Hidden;
                recorder.StopRecordButton.IsEnabled = false;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = true;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = false;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = true;

                recorder.LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (state == AudioState.Loading)
            {
                recorder.CurrentPositionLine.Visibility = Visibility.Collapsed;

                recorder.PlaybackStartPositionLine.Visibility = Visibility.Collapsed;
                recorder.ManipulationCanvas.IsHitTestVisible = false;

                recorder.SaveToFileButton.IsEnabled = false;
                recorder.OpenFileButton.IsEnabled = false;

                recorder.TrimLeftButton.IsEnabled = false;
                recorder.TrimRightButton.IsEnabled = false;

                recorder.NormalizeAudioButton.IsEnabled = false;

                recorder.RecordButton.Visibility = Visibility.Visible;
                recorder.RecordButton.IsEnabled = false;

                recorder.StopRecordButton.Visibility = Visibility.Hidden;
                recorder.StopRecordButton.IsEnabled = false;

                recorder.PlayButton.Visibility = Visibility.Visible;
                recorder.PlayButton.IsEnabled = false;

                recorder.PauseButton.Visibility = Visibility.Collapsed;
                recorder.PauseButton.IsEnabled = false;

                recorder.StopButton.Visibility = Visibility.Visible;
                recorder.StopButton.IsEnabled = false;

                recorder.RewindButton.Visibility = Visibility.Visible;
                recorder.RewindButton.IsEnabled = false;

                recorder.LoadingProgressBar.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Relative start position from 0 to 1
        /// </summary>
        public double StartPlaybackPosition
        {
            get { return (double)GetValue(StartPlaybackPositionProperty); }
            set { SetValue(StartPlaybackPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartPlaybackPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartPlaybackPositionProperty =
            DependencyProperty.Register("StartPlaybackPosition", typeof(double), typeof(RecorderBehavior), new PropertyMetadata(0d, OnStartPlaybackPositionChanged));

        private static void OnStartPlaybackPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var recorder = ((RecorderBehavior)d).AssociatedObject;
            recorder.PlaybackStartPositionLine.X1 = (double)e.NewValue * recorder.ManipulationCanvas.ActualWidth;
            recorder.PlaybackStartPositionLine.X2 = (double)e.NewValue * recorder.ManipulationCanvas.ActualWidth;

        }


        public ObservableCollection<Point> Points
        {
            get { return (ObservableCollection<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(ObservableCollection<Point>), typeof(RecorderBehavior), new PropertyMetadata(null, OnPointsChanged));


        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (RecorderBehavior)d;
            var recorder = ((RecorderBehavior)d).AssociatedObject;
            var polyline = recorder.AudioGraph;
            var points = (ObservableCollection<Point>)e.NewValue;

            if (points is null) return;

            polyline.Points.Clear();

            foreach (var point in points)
            {
                //если конец списка, растягиваем
                if (point.X == -1)
                {
                    behavior.StretchGraph(polyline);
                    break;
                }
                behavior.InsertPointToGraph(polyline, point.Y);
            }


            points.CollectionChanged += (s, a) =>
            {
                polyline.Dispatcher.Invoke(() =>
                {
                    if (a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        foreach (Point point in a.NewItems)
                        {
                            //если конец списка, растягиваем
                            if (point.X == -1)
                            {
                                behavior.StretchGraph(polyline);
                                return;
                            }
                            behavior.InsertPointToGraph(polyline, point.Y);


                            var x = polyline.Points.Count / 2;

                            if (x >= recorder.AudioGraphGrid.ActualWidth)
                            {
                                behavior.StretchGraph(polyline);
                            }
                            else
                            {
                                behavior.ResetGraphScale(polyline);
                            }
                        }
                    }
                    else if (a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                    {
                        polyline.Points.Clear();
                    }
                });

            };


        }

        
        protected override void OnAttached()
        {
            _recorder = AssociatedObject;

            _recorder.Loaded += OnLoaded;
            _recorder.Unloaded += OnUnloaded;  
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //_window = _recorder.FindVisualParent<Window>();
            //_window.Unloaded += OnWindowUnloaded;

            _recorder.ManipulationCanvas.MouseDown += ManipulationCanvasMouseDown;
            //костыль, нужен, так как не сразабывает stretch при загрузке новой коллекции, из-за того что рекордер выгружен
            if (_recorder.AudioGraph.Points.Count > 0)
                StretchGraph(_recorder.AudioGraph);
        }



        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //_recorder.Loaded -= OnLoaded;
            //_recorder.Unloaded -= OnUnloaded;
        }

        private void ManipulationCanvasMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(_recorder.ManipulationCanvas);
            StartPlaybackPosition = position.X / _recorder.ManipulationCanvas.ActualWidth;
        }


        private void InsertPointToGraph(Polyline polyline, double y)
        {
            polyline.Dispatcher.Invoke(() =>
            {
                var x = polyline.Points.Count / 2;
                var height = polyline.Height / 2;

                var top_height = height - height * y;
                var bottom_height = height + height * y;

                var top_point = new Point(x, top_height);
                var bottom_point = new Point(x, bottom_height);
                polyline.Points.Add(top_point);
                polyline.Points.Insert(0, bottom_point);
            });

        }

        private async void StretchGraph(Polyline polyline)
        {
            polyline.Dispatcher.Invoke(() =>
            {
                var scale_transform = (ScaleTransform)polyline.LayoutTransform;
                //scale_transform.ScaleX = AudioGraphGrid.ActualWidth / AudioGraph.ActualWidth;
                scale_transform.ScaleX = _recorder.AudioGraphGrid.ActualWidth / ((polyline.Points.Count - 1) / 2);
            });

        }

        private async void ResetGraphScale(Polyline polyline)
        {

            polyline.Dispatcher.Invoke(() =>
            {
                var scale_transform = (ScaleTransform)polyline.LayoutTransform;
                scale_transform.ScaleX = 1;
            });
        }

        //private void OnWindowUnloaded(object sender, RoutedEventArgs e)
        //{
        //    _window.Unloaded -= OnWindowUnloaded;
        //    _recorder.Loaded -= OnLoaded;
        //    _recorder.Unloaded -= OnUnloaded;

        //}
    }
}
