using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;

using HtmlAttr = System.Web.UI.HtmlTextWriterAttribute;
using HtmlTag = System.Web.UI.HtmlTextWriterTag;

namespace Server.Engines.Reports
{
    /// <summary>
    /// Modified From MS Sample: ChartItem Class
    /// The following class represents a data point in a chart
    /// </summary>
    public class DataItem
    {
        private string _label;
        private string _description;
        private float _value;
        private Color _color;
        private float _startPos;
        private float _sweepSize;

        private DataItem() { }

        public DataItem(string label, string desc, float data, float start, float sweep, Color clr)
        {
            _label = label;
            _description = desc;
            _value = data;
            _startPos = start;
            _sweepSize = sweep;
            _color = clr;
        }

        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public float Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Color ItemColor
        {
            get { return _color; }
            set { _color = value; }
        }

        public float StartPos
        {
            get { return _startPos; }
            set { _startPos = value; }
        }

        public float SweepSize
        {
            get { return _sweepSize; }
            set { _sweepSize = value; }
        }
    }

    /// <summary>
    /// Custom collection for ChartItems in the Chart.cs script
    /// </summary>
    public class ChartItemsCollection : CollectionBase
    {
        public DataItem this[int index]
        {
            get { return (DataItem)(List[index]); }
            set { List[index] = value; }
        }

        public int Add(DataItem value)
        {
            return List.Add(value);
        }

        public int IndexOf(DataItem value)
        {
            return List.IndexOf(value);
        }

        public bool Contains(DataItem value)
        {
            return List.Contains(value);
        }

        public void Remove(DataItem value)
        {
            List.Remove(value);
        }
    }

    /// <summary>
    /// Chart Class: 
    /// Base class implementation for BarChart and PieChart
    /// </summary>
    public abstract class ChartRenderer
    {
        private const int _colorLimit = 9;

        private Color[] _color = 
			{ 
				Color.Firebrick,
				Color.SkyBlue,
				Color.MediumSeaGreen,
				Color.MediumOrchid,
				Color.Chocolate,
				Color.SlateBlue,
				Color.LightPink,
				Color.LightGreen,
				Color.Khaki
			};

        // Represent collection of all data points for the chart
        private ChartItemsCollection _dataPoints = new ChartItemsCollection();

        // The implementation of this method is provided by derived classes
        public abstract Bitmap Draw();

        public ChartItemsCollection DataPoints
        {
            get { return _dataPoints; }
            set { _dataPoints = value; }
        }

        public void SetColor(int index, Color NewColor)
        {
            if (index < _colorLimit)
            {
                _color[index] = NewColor;
            }
            else
            {
                throw new Exception("Color Limit is " + _colorLimit);
            }
        }

        public Color GetColor(int index)
        {
            //return _color[index%_colorLimit];

            if (index < _colorLimit)
            {
                return _color[index];
            }
            else
            {
                return _color[(index + 2) % _colorLimit];
                //throw new Exception("Color Limit is " + _colorLimit);
            }
        }
    }

    /// <summary>
    /// Chart Class: 
    /// The following class uses GDI+ to render Pie Chart
    /// </summary>
    public class PieChartRenderer : ChartRenderer
    {
        private const int _bufferSpace = 125;
        private ArrayList _chartItems;
        private int _perimeter;
        private Color _backgroundColor;
        private Color _borderColor;
        private float _total;
        private int _legendWidth;
        private int _legendHeight;
        private int _legendFontHeight;
        private string _legendFontStyle;
        private float _legendFontSize;
        private bool _showPercents;

        public bool ShowPercents { get { return _showPercents; } set { _showPercents = value; } }

        public PieChartRenderer()
        {
            _chartItems = new ArrayList();
            _perimeter = 250;
            _backgroundColor = Color.White;
            _borderColor = Color.FromArgb(63, 63, 63);
            _legendFontSize = 8;
            _legendFontStyle = "Verdana";
        }

        public PieChartRenderer(Color bgColor)
        {
            _chartItems = new ArrayList();
            _perimeter = 250;
            _backgroundColor = bgColor;
            _borderColor = Color.FromArgb(63, 63, 63);
            _legendFontSize = 8;
            _legendFontStyle = "Verdana";
        }

        //*********************************************************************
        //
        // This method collects all data points and calculate all the necessary dimensions 
        // to draw the chart.  It is the first method called before invoking the Draw() method.
        //
        //*********************************************************************

        public void CollectDataPoints(string[] xValues, string[] yValues)
        {
            _total = 0.0f;

            for (int i = 0; i < xValues.Length; i++)
            {
                float ftemp = Convert.ToSingle(yValues[i]);
                _chartItems.Add(new DataItem(xValues[i], xValues.ToString(), ftemp, 0, 0, Color.AliceBlue));
                _total += ftemp;
            }

            float nextStartPos = 0.0f;
            int counter = 0;
            foreach (DataItem item in _chartItems)
            {
                item.StartPos = nextStartPos;
                item.SweepSize = item.Value / _total * 360;
                nextStartPos = item.StartPos + item.SweepSize;
                item.ItemColor = GetColor(counter++);
            }

            CalculateLegendWidthHeight();
        }

        //*********************************************************************
        //
        // This method returns a bitmap to the calling function.  This is the method
        // that actually draws the pie chart and the legend with it.
        //
        //*********************************************************************

        public override Bitmap Draw()
        {
            int perimeter = _perimeter;
            Rectangle pieRect = new Rectangle(0, 0, perimeter, perimeter - 1);
            Bitmap bmp = new Bitmap(perimeter + _legendWidth, perimeter);
            Font fnt = null;
            Pen pen = null;
            Graphics grp = null;
            StringFormat sf = null, sfp = null;

            try
            {
                grp = Graphics.FromImage(bmp);
                grp.CompositingQuality = CompositingQuality.HighQuality;
                grp.SmoothingMode = SmoothingMode.AntiAlias;
                sf = new StringFormat();

                //Paint Back ground
                using (SolidBrush brsh = new SolidBrush(_backgroundColor))
                    grp.FillRectangle(brsh, -1, -1, perimeter + _legendWidth + 1, perimeter + 1);

                //Align text to the right
                sf.Alignment = StringAlignment.Far;

                //Draw all wedges and legends
                for (int i = 0; i < _chartItems.Count; i++)
                {
                    DataItem item = (DataItem)_chartItems[i];
                    SolidBrush brs = null;
                    try
                    {
                        brs = new SolidBrush(item.ItemColor);
                        grp.FillPie(brs, pieRect, item.StartPos, item.SweepSize);

                        //grp.DrawPie(new Pen(_borderColor,1.2f),pieRect,item.StartPos,item.SweepSize);

                        if (fnt == null)
                            fnt = new Font(_legendFontStyle, _legendFontSize);

                        if (_showPercents && item.SweepSize > 10)
                        {
                            if (sfp == null)
                            {
                                sfp = new StringFormat();
                                sfp.Alignment = StringAlignment.Center;
                                sfp.LineAlignment = StringAlignment.Center;
                            }

                            float perc = (item.SweepSize * 100.0f) / 360.0f;
                            string percString = String.Format("{0:F0}%", perc);

                            float px = pieRect.X + (pieRect.Width / 2);
                            float py = pieRect.Y + (pieRect.Height / 2);

                            double angle = item.StartPos + (item.SweepSize / 2);
                            double rads = (angle / 180.0) * Math.PI;

                            px += (float)(Math.Cos(rads) * perimeter / 3);
                            py += (float)(Math.Sin(rads) * perimeter / 3);

                            grp.DrawString(percString, fnt, Brushes.Gray,
                                new RectangleF(px - 30 - 1, py - 20, 60, 40), sfp);

                            grp.DrawString(percString, fnt, Brushes.Gray,
                                new RectangleF(px - 30 + 1, py - 20, 60, 40), sfp);

                            grp.DrawString(percString, fnt, Brushes.Gray,
                                new RectangleF(px - 30, py - 20 - 1, 60, 40), sfp);

                            grp.DrawString(percString, fnt, Brushes.Gray,
                                new RectangleF(px - 30, py - 20 + 1, 60, 40), sfp);


                            grp.DrawString(percString, fnt, Brushes.White,
                                new RectangleF(px - 30, py - 20, 60, 40), sfp);
                        }

                        if (pen == null)
                            pen = new Pen(_borderColor, 0.5f);

                        grp.FillRectangle(brs, perimeter + _bufferSpace, i * _legendFontHeight + 15, 10, 10);
                        grp.DrawRectangle(pen, perimeter + _bufferSpace, i * _legendFontHeight + 15, 10, 10);

                        grp.DrawString(item.Label, fnt,
                            Brushes.Black, perimeter + _bufferSpace + 20, i * _legendFontHeight + 13);

                        grp.DrawString(item.Value.ToString("#,###.##"), fnt,
                            Brushes.Black, perimeter + _bufferSpace + 200, i * _legendFontHeight + 13, sf);
                    }
                    finally
                    {
                        if (brs != null)
                            brs.Dispose();
                    }
                }

                for (int i = 0; i < _chartItems.Count; i++)
                {
                    DataItem item = (DataItem)_chartItems[i];
                    SolidBrush brs = null;
                    try
                    {
                        grp.DrawPie(new Pen(_borderColor, 0.5f), pieRect, item.StartPos, item.SweepSize);
                    }
                    finally
                    {
                        if (brs != null)
                            brs.Dispose();
                    }
                }

                //draws the border around Pie
                using (Pen pen2 = new Pen(_borderColor, 2))
                    grp.DrawEllipse(pen2, pieRect);

                //draw border around legend
                using (Pen pen1 = new Pen(_borderColor, 1))
                    grp.DrawRectangle(pen1, perimeter + _bufferSpace - 10, 10, 220, _chartItems.Count * _legendFontHeight + 25);

                //Draw Total under legend
                using (Font fntb = new Font(_legendFontStyle, _legendFontSize, FontStyle.Bold))
                {
                    grp.DrawString("Total", fntb,
                        Brushes.Black, perimeter + _bufferSpace + 30, (_chartItems.Count + 1) * _legendFontHeight, sf);
                    grp.DrawString(_total.ToString("#,###.##"), fntb,
                        Brushes.Black, perimeter + _bufferSpace + 200, (_chartItems.Count + 1) * _legendFontHeight, sf);
                }

                grp.SmoothingMode = SmoothingMode.AntiAlias;
            }
            finally
            {
                if (sf != null) sf.Dispose();
                if (grp != null) grp.Dispose();
                if (sfp != null) sfp.Dispose();
                if (fnt != null) fnt.Dispose();
                if (pen != null) pen.Dispose();
            }
            return bmp;
        }

        //*********************************************************************
        //
        //	This method calculates the space required to draw the chart legend.
        //
        //*********************************************************************

        private void CalculateLegendWidthHeight()
        {
            Font fontLegend = new Font(_legendFontStyle, _legendFontSize);
            _legendFontHeight = fontLegend.Height + 3;
            _legendHeight = fontLegend.Height * (_chartItems.Count + 1);
            if (_legendHeight > _perimeter) _perimeter = _legendHeight;

            _legendWidth = _perimeter + _bufferSpace;
            fontLegend.Dispose();
        }
    }

    /// <summary>
    /// BarGraph Class: 
    /// The following class uses GDI+ to render Bar Chart
    /// </summary>
    public class BarRegion
    {
        public int m_RangeFrom, m_RangeTo;
        public string m_Name;

        public BarRegion(int rangeFrom, int rangeTo, string name)
        {
            m_RangeFrom = rangeFrom;
            m_RangeTo = rangeTo;
            m_Name = name;
        }
    }

    public class BarGraphRenderer : ChartRenderer
    {
        private const float _graphLegendSpacer = 15F;
        private const float _labelFontSize = 7f;
        private const int _legendFontSize = 9;
        private const float _legendRectangleSize = 10F;
        private const float _spacer = 5F;

        public BarRegion[] _regions;

        private BarGraphRenderMode _renderMode;

        // Overall related members
        private Color _backColor;
        private string _fontFamily;
        private string _longestTickValue = string.Empty;	// Used to calculate max value width
        private float _maxTickValueWidth;					// Used to calculate left offset of bar graph
        private float _totalHeight;
        private float _totalWidth;

        // Graph related members
        private float _barWidth;
        private float _bottomBuffer;	// Space from bottom to x axis
        private bool _displayBarData;
        private Color _fontColor;
        private float _graphHeight;
        private float _graphWidth;
        private float _maxValue = 0.0f;	// = final tick value * tick count
        private float _scaleFactor;		// = _maxValue / _graphHeight
        private float _spaceBtwBars;	// For now same as _barWidth
        private float _topBuffer;		// Space from top to the top of y axis
        private float _xOrigin;			// x position where graph starts drawing
        private float _yOrigin;			// y position where graph starts drawing
        private string _yLabel;
        private int _yTickCount;
        private float _yTickValue;		// Value for each tick = _maxValue/_yTickCount

        // Legend related members
        private bool _displayLegend;
        private float _legendWidth;
        private string _longestLabel = string.Empty;	// Used to calculate legend width
        private float _maxLabelWidth = 0.0f;

        public string FontFamily
        {
            get { return _fontFamily; }
            set { _fontFamily = value; }
        }

        public BarGraphRenderMode RenderMode
        {
            get { return _renderMode; }
            set { _renderMode = value; }
        }

        public Color BackgroundColor
        {
            set { _backColor = value; }
        }

        public int BottomBuffer
        {
            set { _bottomBuffer = Convert.ToSingle(value); }
        }

        public Color FontColor
        {
            set { _fontColor = value; }
        }

        public int Height
        {
            get { return Convert.ToInt32(_totalHeight); }
            set { _totalHeight = Convert.ToSingle(value); }
        }

        public int Width
        {
            get { return Convert.ToInt32(_totalWidth); }
            set { _totalWidth = Convert.ToSingle(value); }
        }

        public bool ShowLegend
        {
            get { return _displayLegend; }
            set { _displayLegend = value; }
        }

        public bool ShowData
        {
            get { return _displayBarData; }
            set { _displayBarData = value; }
        }
        public int TopBuffer
        {
            set { _topBuffer = Convert.ToSingle(value); }
        }

        public string VerticalLabel
        {
            get { return _yLabel; }
            set { _yLabel = value; }
        }

        public int VerticalTickCount
        {
            get { return _yTickCount; }
            set { _yTickCount = value; }
        }

        private string _xTitle, _yTitle;

        public void SetTitles(string xTitle, string yTitle)
        {
            _xTitle = xTitle;
            _yTitle = yTitle;
        }

        public BarGraphRenderer()
        {
            AssignDefaultSettings();
        }

        public BarGraphRenderer(Color bgColor)
        {
            AssignDefaultSettings();
            BackgroundColor = bgColor;
        }

        //*********************************************************************
        //
        // This method collects all data points and calculate all the necessary dimensions 
        // to draw the bar graph.  It is the method called before invoking the Draw() method.
        // labels is the x values.
        // values is the y values.
        //
        //*********************************************************************

        public void CollectDataPoints(string[] labels, string[] values)
        {
            if (labels.Length == values.Length)
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    float temp = Convert.ToSingle(values[i]);
                    string shortLbl = MakeShortLabel(labels[i]);

                    // For now put 0.0 for start position and sweep size
                    DataPoints.Add(new DataItem(shortLbl, labels[i], temp, 0.0f, 0.0f, GetColor(i)));

                    // Find max value from data; this is only temporary _maxValue
                    if (_maxValue < temp) _maxValue = temp;

                    // Find the longest description
                    if (_displayLegend)
                    {
                        string currentLbl = labels[i] + " (" + shortLbl + ")";
                        float currentWidth = CalculateImgFontWidth(currentLbl, _legendFontSize, FontFamily);
                        if (_maxLabelWidth < currentWidth)
                        {
                            _longestLabel = currentLbl;
                            _maxLabelWidth = currentWidth;
                        }
                    }
                }

                CalculateTickAndMax();
                CalculateGraphDimension();
                CalculateBarWidth(DataPoints.Count, _graphWidth);
                CalculateSweepValues();
            }
            else
                throw new Exception("X data count is different from Y data count");
        }

        //*********************************************************************
        //
        // Same as above; called when user doesn't care about the x values
        //
        //*********************************************************************

        public void CollectDataPoints(string[] values)
        {
            string[] labels = values;
            CollectDataPoints(labels, values);
        }

        public void DrawRegions(Graphics gfx)
        {
            if (_regions == null)
                return;

            using (StringFormat textFormat = new StringFormat())
            {
                textFormat.Alignment = StringAlignment.Center;
                textFormat.LineAlignment = StringAlignment.Center;

                using (Font font = new Font(_fontFamily, _labelFontSize))
                {
                    using (Brush textBrush = new SolidBrush(_fontColor))
                    {
                        using (Pen solidPen = new Pen(_fontColor))
                        {
                            using (Pen lightPen = new Pen(Color.FromArgb(128, _fontColor)))
                            {
                                float labelWidth = _barWidth + _spaceBtwBars;

                                for (int i = 0; i < _regions.Length; ++i)
                                {
                                    BarRegion reg = _regions[i];

                                    RectangleF rc = new RectangleF(_xOrigin + (reg.m_RangeFrom * labelWidth), _yOrigin, (reg.m_RangeTo - reg.m_RangeFrom + 1) * labelWidth, _graphHeight);

                                    if (rc.X + rc.Width > _xOrigin + _graphWidth)
                                        rc.Width = _xOrigin + _graphWidth - rc.X;

                                    using (SolidBrush brsh = new SolidBrush(Color.FromArgb(48, GetColor(i))))
                                        gfx.FillRectangle(brsh, rc);

                                    rc.Offset((rc.Width - 200.0f) * 0.5f, -16.0f);
                                    rc.Width = 200.0f;
                                    rc.Height = 20.0f;

                                    gfx.DrawString(reg.m_Name, font, textBrush, rc, textFormat);
                                }
                            }
                        }
                    }
                }
            }
        }

        //*********************************************************************
        //
        // This method returns a bar graph bitmap to the calling function.  It is called after 
        // all dimensions and data points are calculated.
        //
        //*********************************************************************

        public override Bitmap Draw()
        {
            int height = Convert.ToInt32(_totalHeight);
            int width = Convert.ToInt32(_totalWidth);

            Bitmap bmp = new Bitmap(width, height);

            using (Graphics graph = Graphics.FromImage(bmp))
            {
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush brsh = new SolidBrush(_backColor))
                    graph.FillRectangle(brsh, -1, -1, bmp.Width + 1, bmp.Height + 1);

                DrawRegions(graph);
                DrawVerticalLabelArea(graph);
                DrawXLabelBack(graph);
                DrawBars(graph);
                DrawXLabelArea(graph);

                if (_displayLegend)
                    DrawLegend(graph);
            }

            return bmp;
        }

        //*********************************************************************
        //
        // This method draws all the bars for the graph.
        //
        //*********************************************************************

        public int _interval;

        private void DrawBars(Graphics graph)
        {
            SolidBrush brsFont = null;
            Font valFont = null;
            StringFormat sfFormat = null;

            try
            {
                brsFont = new SolidBrush(_fontColor);
                valFont = new Font(_fontFamily, _labelFontSize);
                sfFormat = new StringFormat();
                sfFormat.Alignment = StringAlignment.Center;
                int i = 0;

                PointF[] linePoints = null;

                if (_renderMode == BarGraphRenderMode.Lines)
                    linePoints = new PointF[DataPoints.Count];

                int pointIndex = 0;

                // Draw bars and the value above each bar
                using (Pen pen = new Pen(_fontColor, 0.15f))
                {
                    using (SolidBrush whiteBrsh = new SolidBrush(Color.FromArgb(128, Color.White)))
                    {
                        foreach (DataItem item in DataPoints)
                        {
                            using (SolidBrush barBrush = new SolidBrush(item.ItemColor))
                            {
                                float itemY = _yOrigin + _graphHeight - item.SweepSize;

                                if (_renderMode == BarGraphRenderMode.Lines)
                                {
                                    linePoints[pointIndex++] = new PointF(_xOrigin + item.StartPos + (_barWidth / 2), itemY);
                                }
                                else if (_renderMode == BarGraphRenderMode.Bars)
                                {
                                    float ox = _xOrigin + item.StartPos;
                                    float oy = itemY;
                                    float ow = _barWidth;
                                    float oh = item.SweepSize;
                                    float of = 9.5f;

                                    PointF[] pts = new PointF[]
									{
										new PointF( ox, oy ),
										new PointF( ox + ow, oy ),
										new PointF( ox + of, oy + of ),
										new PointF( ox + of + ow, oy + of ),
										new PointF( ox, oy + oh ),
										new PointF( ox + of, oy + of + oh ),
										new PointF( ox + of + ow, oy + of + oh )
									};

                                    graph.FillPolygon(barBrush, new PointF[] { pts[2], pts[3], pts[6], pts[5] });

                                    using (SolidBrush ltBrsh = new SolidBrush(System.Windows.Forms.ControlPaint.Light(item.ItemColor, 0.1f)))
                                        graph.FillPolygon(ltBrsh, new PointF[] { pts[0], pts[2], pts[5], pts[4] });

                                    using (SolidBrush drkBrush = new SolidBrush(System.Windows.Forms.ControlPaint.Dark(item.ItemColor, 0.05f)))
                                        graph.FillPolygon(drkBrush, new PointF[] { pts[0], pts[1], pts[3], pts[2] });

                                    graph.DrawLine(pen, pts[0], pts[1]);
                                    graph.DrawLine(pen, pts[0], pts[2]);
                                    graph.DrawLine(pen, pts[1], pts[3]);
                                    graph.DrawLine(pen, pts[2], pts[3]);
                                    graph.DrawLine(pen, pts[2], pts[5]);
                                    graph.DrawLine(pen, pts[0], pts[4]);
                                    graph.DrawLine(pen, pts[4], pts[5]);
                                    graph.DrawLine(pen, pts[5], pts[6]);
                                    graph.DrawLine(pen, pts[3], pts[6]);

                                    // Draw data value
                                    if (_displayBarData && (i % _interval) == 0)
                                    {
                                        float sectionWidth = (_barWidth + _spaceBtwBars);
                                        float startX = _xOrigin + (i * sectionWidth) + (sectionWidth / 2);  // This draws the value on center of the bar
                                        float startY = itemY - 2f - valFont.Height;					  // Positioned on top of each bar by 2 pixels
                                        RectangleF recVal = new RectangleF(startX - ((sectionWidth * _interval) / 2), startY, sectionWidth * _interval, valFont.Height);
                                        SizeF sz = graph.MeasureString(item.Value.ToString("#,###.##"), valFont, recVal.Size, sfFormat);
                                        //using ( SolidBrush brsh = new SolidBrush( Color.FromArgb( 180, 255, 255, 255 ) ) )
                                        //	graph.FillRectangle( brsh, new RectangleF(recVal.X+((recVal.Width-sz.Width)/2),recVal.Y+((recVal.Height-sz.Height)/2),sz.Width+4,sz.Height) );

                                        //graph.DrawString(item.Value.ToString("#,###.##"), valFont, brsFont, recVal, sfFormat);

                                        for (int box = -1; box <= 1; ++box)
                                        {
                                            for (int boy = -1; boy <= 1; ++boy)
                                            {
                                                if (box == 0 && boy == 0)
                                                    continue;

                                                RectangleF rco = new RectangleF(recVal.X + box, recVal.Y + boy, recVal.Width, recVal.Height);
                                                graph.DrawString(item.Value.ToString("#,###.##"), valFont, whiteBrsh, rco, sfFormat);
                                            }
                                        }

                                        graph.DrawString(item.Value.ToString("#,###.##"), valFont, brsFont, recVal, sfFormat);
                                    }
                                }

                                i++;
                            }
                        }

                        if (_renderMode == BarGraphRenderMode.Lines)
                        {
                            if (linePoints.Length >= 2)
                            {
                                using (Pen linePen = new Pen(Color.FromArgb(220, Color.Red), 2.5f))
                                    graph.DrawCurve(linePen, linePoints, 0.5f);
                            }

                            using (Pen linePen = new Pen(Color.FromArgb(40, _fontColor), 0.8f))
                            {
                                for (int j = 0; j < linePoints.Length; ++j)
                                {
                                    graph.DrawLine(linePen, linePoints[j], new PointF(linePoints[j].X, _yOrigin + _graphHeight));

                                    DataItem item = DataPoints[j];
                                    float itemY = _yOrigin + _graphHeight - item.SweepSize;

                                    // Draw data value
                                    if (_displayBarData && (j % _interval) == 0)
                                    {
                                        graph.FillEllipse(brsFont, new RectangleF(linePoints[j].X - 2.0f, linePoints[j].Y - 2.0f, 4.0f, 4.0f));

                                        float sectionWidth = (_barWidth + _spaceBtwBars);
                                        float startX = _xOrigin + (j * sectionWidth) + (sectionWidth / 2);  // This draws the value on center of the bar
                                        float startY = itemY - 2f - valFont.Height;					  // Positioned on top of each bar by 2 pixels
                                        RectangleF recVal = new RectangleF(startX - ((sectionWidth * _interval) / 2), startY, sectionWidth * _interval, valFont.Height);
                                        SizeF sz = graph.MeasureString(item.Value.ToString("#,###.##"), valFont, recVal.Size, sfFormat);
                                        //using ( SolidBrush brsh = new SolidBrush( Color.FromArgb( 48, 255, 255, 255 ) ) )
                                        //	graph.FillRectangle( brsh, new RectangleF(recVal.X+((recVal.Width-sz.Width)/2),recVal.Y+((recVal.Height-sz.Height)/2),sz.Width+4,sz.Height) );

                                        for (int box = -1; box <= 1; ++box)
                                        {
                                            for (int boy = -1; boy <= 1; ++boy)
                                            {
                                                if (box == 0 && boy == 0)
                                                    continue;

                                                RectangleF rco = new RectangleF(recVal.X + box, recVal.Y + boy, recVal.Width, recVal.Height);
                                                graph.DrawString(item.Value.ToString("#,###.##"), valFont, whiteBrsh, rco, sfFormat);
                                            }
                                        }

                                        graph.DrawString(item.Value.ToString("#,###.##"), valFont, brsFont, recVal, sfFormat);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (brsFont != null) brsFont.Dispose();
                if (valFont != null) valFont.Dispose();
                if (sfFormat != null) sfFormat.Dispose();
            }
        }

        //*********************************************************************
        //
        // This method draws the y label, tick marks, tick values, and the y axis.
        //
        //*********************************************************************

        private void DrawVerticalLabelArea(Graphics graph)
        {
            Font lblFont = null;
            SolidBrush brs = null;
            StringFormat lblFormat = null;
            Pen pen = null;
            StringFormat sfVLabel = null;

            float fo = (_yTitle == null ? 0.0f : 20.0f);

            try
            {
                brs = new SolidBrush(_fontColor);
                lblFormat = new StringFormat();
                pen = new Pen(_fontColor);

                if (_yTitle != null)
                {
                    sfVLabel = new StringFormat();
                    sfVLabel.Alignment = StringAlignment.Center;
                    sfVLabel.LineAlignment = StringAlignment.Center;
                    sfVLabel.FormatFlags = StringFormatFlags.DirectionVertical;

                    lblFont = new Font(_fontFamily, _labelFontSize + 4.0f);
                    graph.DrawString(_yTitle, lblFont, brs, new RectangleF(0.0f, _yOrigin, 20.0f, _graphHeight), sfVLabel);
                    lblFont.Dispose();
                }

                sfVLabel = new StringFormat();
                lblFormat.Alignment = StringAlignment.Far;
                lblFormat.FormatFlags |= StringFormatFlags.NoClip;

                // Draw vertical label at the top of y-axis and place it in the middle top of y-axis
                lblFont = new Font(_fontFamily, _labelFontSize + 2.0f, FontStyle.Bold);
                RectangleF recVLabel = new RectangleF(0, _yOrigin - 2 * _spacer - lblFont.Height, _xOrigin * 2, lblFont.Height);
                sfVLabel.Alignment = StringAlignment.Center;
                sfVLabel.FormatFlags |= StringFormatFlags.NoClip;
                //graph.DrawRectangle(Pens.Black,Rectangle.Truncate(recVLabel));
                graph.DrawString(_yLabel, lblFont, brs, recVLabel, sfVLabel);
                lblFont.Dispose();

                lblFont = new Font(_fontFamily, _labelFontSize);
                // Draw all tick values and tick marks
                using (Pen smallPen = new Pen(Color.FromArgb(96, _fontColor), 0.8f))
                {
                    for (int i = 0; i < _yTickCount; i++)
                    {
                        float currentY = _topBuffer + (i * _yTickValue / _scaleFactor);	// Position for tick mark
                        float labelY = currentY - lblFont.Height / 2;						// Place label in the middle of tick
                        RectangleF lblRec = new RectangleF(_spacer + fo - 6, labelY, _maxTickValueWidth, lblFont.Height);

                        float currentTick = _maxValue - i * _yTickValue;					// Calculate tick value from top to bottom
                        graph.DrawString(currentTick.ToString("#,###.##"), lblFont, brs, lblRec, lblFormat);	// Draw tick value  
                        graph.DrawLine(pen, _xOrigin, currentY, _xOrigin - 4.0f, currentY);						// Draw tick mark

                        graph.DrawLine(smallPen, _xOrigin, currentY, _xOrigin + _graphWidth, currentY);
                    }
                }

                // Draw y axis
                graph.DrawLine(pen, _xOrigin, _yOrigin, _xOrigin, _yOrigin + _graphHeight);
            }
            finally
            {
                if (lblFont != null) lblFont.Dispose();
                if (brs != null) brs.Dispose();
                if (lblFormat != null) lblFormat.Dispose();
                if (pen != null) pen.Dispose();
                if (sfVLabel != null) sfVLabel.Dispose();
            }
        }

        //*********************************************************************
        //
        // This method draws x axis and all x labels
        //
        //*********************************************************************

        private void DrawXLabelBack(Graphics graph)
        {
            Font lblFont = null;
            SolidBrush brs = null;
            StringFormat lblFormat = null;
            Pen pen = null;

            try
            {
                lblFont = new Font(_fontFamily, _labelFontSize);
                brs = new SolidBrush(_fontColor);
                lblFormat = new StringFormat();
                pen = new Pen(_fontColor);

                lblFormat.Alignment = StringAlignment.Center;

                // Draw x axis
                graph.DrawLine(pen, _xOrigin, _yOrigin + _graphHeight, _xOrigin + _graphWidth, _yOrigin + _graphHeight);
            }
            finally
            {
                if (lblFont != null) lblFont.Dispose();
                if (brs != null) brs.Dispose();
                if (lblFormat != null) lblFormat.Dispose();
                if (pen != null) pen.Dispose();
            }
        }

        private void DrawXLabelArea(Graphics graph)
        {
            Font lblFont = null;
            SolidBrush brs = null;
            StringFormat lblFormat = null;
            Pen pen = null;

            try
            {
                brs = new SolidBrush(_fontColor);
                pen = new Pen(_fontColor);

                if (_xTitle != null)
                {
                    lblFormat = new StringFormat();
                    lblFormat.Alignment = StringAlignment.Center;
                    lblFormat.LineAlignment = StringAlignment.Center;
                    //					sfVLabel.FormatFlags=StringFormatFlags.DirectionVertical;

                    lblFont = new Font(_fontFamily, _labelFontSize + 2.0f, FontStyle.Bold);
                    graph.DrawString(_xTitle, lblFont, brs, new RectangleF(_xOrigin, _yOrigin + _graphHeight + 14.0f + (_renderMode == BarGraphRenderMode.Bars ? 10.0f : 0.0f) + ((DataPoints.Count / _interval) > 24 ? 16.0f : 0.0f), _graphWidth, 20.0f), lblFormat);
                }

                lblFont = new Font(_fontFamily, _labelFontSize);
                lblFormat = new StringFormat();
                lblFormat.Alignment = StringAlignment.Center;
                lblFormat.FormatFlags |= StringFormatFlags.NoClip;
                lblFormat.Trimming = StringTrimming.None;
                //lblFormat.FormatFlags |= StringFormatFlags.NoWrap;

                float of = 0.0f;

                if (_renderMode == BarGraphRenderMode.Bars)
                {
                    of = 10.0f;

                    // Draw x axis
                    graph.DrawLine(pen, _xOrigin + of, _yOrigin + _graphHeight + of, _xOrigin + _graphWidth + of, _yOrigin + _graphHeight + of);

                    graph.DrawLine(pen, _xOrigin, _yOrigin + _graphHeight, _xOrigin + of, _yOrigin + _graphHeight + of);
                    graph.DrawLine(pen, _xOrigin + _graphWidth, _yOrigin + _graphHeight, _xOrigin + of + _graphWidth, _yOrigin + _graphHeight + of);
                }

                float currentX;
                float currentY = _yOrigin + _graphHeight + 2.0f;	// All x labels are drawn 2 pixels below x-axis
                float labelWidth = _barWidth + _spaceBtwBars;		// Fits exactly below the bar
                int i = 0;

                // Draw x labels
                foreach (DataItem item in DataPoints)
                {
                    if ((i % _interval) == 0)
                    {
                        currentX = _xOrigin + (i * labelWidth) + of + (labelWidth / 2);
                        RectangleF recLbl = new RectangleF(currentX - ((labelWidth * _interval) / 2), currentY + of, labelWidth * _interval, lblFont.Height * 2);
                        string lblString = _displayLegend ? item.Label : item.Description;	// Decide what to show: short or long

                        graph.DrawString(lblString, lblFont, brs, recLbl, lblFormat);
                    }
                    i++;
                }
            }
            finally
            {
                if (lblFont != null) lblFont.Dispose();
                if (brs != null) brs.Dispose();
                if (lblFormat != null) lblFormat.Dispose();
                if (pen != null) pen.Dispose();
            }
        }

        //*********************************************************************
        //
        // This method determines where to place the legend box.
        // It draws the legend border, legend description, and legend color code.
        //
        //*********************************************************************

        private void DrawLegend(Graphics graph)
        {
            Font lblFont = null;
            SolidBrush brs = null;
            StringFormat lblFormat = null;
            Pen pen = null;

            try
            {
                lblFont = new Font(_fontFamily, _legendFontSize);
                brs = new SolidBrush(_fontColor);
                lblFormat = new StringFormat();
                pen = new Pen(_fontColor);
                lblFormat.Alignment = StringAlignment.Near;

                // Calculate Legend drawing start point
                float startX = _xOrigin + _graphWidth + _graphLegendSpacer;
                float startY = _yOrigin;

                float xColorCode = startX + _spacer;
                float xLegendText = xColorCode + _legendRectangleSize + _spacer;
                float legendHeight = 0.0f;
                for (int i = 0; i < DataPoints.Count; i++)
                {
                    DataItem point = DataPoints[i];
                    string text = point.Description + " (" + point.Label + ")";
                    float currentY = startY + _spacer + (i * (lblFont.Height + _spacer));
                    legendHeight += lblFont.Height + _spacer;

                    // Draw legend description
                    graph.DrawString(text, lblFont, brs, xLegendText, currentY, lblFormat);

                    // Draw color code
                    using (SolidBrush brsh = new SolidBrush(DataPoints[i].ItemColor))
                        graph.FillRectangle(brsh, xColorCode, currentY + 3f, _legendRectangleSize, _legendRectangleSize);
                }

                // Draw legend border
                graph.DrawRectangle(pen, startX, startY, _legendWidth, legendHeight + _spacer);
            }
            finally
            {
                if (lblFont != null) lblFont.Dispose();
                if (brs != null) brs.Dispose();
                if (lblFormat != null) lblFormat.Dispose();
                if (pen != null) pen.Dispose();
            }
        }

        //*********************************************************************
        //
        // This method calculates all measurement aspects of the bar graph from the given data points
        //
        //*********************************************************************

        private void CalculateGraphDimension()
        {
            FindLongestTickValue();

            // Need to add another character for spacing; this is not used for drawing, just for calculation
            _longestTickValue += "0";
            //_maxTickValueWidth = CalculateImgFontWidth(_longestTickValue, _labelFontSize, FontFamily);
            _maxTickValueWidth = 0.0f;


            float currentTick;
            string tickString;
            for (int i = 0; i < _yTickCount; i++)
            {
                currentTick = _maxValue - i * _yTickValue;
                tickString = currentTick.ToString("#,###.##");

                float measured = CalculateImgFontWidth(tickString, _labelFontSize, FontFamily);

                if (measured > _maxTickValueWidth)
                    _maxTickValueWidth = measured;
            }

            float leftOffset = _spacer + _maxTickValueWidth + (_yTitle == null ? 0.0f : 20.0f);
            float rtOffset = 0.0f;

            if (_displayLegend)
            {
                _legendWidth = _spacer + _legendRectangleSize + _spacer + _maxLabelWidth + _spacer;
                rtOffset = _graphLegendSpacer + _legendWidth + _spacer;
            }
            else
                rtOffset = _spacer;		// Make graph in the middle

            if (_renderMode == BarGraphRenderMode.Bars)
                rtOffset += 10.0f;

            rtOffset += 10.0f;

            _graphHeight = _totalHeight - _topBuffer - _bottomBuffer - (_xTitle == null ? 0.0f : 20.0f);	// Buffer spaces are used to print labels
            _graphWidth = _totalWidth - leftOffset - rtOffset;
            _xOrigin = leftOffset;
            _yOrigin = _topBuffer;

            // Once the correct _maxValue is determined, then calculate _scaleFactor
            _scaleFactor = _maxValue / _graphHeight;
        }

        //*********************************************************************
        //
        // This method determines the longest tick value from the given data points.
        // The result is needed to calculate the correct graph dimension.
        //
        //*********************************************************************

        private void FindLongestTickValue()
        {
            float currentTick;
            string tickString;
            for (int i = 0; i < _yTickCount; i++)
            {
                currentTick = _maxValue - i * _yTickValue;
                tickString = currentTick.ToString("#,###.##");
                if (_longestTickValue.Length < tickString.Length)
                    _longestTickValue = tickString;
            }
        }

        //*********************************************************************
        //
        // This method calculates the image width in pixel for a given text
        //
        //*********************************************************************

        private float CalculateImgFontWidth(string text, float size, string family)
        {
            Bitmap bmp = null;
            Graphics graph = null;
            Font font = null;

            try
            {
                font = new Font(family, size);

                // Calculate the size of the string.
                bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
                graph = Graphics.FromImage(bmp);
                SizeF oSize = graph.MeasureString(text, font);
                oSize.Width = 4 + (float)Math.Ceiling(oSize.Width);

                return oSize.Width;
            }
            finally
            {
                if (graph != null) graph.Dispose();
                if (bmp != null) bmp.Dispose();
                if (font != null) font.Dispose();
            }
        }

        //*********************************************************************
        //
        // This method creates abbreviation from long description; used for making legend
        //
        //*********************************************************************

        private string MakeShortLabel(string text)
        {
            string label = text;
            if (text.Length > 2)
            {
                int midPostition = Convert.ToInt32(Math.Floor(text.Length / 2.0));
                label = text.Substring(0, 1) + text.Substring(midPostition, 1) + text.Substring(text.Length - 1, 1);
            }
            return label;
        }

        //*********************************************************************
        //
        // This method calculates the max value and each tick mark value for the bar graph.
        //
        //*********************************************************************

        private void CalculateTickAndMax()
        {
            float tempMax = 0.0f;

            // Give graph some head room first about 10% of current max
            _maxValue *= 1.1f;

            if (_maxValue != 0.0f)
            {
                // Find a rounded value nearest to the current max value
                // Calculate this max first to give enough space to draw value on each bar
                double exp = Convert.ToDouble(Math.Floor(Math.Log10(_maxValue)));
                tempMax = Convert.ToSingle(Math.Ceiling(_maxValue / Math.Pow(10, exp)) * Math.Pow(10, exp));
            }
            else
                tempMax = 1.0f;

            // Once max value is calculated, tick value can be determined; tick value should be a whole number
            _yTickValue = tempMax / _yTickCount;
            double expTick = Convert.ToDouble(Math.Floor(Math.Log10(_yTickValue)));
            _yTickValue = Convert.ToSingle(Math.Ceiling(_yTickValue / Math.Pow(10, expTick)) * Math.Pow(10, expTick));

            // Re-calculate the max value with the new tick value
            _maxValue = _yTickValue * _yTickCount;
        }

        //*********************************************************************
        //
        // This method calculates the height for each bar in the graph
        //
        //*********************************************************************

        private void CalculateSweepValues()
        {
            // Called when all values and scale factor are known
            // All values calculated here are relative from (_xOrigin, _yOrigin)
            int i = 0;
            foreach (DataItem item in DataPoints)
            {
                // This implementation does not support negative value
                if (item.Value >= 0) item.SweepSize = item.Value / _scaleFactor;

                // (_spaceBtwBars/2) makes half white space for the first bar
                item.StartPos = (_spaceBtwBars / 2) + i * (_barWidth + _spaceBtwBars);
                i++;
            }
        }

        //*********************************************************************
        //
        // This method calculates the width for each bar in the graph
        //
        //*********************************************************************

        private void CalculateBarWidth(int dataCount, float barGraphWidth)
        {
            // White space between each bar is the same as bar width itself
            _barWidth = barGraphWidth / (dataCount * 2);  // Each bar has 1 white space 
            //_barWidth =/* (float)Math.Floor(*/_barWidth/*)*/;
            _spaceBtwBars = _barWidth;
        }

        //*********************************************************************
        //
        // This method assigns default value to the bar graph properties and is only 
        // called from BarGraph constructors
        //
        //*********************************************************************

        private void AssignDefaultSettings()
        {
            // default values
            _totalWidth = 680f;
            _totalHeight = 450f;
            _fontFamily = "Verdana";
            _backColor = Color.White;
            _fontColor = Color.Black;
            _topBuffer = 30f;
            _bottomBuffer = 30f;
            _yTickCount = 2;
            _displayLegend = false;
            _displayBarData = false;
        }
    }

    public class HtmlRenderer
    {
        private string m_Type;
        private string m_Title;
        private string m_OutputDirectory;

        private DateTime m_TimeStamp;
        private ObjectCollection m_Objects;

        private HtmlRenderer(string outputDirectory)
        {
            m_Type = outputDirectory;
            m_Title = (m_Type == "staff" ? "Staff" : "Stats");
            m_OutputDirectory = Path.Combine(Core.BaseDirectory, "output");

            if (!Directory.Exists(m_OutputDirectory))
                Directory.CreateDirectory(m_OutputDirectory);

            m_OutputDirectory = Path.Combine(m_OutputDirectory, outputDirectory);

            if (!Directory.Exists(m_OutputDirectory))
                Directory.CreateDirectory(m_OutputDirectory);
        }

        public HtmlRenderer(string outputDirectory, Snapshot ss, SnapshotHistory history)
            : this(outputDirectory)
        {
            m_TimeStamp = ss.TimeStamp;

            m_Objects = new ObjectCollection();

            for (int i = 0; i < ss.Children.Count; ++i)
                m_Objects.Add(ss.Children[i]);

            m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Clients", 1, 100, 6));
            m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Items", 24, 9, 1));
            m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Players", 24, 9, 1));
            m_Objects.Add(BarGraph.OverTime(history, "General Stats", "NPCs", 24, 9, 1));
            m_Objects.Add(BarGraph.DailyAverage(history, "General Stats", "Clients"));
            m_Objects.Add(BarGraph.Growth(history, "General Stats", "Clients"));
        }

        public HtmlRenderer(string outputDirectory, StaffHistory history)
            : this(outputDirectory)
        {
            m_TimeStamp = DateTime.UtcNow;

            m_Objects = new ObjectCollection();

            history.Render(m_Objects);
        }

        public void Render()
        {
            Console.WriteLine("Reports: {0}: Render started", m_Title);

            RenderFull();

            for (int i = 0; i < m_Objects.Count; ++i)
                RenderSingle(m_Objects[i]);

            Console.WriteLine("Reports: {0}: Render complete", m_Title);
        }

        private static readonly string FtpHost = null;

        private static readonly string FtpUsername = null;
        private static readonly string FtpPassword = null;

        private static readonly string FtpStatsDirectory = null;
        private static readonly string FtpStaffDirectory = null;

        public void Upload()
        {
            if (FtpHost == null)
                return;

            Console.WriteLine("Reports: {0}: Upload started", m_Title);

            string filePath = Path.Combine(m_OutputDirectory, "upload.ftp");

            using (StreamWriter op = new StreamWriter(filePath))
            {
                op.WriteLine("open \"{0}\"", FtpHost);
                op.WriteLine(FtpUsername);
                op.WriteLine(FtpPassword);
                op.WriteLine("cd \"{0}\"", (m_Type == "staff" ? FtpStaffDirectory : FtpStatsDirectory));
                op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.html"));
                op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.css"));
                op.WriteLine("binary");
                op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.png"));
                op.WriteLine("disconnect");
                op.Write("quit");
            }

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = "ftp";
            psi.Arguments = String.Format("-i -s:\"{0}\"", filePath);

            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            //psi.UseShellExecute = true;

            try
            {
                Process p = Process.Start(psi);

                p.WaitForExit();
            }
            catch
            {
            }

            Console.WriteLine("Reports: {0}: Upload complete", m_Title);

            try { File.Delete(filePath); }
            catch { }
        }

        public void RenderFull()
        {
            string filePath = Path.Combine(m_OutputDirectory, "reports.html");

            using (StreamWriter op = new StreamWriter(filePath))
            {
                using (HtmlTextWriter html = new HtmlTextWriter(op, "\t"))
                    RenderFull(html);
            }

            string cssPath = Path.Combine(m_OutputDirectory, "styles.css");

            if (File.Exists(cssPath))
                return;

            using (StreamWriter css = new StreamWriter(cssPath))
            {
                css.WriteLine("body { background-color: #FFFFFF; font-family: verdana, arial; font-size: 11px; }");
                css.WriteLine("a { color: #28435E; }");
                css.WriteLine("a:hover { color: #4878A9; }");
                css.WriteLine("td.header { background-color: #9696AA; font-weight: bold; font-size: 12px; }");
                css.WriteLine("td.lentry { background-color: #D7D7EB; width: 10%; }");
                css.WriteLine("td.rentry { background-color: #FFFFFF; width: 90%; }");
                css.WriteLine("td.entry { background-color: #FFFFFF; }");
                css.WriteLine("td { font-size: 11px; }");
                css.Write(".tbl-border { background-color: #46465A; }");
            }
        }

        private const string ShardTitle = "Shard";

        public void RenderFull(HtmlTextWriter html)
        {
            html.RenderBeginTag(HtmlTag.Html);

            html.RenderBeginTag(HtmlTag.Head);

            html.RenderBeginTag(HtmlTag.Title);
            html.Write("{0} Statistics", ShardTitle);
            html.RenderEndTag();

            html.AddAttribute("rel", "stylesheet");
            html.AddAttribute(HtmlAttr.Type, "text/css");
            html.AddAttribute(HtmlAttr.Href, "styles.css");
            html.RenderBeginTag(HtmlTag.Link);
            html.RenderEndTag();

            html.RenderEndTag();

            html.RenderBeginTag(HtmlTag.Body);

            for (int i = 0; i < m_Objects.Count; ++i)
            {
                RenderDirect(m_Objects[i], html);
                html.Write("<br><br>");
            }

            html.RenderBeginTag(HtmlTag.Center);
            TimeZone tz = TimeZone.CurrentTimeZone;
            bool isDaylight = tz.IsDaylightSavingTime(m_TimeStamp);
            TimeSpan utcOffset = tz.GetUtcOffset(m_TimeStamp);

            html.Write("Snapshot taken at {0:d} {0:t}. All times are {1}.", m_TimeStamp, tz.StandardName);
            html.RenderEndTag();

            html.RenderEndTag();

            html.RenderEndTag();
        }

        public static string SafeFileName(string name)
        {
            return name.ToLower().Replace(' ', '_');
        }

        public void RenderSingle(PersistableObject obj)
        {
            string filePath = Path.Combine(m_OutputDirectory, SafeFileName(FindNameFrom(obj)) + ".html");

            using (StreamWriter op = new StreamWriter(filePath))
            {
                using (HtmlTextWriter html = new HtmlTextWriter(op, "\t"))
                    RenderSingle(obj, html);
            }
        }

        private string FindNameFrom(PersistableObject obj)
        {
            if (obj is Report)
                return (obj as Report).Name;
            else if (obj is Chart)
                return (obj as Chart).Name;

            return "Invalid";
        }

        public void RenderSingle(PersistableObject obj, HtmlTextWriter html)
        {
            html.RenderBeginTag(HtmlTag.Html);

            html.RenderBeginTag(HtmlTag.Head);

            html.RenderBeginTag(HtmlTag.Title);
            html.Write("{0} Statistics - {1}", ShardTitle, FindNameFrom(obj));
            html.RenderEndTag();

            html.AddAttribute("rel", "stylesheet");
            html.AddAttribute(HtmlAttr.Type, "text/css");
            html.AddAttribute(HtmlAttr.Href, "styles.css");
            html.RenderBeginTag(HtmlTag.Link);
            html.RenderEndTag();

            html.RenderEndTag();

            html.RenderBeginTag(HtmlTag.Body);

            html.RenderBeginTag(HtmlTag.Center);

            RenderDirect(obj, html);

            html.Write("<br>");

            TimeZone tz = TimeZone.CurrentTimeZone;
            bool isDaylight = tz.IsDaylightSavingTime(m_TimeStamp);
            TimeSpan utcOffset = tz.GetUtcOffset(m_TimeStamp);

            html.Write("Snapshot taken at {0:d} {0:t}. All times are {1}.", m_TimeStamp, tz.StandardName);
            html.RenderEndTag();

            html.RenderEndTag();

            html.RenderEndTag();
        }

        public void RenderDirect(PersistableObject obj, HtmlTextWriter html)
        {
            if (obj is Report)
                RenderReport(obj as Report, html);
            else if (obj is BarGraph)
                RenderBarGraph(obj as BarGraph, html);
            else if (obj is PieChart)
                RenderPieChart(obj as PieChart, html);
        }

        private void RenderPieChart(PieChart chart, HtmlTextWriter html)
        {
            PieChartRenderer pieChart = new PieChartRenderer(Color.White);

            pieChart.ShowPercents = chart.ShowPercents;

            string[] labels = new string[chart.Items.Count];
            string[] values = new string[chart.Items.Count];

            for (int i = 0; i < chart.Items.Count; ++i)
            {
                ChartItem item = chart.Items[i];

                labels[i] = item.Name;
                values[i] = item.Value.ToString();
            }

            pieChart.CollectDataPoints(labels, values);

            Bitmap bmp = pieChart.Draw();

            string fileName = chart.FileName + ".png";
            bmp.Save(Path.Combine(m_OutputDirectory, fileName), ImageFormat.Png);

            html.Write("<!-- ");

            html.AddAttribute(HtmlAttr.Href, "#");
            html.AddAttribute(HtmlAttr.Onclick, String.Format("javascript:window.open('{0}.html','ChildWindow','width={1},height={2},resizable=no,status=no,toolbar=no')", SafeFileName(FindNameFrom(chart)), bmp.Width + 30, bmp.Height + 80));
            html.RenderBeginTag(HtmlTag.A);
            html.Write(chart.Name);
            html.RenderEndTag();

            html.Write(" -->");

            html.AddAttribute(HtmlAttr.Cellpadding, "0");
            html.AddAttribute(HtmlAttr.Cellspacing, "0");
            html.AddAttribute(HtmlAttr.Border, "0");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);
            html.AddAttribute(HtmlAttr.Class, "tbl-border");
            html.RenderBeginTag(HtmlTag.Td);

            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Cellpadding, "4");
            html.AddAttribute(HtmlAttr.Cellspacing, "1");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);

            html.AddAttribute(HtmlAttr.Colspan, "10");
            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Align, "center");
            html.AddAttribute(HtmlAttr.Class, "header");
            html.RenderBeginTag(HtmlTag.Td);
            html.Write(chart.Name);
            html.RenderEndTag();
            html.RenderEndTag();

            html.RenderBeginTag(HtmlTag.Tr);

            html.AddAttribute(HtmlAttr.Colspan, "10");
            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Align, "center");
            html.AddAttribute(HtmlAttr.Class, "entry");
            html.RenderBeginTag(HtmlTag.Td);

            html.AddAttribute(HtmlAttr.Width, bmp.Width.ToString());
            html.AddAttribute(HtmlAttr.Height, bmp.Height.ToString());
            html.AddAttribute(HtmlAttr.Src, fileName);
            html.RenderBeginTag(HtmlTag.Img);
            html.RenderEndTag();

            html.RenderEndTag();
            html.RenderEndTag();

            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();

            bmp.Dispose();
        }

        private void RenderBarGraph(BarGraph graph, HtmlTextWriter html)
        {
            BarGraphRenderer barGraph = new BarGraphRenderer(Color.White);

            barGraph.RenderMode = graph.RenderMode;

            barGraph._regions = graph.Regions;
            barGraph.SetTitles(graph.xTitle, null);

            if (graph.yTitle != null)
                barGraph.VerticalLabel = graph.yTitle;

            barGraph.FontColor = Color.Black;
            barGraph.ShowData = (graph.Interval == 1);
            barGraph.VerticalTickCount = graph.Ticks;

            string[] labels = new string[graph.Items.Count];
            string[] values = new string[graph.Items.Count];

            for (int i = 0; i < graph.Items.Count; ++i)
            {
                ChartItem item = graph.Items[i];

                labels[i] = item.Name;
                values[i] = item.Value.ToString();
            }

            barGraph._interval = graph.Interval;
            barGraph.CollectDataPoints(labels, values);

            Bitmap bmp = barGraph.Draw();

            string fileName = graph.FileName + ".png";
            bmp.Save(Path.Combine(m_OutputDirectory, fileName), ImageFormat.Png);

            html.Write("<!-- ");

            html.AddAttribute(HtmlAttr.Href, "#");
            html.AddAttribute(HtmlAttr.Onclick, String.Format("javascript:window.open('{0}.html','ChildWindow','width={1},height={2},resizable=no,status=no,toolbar=no')", SafeFileName(FindNameFrom(graph)), bmp.Width + 30, bmp.Height + 80));
            html.RenderBeginTag(HtmlTag.A);
            html.Write(graph.Name);
            html.RenderEndTag();

            html.Write(" -->");

            html.AddAttribute(HtmlAttr.Cellpadding, "0");
            html.AddAttribute(HtmlAttr.Cellspacing, "0");
            html.AddAttribute(HtmlAttr.Border, "0");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);
            html.AddAttribute(HtmlAttr.Class, "tbl-border");
            html.RenderBeginTag(HtmlTag.Td);

            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Cellpadding, "4");
            html.AddAttribute(HtmlAttr.Cellspacing, "1");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);

            html.AddAttribute(HtmlAttr.Colspan, "10");
            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Align, "center");
            html.AddAttribute(HtmlAttr.Class, "header");
            html.RenderBeginTag(HtmlTag.Td);
            html.Write(graph.Name);
            html.RenderEndTag();
            html.RenderEndTag();

            html.RenderBeginTag(HtmlTag.Tr);

            html.AddAttribute(HtmlAttr.Colspan, "10");
            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Align, "center");
            html.AddAttribute(HtmlAttr.Class, "entry");
            html.RenderBeginTag(HtmlTag.Td);

            html.AddAttribute(HtmlAttr.Width, bmp.Width.ToString());
            html.AddAttribute(HtmlAttr.Height, bmp.Height.ToString());
            html.AddAttribute(HtmlAttr.Src, fileName);
            html.RenderBeginTag(HtmlTag.Img);
            html.RenderEndTag();

            html.RenderEndTag();
            html.RenderEndTag();

            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();

            bmp.Dispose();
        }

        private void RenderReport(Report report, HtmlTextWriter html)
        {
            html.AddAttribute(HtmlAttr.Width, report.Width);
            html.AddAttribute(HtmlAttr.Cellpadding, "0");
            html.AddAttribute(HtmlAttr.Cellspacing, "0");
            html.AddAttribute(HtmlAttr.Border, "0");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);
            html.AddAttribute(HtmlAttr.Class, "tbl-border");
            html.RenderBeginTag(HtmlTag.Td);

            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Cellpadding, "4");
            html.AddAttribute(HtmlAttr.Cellspacing, "1");
            html.RenderBeginTag(HtmlTag.Table);

            html.RenderBeginTag(HtmlTag.Tr);
            html.AddAttribute(HtmlAttr.Colspan, "10");
            html.AddAttribute(HtmlAttr.Width, "100%");
            html.AddAttribute(HtmlAttr.Align, "center");
            html.AddAttribute(HtmlAttr.Class, "header");
            html.RenderBeginTag(HtmlTag.Td);
            html.Write(report.Name);
            html.RenderEndTag();
            html.RenderEndTag();

            bool isNamed = false;

            for (int i = 0; i < report.Columns.Count && !isNamed; ++i)
                isNamed = (report.Columns[i].Name != null);

            if (isNamed)
            {
                html.RenderBeginTag(HtmlTag.Tr);

                for (int i = 0; i < report.Columns.Count; ++i)
                {
                    ReportColumn column = report.Columns[i];

                    html.AddAttribute(HtmlAttr.Class, "header");
                    html.AddAttribute(HtmlAttr.Width, column.Width);
                    html.AddAttribute(HtmlAttr.Align, column.Align);
                    html.RenderBeginTag(HtmlTag.Td);

                    html.Write(column.Name);

                    html.RenderEndTag();
                }

                html.RenderEndTag();
            }

            for (int i = 0; i < report.Items.Count; ++i)
            {
                ReportItem item = report.Items[i];

                html.RenderBeginTag(HtmlTag.Tr);

                for (int j = 0; j < item.Values.Count; ++j)
                {
                    if (!isNamed && j == 0)
                        html.AddAttribute(HtmlAttr.Width, report.Columns[j].Width);

                    html.AddAttribute(HtmlAttr.Align, report.Columns[j].Align);
                    html.AddAttribute(HtmlAttr.Class, "entry");
                    html.RenderBeginTag(HtmlTag.Td);

                    if (item.Values[j].Format == null)
                        html.Write(item.Values[j].Value);
                    else
                        html.Write(int.Parse(item.Values[j].Value).ToString(item.Values[j].Format));

                    html.RenderEndTag();
                }

                html.RenderEndTag();
            }

            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();
            html.RenderEndTag();
        }
    }
}