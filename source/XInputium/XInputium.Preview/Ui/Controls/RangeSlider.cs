using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XInputium.Preview.Ui.Controls;

// TODO Add keyboard support for RangeSlider control.

/// <summary>
/// Provides a control that allows the user to specify a 
/// range composed of a minimum and maximum value, using 
/// sliding handles.
/// </summary>
/// <seealso cref="Control"/>
[TemplatePart(Name = PART_Canvas, Type = typeof(Canvas))]
[TemplatePart(Name = PART_Minimum, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_Maximum, Type = typeof(FrameworkElement))]
public class RangeSlider : Control
{


    #region Fields and constants

    private const string PART_Canvas = nameof(PART_Canvas);
    private const string PART_Minimum = nameof(PART_Minimum);
    private const string PART_Maximum = nameof(PART_Maximum);
    private const MouseButton SliderMouseButton = MouseButton.Left;

    private Canvas? _canvas;
    private FrameworkElement? _minimumHandle, _maximumHandle;

    #endregion Fields and constants


    #region Constructors

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(typeof(RangeSlider)));
    }


    public RangeSlider()
        : base()
    {
        Loaded += RangeSlider_Loaded;
    }

    #endregion Constructors


    #region Properties

    #region Minimum dependency property

    /// <summary>
    /// Gets or sets the minimum value the <see cref="RangeSlider"/> 
    /// should allow.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum),
            typeof(double), typeof(RangeSlider),
            new PropertyMetadata(0d,
                new PropertyChangedCallback(MinimumPropertyChanged),
                new CoerceValueCallback(MinimumPropertyCoerce)),
                new ValidateValueCallback(MinimumPropertyValidate));

    private static void MinimumPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        RangeSlider target = (RangeSlider)d;
        target.InvalidateProperty(MaximumProperty);
        target.InvalidateProperty(FromProperty);
        target.InvalidateProperty(ToProperty);
        target.UpdateHandles();
    }

    private static object MinimumPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        RangeSlider target = (RangeSlider)d;
        double value = (double)baseValue;
        return Math.Min(value, target.Maximum);
    }

    private static bool MinimumPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value);
    }

    #endregion


    #region Maximum dependency property

    /// <summary>
    /// Gets or sets the maximum value the <see cref="RangeSlider"/> 
    /// should allow.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum),
            typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1d,
                new PropertyChangedCallback(MaximumPropertyChanged),
                new CoerceValueCallback(MaximumPropertyCoerce)),
                new ValidateValueCallback(MaximumPropertyValidate));

    private static void MaximumPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        RangeSlider target = (RangeSlider)d;
        target.InvalidateProperty(MinimumProperty);
        target.InvalidateProperty(FromProperty);
        target.InvalidateProperty(ToProperty);
        target.UpdateHandles();
    }

    private static object MaximumPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        RangeSlider target = (RangeSlider)d;
        double value = (double)baseValue;
        return Math.Max(value, target.Minimum);
    }

    private static bool MaximumPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value);
    }

    #endregion


    #region From dependency property

    /// <summary>
    /// Gets or sets the range beginning value.
    /// </summary>
    public double From
    {
        get => (double)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public static readonly DependencyProperty FromProperty
        = DependencyProperty.Register(nameof(From),
            typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(FromPropertyChanged),
                new CoerceValueCallback(FromPropertyCoerce)),
            new ValidateValueCallback(FromPropertyValidate));

    private static void FromPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        RangeSlider target = (RangeSlider)d;
        target.InvalidateProperty(ToProperty);
        target.UpdateMinimumHandle();
    }

    private static object FromPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        RangeSlider target = (RangeSlider)d;
        double value = (double)baseValue;

        value = target.ApplyValueStepping(value);
        value = Math.Min(value, target.To);
        value = Math.Clamp(value, target.Minimum, target.Maximum);
        return value;
    }

    private static bool FromPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value);
    }

    #endregion


    #region To dependency property

    /// <summary>
    /// Gets or sets the range ending value.
    /// </summary>
    public double To
    {
        get => (double)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public static readonly DependencyProperty ToProperty
        = DependencyProperty.Register(nameof(To),
            typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(ToPropertyChanged),
                new CoerceValueCallback(ToPropertyCoerce)),
            new ValidateValueCallback(ToPropertyValidate));

    private static void ToPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        RangeSlider target = (RangeSlider)d;
        target.InvalidateProperty(FromProperty);
        target.UpdateMaximumHandle();
    }

    private static object ToPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        RangeSlider target = (RangeSlider)d;
        double value = (double)baseValue;

        value = target.ApplyValueStepping(value);
        value = Math.Max(value, target.From);
        value = Math.Clamp(value, target.Minimum, target.Maximum);
        return value;
    }

    private static bool ToPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value);
    }

    #endregion


    #region Step dependency property

    /// <summary>
    /// Gets or sets the value by which the range changes.
    /// </summary>
    /// <value>A number equal to or greater than 0. 
    /// The default value is 0.</value>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public static readonly DependencyProperty StepProperty
        = DependencyProperty.Register(nameof(Step),
            typeof(double), typeof(RangeSlider),
            new PropertyMetadata(0d,
                null,
                new CoerceValueCallback(StepPropertyCoerce)),
            new ValidateValueCallback(StepPropertyValidate));

    private static object StepPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return Math.Max(value, 0d);
    }

    private static bool StepPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    #endregion


    #region HandleBrush dependency property

    public Brush? HandleBrush
    {
        get => (Brush?)GetValue(HandleBrushProperty);
        set => SetValue(HandleBrushProperty, value);
    }

    public static readonly DependencyProperty HandleBrushProperty
        = DependencyProperty.Register(nameof(HandleBrush),
            typeof(Brush), typeof(RangeSlider),
            new PropertyMetadata(null));

    #endregion


    #region OutOfRangeBackground dependency property

    public Brush? OutOfRangeBackground
    {
        get => (Brush?)GetValue(OutOfRangeBackgroundProperty);
        set => SetValue(OutOfRangeBackgroundProperty, value);
    }

    public static readonly DependencyProperty OutOfRangeBackgroundProperty
        = DependencyProperty.Register(nameof(OutOfRangeBackground),
            typeof(Brush), typeof(RangeSlider),
            new PropertyMetadata(null));

    #endregion

    #endregion Properties


    #region Methods

    public override void OnApplyTemplate()
    {
        // Detach old template parts.
        if (_canvas is not null)
        {
            _canvas.MouseDown -= Canvas_MouseDown;
            _canvas.SizeChanged -= Canvas_SizeChanged;
        }
        if (_minimumHandle is not null)
        {
            _minimumHandle.MouseDown -= MinimumHandle_MouseDown;
            _minimumHandle.MouseUp -= MinimumHandle_MouseUp;
            _minimumHandle.MouseMove -= MinimumHandle_MouseMove;
            _minimumHandle.SizeChanged -= MinimumHandle_SizeChanged;
        }
        if (_maximumHandle is not null)
        {
            _maximumHandle.MouseDown -= MaximumHandle_MouseDown;
            _maximumHandle.MouseUp -= MaximumHandle_MouseUp;
            _maximumHandle.MouseMove -= MaximumHandle_MouseMove;
            _maximumHandle.SizeChanged -= MaximumHandle_SizeChanged;
        }

        // Get the new template parts.
        _canvas = GetTemplateChild(PART_Canvas) as Canvas;
        _minimumHandle = GetTemplateChild(PART_Minimum) as FrameworkElement;
        _maximumHandle = GetTemplateChild(PART_Maximum) as FrameworkElement;

        // Ensure template parts are hit-test enabled.
        if (_canvas is not null && _canvas.Background is null)
        {
            _canvas.Background = Brushes.Transparent;
        }

        // Attach new template parts.
        if (_canvas is not null)
        {
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.SizeChanged += Canvas_SizeChanged;
        }
        if (_minimumHandle is not null)
        {
            _minimumHandle.MouseDown += MinimumHandle_MouseDown;
            _minimumHandle.MouseUp += MinimumHandle_MouseUp;
            _minimumHandle.MouseMove += MinimumHandle_MouseMove;
            _minimumHandle.SizeChanged += MinimumHandle_SizeChanged;
        }
        if (_maximumHandle is not null)
        {
            _maximumHandle.MouseDown += MaximumHandle_MouseDown;
            _maximumHandle.MouseUp += MaximumHandle_MouseUp;
            _maximumHandle.MouseMove += MaximumHandle_MouseMove;
            _maximumHandle.SizeChanged += MaximumHandle_SizeChanged;
        }

        // Return control to the base implementation.
        base.OnApplyTemplate();

        // Update handles.
        UpdateHandles();
    }


    private void UpdateMinimumHandle()
    {
        if (_canvas is not null && _minimumHandle is not null)
        {
            double minimum = Minimum;
            double maximum = Maximum;
            double width = _canvas.ActualWidth;
            double minimumHandleWidth = _minimumHandle.ActualWidth;
            double maximumHandleWidth = _maximumHandle?.ActualWidth ?? 0d;
            double x;
            if (width <= 0d || maximum - minimum <= 0d)
            {
                x = 0d;
            }
            else
            {
                double from = From;
                x = (((from - minimum) / (maximum - minimum)) * (width - maximumHandleWidth - minimumHandleWidth));
            }
            Canvas.SetLeft(_minimumHandle, x);
        }
    }


    private void UpdateMaximumHandle()
    {
        if (_canvas is not null && _maximumHandle is not null)
        {
            double minimum = Minimum;
            double maximum = Maximum;
            double width = _canvas.ActualWidth;
            double minimumHandleWidth = _minimumHandle?.ActualWidth ?? 0d;
            double maximumHandleWidth = _maximumHandle.ActualWidth;
            double x;
            if (width <= 0d || maximum - minimum <= 0d)
            {
                x = 0d;
            }
            else
            {
                double to = To;
                x = (((to - minimum) / (maximum - minimum)) * (width - minimumHandleWidth - maximumHandleWidth))
                    + minimumHandleWidth;
            }
            Canvas.SetLeft(_maximumHandle, x);
        }
    }


    private void UpdateHandles()
    {
        UpdateMinimumHandle();
        UpdateMaximumHandle();
    }


    private double ApplyValueStepping(double value)
    {
        double step = Step;
        if (step > 0d)
        {
            value = Math.Floor(value / step) * step;
        }
        return value;
    }

    #endregion Methods


    #region Event handlers

    private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateHandles();
    }


    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == SliderMouseButton
            && _canvas is not null)
        {
            // If the user just clicked in an area between the edge of 
            // the canvas and the closest handle, immediately move that 
            // handle to the mouse position (by changing the range value)
            // and start controlling the handle. If the user clicked within
            // the inside range area, do the same thing, but to the handle
            // that is closer to the mouse.
            double width = _canvas.ActualWidth;
            if (width > 0d)
            {
                e.Handled = true;
                Point mousePosition = e.GetPosition(_canvas);
                if (mousePosition.X >= 0d && mousePosition.X <= width)
                {
                    double minimum = Minimum;
                    double maximum = Maximum;
                    double from = From;
                    double to = To;
                    double rangeLength = to - from;
                    double value = mousePosition.X / width * (maximum - minimum) + minimum;
                    value = Math.Clamp(ApplyValueStepping(value), minimum, maximum);
                    if (value < from
                        || (value < to && value < from + rangeLength / 2))
                    {
                        From = value;
                        if (_minimumHandle is not null)
                        {
                            _minimumHandle.CaptureMouse();
                        }
                    }
                    else if (value > to
                        || (value > from && value >= to - rangeLength / 2))
                    {
                        To = value;
                        if (_maximumHandle is not null)
                        {
                            _maximumHandle.CaptureMouse();
                        }
                    }
                    else
                    {

                    }
                }
            }
        }
    }


    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateHandles();
    }


    private void MinimumHandle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == SliderMouseButton
            && _minimumHandle is not null)
        {
            _minimumHandle.CaptureMouse();
            e.Handled = true;
        }
    }


    private void MinimumHandle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == SliderMouseButton
            && _minimumHandle is not null
            && _minimumHandle.IsMouseCaptured)
        {
            _minimumHandle.ReleaseMouseCapture();
            e.Handled = true;
        }
    }


    private void MinimumHandle_MouseMove(object sender, MouseEventArgs e)
    {
        if (_canvas is not null
            && _minimumHandle is not null
            && _minimumHandle.IsMouseCaptured)
        {
            double width = _canvas.ActualWidth;
            Point mousePosition = e.GetPosition(_canvas);
            if (width > 0d)
            {
                double minimumHandleWidth = _minimumHandle.ActualWidth;
                double maximumHandleWidth = _maximumHandle?.ActualWidth ?? 0d;
                double value = Minimum + (mousePosition.X - minimumHandleWidth / 2)
                    / (width - maximumHandleWidth - minimumHandleWidth)
                    * (Maximum - Minimum);
                value = ApplyValueStepping(value);
                value = Math.Min(value, To);  // Necessary, to prevent weird behavior when the handles are close to each other.
                value = Math.Clamp(value, Minimum, Maximum);
                From = value;
            }
        }
    }


    private void MinimumHandle_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateHandles();
    }


    private void MaximumHandle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == SliderMouseButton
            && _maximumHandle is not null)
        {
            _maximumHandle.CaptureMouse();
            e.Handled = true;
        }
    }


    private void MaximumHandle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == SliderMouseButton
            && _maximumHandle is not null
            && _maximumHandle.IsMouseCaptured)
        {
            _maximumHandle.ReleaseMouseCapture();
            e.Handled = true;
        }
    }


    private void MaximumHandle_MouseMove(object sender, MouseEventArgs e)
    {
        if (_canvas is not null
            && _maximumHandle is not null
            && _maximumHandle.IsMouseCaptured)
        {
            double width = _canvas.ActualWidth;
            Point mousePosition = e.GetPosition(_canvas);
            if (width > 0d)
            {
                double minimumHandleWidth = _minimumHandle?.ActualWidth ?? 0d;
                double maximumHandleWidth = _maximumHandle.ActualWidth;
                double value = Minimum + (mousePosition.X - (maximumHandleWidth / 2) - minimumHandleWidth)
                    / (width - minimumHandleWidth - maximumHandleWidth)
                    * (Maximum - Minimum);
                value = ApplyValueStepping(value);
                value = Math.Max(value, From);  // Necessary, to prevent weird behavior when the handles are close to each other.
                value = Math.Clamp(value, Minimum, Maximum);
                To = value;
            }
        }
    }


    private void MaximumHandle_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateHandles();
    }

    #endregion Event handlers


}
