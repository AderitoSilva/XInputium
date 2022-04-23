using System;
using System.Windows;
using System.Windows.Media;

namespace XInputium.Preview.Ui.Controls;

public class ModifierFunctionView : FrameworkElement
{


    #region Constructors

    public ModifierFunctionView()
        : base()
    {

    }

    #endregion Constructors


    #region Properties

    #region Function dependency property

    public ModifierFunction? Function
    {
        get => (ModifierFunction?)GetValue(ModifierFunctionProperty);
        set => SetValue(ModifierFunctionProperty, value);
    }

    public static readonly DependencyProperty ModifierFunctionProperty
        = DependencyProperty.Register(nameof(Function),
            typeof(ModifierFunction), typeof(ModifierFunctionView),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion


    #region LineBrush dependency property

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to paint the function 
    /// value line.
    /// </summary>
    public Brush? LineBrush
    {
        get => (Brush?)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public static readonly DependencyProperty LineBrushProperty
        = DependencyProperty.Register(nameof(LineBrush),
            typeof(Brush), typeof(ModifierFunctionView),
            new FrameworkPropertyMetadata(Brushes.Cyan,
                FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion


    #region LineThickness dependency property

    /// <summary>
    /// Gets or sets the thickness of the function line.
    /// </summary>
    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public static readonly DependencyProperty LineThicknessProperty
        = DependencyProperty.Register(nameof(LineThickness),
            typeof(double), typeof(ModifierFunctionView),
            new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                new CoerceValueCallback(LineThicknessPropertyCoerce)),
            new ValidateValueCallback(LineThicknessPropertyValidate));

    private static object LineThicknessPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return Math.Max(value, 0d);
    }

    private static bool LineThicknessPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    #endregion


    #region PlotLinesBrush dependency property

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> used to paint the plot 
    /// grid lines.
    /// </summary>
    public Brush? PlotLinesBrush
    {
        get => (Brush?)GetValue(PlotLinesBrushProperty);
        set => SetValue(PlotLinesBrushProperty, value);
    }

    public static readonly DependencyProperty PlotLinesBrushProperty
        = DependencyProperty.Register(nameof(PlotLinesBrush),
            typeof(Brush), typeof(ModifierFunctionView),
            new FrameworkPropertyMetadata(Brushes.White,
                FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion


    #region LineThickness dependency property

    /// <summary>
    /// Gets or sets the thickness of the plot lines.
    /// </summary>
    public double PlotLinesThickness
    {
        get => (double)GetValue(PlotLinesThicknessProperty);
        set => SetValue(PlotLinesThicknessProperty, value);
    }

    public static readonly DependencyProperty PlotLinesThicknessProperty
        = DependencyProperty.Register(nameof(PlotLinesThickness),
            typeof(double), typeof(ModifierFunctionView),
            new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                new CoerceValueCallback(PlotLinesThicknessPropertyCoerce)),
            new ValidateValueCallback(PlotLinesThicknessPropertyValidate));

    private static object PlotLinesThicknessPropertyCoerce(
        DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return Math.Max(value, 0d);
    }

    private static bool PlotLinesThicknessPropertyValidate(object baseValue)
    {
        double value = (double)baseValue;
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    #endregion

    #endregion Properties


    #region Methods

    protected override void OnRender(DrawingContext drawingContext)
    {
        Rect renderRect = new(new Point(0d, 0d), RenderSize);
        Pen plotLinesPen = new(PlotLinesBrush, PlotLinesThickness);
        if (plotLinesPen.CanFreeze)
            plotLinesPen.Freeze();
        Pen linePen = new(LineBrush, LineThickness);
        if (linePen.CanFreeze)
            linePen.Freeze();

        RenderFunction(drawingContext, renderRect, plotLinesPen, linePen);

        base.OnRender(drawingContext);
    }


    private void RenderFunction(DrawingContext dc, Rect renderRect,
        Pen? plotLinesPen, Pen? linePen)
    {
        // Validate parameters.
        if (dc is null)
            throw new ArgumentNullException(nameof(dc));
        if (renderRect.IsEmpty
            || renderRect.Width <= 0d || renderRect.Height <= 0d)
            return;

        // Render plot lines.
        if (plotLinesPen is not null && plotLinesPen.Brush is not null
            && plotLinesPen.Thickness > 0d)
        {
            Point hLine_Start = new(renderRect.Left, renderRect.Top + renderRect.Height / 2);
            Point hLine_End = new(renderRect.Right, hLine_Start.Y);
            Point vLine_Start = new(renderRect.Left + renderRect.Width / 2, renderRect.Top);
            Point vLine_End = new(vLine_Start.X, renderRect.Bottom);

            GuidelineSet guidelines = new();
            guidelines.GuidelinesX.Add(vLine_Start.X);
            guidelines.GuidelinesY.Add(hLine_Start.Y);
            if (guidelines.CanFreeze)
                guidelines.Freeze();

            dc.PushGuidelineSet(guidelines);  // Push guidelines.
            dc.DrawLine(plotLinesPen, hLine_Start, hLine_End);  // Draw horizontal center line.
            dc.DrawLine(plotLinesPen, vLine_Start, vLine_End);  // Draw vertical center line.
            dc.Pop();  // Pop guidelines.
        }

        // Render function line.
        ModifierFunction? function = Function is null ? null
            : value => InputMath.Clamp11(Function(value));
        double stepLength = 1d;
        int stepCount = (int)Math.Ceiling(RenderSize.Width / stepLength);
        if (stepCount > 0 && function is not null
            && linePen is not null && linePen.Brush is not null && linePen.Thickness > 0d)
        {
            PathFigure lineFigure = new();
            lineFigure.StartPoint = new Point(renderRect.Left,
                renderRect.Top + ((1d - ((function(-1f) + 1f) / 2f)) * renderRect.Height));
            for (int i = 1; i <= stepCount; i++)
            {
                float value = ((float)i) / stepCount;
                value = value * 2f - 1f;
                value = function(value);
                double x = renderRect.Left + (stepLength * i);
                double y = renderRect.Top + ((1d - ((value + 1f) / 2f)) * renderRect.Height);
                LineSegment lineSegment = new(new Point(x, y), true);
                lineFigure.Segments.Add(lineSegment);
            }
            PathGeometry lineGeometry = new(new PathFigure[] { lineFigure });
            if (lineGeometry.CanFreeze)
                lineGeometry.Freeze();
            dc.DrawGeometry(null, linePen, lineGeometry);
        }

    }

    #endregion Methods


}
