using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioRecorder.Controls
{
    public class NumericControl : ContentControl
    {

        private double _initY;
        private int _initialValue;
        private bool _ctrlPressed = false;


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NumericControl), new PropertyMetadata(default(CornerRadius)));





        public Visibility MouseOverBackgroundVisibility
        {
            get { return (Visibility)GetValue(MouseOverBackgroundVisibilityProperty); }
            set { SetValue(MouseOverBackgroundVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MouseOverBackgroundVisibilityProperty =
            DependencyProperty.Register("MouseOverBackgroundVisibility", typeof(Visibility), typeof(NumericControl), new PropertyMetadata(Visibility.Visible));




        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(NumericControl), new PropertyMetadata(int.MinValue));


        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericControl), new PropertyMetadata(int.MaxValue));


        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericControl), new PropertyMetadata(0));




        public int Frequency
        {
            get { return (int)GetValue(FrequencyProperty); }
            set { SetValue(FrequencyProperty, value); }
        }

        public static readonly DependencyProperty FrequencyProperty =
            DependencyProperty.Register("Frequency", typeof(int), typeof(NumericControl), new PropertyMetadata(1));


        public int AlternativeFrequency
        {
            get { return (int)GetValue(AlternativeFrequencyProperty); }
            set { SetValue(AlternativeFrequencyProperty, value); }
        }

        public static readonly DependencyProperty AlternativeFrequencyProperty =
            DependencyProperty.Register("AlternativeFrequency", typeof(int), typeof(NumericControl), new PropertyMetadata(10));


        public int Multiplier
        {
            get { return (int)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        public static readonly DependencyProperty MultiplierProperty =
            DependencyProperty.Register("Multiplier", typeof(int), typeof(NumericControl), new PropertyMetadata(5));




        public int DefaultValue
        {
            get { return (int)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(int), typeof(NumericControl), new PropertyMetadata(0, OnDefaultValueChanged));

        private static void OnDefaultValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public MouseButton MouseButton
        {
            get { return (MouseButton)GetValue(MouseButtonProperty); }
            set { SetValue(MouseButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseButtonProperty =
            DependencyProperty.Register("MouseButton", typeof(MouseButton), typeof(NumericControl), new PropertyMetadata(MouseButton.Left));




        static NumericControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericControl), new FrameworkPropertyMetadata(typeof(NumericControl)));

        }

        public NumericControl()
        {
            this.PreviewMouseDown += OnMouseDown;
            this.PreviewMouseDoubleClick += OnMouseDoubleClick;
            this.PreviewMouseWheel += OnMouseWheel;
            App.Current.MainWindow.KeyUp += OnKeyUp;
            App.Current.MainWindow.KeyDown += OnKeyDown;
        }


        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) _ctrlPressed = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) _ctrlPressed = true;
        }


        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton)
            {
                Value = GetValue(DefaultValue);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton)
            {
                e.Handled = true;

                _initY = e.GetPosition(this).Y;
                _initialValue = Value;


                this.CaptureMouse();
                this.PreviewMouseMove += OnMouseMove;
                this.PreviewMouseUp += OnMouseUp;

                this.Cursor = Cursors.Hand;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton)
            {
                this.PreviewMouseMove -= OnMouseMove;
                this.PreviewMouseUp -= OnMouseUp;

                this.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNS;

            var freq = _ctrlPressed ? AlternativeFrequency : Frequency;

            var delta = (int)((_initY - e.GetPosition(this).Y) / 20);

            Value = GetValue(_initialValue + delta * freq);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var freq = _ctrlPressed ? AlternativeFrequency : Frequency;

            if (e.Delta > 0)
            {
                Value = GetValue(Value + freq);
            }
            else
            {
                Value = GetValue(Value - freq);
            }
        }

        private int GetValue(int parameter)
        {
            return (int)Math.Max(MinValue, Math.Min(parameter, MaxValue));
        }

    }
}
