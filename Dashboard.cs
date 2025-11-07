using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyApps
{
    public class Dashboard : Form
    {
        // ... (Deklarasi field tetap sama)
        private Button btnToggleMath, btnAddImage;
        private FlowLayoutPanel mathLayout;

        private Button btnAnalyzeMultiplication;

        private PictureBox pictureBox;
        private Label lblPath, lblInfo;
        private ProgressBar progressBar;
        private Button btnPilihGambar, btnEkstrakRGB, btnRed, btnGreen, btnBlue, btnGrayscale, btnHistogram, btnReset;
        private Button btnBiner;
        private string currentImagePath = string.Empty;
        private Image originalImage;
        private Panel welcomePanel;

        private Panel sidebarPanel;
        private Button btnToggleMenu;
        private bool isSidebarOpen = true;
        private Button btnBlackWhite;
        // NEW: Panel dan Kontrol untuk Black White
        private Panel blackWhiteControlPanel;
        private TrackBar bwTrackBar;
        private Label lblBWThreshold;
        private bool isBlackWhiteActive = false;

        private Button btnNegate;
        private Button btnBrightness;
        private Panel brightnessControlPanel;
        private TrackBar brTrackBar;
        private Label lblBRLevel;
        private bool isBrightnessActive = false;
        private Button btnColorSelect;
        private bool isColorSelectActive = false;
        private Button btnMultiplyImage; // PASTI ADA
        private Button btnDivideImage;
        private FlowLayoutPanel numericLayout;
        private Button btnToggleNumeric;
        private Button btnAnalyzeDivision;

        // NEW: FlowLayoutPanels untuk menampung grup tombol (untuk di-toggle)
        private FlowLayoutPanel toolsLayout, colorLayout, analysisLayout;
        private Button btnToggleTools, btnToggleColor, btnToggleAnalysis;

        // NEW: Definisi Enum untuk Operasi Matematika - DIUBAH MENJADI PUBLIC
        public enum MathOperation { Add, Multiply, Divide }



        private enum Channel { Red, Green, Blue }

        public Dashboard()
        {
            Text = "Image Processing Studio | Professional Edition";
            WindowState = FormWindowState.Maximized;
            BackColor = Colors.DarkPrimary;
            Font = new Font("Segoe UI", 10f);
            DoubleBuffered = true;
            MinimumSize = new Size(1300, 800);

            InitializeComponents();
            SetButtonsEnabled(false);
        }

        // --- UI Component Creation & Handlers ---

        private void InitializeComponents()
        {
            Panel footer = CreateFooter();
            Controls.Add(footer);

            Panel contentPanel = CreateContentArea();
            Controls.Add(contentPanel);

            // NEW: Tambahkan panel kontrol Black White di atas footer
            blackWhiteControlPanel = CreateBlackWhiteControlPanel();
            Controls.Add(blackWhiteControlPanel);

            brightnessControlPanel = CreateBrightnessControlPanel();
            Controls.Add(brightnessControlPanel);

            sidebarPanel = CreateSidebar();
            Controls.Add(sidebarPanel);

            Panel header = CreateHeader();
            Controls.Add(header);

            SetButtonsEnabled(false);
        }

        private Panel CreateHeader()
        {
            // ... (Kode CreateHeader SAMA PERSIS dan sudah benar)
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(10, 0, 10, 0)
            };

            header.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, header.Height - 1, header.Width, header.Height - 1);
                }
            };

            btnToggleMenu = new Button
            {
                Text = "",
                Width = 50,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Location = new Point(10, 7)
            };

            btnToggleMenu.FlatAppearance.BorderSize = 0;
            btnToggleMenu.FlatAppearance.MouseOverBackColor = Colors.DarkTertiary;
            btnToggleMenu.FlatAppearance.MouseDownBackColor = Colors.DarkPrimary;

            btnToggleMenu.Click += BtnToggleMenu_Click;
            btnToggleMenu.Paint += DrawToggleIcon;

            header.Controls.Add(btnToggleMenu);

            Label titleLabel = new Label
            {
                Text = "IMAGE PROCESSING STUDIO",
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(65, 18)
            };

            header.Controls.Add(titleLabel);
            return header;
        }

        private void DrawToggleIcon(object sender, PaintEventArgs e)
        {
            // ... (Kode DrawToggleIcon SAMA PERSIS dan sudah benar)
            Button btn = (Button)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int lineLength = 24;
            int lineHeight = 3;
            int padding = (btn.Width - lineLength) / 2;

            using (Pen pen = new Pen(Colors.TextPrimary, lineHeight) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (isSidebarOpen) // Draw 'X'
                {
                    g.DrawLine(pen, padding + 4, btn.Height / 2 - 8, btn.Width - padding - 4, btn.Height / 2 + 8);
                    g.DrawLine(pen, padding + 4, btn.Height / 2 + 8, btn.Width - padding - 4, btn.Height / 2 - 8);
                }
                else // Draw Hamburger (≡)
                {
                    g.DrawLine(pen, padding, btn.Height / 2 - 8, padding + lineLength, btn.Height / 2 - 8);
                    g.DrawLine(pen, padding, btn.Height / 2, padding + lineLength, btn.Height / 2);
                    g.DrawLine(pen, padding, btn.Height / 2 + 8, padding + lineLength, btn.Height / 2 + 8);
                }
            }
        }

        private Panel CreateSidebar()
        {
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(15, 20, 15, 20),
                Location = new Point(0, 65),
                BorderStyle = BorderStyle.None
            };

            sidebar.BringToFront();
            sidebar.Height = this.Height - 65 - 55;
            this.Resize += (s, e) =>
            {
                sidebar.Height = this.Height - 65 - 55;
            };

            FlowLayoutPanel buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0)
            };

            btnPilihGambar = CreatePrimaryButton("📂 OPEN IMAGE", Colors.Primary);
            btnPilihGambar.Click += BtnPilihGambar_Click;

            // --- 1. PROCESSING TOOLS ---
            btnToggleTools = CreateToggleButton("🛠️ PROCESSING TOOLS", true);
            toolsLayout = CreateCollapsedPanel();
            btnReset = CreateButton("🔄 Reset View", Colors.DarkTertiary, false);
            btnEkstrakRGB = CreateButton("📄 Export RGB Data (.txt)", Colors.DarkTertiary, false);
            btnBiner = CreateButton("🔢 Export Binary Data (.txt)", Colors.DarkTertiary, false);

            btnReset.Click += BtnReset_Click;
            btnEkstrakRGB.Click += (s, e) => EkstrakRGBData();
            btnBiner.Click += BtnBiner_Click;

            toolsLayout.Controls.Add(btnReset);
            toolsLayout.Controls.Add(btnEkstrakRGB);
            toolsLayout.Controls.Add(btnBiner);
            toolsLayout.Visible = true; // Default open
            btnToggleTools.Click += (s, e) => TogglePanelVisibility(toolsLayout, btnToggleTools);

            // --- 2. COLOR MANIPULATION ---
            btnToggleColor = CreateToggleButton("🎨 COLOR MANIPULATION", false);
            colorLayout = CreateCollapsedPanel();
            btnRed = CreateButton("🔴 Red Channel Isolation", Colors.DarkTertiary, false);
            btnGreen = CreateButton("🟢 Green Channel Isolation", Colors.DarkTertiary, false);
            btnBlue = CreateButton("🔵 Blue Channel Isolation", Colors.DarkTertiary, false);
            btnGrayscale = CreateButton("⚫ Grayscale Conversion", Colors.DarkTertiary, false);
            btnBlackWhite = CreateButton("⚪⚫ Black White Level", Colors.DarkTertiary, false);
            btnNegate = CreateButton("⚫ Invert / Negate", Colors.DarkTertiary, false);
            btnBrightness = CreateButton("☀️ Brightness Level", Colors.DarkTertiary, false);
            btnColorSelect = CreateButton("🖱️ Interactive Color Select", Colors.DarkTertiary, false);

            // math operation
            btnToggleMath = CreateToggleButton("➕ MATH OPERATIONS", false);
            mathLayout = CreateCollapsedPanel();
            btnAddImage = CreateButton("➕", Colors.DarkTertiary, false);
            btnMultiplyImage = CreateButton("✖️", Colors.DarkTertiary, false);
            btnDivideImage = CreateButton("➗", Colors.DarkTertiary, false);

            btnAddImage.Click += BtnAddImage_Click;
            btnMultiplyImage.Click += (s, e) => BtnMathOperation_Click(MathOperation.Multiply);
            btnDivideImage.Click += (s, e) => BtnMathOperation_Click(MathOperation.Divide);// NEW Handler

            mathLayout.Controls.Add(btnAddImage);
            mathLayout.Controls.Add(btnMultiplyImage);
            mathLayout.Controls.Add(btnDivideImage);  // NEW

            btnToggleNumeric = CreateToggleButton("🧮 NUMERIC ANALYSIS", false);
            numericLayout = CreateCollapsedPanel();

            // NEW: Tombol untuk Analisis Perkalian
            btnAnalyzeMultiplication = CreateButton("A*B", Colors.DarkTertiary, false);
            btnAnalyzeMultiplication.Click += BtnAnalyzeMultiplication_Click;

            btnAnalyzeDivision = CreateButton("A/B", Colors.DarkTertiary, false);
            btnAnalyzeDivision.Click += BtnAnalyzeDivision_Click;

            numericLayout.Controls.Add(btnAnalyzeMultiplication);
            numericLayout.Controls.Add(btnAnalyzeDivision);

            btnToggleNumeric.Click += (s, e) => TogglePanelVisibility(numericLayout, btnToggleNumeric);

            btnToggleMath.Click += (s, e) => TogglePanelVisibility(mathLayout, btnToggleMath);



            btnRed.Click += (s, e) => DisplayChannel(Channel.Red);
            btnGreen.Click += (s, e) => DisplayChannel(Channel.Green);
            btnBlue.Click += (s, e) => DisplayChannel(Channel.Blue);
            btnGrayscale.Click += (s, e) => DisplayGrayscale();
            btnBlackWhite.Click += BtnBlackWhite_Click;
            btnNegate.Click += BtnNegate_Click;
            btnBrightness.Click += BtnBrightness_Click;
            btnColorSelect.Click += BtnColorSelect_Click;

            colorLayout.Controls.Add(btnRed);
            colorLayout.Controls.Add(btnGreen);
            colorLayout.Controls.Add(btnBlue);
            colorLayout.Controls.Add(btnGrayscale);
            colorLayout.Controls.Add(btnNegate);
            colorLayout.Controls.Add(btnBrightness);
            colorLayout.Controls.Add(btnColorSelect);
            btnToggleColor.Click += (s, e) => TogglePanelVisibility(colorLayout, btnToggleColor);

            // --- 3. VISUAL ANALYSIS ---
            btnToggleAnalysis = CreateToggleButton("📈 VISUAL ANALYSIS", false);
            analysisLayout = CreateCollapsedPanel();
            btnHistogram = CreateButton("📊 Show Histogram", Colors.DarkTertiary, false);

            btnHistogram.Click += BtnHistogram_Click;

            analysisLayout.Controls.Add(btnHistogram);
            btnToggleAnalysis.Click += (s, e) => TogglePanelVisibility(analysisLayout, btnToggleAnalysis);

            // Menambahkan ke Layout Utama
            buttonLayout.Controls.Add(btnPilihGambar);
            buttonLayout.Controls.Add(CreateSpacer(15));

            buttonLayout.Controls.Add(btnToggleTools);
            buttonLayout.Controls.Add(toolsLayout);

            buttonLayout.Controls.Add(btnToggleColor);
            buttonLayout.Controls.Add(colorLayout);

            buttonLayout.Controls.Add(btnToggleAnalysis);
            buttonLayout.Controls.Add(analysisLayout);

            buttonLayout.Controls.Add(btnToggleMath);
            buttonLayout.Controls.Add(mathLayout);

            buttonLayout.Controls.Add(btnToggleNumeric);
            buttonLayout.Controls.Add(numericLayout);

            

            colorLayout.Controls.Add(btnBlackWhite);
            sidebar.Controls.Add(buttonLayout);
            return sidebar;
        }

        // NEW: Membuat FlowLayoutPanel tertutup
        private FlowLayoutPanel CreateCollapsedPanel()
        {
            return new FlowLayoutPanel
            {
                Width = 250,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0, 0, 0, 5), // Jarak di bawah grup
                Visible = false // Default tertutup
            };
        }

        // NEW: Tombol toggle untuk header grup
        private Button CreateToggleButton(string text, bool defaultOpen)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 250,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 15, 0, 0),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                ForeColor = Colors.TextPrimary,
                BackColor = Colors.DarkSecondary,
                Tag = defaultOpen // Menyimpan status awal/status buka
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Colors.DarkTertiary;
            btn.FlatAppearance.MouseDownBackColor = Colors.DarkPrimary;

            btn.Paint += (s, e) => DrawToggleArrow(s, e, (bool)((Button)s).Tag);

            return btn;
        }

        // NEW: Menggambar panah toggle
        private void DrawToggleArrow(object sender, PaintEventArgs e, bool isOpen)
        {
            Button btn = (Button)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = btn.Width - 25;
            int y = btn.Height / 2;

            Point[] arrowPoints;

            if (isOpen)
            {
                // Panah ke Bawah (v)
                arrowPoints = new Point[] {
                    new Point(x - 5, y - 2),
                    new Point(x + 5, y - 2),
                    new Point(x, y + 4)
                };
            }
            else
            {
                // Panah ke Kanan (>)
                arrowPoints = new Point[] {
                    new Point(x - 3, y - 6),
                    new Point(x - 3, y + 6),
                    new Point(x + 3, y)
                };
            }

            using (SolidBrush brush = new SolidBrush(Colors.TextSecondary))
            {
                g.FillPolygon(brush, arrowPoints);
            }

            // Draw Separator Line
            using (Pen pen = new Pen(Colors.Border, 1))
            {
                g.DrawLine(pen, 0, btn.Height - 1, btn.Width, btn.Height - 1);
            }

            // Draw Line above
            if (btn.Margin.Top > 10)
            {
                using (Pen pen = new Pen(Colors.Border, 1))
                {
                    g.DrawLine(pen, 0, 0, btn.Width, 0);
                }
            }
        }

        // NEW: Logika Toggle Panel
        private void TogglePanelVisibility(FlowLayoutPanel panel, Button toggleButton)
        {
            panel.Visible = !panel.Visible;
            toggleButton.Tag = panel.Visible;
            toggleButton.Invalidate(); // Redraw untuk mengubah ikon panah
        }


        // ... (Kode BtnToggleMenu_Click, FooterPanelLayout SAMA PERSIS dan sudah benar)
        private void BtnToggleMenu_Click(object sender, EventArgs e)
        {
            if (isSidebarOpen)
            {
                sidebarPanel.Width = 0;
                sidebarPanel.Visible = false;
                FooterPanelLayout(30, 8, 30, 8);
            }
            else
            {
                sidebarPanel.Width = 280;
                sidebarPanel.Visible = true;
                FooterPanelLayout(280 + 30, 8, 30, 8);
            }
            isSidebarOpen = !isSidebarOpen;

            this.PerformLayout();
            btnToggleMenu.Invalidate();
            pictureBox.Invalidate();
        }

        private void FooterPanelLayout(int left, int top, int right, int bottom)
        {
            Panel footerPanel = Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom);
            if (footerPanel != null)
            {
                footerPanel.Padding = new Padding(left, top, right, bottom);
            }
        }

        private Panel CreateContentArea()
        {
            // ... (Kode CreateContentArea SAMA PERSIS dan sudah benar)
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Colors.DarkPrimary
            };

            Panel canvasPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkSecondary,
                BorderStyle = BorderStyle.None
            };

            canvasPanel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, canvasPanel.Width - 1, canvasPanel.Height - 1);
                }
            };

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkSecondary,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(1)
            };
            pictureBox.MouseClick += PictureBox_MouseClick;
            welcomePanel = CreateWelcomePanel();
            welcomePanel.Dock = DockStyle.Fill;

            canvasPanel.Controls.Add(pictureBox);
            canvasPanel.Controls.Add(welcomePanel);

            contentPanel.Controls.Add(canvasPanel);
            return contentPanel;
        }

        private Panel CreateFooter()
        {
            // ... (Kode CreateFooter SAMA PERSIS dan sudah benar)
            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(310, 8, 30, 8)
            };

            footer.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, 0, footer.Width, 0);
                }
            };

            TableLayoutPanel footerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0)
            };
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));

            lblPath = new Label
            {
                Text = "NO IMAGE SELECTED",
                Dock = DockStyle.Fill,
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblInfo = new Label
            {
                Text = "READY",
                Dock = DockStyle.Fill,
                ForeColor = Colors.TextSecondary,
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblStatus = new Label
            {
                Text = "STATUS: IDLE",
                Dock = DockStyle.Fill,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 5, 0, 0)
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                Height = 3,
                Margin = new Padding(0, 2, 0, 0)
            };

            footerLayout.Controls.Add(lblPath, 0, 0);
            footerLayout.Controls.Add(lblStatus, 1, 0);
            footerLayout.Controls.Add(lblInfo, 0, 1);
            footerLayout.Controls.Add(progressBar, 1, 1);

            footer.Controls.Add(footerLayout);
            return footer;
        }

        private Panel CreateWelcomePanel()
        {
            // ... (Kode CreateWelcomePanel SAMA PERSIS dan sudah benar)
            Panel panel = new Panel
            {
                BackColor = Colors.DarkSecondary
            };

            Label lblWelcome = new Label
            {
                Text = "VISUAL IMAGE PROCESSOR",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Colors.TextPrimary
            };

            Label instructionLabel = new Label
            {
                Text = "Click 'OPEN IMAGE' on the sidebar to load a file and begin analysis.",
                Font = new Font("Segoe UI", 10f),
                AutoSize = true,
                ForeColor = Colors.TextSecondary
            };

            panel.Resize += (s, e) =>
            {
                int centerX = panel.Width / 2;
                int centerY = panel.Height / 2;

                lblWelcome.Location = new Point(centerX - lblWelcome.Width / 2, centerY - 32);
                instructionLabel.Location = new Point(centerX - instructionLabel.Width / 2, lblWelcome.Bottom + 8);
            };

            panel.Controls.Add(lblWelcome);
            panel.Controls.Add(instructionLabel);

            return panel;
        }

        private Button CreateButton(string text, Color backgroundColor, bool isPrimary)
        {
            // ... (Kode CreateButton SAMA PERSIS dan sudah benar)
            Button btn = new Button
            {
                Text = text,
                Width = 250,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Margin = new Padding(0, 3, 0, 3),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                ForeColor = Colors.TextPrimary,
                BackColor = backgroundColor
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => { if (btn.Enabled) btn.BackColor = Colors.DarkTertiary; };
            btn.MouseLeave += (s, e) => { if (btn.Enabled) btn.BackColor = backgroundColor; };

            btn.EnabledChanged += (s, e) =>
            {
                btn.BackColor = btn.Enabled ? backgroundColor : Colors.DarkSecondary;
                btn.ForeColor = btn.Enabled ? Colors.TextPrimary : Colors.TextMuted;
            };

            return btn;
        }

        private Button CreatePrimaryButton(string text, Color startColor)
        {
            // ... (Kode CreatePrimaryButton SAMA PERSIS dan sudah benar)
            Button btn = new Button
            {
                Text = text,
                Width = 250,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Margin = new Padding(0, 3, 0, 3),
                Cursor = Cursors.Hand,
                ForeColor = Color.White,
                BackColor = startColor,
                Tag = startColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                Button b = (Button)s;
                if (!b.Enabled) return;

                Color currentStartColor = (Color)b.Tag;
                Color endColor = ControlPaint.Dark(currentStartColor, 0.15f);

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    b.ClientRectangle,
                    currentStartColor,
                    endColor,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, b.ClientRectangle);
                }

                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, b.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btn.MouseEnter += (s, e) =>
            {
                if (btn.Enabled)
                {
                    btn.Tag = Colors.PrimaryHover;
                    btn.Invalidate();
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.Enabled)
                {
                    btn.Tag = startColor;
                    btn.Invalidate();
                }
            };

            return btn;
        }

        private Panel CreateSpacer(int height)
        {
            // ... (Kode CreateSpacer SAMA PERSIS dan sudah benar)
            return new Panel
            {
                Width = 250,
                Height = height,
                Margin = new Padding(0)
            };
        }

        private void SetButtonsEnabled(bool enabled)
        {
            // ... (Kode SetButtonsEnabled SAMA PERSIS dan sudah benar)
            btnReset.Enabled = enabled;
            btnEkstrakRGB.Enabled = enabled;
            btnBiner.Enabled = enabled;
            btnRed.Enabled = enabled;
            btnGreen.Enabled = enabled;
            btnBlue.Enabled = enabled;
            btnGrayscale.Enabled = enabled;
            btnHistogram.Enabled = enabled;
            btnBlackWhite.Enabled = enabled;
            btnNegate.Enabled = enabled;
            btnBrightness.Enabled = enabled;
            btnColorSelect.Enabled = enabled;
            btnAddImage.Enabled = enabled;
            btnMultiplyImage.Enabled = enabled;
            btnDivideImage.Enabled = enabled;
            btnAnalyzeDivision.Enabled = enabled;
            btnAnalyzeMultiplication.Enabled = enabled;

            lblInfo.Text = enabled ? "Image loaded. Ready for processing." : "Ready";
        }

        // --- Core Image Processing Methods (LOGIC IMPLEMENTED) ---

        // ... (Kode BtnPilihGambar_Click, BtnReset_Click, EkstrakRGBData, DisplayChannel, 
        // DisplayGrayscale, CalculateHistograms, BtnHistogram_Click, Dispose SAMA PERSIS dan sudah benar)

        private void BtnPilihGambar_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select Image File",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentImagePath = ofd.FileName;
                originalImage?.Dispose();

                try
                {
                    // Gunakan MemoryStream untuk menghindari lock file
                    using (var ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        originalImage = new Bitmap(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                welcomePanel.Visible = false;
                pictureBox.Image?.Dispose();
                // Selalu tampilkan salinan Bitmap agar originalImage tidak diubah
                pictureBox.Image = new Bitmap((Bitmap)originalImage);

                FileInfo fi = new FileInfo(currentImagePath);
                lblPath.Text = Path.GetFileName(currentImagePath).ToUpper();
                lblInfo.Text = $"{fi.Length / 1024:N0} KB · {((Bitmap)originalImage).Width} × {((Bitmap)originalImage).Height} px";

                SetButtonsEnabled(true);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            pictureBox.Image?.Dispose();
            pictureBox.Image = new Bitmap((Bitmap)originalImage);
            lblInfo.Text = "View reset to original image.";
        }

        private void EkstrakRGBData()
        {
            if (originalImage is not Bitmap originalBmp) return;

            // Memastikan gambar dalam format 32bpp untuk akses cepat
            Bitmap dataBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(dataBmp))
            {
                g.DrawImage(originalBmp, 0, 0, originalBmp.Width, originalBmp.Height);
            }

            Rectangle rect = new Rectangle(0, 0, dataBmp.Width, dataBmp.Height);
            BitmapData bmpData = dataBmp.LockBits(rect, ImageLockMode.ReadOnly, dataBmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * dataBmp.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);
            dataBmp.UnlockBits(bmpData);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- RGB Data Extraction: {Path.GetFileName(currentImagePath)} ---");
            sb.AppendLine($"Width: {dataBmp.Width}, Height: {dataBmp.Height}");
            sb.AppendLine("Format: [X, Y] | R | G | B");
            sb.AppendLine("----------------------------------------------------------------");

            // Loop melalui setiap baris dan kolom
            int stride = bmpData.Stride;
            for (int y = 0; y < dataBmp.Height; y++)
            {
                for (int x = 0; x < dataBmp.Width; x++)
                {
                    // BitmapData diatur dalam format BGR (Blue-Green-Red)
                    int position = (y * stride) + (x * 4);
                    byte blue = rgbValues[position];
                    byte green = rgbValues[position + 1];
                    byte red = rgbValues[position + 2];

                    sb.AppendLine($"[{x,4}, {y,4}] | {red,3} | {green,3} | {blue,3}");
                }
            }

            // Simpan ke file TXT
            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text File|*.txt",
                Title = "Save RGB Data File",
                FileName = Path.GetFileNameWithoutExtension(currentImagePath) + "_RGB_Data.txt"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, sb.ToString());
                        lblInfo.Text = $"RGB data exported successfully to: {Path.GetFileName(sfd.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            dataBmp.Dispose();
        }

        private void BtnBiner_Click(object sender, EventArgs e)
        {
            if (originalImage is not Bitmap originalBmp)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- Binary Data Extraction: {Path.GetFileName(currentImagePath)} ---");
            sb.AppendLine($"Width: {originalBmp.Width}, Height: {originalBmp.Height}");
            sb.AppendLine("Format: [X, Y] | R (binary) | G (binary) | B (binary)");
            sb.AppendLine("----------------------------------------------------------------");

            try
            {
                Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
                BitmapData bmpData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                IntPtr ptr = bmpData.Scan0;
                int bytes = Math.Abs(bmpData.Stride) * originalBmp.Height;
                byte[] rgbValues = new byte[bytes];

                Marshal.Copy(ptr, rgbValues, 0, bytes);
                originalBmp.UnlockBits(bmpData);

                int stride = bmpData.Stride;
                for (int y = 0; y < originalBmp.Height; y++)
                {
                    for (int x = 0; x < originalBmp.Width; x++)
                    {
                        int position = (y * stride) + (x * 4);
                        byte blue = rgbValues[position];
                        byte green = rgbValues[position + 1];
                        byte red = rgbValues[position + 2];

                        // Konversi setiap channel ke string biner 8-bit
                        string redBinary = Convert.ToString(red, 2).PadLeft(8, '0');
                        string greenBinary = Convert.ToString(green, 2).PadLeft(8, '0');
                        string blueBinary = Convert.ToString(blue, 2).PadLeft(8, '0');

                        sb.AppendLine($"[{x,4}, {y,4}] | {redBinary} | {greenBinary} | {blueBinary}");
                    }
                }

                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Text File|*.txt",
                    Title = "Save Binary Data File",
                    FileName = Path.GetFileNameWithoutExtension(currentImagePath) + "_Binary_Data.txt"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, sb.ToString());
                        lblInfo.Text = $"Binary data exported successfully to: {Path.GetFileName(sfd.FileName)}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengekspor data biner: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblInfo.Text = "Binary data export failed.";
            }
            finally
            {
                BtnReset_Click(null, null);
            }
        }

        private void DisplayChannel(Channel channel)
        {
            if (originalImage is not Bitmap originalBmp) return;

            // Menggunakan LockBits untuk performa yang lebih baik (opsional, tapi disarankan)
            HideControlPanels();
            Bitmap newBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);

            // Kunci bit data dari kedua gambar
            BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData newData = newBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int byteCount = originalData.Stride * originalData.Height;
            byte[] originalBytes = new byte[byteCount];
            byte[] newBytes = new byte[byteCount];

            Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

            // Channel diatur dalam BGR
            int channelIndex = 0; // B
            if (channel == Channel.Green) channelIndex = 1; // G
            else if (channel == Channel.Red) channelIndex = 2; // R

            for (int i = 0; i < byteCount; i += 4)
            {
                byte B = originalBytes[i];
                byte G = originalBytes[i + 1];
                byte R = originalBytes[i + 2];
                byte A = originalBytes[i + 3];

                if (channel == Channel.Blue)
                {
                    newBytes[i] = B;
                    newBytes[i + 1] = 0;
                    newBytes[i + 2] = 0;
                }
                else if (channel == Channel.Green)
                {
                    newBytes[i] = 0;
                    newBytes[i + 1] = G;
                    newBytes[i + 2] = 0;
                }
                else // Red
                {
                    newBytes[i] = 0;
                    newBytes[i + 1] = 0;
                    newBytes[i + 2] = R;
                }
                newBytes[i + 3] = A; // Alpha tetap
            }

            Marshal.Copy(newBytes, 0, newData.Scan0, byteCount);

            originalBmp.UnlockBits(originalData);
            newBmp.UnlockBits(newData);

            pictureBox.Image?.Dispose();
            pictureBox.Image = newBmp;
            lblInfo.Text = $"{channel} Channel isolated.";
        }

        private void DisplayGrayscale()
        {
            if (originalImage is not Bitmap originalBmp) return;
            HideControlPanels();

            // Menggunakan LockBits
            Bitmap newBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);

            BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData newData = newBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int byteCount = originalData.Stride * originalData.Height;
            byte[] originalBytes = new byte[byteCount];
            byte[] newBytes = new byte[byteCount];

            Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

            for (int i = 0; i < byteCount; i += 4)
            {
                byte B = originalBytes[i];
                byte G = originalBytes[i + 1];
                byte R = originalBytes[i + 2];
                byte A = originalBytes[i + 3];

                // Luma formula for accurate Grayscale
                int grayValue = (int)(R * 0.299 + G * 0.587 + B * 0.114);

                // Set R, G, B ke nilai abu-abu
                newBytes[i] = (byte)grayValue; // B
                newBytes[i + 1] = (byte)grayValue; // G
                newBytes[i + 2] = (byte)grayValue; // R
                newBytes[i + 3] = A; // Alpha tetap
            }

            Marshal.Copy(newBytes, 0, newData.Scan0, byteCount);

            originalBmp.UnlockBits(originalData);
            newBmp.UnlockBits(newData);

            pictureBox.Image?.Dispose();
            pictureBox.Image = newBmp;
            lblInfo.Text = "Image converted to Grayscale.";
        }

        private Dictionary<string, int[]> CalculateHistograms()
        {
            if (originalImage is not Bitmap originalBmp) return null;

            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];
            int[] histGray = new int[256];

            // LockBits untuk performa lebih baik dalam perhitungan histogram
            Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
            BitmapData bmpData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * originalBmp.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            originalBmp.UnlockBits(bmpData);

            for (int i = 0; i < bytes; i += 4)
            {
                byte B = rgbValues[i];
                byte G = rgbValues[i + 1];
                byte R = rgbValues[i + 2];

                histR[R]++;
                histG[G]++;
                histB[B]++;

                // Menggunakan formula Luma
                int grayValue = (int)(R * 0.299 + G * 0.587 + B * 0.114);
                histGray[grayValue]++;
            }

            return new Dictionary<string, int[]>
            {
                { "Red", histR },
                { "Green", histG },
                { "Blue", histB },
                { "Grayscale", histGray }
            };
        }

        private void BtnHistogram_Click(object sender, EventArgs e)
        {
            var histogramData = CalculateHistograms();

            if (histogramData != null)
            {
                string fileName = Path.GetFileName(currentImagePath);
                BtnReset_Click(null, null); // Reset tampilan utama

                // Pastikan HistogramForm sudah tersedia di namespace
                using (HistogramForm histogramForm = new HistogramForm(histogramData, fileName))
                {
                    histogramForm.ShowDialog();
                }
            }
        }
        private Panel CreateBlackWhiteControlPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(30, 10, 30, 10),
                Visible = false // Sembunyikan secara default
            };

            bwTrackBar = new TrackBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 255,
                Value = 128, // Nilai default tengah
                TickFrequency = 10,
                SmallChange = 1,
                LargeChange = 20,
                BackColor = Colors.DarkSecondary,
                Cursor = Cursors.Hand,
                Height = 40
            };
            bwTrackBar.Scroll += BwTrackBar_Scroll;

            lblBWThreshold = new Label
            {
                Text = "Threshold: 128",
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            Label lblLow = new Label
            {
                Text = "Black (0)",
                Width = 80,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 9f)
            };

            Label lblHigh = new Label
            {
                Text = "White (255)",
                Width = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 9f)
            };

            // Tata Letak menggunakan TableLayoutPanel
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Black (0)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // TrackBar
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // White (255)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Threshold Value
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Masukkan kontrol ke layout
            layout.Controls.Add(lblLow, 0, 0);
            layout.Controls.Add(bwTrackBar, 1, 0);
            layout.Controls.Add(lblHigh, 2, 0);
            layout.Controls.Add(lblBWThreshold, 3, 0);
            layout.SetRowSpan(bwTrackBar, 1);

            panel.Controls.Add(layout);

            // Gambar garis di bagian atas panel
            panel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0);
                }
            };

            return panel;
        }
        private void BtnBlackWhite_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Mutually exclusive UI logic: Sembunyikan panel Brightness jika aktif (BARU DITAMBAHKAN)
            if (isBrightnessActive)
            {
                isBrightnessActive = false;
                brightnessControlPanel.Visible = false;
                btnBrightness.BackColor = Colors.DarkTertiary;
            }

            // Toggle status Black White
            isBlackWhiteActive = !isBlackWhiteActive;
            blackWhiteControlPanel.Visible = isBlackWhiteActive;

            if (isBlackWhiteActive)
            {
                // Terapkan efek dengan nilai default (128) saat pertama kali dibuka
                ApplyBlackWhiteThreshold(bwTrackBar.Value);
                btnBlackWhite.BackColor = Colors.DarkHover;
                // Pastikan panel kontrol berada di atas kontrol lain
                Controls.SetChildIndex(blackWhiteControlPanel, 1);
            }
            else
            {
                // Reset tampilan saat ditutup
                BtnReset_Click(null, null);
                btnBlackWhite.BackColor = Colors.DarkTertiary;
            }
        }

        private void BwTrackBar_Scroll(object sender, EventArgs e)
        {
            int threshold = bwTrackBar.Value;
            lblBWThreshold.Text = $"Threshold: {threshold}";
            // Panggil fungsi pemrosesan gambar
            ApplyBlackWhiteThreshold(threshold);
        }
        // Tambahkan di mana saja di dalam kelas Dashboard (sebaiknya dekat dengan DisplayGrayscale atau DisplayChannel)

        public void ApplyBlackWhiteThreshold(int thresholdValue)
        {
            if (originalImage == null) return;

            try
            {
                // 1. Buat salinan dari gambar asli
                Bitmap processedBitmap = new Bitmap(originalImage);

                // Pastikan format gambar setidaknya 32bpp untuk akses byte yang konsisten
                if (processedBitmap.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    Bitmap temp = new Bitmap(processedBitmap.Width, processedBitmap.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(temp))
                    {
                        g.DrawImage(processedBitmap, 0, 0, processedBitmap.Width, processedBitmap.Height);
                    }
                    processedBitmap.Dispose();
                    processedBitmap = temp;
                }

                // 2. Lock the bits untuk akses piksel yang cepat
                Rectangle rect = new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height);
                BitmapData bmpData = processedBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                IntPtr ptr = bmpData.Scan0;

                int stride = bmpData.Stride;
                int totalBytes = stride * processedBitmap.Height;
                byte[] rgbValues = new byte[totalBytes];

                // Salin nilai RGB ke array
                Marshal.Copy(ptr, rgbValues, 0, totalBytes);

                // 3. Iterasi dan ubah setiap piksel
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    // Ambil nilai B, G, R (format Windows adalah BGR)
                    byte B = rgbValues[i];
                    byte G = rgbValues[i + 1];
                    byte R = rgbValues[i + 2];

                    // Konversi ke Grayscale (Luma/Intensitas)
                    int grayValue = (int)(R * 0.299 + G * 0.587 + B * 0.114);

                    byte newValue;
                    if (grayValue > thresholdValue)
                    {
                        // Jika lebih terang dari threshold, jadikan Putih (255)
                        newValue = 255;
                    }
                    else
                    {
                        // Jika lebih gelap atau sama dengan threshold, jadikan Hitam (0)
                        newValue = 0;
                    }

                    // Terapkan nilai baru (Hitam atau Putih) ke semua saluran B, G, R
                    rgbValues[i] = newValue;     // B
                    rgbValues[i + 1] = newValue; // G
                    rgbValues[i + 2] = newValue; // R
                                                 // Alpha (rgbValues[i + 3]) dibiarkan
                }

                // 4. Salin array kembali ke bitmap
                Marshal.Copy(rgbValues, 0, ptr, totalBytes);

                // 5. Unlock the bits
                processedBitmap.UnlockBits(bmpData);

                // 6. Tampilkan gambar yang diproses
                if (pictureBox.Image != originalImage) // Cek agar tidak dispose originalImage
                {
                    pictureBox.Image?.Dispose();
                }
                pictureBox.Image = processedBitmap;
                pictureBox.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat menerapkan Black White Level: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Reset tampilan jika ada error
                BtnReset_Click(null, null);
                isBlackWhiteActive = false;
                blackWhiteControlPanel.Visible = false;
                btnBlackWhite.BackColor = Colors.DarkTertiary;
            }
        }
        // Tambahkan di mana saja di dalam kelas Dashboard

        private void BtnNegate_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyNegation();
        }

        private void ApplyNegation()
        {
            if (originalImage is not Bitmap originalBmp) return;

            try
            {
                // 1. Reset tampilan ke original image sebelum menerapkan efek negasi
                HideControlPanels();
                BtnReset_Click(null, null);

                Bitmap currentBmp = (Bitmap)pictureBox.Image;

                // 2. Lock the bits
                Rectangle rect = new Rectangle(0, 0, currentBmp.Width, currentBmp.Height);
                BitmapData bmpData = currentBmp.LockBits(rect, ImageLockMode.ReadWrite, currentBmp.PixelFormat);

                int bytesPerPixel = 4;
                IntPtr ptr = bmpData.Scan0;

                int stride = bmpData.Stride;
                int totalBytes = stride * currentBmp.Height;
                byte[] rgbValues = new byte[totalBytes];

                // Salin nilai RGB ke array
                Marshal.Copy(ptr, rgbValues, 0, totalBytes);

                // 3. Iterasi dan ubah setiap piksel (Negasi: newValue = 255 - currentValue)
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    // Ambil nilai B, G, R (format Windows adalah BGR)
                    byte B = rgbValues[i];
                    byte G = rgbValues[i + 1];
                    byte R = rgbValues[i + 2];
                    // Alpha (rgbValues[i + 3]) dibiarkan

                    rgbValues[i] = (byte)(255 - B);     // Invert B
                    rgbValues[i + 1] = (byte)(255 - G); // Invert G
                    rgbValues[i + 2] = (byte)(255 - R); // Invert R
                }

                // 4. Salin array kembali ke bitmap
                Marshal.Copy(rgbValues, 0, ptr, totalBytes);

                // 5. Unlock the bits
                currentBmp.UnlockBits(bmpData);

                pictureBox.Invalidate();
                lblInfo.Text = "Image colors successfully inverted (negated).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat menerapkan Negasi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BtnReset_Click(null, null);
            }
        }
        private Panel CreateBrightnessControlPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(30, 10, 30, 10),
                Visible = false // Sembunyikan secara default
            };

            brTrackBar = new TrackBar
            {
                Dock = DockStyle.Fill,
                Minimum = -255, // Gelap
                Maximum = 255,  // Terang
                Value = 0,      // Nilai default tengah (normal)
                TickFrequency = 10,
                SmallChange = 5,
                LargeChange = 20,
                BackColor = Colors.DarkSecondary,
                Cursor = Cursors.Hand,
                Height = 40
            };
            brTrackBar.Scroll += BrTrackBar_Scroll;

            lblBRLevel = new Label
            {
                Text = "Level: 0",
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            Label lblLow = new Label
            {
                Text = "Dark (-255)",
                Width = 80,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 9f)
            };

            Label lblHigh = new Label
            {
                Text = "Bright (+255)",
                Width = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 9f)
            };

            // Tata Letak menggunakan TableLayoutPanel
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Dark
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // TrackBar
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Bright
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Level Value
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Masukkan kontrol ke layout
            layout.Controls.Add(lblLow, 0, 0);
            layout.Controls.Add(brTrackBar, 1, 0);
            layout.Controls.Add(lblHigh, 2, 0);
            layout.Controls.Add(lblBRLevel, 3, 0);
            layout.SetRowSpan(brTrackBar, 1);

            panel.Controls.Add(layout);

            // Gambar garis di bagian atas panel
            panel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0);
                }
            };

            return panel;
        }
        private void BtnBrightness_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Mutually exclusive UI logic: Sembunyikan panel BW jika aktif
            if (isBlackWhiteActive)
            {
                isBlackWhiteActive = false;
                blackWhiteControlPanel.Visible = false;
                btnBlackWhite.BackColor = Colors.DarkTertiary;
            }

            // Toggle status Brightness
            isBrightnessActive = !isBrightnessActive;
            brightnessControlPanel.Visible = isBrightnessActive;

            if (isBrightnessActive)
            {
                // Reset view to original before applying brightness change
                BtnReset_Click(null, null);

                // Terapkan efek dengan nilai default (0)
                ApplyBrightness(brTrackBar.Value);
                btnBrightness.BackColor = Colors.DarkHover;
                // Pastikan panel kontrol berada di atas kontrol lain
                Controls.SetChildIndex(brightnessControlPanel, 1);
            }
            else
            {
                // Reset tampilan saat ditutup
                BtnReset_Click(null, null);
                btnBrightness.BackColor = Colors.DarkTertiary;
            }
        }

        private void BrTrackBar_Scroll(object sender, EventArgs e)
        {
            int level = brTrackBar.Value;
            lblBRLevel.Text = $"Level: {level}";
            // Panggil fungsi pemrosesan gambar
            ApplyBrightness(level);
        }
        public void ApplyBrightness(int level)
        {
            if (originalImage == null) return;

            try
            {
                // 1. Reset tampilan ke original image sebelum menerapkan efek
                BtnReset_Click(null, null);
                Bitmap currentBmp = (Bitmap)pictureBox.Image;

                // 2. Lock the bits untuk akses piksel yang cepat
                Rectangle rect = new Rectangle(0, 0, currentBmp.Width, currentBmp.Height);
                BitmapData bmpData = currentBmp.LockBits(rect, ImageLockMode.ReadWrite, currentBmp.PixelFormat);

                int bytesPerPixel = 4;
                IntPtr ptr = bmpData.Scan0;

                int stride = bmpData.Stride;
                int totalBytes = stride * currentBmp.Height;
                byte[] rgbValues = new byte[totalBytes];

                // Salin nilai RGB ke array
                Marshal.Copy(ptr, rgbValues, 0, totalBytes);

                // 3. Iterasi dan ubah setiap piksel
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    // Ambil nilai B, G, R (format Windows adalah BGR)
                    byte B = rgbValues[i];
                    byte G = rgbValues[i + 1];
                    byte R = rgbValues[i + 2];
                    // Alpha (rgbValues[i + 3]) dibiarkan

                    // Terapkan penyesuaian kecerahan dan pastikan nilainya di antara 0 dan 255 (Clamping)
                    rgbValues[i] = (byte)Math.Max(0, Math.Min(255, B + level));     // B
                    rgbValues[i + 1] = (byte)Math.Max(0, Math.Min(255, G + level)); // G
                    rgbValues[i + 2] = (byte)Math.Max(0, Math.Min(255, R + level)); // R
                }

                // 4. Salin array kembali ke bitmap
                Marshal.Copy(rgbValues, 0, ptr, totalBytes);

                // 5. Unlock the bits
                currentBmp.UnlockBits(bmpData);

                pictureBox.Invalidate();
                lblInfo.Text = $"Brightness adjusted to level: {level}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat menerapkan Brightness: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BtnReset_Click(null, null);
            }
        }
        private void BtnColorSelect_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Nonaktifkan mode interaktif lainnya jika aktif
            if (isBlackWhiteActive)
            {
                isBlackWhiteActive = false;
                blackWhiteControlPanel.Visible = false;
                btnBlackWhite.BackColor = Colors.DarkTertiary;
            }
            if (isBrightnessActive)
            {
                isBrightnessActive = false;
                brightnessControlPanel.Visible = false;
                btnBrightness.BackColor = Colors.DarkTertiary;
            }
            HideControlPanels();

            // Toggle status Color Select
            isColorSelectActive = !isColorSelectActive;

            if (isColorSelectActive)
            {
                // Reset tampilan ke original dan beri feedback
                BtnReset_Click(null, null);
                btnColorSelect.BackColor = Colors.DarkHover;
                lblInfo.Text = "Color Selection Mode: ACTIVE. Click on the image to select a color.";
            }
            else
            {
                // Nonaktifkan mode
                BtnReset_Click(null, null);
                btnColorSelect.BackColor = Colors.DarkTertiary;
                lblInfo.Text = "Color Selection Mode: OFF. View reset to original image.";
            }
        }


        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isColorSelectActive || originalImage == null || pictureBox.Image == null)
            {
                return;
            }

            // 1. Dapatkan posisi piksel yang diklik pada gambar asli
            // Karena PictureBoxSizeMode.Zoom digunakan, perlu menghitung koordinat asli.
            int x = e.X;
            int y = e.Y;

            // Koordinat gambar asli
            int targetX, targetY;

            // Cek apakah gambar telah dimuat
            if (originalImage.Width > 0 && originalImage.Height > 0)
            {
                // Hitung scaling factor dan offset
                float imgRatio = (float)originalImage.Width / originalImage.Height;
                float boxRatio = (float)pictureBox.Width / pictureBox.Height;

                int drawWidth, drawHeight, offsetX, offsetY;

                if (imgRatio > boxRatio) // Dibatasi oleh lebar (Zoom Horizontal)
                {
                    drawWidth = pictureBox.Width;
                    drawHeight = (int)(pictureBox.Width / imgRatio);
                    offsetX = 0;
                    offsetY = (pictureBox.Height - drawHeight) / 2;
                }
                else // Dibatasi oleh tinggi (Zoom Vertical)
                {
                    drawWidth = (int)(pictureBox.Height * imgRatio);
                    drawHeight = pictureBox.Height;
                    offsetX = (pictureBox.Width - drawWidth) / 2;
                    offsetY = 0;
                }

                // Konversi koordinat klik (x, y) ke koordinat gambar asli
                if (x < offsetX || x > offsetX + drawWidth || y < offsetY || y > offsetY + drawHeight)
                {
                    // Klik di area padding (di luar gambar yang dizoom)
                    lblInfo.Text = "Clicked outside the image area. Please click on the picture.";
                    return;
                }

                targetX = (int)(((float)(x - offsetX) / drawWidth) * originalImage.Width);
                targetY = (int)(((float)(y - offsetY) / drawHeight) * originalImage.Height);

            }
            else
            {
                return;
            }

            // 2. Ambil warna piksel target
            Color targetColor;
            using (Bitmap originalBmp = new Bitmap(originalImage))
            {
                // Pastikan koordinat valid sebelum memanggil GetPixel
                if (targetX >= 0 && targetX < originalBmp.Width && targetY >= 0 && targetY < originalBmp.Height)
                {
                    targetColor = originalBmp.GetPixel(targetX, targetY);
                }
                else
                {
                    lblInfo.Text = "Error calculating pixel coordinates.";
                    return;
                }
            }

            // 3. Terapkan pemrosesan
            const int DEFAULT_TOLERANCE = 30; // Hardcoded tolerance
            ApplyColorSelection(targetColor, DEFAULT_TOLERANCE);
            lblInfo.Text = $"Selected Color: R:{targetColor.R}, G:{targetColor.G}, B:{targetColor.B}. Tolerance: {DEFAULT_TOLERANCE}.";
        }
        public void ApplyColorSelection(Color targetColor, int tolerance)
        {
            if (originalImage is not Bitmap originalBmp) return;

            try
            {
                // Selalu reset tampilan ke original, lalu operasikan pada copy-nya
                BtnReset_Click(null, null);
                Bitmap processedBmp = (Bitmap)pictureBox.Image;

                // Lock the bits
                Rectangle rect = new Rectangle(0, 0, processedBmp.Width, processedBmp.Height);
                BitmapData bmpData = processedBmp.LockBits(rect, ImageLockMode.ReadWrite, processedBmp.PixelFormat);

                int bytesPerPixel = 4;
                IntPtr ptr = bmpData.Scan0;

                int stride = bmpData.Stride;
                int totalBytes = stride * processedBmp.Height;
                byte[] rgbValues = new byte[totalBytes];

                // Salin nilai RGB ke array
                Marshal.Copy(ptr, rgbValues, 0, totalBytes);

                // Hitung kuadrat tolerance untuk perbandingan yang lebih cepat
                int toleranceSq = tolerance * tolerance;

                // 3. Iterasi dan ubah setiap piksel
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    // Ambil nilai B, G, R (format Windows adalah BGR)
                    byte B = rgbValues[i];
                    byte G = rgbValues[i + 1];
                    byte R = rgbValues[i + 2];

                    // Hitung Euclidean distance squared (Jarak kuadrat)
                    int dR = targetColor.R - R;
                    int dG = targetColor.G - G;
                    int dB = targetColor.B - B;

                    int distanceSq = dR * dR + dG * dG + dB * dB;

                    if (distanceSq > toleranceSq)
                    {
                        // Jika warna di luar toleransi, jadikan Hitam (0, 0, 0)
                        rgbValues[i] = 0;     // B
                        rgbValues[i + 1] = 0; // G
                        rgbValues[i + 2] = 0; // R
                                              // Alpha (rgbValues[i + 3]) dibiarkan (biasanya 255)
                    }
                    // Jika warna di dalam toleransi, biarkan warna aslinya.
                }

                // 4. Salin array kembali ke bitmap
                Marshal.Copy(rgbValues, 0, ptr, totalBytes);

                // 5. Unlock the bits
                processedBmp.UnlockBits(bmpData);

                pictureBox.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat menerapkan Seleksi Warna: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BtnReset_Click(null, null);
            }
        }
        private void HideControlPanels()
        {
            // Sembunyikan panel Black White jika aktif
            if (isBlackWhiteActive)
            {
                isBlackWhiteActive = false;
                blackWhiteControlPanel.Visible = false;
                btnBlackWhite.BackColor = Colors.DarkTertiary;
            }

            // Sembunyikan panel Brightness jika aktif
            if (isBrightnessActive)
            {
                isBrightnessActive = false;
                brightnessControlPanel.Visible = false;
                btnBrightness.BackColor = Colors.DarkTertiary;
            }

            // Nonaktifkan mode Color Select
            if (isColorSelectActive)
            {
                isColorSelectActive = false;
                btnColorSelect.BackColor = Colors.DarkTertiary;
                lblInfo.Text = "Color Selection Mode: OFF. View reset to original image.";
            }
        }
        // DI DALAM KELAS Dashboard, UBAH metode AddImages
        // Ubah void menjadi Bitmap dan hapus logika tampilan.
        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            BtnMathOperation_Click(MathOperation.Add);
        }
        private Bitmap AddImages(Bitmap bmp1, Bitmap bmp2)
        {
            // Cek dimensi sudah dilakukan di BtnAddImage_Click, tapi tetap pertahankan untuk keamanan
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
            {
                return null;
            }

            try
            {
                // Pastikan kita bekerja pada salinan citra pertama
                Bitmap resultBmp = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format32bppArgb);

                // Lock the bits untuk ketiga citra
                Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);

                // PENTING: Kita mengunci bit bmp1 (originalImage) dan bmp2 (secondImage) di sini.
                // Kita berasumsi bmp1 dan bmp2 adalah salinan yang aman atau citra yang hanya diakses di sini.
                BitmapData data1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData data2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int totalBytes = data1.Stride * bmp1.Height;

                byte[] rgbValues1 = new byte[totalBytes];
                byte[] rgbValues2 = new byte[totalBytes];
                byte[] rgbValuesR = new byte[totalBytes];

                // Salin data ke array
                Marshal.Copy(data1.Scan0, rgbValues1, 0, totalBytes);
                Marshal.Copy(data2.Scan0, rgbValues2, 0, totalBytes);

                // Operasi Penambahan
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    byte B1 = rgbValues1[i];
                    byte G1 = rgbValues1[i + 1];
                    byte R1 = rgbValues1[i + 2];

                    byte B2 = rgbValues2[i];
                    byte G2 = rgbValues2[i + 1];
                    byte R2 = rgbValues2[i + 2];

                    // Penambahan Piksel dengan Saturasi (Clamping)
                    rgbValuesR[i] = (byte)Math.Min(255, B1 + B2);     // B
                    rgbValuesR[i + 1] = (byte)Math.Min(255, G1 + G2); // G
                    rgbValuesR[i + 2] = (byte)Math.Min(255, R1 + R2); // R
                    rgbValuesR[i + 3] = 255; // Alpha
                }

                // Salin array hasil kembali ke bitmap
                Marshal.Copy(rgbValuesR, 0, dataR.Scan0, totalBytes);

                // Unlock the bits
                bmp1.UnlockBits(data1);
                bmp2.UnlockBits(data2);
                resultBmp.UnlockBits(dataR);

                lblInfo.Text = "Calculating Image Addition completed.";

                return resultBmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat operasi penambahan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void BtnMathOperation_Click(MathOperation operation)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Citra utama belum dimuat. Silakan buka citra terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HideControlPanels();

            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = $"Select Second Image for {operation}",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // secondImage?.Dispose(); <--- HAPUS: Ini adalah sisa dari field lama

                // DEKLARASI secondImage sebagai variabel LOKAL
                Image secondImage;

                try
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        secondImage = new Bitmap(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal memuat citra kedua: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Cek dimensi sebelum memproses
                if (originalImage.Width != secondImage.Width || originalImage.Height != secondImage.Height)
                {
                    MessageBox.Show("Operasi citra gagal. Kedua citra harus memiliki dimensi (ukuran) yang sama.", "Error Dimensi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    secondImage.Dispose(); // WAJIB: Dispose variabel lokal yang baru dibuat jika ada error dimensi.
                    return;
                }

                // 2. Lakukan operasi dan dapatkan hasilnya
                Bitmap resultImage = null;

                if (operation == MathOperation.Multiply)
                    resultImage = MultiplyImages((Bitmap)originalImage, (Bitmap)secondImage);
                else if (operation == MathOperation.Divide)
                    resultImage = DivideImages((Bitmap)originalImage, (Bitmap)secondImage);
                else if (operation == MathOperation.Add)
                    resultImage = AddImages((Bitmap)originalImage, (Bitmap)secondImage);

                if (resultImage != null)
                {
                    // Tampilkan hasil dalam Form baru
                    using (ImageMathForm mathForm = new ImageMathForm(originalImage, secondImage, resultImage, operation))
                    {
                        mathForm.ShowDialog();
                    }
                    // secondImage (lokal) dan resultImage sekarang akan di-dispose oleh ImageMathForm.

                    BtnReset_Click(null, null);
                }
            }
        }
        private Bitmap MultiplyImages(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height) return null;

            Bitmap resultBmp = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format32bppArgb);

            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
                BitmapData data1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData data2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int totalBytes = data1.Stride * bmp1.Height;
                byte[] rgbValues1 = new byte[totalBytes];
                byte[] rgbValues2 = new byte[totalBytes];
                byte[] rgbValuesR = new byte[totalBytes];

                Marshal.Copy(data1.Scan0, rgbValues1, 0, totalBytes);
                Marshal.Copy(data2.Scan0, rgbValues2, 0, totalBytes);

                // Operasi Perkalian
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    byte B1 = rgbValues1[i];
                    byte G1 = rgbValues1[i + 1];
                    byte R1 = rgbValues1[i + 2];

                    byte B2 = rgbValues2[i];
                    byte G2 = rgbValues2[i + 1];
                    byte R2 = rgbValues2[i + 2];

                    // Perkalian Piksel dan Normalisasi. 
                    // Rumus: (A * B) / 255. Tujuannya adalah menjaga rentang 0-255 setelah perkalian.
                    // Casting ke int sebelum perkalian untuk menghindari overflow, lalu clamp ke 255.
                    rgbValuesR[i] = (byte)Math.Min(255, (int)B1 * B2);
                    rgbValuesR[i + 1] = (byte)Math.Min(255, (int)G1 * G2);
                    rgbValuesR[i + 2] = (byte)Math.Min(255, (int)R1 * R2);
                    rgbValuesR[i + 3] = 255;
                }

                Marshal.Copy(rgbValuesR, 0, dataR.Scan0, totalBytes);
                bmp1.UnlockBits(data1);
                bmp2.UnlockBits(data2);
                resultBmp.UnlockBits(dataR);

                lblInfo.Text = "Calculating Image Multiplication completed.";
                return resultBmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat operasi perkalian: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // NEW: Metode Pembagian Citra
        private Bitmap DivideImages(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height) return null;

            Bitmap resultBmp = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format32bppArgb);

            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
                BitmapData data1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData data2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int totalBytes = data1.Stride * bmp1.Height;
                byte[] rgbValues1 = new byte[totalBytes];
                byte[] rgbValues2 = new byte[totalBytes];
                byte[] rgbValuesR = new byte[totalBytes];

                Marshal.Copy(data1.Scan0, rgbValues1, 0, totalBytes);
                Marshal.Copy(data2.Scan0, rgbValues2, 0, totalBytes);

                // Operasi Pembagian
                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    byte B1 = rgbValues1[i];
                    byte G1 = rgbValues1[i + 1];
                    byte R1 = rgbValues1[i + 2];

                    byte B2 = rgbValues2[i];
                    byte G2 = rgbValues2[i + 1];
                    byte R2 = rgbValues2[i + 2];

                    // Pembagian Piksel dan Normalisasi. 
                    // Rumus: (A / B) * 255. (Jika B=0, kita anggap 1 atau 255 untuk menghindari division by zero)

                    // B
                    // Ubah ke int sebelum operasi pembagian untuk hasil yang benar, kemudian lakukan clamping
                    if (B2 == 0) rgbValuesR[i] = 255; else rgbValuesR[i] = (byte)Math.Min(255, B1 / B2);
                    // G
                    if (G2 == 0) rgbValuesR[i + 1] = 255; else rgbValuesR[i + 1] = (byte)Math.Min(255, G1 / G2);
                    // R
                    if (R2 == 0) rgbValuesR[i + 2] = 255; else rgbValuesR[i + 2] = (byte)Math.Min(255, R1 / R2);

                    rgbValuesR[i + 3] = 255;
                }

                Marshal.Copy(rgbValuesR, 0, dataR.Scan0, totalBytes);
                bmp1.UnlockBits(data1);
                bmp2.UnlockBits(data2);
                resultBmp.UnlockBits(dataR);

                lblInfo.Text = "Calculating Image Division completed.";
                return resultBmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat operasi pembagian: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void BtnAnalyzeDivision_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Citra utama belum dimuat. Silakan buka citra terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HideControlPanels();

            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select Second Image for Numeric Division Analysis",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image secondImage;

                try
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        secondImage = new Bitmap(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal memuat citra kedua: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Cek dimensi
                if (originalImage.Width != secondImage.Width || originalImage.Height != secondImage.Height)
                {
                    MessageBox.Show("Operasi citra gagal. Kedua citra harus memiliki dimensi (ukuran) yang sama.", "Error Dimensi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    secondImage.Dispose();
                    return;
                }

                // Lakukan analisis
                AnalyzeImageDivision((Bitmap)originalImage, (Bitmap)secondImage, Path.GetFileName(ofd.FileName));

                // secondImage sudah di-dispose di dalam AnalyzeImageDivision
            }
        }

        // NEW: Metode untuk melakukan Pembagian dan menampilkan Hasil Numerik
        private void AnalyzeImageDivision(Bitmap bmp1, Bitmap bmp2, string secondImageName)
        {
            Bitmap resultBmp = DivideImages(bmp1, bmp2); // Gunakan DivideImages yang sudah ada untuk mendapatkan citra hasil

            if (resultBmp == null)
            {
                MessageBox.Show("Analisis gagal menghasilkan citra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 1. Lock the bits citra hasil pembagian
                Rectangle rect = new Rectangle(0, 0, resultBmp.Width, resultBmp.Height);
                BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int totalBytes = dataR.Stride * resultBmp.Height;
                byte[] rgbValuesR = new byte[totalBytes];

                Marshal.Copy(dataR.Scan0, rgbValuesR, 0, totalBytes);
                resultBmp.UnlockBits(dataR);

                // 2. Hitung statistik
                long sumR = 0;
                long sumG = 0;
                long sumB = 0;
                long totalPixels = (long)resultBmp.Width * resultBmp.Height;

                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    sumB += rgbValuesR[i];
                    sumG += rgbValuesR[i + 1];
                    sumR += rgbValuesR[i + 2];
                    // Alpha (i+3) diabaikan
                }

                double avgR = (double)sumR / totalPixels;
                double avgG = (double)sumG / totalPixels;
                double avgB = (double)sumB / totalPixels;
                double avgOverall = (avgR + avgG + avgB) / 3.0;

                // 3. Tampilkan hasilnya
                string message =
                    $"--- NUMERIC ANALYSIS: Division ({Path.GetFileNameWithoutExtension(currentImagePath)} / {Path.GetFileNameWithoutExtension(secondImageName)}) ---\n\n" +
                    $"Total Pixels: {totalPixels:N0}\n\n" +
                    $"Formula: Image A / Image B\n\n" +
                    $"*** Average Pixel Intensity (0-255) ***\n" +
                    $"Red Channel (Avg R): {avgR:N2}\n" +
                    $"Green Channel (Avg G): {avgG:N2}\n" +
                    $"Blue Channel (Avg B): {avgB:N2}\n\n" +
                    $"Overall Average Intensity: {avgOverall:N2}";

                MessageBox.Show(message, "Image Division Numeric Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblInfo.Text = $"Numeric analysis for division completed. Overall Average Intensity: {avgOverall:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat analisis numerik: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Sangat penting untuk membuang citra hasil yang dibuat oleh DivideImages
                resultBmp?.Dispose();
                bmp2?.Dispose(); // Disposes the local secondImage copy
                BtnReset_Click(null, null);
            }
        }
        private void BtnAnalyzeMultiplication_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Citra utama belum dimuat. Silakan buka citra terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HideControlPanels();

            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select Second Image for Numeric Multiplication Analysis",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                FilterIndex = 1
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image secondImage;

                try
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(ofd.FileName)))
                    {
                        secondImage = new Bitmap(ms);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal memuat citra kedua: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Cek dimensi
                if (originalImage.Width != secondImage.Width || originalImage.Height != secondImage.Height)
                {
                    MessageBox.Show("Operasi citra gagal. Kedua citra harus memiliki dimensi (ukuran) yang sama.", "Error Dimensi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    secondImage.Dispose();
                    return;
                }

                // Lakukan analisis
                AnalyzeImageMultiplication((Bitmap)originalImage, (Bitmap)secondImage, Path.GetFileName(ofd.FileName));

                // secondImage sudah di-dispose di dalam AnalyzeImageMultiplication
            }
        }

        // NEW: Metode untuk melakukan Perkalian dan menampilkan Hasil Numerik
        private void AnalyzeImageMultiplication(Bitmap bmp1, Bitmap bmp2, string secondImageName)
        {
            // Gunakan MultiplyImages yang sudah ada untuk mendapatkan citra hasil
            Bitmap resultBmp = MultiplyImages(bmp1, bmp2);

            if (resultBmp == null)
            {
                MessageBox.Show("Analisis gagal menghasilkan citra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 1. Lock the bits citra hasil perkalian
                Rectangle rect = new Rectangle(0, 0, resultBmp.Width, resultBmp.Height);
                BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int totalBytes = dataR.Stride * resultBmp.Height;
                byte[] rgbValuesR = new byte[totalBytes];

                Marshal.Copy(dataR.Scan0, rgbValuesR, 0, totalBytes);
                resultBmp.UnlockBits(dataR);

                // 2. Hitung statistik
                long sumR = 0;
                long sumG = 0;
                long sumB = 0;
                long totalPixels = (long)resultBmp.Width * resultBmp.Height;

                for (int i = 0; i < totalBytes; i += bytesPerPixel)
                {
                    sumB += rgbValuesR[i];
                    sumG += rgbValuesR[i + 1];
                    sumR += rgbValuesR[i + 2];
                }

                double avgR = (double)sumR / totalPixels;
                double avgG = (double)sumG / totalPixels;
                double avgB = (double)sumB / totalPixels;
                double avgOverall = (avgR + avgG + avgB) / 3.0;

                // 3. Tampilkan hasilnya
                string message =
                    $"--- NUMERIC ANALYSIS: Multiplication ({Path.GetFileNameWithoutExtension(currentImagePath)} x {Path.GetFileNameWithoutExtension(secondImageName)}) ---\n\n" +
                    $"Total Pixels: {totalPixels:N0}\n\n" +
                    $"Formula: Image A x Image B (Clamped at 255)\n\n" +
                    $"*** Average Pixel Intensity (0-255) ***\n" +
                    $"Red Channel (Avg R): {avgR:N2}\n" +
                    $"Green Channel (Avg G): {avgG:N2}\n" +
                    $"Blue Channel (Avg B): {avgB:N2}\n\n" +
                    $"Overall Average Intensity: {avgOverall:N2}";

                MessageBox.Show(message, "Image Multiplication Numeric Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblInfo.Text = $"Numeric analysis for multiplication completed. Overall Average Intensity: {avgOverall:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi error saat analisis numerik: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Buang citra hasil yang dibuat oleh MultiplyImages
                resultBmp?.Dispose();
                bmp2?.Dispose(); // Membuang salinan citra kedua
                BtnReset_Click(null, null);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                originalImage?.Dispose();
                pictureBox.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}