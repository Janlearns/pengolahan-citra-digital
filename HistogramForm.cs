using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace MyApps
{
    public class HistogramForm : Form
    {
        private readonly Dictionary<string, int[]> _histogramData;
        private readonly string _imageFileName;
        private PictureBox pbHistogram;
        private ComboBox cmbChannel;
        private Label lblStats;

        public HistogramForm(Dictionary<string, int[]> histogramData, string imageFileName)
        {
            _histogramData = histogramData;
            _imageFileName = imageFileName;
            InitializeHistogramComponents();
            UpdateStatistics(); // Initial update
            DrawHistogram();
        }

        private void InitializeHistogramComponents()
        {
            Text = $"Histogram Analysis - {Path.GetFileNameWithoutExtension(_imageFileName)}";
            ClientSize = new Size(1100, 700); 
            BackColor = Colors.DarkPrimary;
            Font = new Font("Segoe UI", 10f);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 600);
            
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); 
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); 

            Panel topPanel = CreateTopControlPanel();
            mainLayout.Controls.Add(topPanel, 0, 0);

            Panel contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            contentPanel.Controls.Add(CreateAnalysisLayout());
            mainLayout.Controls.Add(contentPanel, 0, 1);

            Controls.Add(mainLayout);
        }

        private Panel CreateTopControlPanel()
        {
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(20, 15, 20, 15)
            };
            
            topPanel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
                }
            };

            Label lblChannel = new Label
            {
                Text = "Select Channel:",
                ForeColor = Colors.TextPrimary,
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            topPanel.Controls.Add(lblChannel);

            cmbChannel = new ComboBox
            {
                Location = new Point(160, 15),
                Width = 150,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.DarkTertiary,
                ForeColor = Colors.TextPrimary,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Cursor = Cursors.Hand
            };
            cmbChannel.Items.AddRange(_histogramData.Keys.ToArray());
            cmbChannel.SelectedIndex = 3; 
            cmbChannel.SelectedIndexChanged += (s, e) => 
            {
                DrawHistogram();
                UpdateStatistics();
            };
            topPanel.Controls.Add(cmbChannel);
            
            return topPanel;
        }

        private TableLayoutPanel CreateAnalysisLayout()
        {
            TableLayoutPanel analysisLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            analysisLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); 
            analysisLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); 

            pbHistogram = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkSecondary, 
                Padding = new Padding(20)
            };
            pbHistogram.Paint += PbHistogram_Paint;
            pbHistogram.Resize += (s, e) => DrawHistogram();
            
            Panel infoPanel = CreateInfoPanel();

            analysisLayout.Controls.Add(pbHistogram, 0, 0);
            analysisLayout.Controls.Add(infoPanel, 1, 0);

            return analysisLayout;
        }

        private Panel CreateInfoPanel()
        {
            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(15)
            };
            
            infoPanel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, 0, 0, infoPanel.Height);
                }
            };

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            
            // A. STATS 
            Label lblStatsTitle = new Label 
            { 
                Text = "CHANNEL STATISTICS", 
                Font = new Font("Segoe UI", 11f, FontStyle.Bold), 
                ForeColor = Colors.Primary, 
                AutoSize = true, 
                Margin = new Padding(0, 5, 0, 5) 
            };
            lblStats = new Label
            {
                Text = "Loading...", 
                Font = new Font("Consolas", 9f), 
                ForeColor = Colors.TextPrimary, 
                AutoSize = false,
                Width = 300, 
                Height = 80,
                BackColor = Colors.DarkTertiary,
                Padding = new Padding(8),
                Margin = new Padding(0, 5, 0, 15)
            };
            
            // B. EXPLANATION
            Label lblTitle = new Label 
            { 
                Text = "WHAT IS A HISTOGRAM?", 
                Font = new Font("Segoe UI", 11f, FontStyle.Bold), 
                ForeColor = Colors.Primary, 
                AutoSize = true, 
                Margin = new Padding(0, 20, 0, 5) 
            };
            
            string explanationText = 
                "A histogram is a graphical representation of the distribution of pixel intensity values in an image. \n\n" +
                "X-Axis (Horizontal): Represents the intensity (brightness) from 0 (Black/Shadows) to 255 (White/Highlights).\n" +
                "Y-Axis (Vertical): Represents the frequency (count) of pixels that have that specific intensity value.\n\n" +
                "Interpretation:\n" +
                "• Peaks on the Left (0-50): Indicates a dark image, dominated by shadows.\n" +
                "• Peaks on the Right (200-255): Indicates a bright image, dominated by highlights.\n" +
                "• Centralized Peaks (100-150): Indicates a well-exposed image with good contrast.";

            Label lblExplanation = new Label
            {
                Text = explanationText, 
                Font = new Font("Segoe UI", 9f),
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = 300, 
                Height = 250, 
                Margin = new Padding(0, 5, 0, 0)
            };
            
            flow.Controls.Add(lblStatsTitle);
            flow.Controls.Add(lblStats);
            flow.Controls.Add(lblTitle);
            flow.Controls.Add(lblExplanation);
            
            infoPanel.Controls.Add(flow);
            
            return infoPanel;
        }

        private void UpdateStatistics()
        {
            if (cmbChannel.SelectedItem == null || _histogramData == null) return;
            
            string selectedChannel = cmbChannel.SelectedItem.ToString();
            int[] data = _histogramData[selectedChannel];
            
            long totalPixels = data.Sum();
            if (totalPixels == 0)
            {
                lblStats.Text = "No data available.";
                return;
            }
            
            int maxCount = data.Max();
            int peakIntensity = Array.IndexOf(data, maxCount);
            
            long sumIntensity = 0;
            for (int i = 0; i < 256; i++)
            {
                sumIntensity += (long)data[i] * i;
            }
            double mean = (double)sumIntensity / totalPixels;
            
            long halfPixels = totalPixels / 2;
            long cumulative = 0;
            int median = 0;
            for (int i = 0; i < 256; i++)
            {
                cumulative += data[i];
                if (cumulative >= halfPixels)
                {
                    median = i;
                    break;
                }
            }

            lblStats.Text = 
                $"Channel: {selectedChannel.ToUpper()}\n" +
                $"Peak Count: {maxCount:N0} (at {peakIntensity})\n" +
                $"Mean Intensity: {mean:N2}\n" +
                $"Median Intensity: {median}";
        }


        private void DrawHistogram()
        {
            pbHistogram.Invalidate(); 
        }

        private void PbHistogram_Paint(object sender, PaintEventArgs e)
        {
            if (cmbChannel.SelectedItem == null) return;

            string selectedChannel = cmbChannel.SelectedItem.ToString();
            int[] data = _histogramData[selectedChannel];
            int maxCount = data.Max();
            if (maxCount == 0) return;

            const int chartMargin = 40; 
            int chartWidth = pbHistogram.Width - 2 * chartMargin;
            int chartHeight = pbHistogram.Height - 2 * chartMargin;
            int xOffset = chartMargin;
            int yOffset = chartMargin;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color barColor;
            if (selectedChannel == "Red") barColor = Colors.RedChannel;
            else if (selectedChannel == "Green") barColor = Colors.GreenChannel;
            else if (selectedChannel == "Blue") barColor = Colors.BlueChannel;
            else barColor = Colors.GrayChannel;
            
            Color gridColor = Colors.GridLine;

            using (Pen axisPen = new Pen(Colors.Border, 1))
            using (Brush barBrush = new SolidBrush(barColor))
            using (Brush textBrush = new SolidBrush(Colors.TextSecondary))
            using (Font smallFont = new Font("Segoe UI", 9f))
            using (Font titleFont = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Colors.TextPrimary))
            {
                // Draw Plot Border (Axis)
                g.DrawRectangle(axisPen, xOffset, yOffset, chartWidth, chartHeight);

                // --- Draw Horizontal Gridlines (4 lines + 0 line) ---
                for (int i = 0; i <= 4; i++)
                {
                    float yGrid = yOffset + (chartHeight / 4f) * i;
                    
                    using (Pen gridPen = new Pen(gridColor) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawLine(gridPen, xOffset, yGrid, xOffset + chartWidth, yGrid);
                    }
                    
                    // Add Y-Axis Labels
                    long value = (long)(maxCount * (4 - i) / 4.0);
                    string label = $"{value:N0}";
                    
                    g.DrawString(label, smallFont, textBrush, 
                        xOffset - g.MeasureString(label, smallFont).Width - 5, 
                        yGrid - g.MeasureString(label, smallFont).Height / 2);
                }

                // --- Draw Histogram Bars ---
                float barWidth = (float)chartWidth / 256;

                for (int i = 0; i < 256; i++)
                {
                    if (data[i] > 0)
                    {
                        float barHeight = (float)data[i] / maxCount * chartHeight; 
                        float x = xOffset + i * barWidth;
                        float y = yOffset + chartHeight - barHeight;

                        g.FillRectangle(barBrush, x, y, barWidth + 0.5f, barHeight); 
                    }
                }

                // --- Axis Labels and Titles ---
                
                g.DrawString("0", smallFont, textBrush, xOffset, yOffset + chartHeight + 3);
                g.DrawString("128", smallFont, textBrush, xOffset + chartWidth / 2 - g.MeasureString("128", smallFont).Width / 2, yOffset + chartHeight + 3);
                g.DrawString("255", smallFont, textBrush, xOffset + chartWidth - g.MeasureString("255", smallFont).Width, yOffset + chartHeight + 3);
                
                string xTitle = "Pixel Intensity Value (Shadows ⬅️ Tone ➡️ Highlights)";
                SizeF xTitleSize = g.MeasureString(xTitle, smallFont);
                g.DrawString(xTitle, smallFont, textBrush, xOffset + chartWidth / 2 - xTitleSize.Width / 2, yOffset + chartHeight + 20);

                string yTitle = "Pixel Count (Frequency)";
                SizeF yTitleSize = g.MeasureString(yTitle, smallFont);
                
                g.TranslateTransform(xOffset - 25, yOffset + chartHeight / 2 + yTitleSize.Width / 2);
                g.RotateTransform(-90);
                g.DrawString(yTitle, smallFont, textBrush, 0, 0);
                g.ResetTransform();
            
                string title = $"{selectedChannel.ToUpper()} CHANNEL DISTRIBUTION";
                SizeF titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, titleBrush, xOffset, 10);
            }
        }
    }
}