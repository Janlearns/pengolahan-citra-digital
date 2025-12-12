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

        private Button btnToggleMath, btnAddImage;
        private FlowLayoutPanel mathLayout; //

        private Button btnAnalyzeMultiplication;

        private PictureBox pictureBox;
        private Label lblPath, lblInfo;
        private ProgressBar progressBar;
        private Button btnPilihGambar, btnEkstrakRGB, btnRed, btnGreen, btnBlue, btnGrayscale, btnHistogram, btnReset;
        private Button btnBiner, btnZoomIn, btnZoomOut, btnFitToScreen;
        private string currentImagePath = string.Empty;
        private Image originalImage;
        private Panel welcomePanel;

        private Panel sidebarPanel;
        private Button btnToggleMenu;
        private bool isSidebarOpen = true;
        private Button btnBlackWhite;

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
        private Button btnMultiplyImage;
        private Button btnDivideImage;
        private FlowLayoutPanel numericLayout;
        private Button btnToggleNumeric;
        private Button btnAnalyzeDivision;
        private FlowLayoutPanel toolsLayout, colorLayout, analysisLayout;
        private Button btnToggleTools, btnToggleColor, btnToggleAnalysis;
        private FlowLayoutPanel logicalLayout;
        private Button btnToggleLogical;
        private Button btnAnd, btnOr, btnXor, btnNot;
        private FlowLayoutPanel rotationLayout;
        private Button btnToggleRotation;
        private Button btnRotate90, btnRotate180, btnRotate270, btnRotate45, btnRotateFree;

        private Panel rotationControlPanel;
        private TrackBar rotationTrackBar;
        private Label lblRotationAngle;
        private bool isRotationActive = false;

        private Panel translationControlPanel;
        private TrackBar xTranslationTrackBar, yTranslationTrackBar;
        private Label lblXOffset, lblYOffset;
        private bool isTranslationActive = false;
        private Button btnTranslate;

        private float zoomFactor = 1.0f;
        private Label lblZoom;

        // Kontrol untuk Distorsi
        private FlowLayoutPanel distortionLayout;
        private Button btnToggleDistortion;
        private Button btnRippleEffect;
        private Panel rippleControlPanel;
        private TrackBar amplitudeTrackBar, frequencyTrackBar;
        private Label lblAmplitude, lblFrequency;
        private bool isRippleEffectActive = false;

        // Kontrol untuk Konvolusi
        private FlowLayoutPanel convolutionLayout;
        private Button btnToggleConvolution;
        private Button btnGaussianBlur, btnSharpen, btnEdgeDetection, btnSobel, btnRoberts, btnCanny;
        private Button btnCustomKernel;
        private Panel convolutionControlPanel;
        private NumericUpDown[,] customKernelInputs = new NumericUpDown[3, 3];
        private NumericUpDown nudFactor, nudBias;
        private Button btnApplyCustomKernel;
        private bool isConvolutionActive = false;


        public enum MathOperation { Add, Multiply, Divide }
        public enum LogicalOperation { AND, OR, XOR, NOT }



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

            // : Tambahkan panel kontrol Black White di atas footer
            blackWhiteControlPanel = CreateBlackWhiteControlPanel();
            Controls.Add(blackWhiteControlPanel);

            brightnessControlPanel = CreateBrightnessControlPanel();
            Controls.Add(brightnessControlPanel);

            rotationControlPanel = CreateRotationControlPanel();
            Controls.Add(rotationControlPanel);

            translationControlPanel = CreateTranslationControlPanel();
            Controls.Add(translationControlPanel);

            rippleControlPanel = CreateRippleControlPanel();
            Controls.Add(rippleControlPanel);

            convolutionControlPanel = CreateConvolutionControlPanel();
            Controls.Add(convolutionControlPanel);

            sidebarPanel = CreateSidebar();
            Controls.Add(sidebarPanel);

            Panel header = CreateHeader();
            Controls.Add(header);

            SetButtonsEnabled(false);
        }

        private Panel CreateHeader()
        {
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

            // TOMBOL: Toggle Sidebar (Hamburger/X)
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

            // FUNGSI: Menyembunyikan atau menampilkan sidebar.
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
            
            // TOMBOL: Membuka file gambar dari komputer.
            btnPilihGambar = CreatePrimaryButton("📂 OPEN IMAGE", Colors.Primary);
            // FUNGSI: Membuka dialog pemilihan file dan memuat gambar yang dipilih.
            btnPilihGambar.Click += BtnPilihGambar_Click;

            // --- 1. PROCESSING TOOLS ---
            // TOMBOL: Toggle untuk menampilkan/menyembunyikan grup "Processing Tools".
            btnToggleTools = CreateToggleButton("🛠️ PROCESSING TOOLS", true);
            toolsLayout = CreateCollapsedPanel();
            // TOMBOL: Mengembalikan tampilan gambar ke kondisi asli.
            btnReset = CreateButton("🔄 Reset View", Colors.DarkTertiary, false);
            btnZoomIn = CreateButton("➕ Zoom In", Colors.DarkTertiary, false);
            btnZoomOut = CreateButton("➖ Zoom Out", Colors.DarkTertiary, false);
            btnFitToScreen = CreateButton("🔍 Fit to Screen", Colors.DarkTertiary, false);
            // TOMBOL: Mengekspor nilai R, G, B dari setiap piksel ke file teks.
            btnEkstrakRGB = CreateButton("📄 Export RGB Data (.txt)", Colors.DarkTertiary, false);
            // TOMBOL: Mengekspor nilai biner dari setiap channel R, G, B ke file teks.
            btnBiner = CreateButton("🔢 Export Binary Data (.txt)", Colors.DarkTertiary, false);

            // FUNGSI: Mengembalikan gambar ke aslinya.
            btnZoomIn.Click += BtnZoomIn_Click;
            btnZoomOut.Click += BtnZoomOut_Click;
            btnFitToScreen.Click += BtnFitToScreen_Click;


            btnReset.Click += BtnReset_Click;
            // FUNGSI: Memanggil metode EkstrakRGBData.
            btnEkstrakRGB.Click += (s, e) => EkstrakRGBData();
            // FUNGSI: Memanggil metode BtnBiner_Click.
            btnBiner.Click += BtnBiner_Click;

            toolsLayout.Controls.Add(btnReset);
            toolsLayout.Controls.Add(btnZoomIn);
            toolsLayout.Controls.Add(btnZoomOut);
            toolsLayout.Controls.Add(btnFitToScreen);
            toolsLayout.Controls.Add(btnEkstrakRGB);
            toolsLayout.Controls.Add(btnBiner);
            toolsLayout.Visible = true; // Default open
            // FUNGSI: Mengatur visibilitas panel 'toolsLayout'.
            btnToggleTools.Click += (s, e) => TogglePanelVisibility(toolsLayout, btnToggleTools);

            // --- 2. COLOR MANIPULATION ---
            // TOMBOL: Toggle untuk menampilkan/menyembunyikan grup "Color Manipulation".
            btnToggleColor = CreateToggleButton("🎨 COLOR MANIPULATION", false);
            colorLayout = CreateCollapsedPanel();
            // TOMBOL: Mengisolasi channel warna Merah.
            btnRed = CreateButton("🔴 Red Channel Isolation", Colors.DarkTertiary, false);
            // TOMBOL: Mengisolasi channel warna Hijau.
            btnGreen = CreateButton("🟢 Green Channel Isolation", Colors.DarkTertiary, false);
            // TOMBOL: Mengisolasi channel warna Biru.
            btnBlue = CreateButton("🔵 Blue Channel Isolation", Colors.DarkTertiary, false);
            // TOMBOL: Mengubah gambar menjadi grayscale (abu-abu).
            btnGrayscale = CreateButton("⚫ Grayscale Conversion", Colors.DarkTertiary, false);
            // TOMBOL: Mengaktifkan panel kontrol untuk binerisasi (hitam-putih).
            btnBlackWhite = CreateButton("⚪⚫ Black White Level", Colors.DarkTertiary, false);
            // TOMBOL: Membalikkan warna gambar (negasi).
            btnNegate = CreateButton("⚫ Invert / Negate", Colors.DarkTertiary, false);
            // TOMBOL: Mengaktifkan panel kontrol untuk penyesuaian kecerahan.
            btnBrightness = CreateButton("☀️ Brightness Level", Colors.DarkTertiary, false);
            // TOMBOL: Mengaktifkan mode interaktif untuk memilih warna dari gambar.
            btnColorSelect = CreateButton("🖱️ Interactive Color Select", Colors.DarkTertiary, false);

            // math operation
            btnToggleMath = CreateToggleButton("➕ MATH OPERATIONS", false);
            mathLayout = CreateCollapsedPanel();
            btnAddImage = CreateButton("➕", Colors.DarkTertiary, false);
            btnMultiplyImage = CreateButton("✖️", Colors.DarkTertiary, false);
            btnDivideImage = CreateButton("➗", Colors.DarkTertiary, false);

            // FUNGSI: Memanggil operasi matematika Penjumlahan.
            btnAddImage.Click += BtnAddImage_Click;
            // FUNGSI: Memanggil operasi matematika Perkalian.
            btnMultiplyImage.Click += (s, e) => BtnMathOperation_Click(MathOperation.Multiply);
            // FUNGSI: Memanggil operasi matematika Pembagian.
            btnDivideImage.Click += (s, e) => BtnMathOperation_Click(MathOperation.Divide);//  Handler

            mathLayout.Controls.Add(btnAddImage);
            mathLayout.Controls.Add(btnMultiplyImage);
            mathLayout.Controls.Add(btnDivideImage);  // 

            // TOMBOL: Toggle untuk menampilkan/menyembunyikan grup "Numeric Analysis".
            btnToggleNumeric = CreateToggleButton("🧮 NUMERIC ANALYSIS", false);
            numericLayout = CreateCollapsedPanel();

            // TOMBOL: Menganalisis hasil perkalian dua gambar secara numerik.
            btnAnalyzeMultiplication = CreateButton("A*B", Colors.DarkTertiary, false);
            // FUNGSI: Memanggil metode BtnAnalyzeMultiplication_Click.
            btnAnalyzeMultiplication.Click += BtnAnalyzeMultiplication_Click;

            // TOMBOL: Menganalisis hasil pembagian dua gambar secara numerik.
            btnAnalyzeDivision = CreateButton("A/B", Colors.DarkTertiary, false);
            // FUNGSI: Memanggil metode BtnAnalyzeDivision_Click.
            btnAnalyzeDivision.Click += BtnAnalyzeDivision_Click;

            numericLayout.Controls.Add(btnAnalyzeMultiplication);
            numericLayout.Controls.Add(btnAnalyzeDivision);

            // FUNGSI: Mengatur visibilitas panel 'numericLayout'.
            btnToggleNumeric.Click += (s, e) => TogglePanelVisibility(numericLayout, btnToggleNumeric);

            // FUNGSI: Mengatur visibilitas panel 'mathLayout'.
            btnToggleMath.Click += (s, e) => TogglePanelVisibility(mathLayout, btnToggleMath);



            btnRed.Click += (s, e) => DisplayChannel(Channel.Red);
            btnGreen.Click += (s, e) => DisplayChannel(Channel.Green);
            btnBlue.Click += (s, e) => DisplayChannel(Channel.Blue);
            // FUNGSI: Mengubah gambar menjadi grayscale.
            btnGrayscale.Click += (s, e) => DisplayGrayscale();
            // FUNGSI: Menampilkan/menyembunyikan panel kontrol hitam-putih.
            btnBlackWhite.Click += BtnBlackWhite_Click;
            // FUNGSI: Menerapkan efek negasi pada gambar.
            btnNegate.Click += BtnNegate_Click;
            // FUNGSI: Menampilkan/menyembunyikan panel kontrol kecerahan.
            btnBrightness.Click += BtnBrightness_Click;
            // FUNGSI: Mengaktifkan/menonaktifkan mode pemilihan warna.
            btnColorSelect.Click += BtnColorSelect_Click;

            colorLayout.Controls.Add(btnRed);
            colorLayout.Controls.Add(btnGreen);
            colorLayout.Controls.Add(btnBlue);
            colorLayout.Controls.Add(btnGrayscale);
            colorLayout.Controls.Add(btnNegate);
            colorLayout.Controls.Add(btnBrightness);
            colorLayout.Controls.Add(btnBlackWhite);
            colorLayout.Controls.Add(btnColorSelect);
            // FUNGSI: Mengatur visibilitas panel 'colorLayout'.
            btnToggleColor.Click += (s, e) => TogglePanelVisibility(colorLayout, btnToggleColor);

            // --- 3. VISUAL ANALYSIS ---
            // TOMBOL: Toggle untuk menampilkan/menyembunyikan grup "Visual Analysis".
            btnToggleAnalysis = CreateToggleButton("📈 VISUAL ANALYSIS", false);
            analysisLayout = CreateCollapsedPanel();
            // TOMBOL: Menampilkan jendela histogram gambar.
            btnHistogram = CreateButton("📊 Show Histogram", Colors.DarkTertiary, false);

            // FUNGSI: Menghitung dan menampilkan histogram.
            btnHistogram.Click += BtnHistogram_Click;

            analysisLayout.Controls.Add(btnHistogram);
            // FUNGSI: Mengatur visibilitas panel 'analysisLayout'.
            btnToggleAnalysis.Click += (s, e) => TogglePanelVisibility(analysisLayout, btnToggleAnalysis);

            // Menambahkan ke Layout Utama
            buttonLayout.Controls.Add(btnPilihGambar);
            buttonLayout.Controls.Add(CreateSpacer(15));

            // --- 4. IMAGE ROTATION ---
            btnToggleRotation = CreateToggleButton("🔄 IMAGE ROTATION", false);
            rotationLayout = CreateCollapsedPanel();
            btnRotate90 = CreateButton("↪️ Rotate 90°", Colors.DarkTertiary, false);
            btnRotate180 = CreateButton("🔄 Rotate 180°", Colors.DarkTertiary, false);
            btnRotate270 = CreateButton("↩️ Rotate 270°", Colors.DarkTertiary, false);
            btnRotate45 = CreateButton("↗️ Rotate 45°", Colors.DarkTertiary, false);
            btnRotateFree = CreateButton("📐 Free Rotate", Colors.DarkTertiary, false);
            btnTranslate = CreateButton("↔️ Translate / Pan", Colors.DarkTertiary, false);

            btnRotate90.Click += (s, e) => RotateImage(90);
            btnRotate180.Click += (s, e) => RotateImage(180);
            btnRotate270.Click += (s, e) => RotateImage(270);
            btnRotate45.Click += (s, e) => RotateImage(45);
            btnRotateFree.Click += BtnRotateFree_Click;
            btnTranslate.Click += BtnTranslate_Click;


            rotationLayout.Controls.Add(btnRotate90);
            rotationLayout.Controls.Add(btnRotate180);
            rotationLayout.Controls.Add(btnRotate270);
            rotationLayout.Controls.Add(btnRotate45);
            rotationLayout.Controls.Add(btnRotateFree);
            rotationLayout.Controls.Add(btnTranslate);
            btnToggleRotation.Click += (s, e) => TogglePanelVisibility(rotationLayout, btnToggleRotation);


            buttonLayout.Controls.Add(btnToggleTools);
            buttonLayout.Controls.Add(toolsLayout);

            buttonLayout.Controls.Add(btnToggleColor);
            buttonLayout.Controls.Add(colorLayout);

            buttonLayout.Controls.Add(btnToggleAnalysis);
            buttonLayout.Controls.Add(analysisLayout);

            buttonLayout.Controls.Add(btnToggleRotation);
            buttonLayout.Controls.Add(rotationLayout);

            buttonLayout.Controls.Add(btnToggleMath);
            buttonLayout.Controls.Add(mathLayout);

            buttonLayout.Controls.Add(btnToggleNumeric);
            buttonLayout.Controls.Add(numericLayout);

            // --- 5. LOGICAL OPERATIONS ---
            // TOMBOL: Toggle untuk menampilkan/menyembunyikan grup "Logical Operations".
            btnToggleLogical = CreateToggleButton("🔣 LOGICAL OPERATIONS", false);
            logicalLayout = CreateCollapsedPanel();
            // TOMBOL: Melakukan operasi logika AND antara dua gambar.
            btnAnd = CreateButton("AND", Colors.DarkTertiary, false);
            // TOMBOL: Melakukan operasi logika OR antara dua gambar.
            btnOr = CreateButton("OR", Colors.DarkTertiary, false);
            // TOMBOL: Melakukan operasi logika XOR antara dua gambar.
            btnXor = CreateButton("XOR", Colors.DarkTertiary, false);
            // TOMBOL: Melakukan operasi logika NOT pada gambar saat ini.
            btnNot = CreateButton("NOT (Negasi)", Colors.DarkTertiary, false);

            // FUNGSI: Memanggil operasi logika AND.
            btnAnd.Click += (s, e) => BtnLogicalOperation_Click(LogicalOperation.AND);
            // FUNGSI: Memanggil operasi logika OR.
            btnOr.Click += (s, e) => BtnLogicalOperation_Click(LogicalOperation.OR);
            // FUNGSI: Memanggil operasi logika XOR.
            btnXor.Click += (s, e) => BtnLogicalOperation_Click(LogicalOperation.XOR);
            // FUNGSI: Menerapkan operasi logika NOT.
            btnNot.Click += (s, e) => ApplyLogicalNot(); // Panggil langsung fungsi NOT

            logicalLayout.Controls.AddRange(new Control[] { btnAnd, btnOr, btnXor, btnNot });
            // FUNGSI: Mengatur visibilitas panel 'logicalLayout'.
            btnToggleLogical.Click += (s, e) => TogglePanelVisibility(logicalLayout, btnToggleLogical);

            // Tambahkan ke layout utama SETELAH dibuat
            buttonLayout.Controls.Add(btnToggleLogical);
            buttonLayout.Controls.Add(logicalLayout);

            // --- 6. DISTORTION EFFECTS ---
            btnToggleDistortion = CreateToggleButton("🌊 DISTORTION EFFECTS", false);
            distortionLayout = CreateCollapsedPanel();
            btnRippleEffect = CreateButton("💧 Ripple Effect", Colors.DarkTertiary, false);

            // FUNGSI: Mengaktifkan/menonaktifkan panel kontrol ripple effect.
            btnRippleEffect.Click += BtnRippleEffect_Click;

            distortionLayout.Controls.Add(btnRippleEffect);
            btnToggleDistortion.Click += (s, e) => TogglePanelVisibility(distortionLayout, btnToggleDistortion);

            buttonLayout.Controls.Add(btnToggleDistortion);
            buttonLayout.Controls.Add(distortionLayout);

            // --- 7. CONVOLUTION / FILTERS ---
            btnToggleConvolution = CreateToggleButton("🔬 CONVOLUTION / FILTERS", false);
            convolutionLayout = CreateCollapsedPanel();
            btnGaussianBlur = CreateButton("🌫️ Gaussian Blur", Colors.DarkTertiary, false);
            btnSharpen = CreateButton("✨ Sharpen", Colors.DarkTertiary, false);
            btnEdgeDetection = CreateButton("✒️ Edge Detection", Colors.DarkTertiary, false);
            btnSobel = CreateButton("📐 Sobel (Gx + Gy)", Colors.DarkTertiary, false);
            btnRoberts = CreateButton("❌ Roberts Cross", Colors.DarkTertiary, false);
            btnCanny = CreateButton("🦀 Canny Edge Detection", Colors.DarkTertiary, false);
            btnCustomKernel = CreateButton("⚙️ Custom 3x3 Kernel", Colors.DarkTertiary, false);

            btnGaussianBlur.Click += (s, e) => ApplyPredefinedConvolution("GaussianBlur");
            btnSharpen.Click += (s, e) => ApplyPredefinedConvolution("Sharpen");
            btnEdgeDetection.Click += (s, e) => ApplyPredefinedConvolution("EdgeDetection");
            btnSobel.Click += (s, e) => ApplySobelEdgeDetection();
            btnRoberts.Click += (s, e) => ApplyRobertsEdgeDetection();
            btnCanny.Click += (s, e) => ApplyCannyEdgeDetection();
            btnCustomKernel.Click += BtnCustomKernel_Click;

            convolutionLayout.Controls.AddRange(new Control[] { btnGaussianBlur, btnSharpen, btnEdgeDetection, btnSobel, btnRoberts, btnCanny, btnCustomKernel });
            btnToggleConvolution.Click += (s, e) => TogglePanelVisibility(convolutionLayout, btnToggleConvolution);

            buttonLayout.Controls.Add(btnToggleConvolution);
            buttonLayout.Controls.Add(convolutionLayout);


            sidebar.Controls.Add(buttonLayout);
            return sidebar;
        }

        // : Membuat FlowLayoutPanel tertutup
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
                Visible = false//  tertutup
            };
        }

        // : Tombol toggle untuk header grup
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

        // Menggambar panah toggle
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

        // logika agar arroy panel nya bisa di gunakan
        private void TogglePanelVisibility(FlowLayoutPanel panel, Button toggleButton)
        {
            panel.Visible = !panel.Visible;
            toggleButton.Tag = panel.Visible;
            toggleButton.Invalidate();
        }


        
        /// Fungsi ini dipanggil saat tombol toggle menu (hamburger/X) diklik.
        /// Fungsinya adalah untuk menampilkan atau menyembunyikan sidebar
        /// dengan mengubah lebar dan visibilitasnya.
        
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

        
        /// Helper function untuk menyesuaikan padding pada footer
        /// saat sidebar dibuka atau ditutup, agar layout tetap rapi.
        
        /// <param name="left">Padding kiri baru.</param>
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
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Colors.DarkPrimary
            };

            Panel canvasPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkPrimary, // Ubah background agar sesuai dengan tema
                BorderStyle = BorderStyle.None,
                AutoScroll = true // PENTING: Aktifkan auto-scroll
            };

            canvasPanel.Paint += (s, e) =>
            {
                // Gambar border di sekitar panel, bukan di PictureBox
                using (Pen borderPen = new Pen(Colors.DarkSecondary, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, canvasPanel.Width - 1, canvasPanel.Height - 1);
                }
            };

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.DarkSecondary,
                SizeMode = PictureBoxSizeMode.AutoSize, // PENTING: Ubah ke AutoSize
                BorderStyle = BorderStyle.None,
                Location = new Point(0, 0) // Mulai dari pojok kiri atas
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
                ColumnCount = 3, // Tambah satu kolom untuk Zoom
                RowCount = 2,
                Padding = new Padding(0)
            };
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Kolom untuk Zoom
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

            lblZoom = new Label
            {
                Text = "Zoom: 100%",
                Dock = DockStyle.Fill,
                ForeColor = Colors.TextSecondary,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false // Sembunyikan sampai ada gambar
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

            footerLayout.Controls.Add(lblPath, 1, 0);
            footerLayout.Controls.Add(lblStatus, 2, 0);
            footerLayout.Controls.Add(lblInfo, 1, 1);
            footerLayout.Controls.Add(progressBar, 2, 1);
            footerLayout.Controls.Add(lblZoom, 0, 1); // Tambahkan label zoom

            footer.Controls.Add(footerLayout);
            return footer;
        }

        private Panel CreateWelcomePanel()
        {
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

            return new Panel
            {
                Width = 250,
                Height = height,
                Margin = new Padding(0)
            };
        }

        private void SetButtonsEnabled(bool enabled)
        {

            btnZoomIn.Enabled = enabled;
            btnZoomOut.Enabled = enabled;
            btnFitToScreen.Enabled = enabled;
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
            // Aktifkan tombol logika
            btnAnd.Enabled = enabled;
            btnOr.Enabled = enabled;
            btnXor.Enabled = enabled;
            btnNot.Enabled = enabled;
            // Aktifkan tombol rotasi
            btnRotate45.Enabled = enabled;
            btnRotate90.Enabled = enabled;
            btnRotate180.Enabled = enabled;
            btnRotate270.Enabled = enabled;
            btnRotateFree.Enabled = enabled;
            btnTranslate.Enabled = enabled;

            // Aktifkan tombol distorsi
            btnRippleEffect.Enabled = enabled;

            // Aktifkan tombol konvolusi
            btnGaussianBlur.Enabled = enabled;
            btnSharpen.Enabled = enabled;
            btnEdgeDetection.Enabled = enabled;
            btnSobel.Enabled = enabled;
            btnRoberts.Enabled = enabled;
            btnCanny.Enabled = enabled;
            btnCustomKernel.Enabled = enabled;



            lblInfo.Text = enabled ? "Image loaded. Ready for processing." : "Ready";
            lblZoom.Visible = enabled;
        }



        
        /// Fungsi ini dipanggil saat tombol "OPEN IMAGE" diklik.
        /// Menampilkan dialog untuk memilih file gambar, kemudian memuat gambar tersebut
        /// ke dalam `originalImage` dan menampilkannya di `pictureBox`.
        
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
                FitImageToScreen(); // Panggil Fit to Screen saat gambar baru dimuat

                FileInfo fi = new FileInfo(currentImagePath);
                lblPath.Text = Path.GetFileName(currentImagePath).ToUpper();
                lblInfo.Text = $"{fi.Length / 1024:N0} KB · {((Bitmap)originalImage).Width} × {((Bitmap)originalImage).Height} px";


                SetButtonsEnabled(true);
            }
        }

        
        /// Fungsi ini dipanggil saat tombol "Reset View" diklik.
        /// Mengembalikan gambar yang ditampilkan di `pictureBox` ke gambar aslinya
        /// yang tersimpan di `originalImage`.
        
        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            FitImageToScreen(); // Reset view sekarang berarti fit to screen
            lblInfo.Text = "View reset to fit screen.";
        }

        
        /// Fungsi ini dipanggil saat tombol "Export RGB Data" diklik.
        /// Mengekstrak nilai Red, Green, dan Blue dari setiap piksel gambar asli,
        /// memformatnya ke dalam sebuah string, dan menyimpannya sebagai file .txt.
        /// Menggunakan LockBits untuk performa tinggi.
        
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

        
        /// Fungsi ini dipanggil saat tombol "Export Binary Data" diklik.
        /// Mengonversi nilai Red, Green, dan Blue dari setiap piksel ke dalam representasi biner (8-bit),
        /// memformatnya, dan menyimpannya sebagai file .txt.
        /// Menggunakan LockBits untuk performa tinggi.
        
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

        
        /// Fungsi pemrosesan untuk mengisolasi salah satu channel warna (Merah, Hijau, atau Biru).
        /// Piksel diubah sehingga hanya nilai dari channel yang dipilih yang dipertahankan,
        /// sementara dua channel lainnya diatur menjadi 0.
        
        /// <param name="channel">Channel warna yang ingin diisolasi (Red, Green, atau Blue).</param>
        private void DisplayChannel(Channel channel)
        {
            if (originalImage is not Bitmap originalBmp) return;

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

        
        /// Fungsi pemrosesan untuk mengubah gambar menjadi grayscale (skala abu-abu).
        /// Ini dilakukan dengan menghitung nilai rata-rata dari R, G, dan B untuk setiap piksel,
        /// lalu mengatur ketiga channel tersebut ke nilai rata-rata itu.
        
        private void DisplayGrayscale()
        {
            if (originalImage is not Bitmap originalBmp) return;
            HideControlPanels();
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

        
        /// Menghitung data histogram untuk channel Merah, Hijau, Biru, dan Grayscale.
        /// Fungsi ini mengiterasi setiap piksel gambar dan menghitung frekuensi
        /// dari setiap tingkat intensitas (0-255) untuk setiap channel.
        
        /// <returns>Sebuah Dictionary yang memetakan nama channel ke array frekuensi intensitasnya.</returns>
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

        
        /// Fungsi ini dipanggil saat tombol "Show Histogram" diklik.
        /// Ia memanggil `CalculateHistograms()` untuk mendapatkan data, lalu
        /// membuat dan menampilkan `HistogramForm` baru dengan data tersebut.
        
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
        
        /// Fungsi ini dipanggil saat tombol "Black White Level" diklik.
        /// Ia berfungsi sebagai toggle untuk menampilkan atau menyembunyikan
        /// panel kontrol `TrackBar` untuk binerisasi. Jika panel lain aktif,
        /// panel itu akan disembunyikan terlebih dahulu.
        
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

        
        /// Fungsi ini dipanggil setiap kali slider (TrackBar) untuk Black & White digerakkan.
        /// Ia memperbarui label threshold dan memanggil fungsi `ApplyBlackWhiteThreshold`
        /// secara real-time untuk menerapkan efek binerisasi.
        
        private void BwTrackBar_Scroll(object sender, EventArgs e)
        {
            int threshold = bwTrackBar.Value;
            lblBWThreshold.Text = $"Threshold: {threshold}";
            // Panggil fungsi pemrosesan gambar
            ApplyBlackWhiteThreshold(threshold);
        }

        
        /// Fungsi inti untuk menerapkan efek binerisasi (hitam-putih).
        /// Gambar pertama-tama diubah menjadi grayscale, kemudian setiap piksel dibandingkan
        /// dengan `thresholdValue`. Piksel menjadi putih jika lebih terang dari threshold,
        /// dan hitam jika lebih gelap.
        
        /// <param name="thresholdValue">Nilai ambang batas (0-255) untuk binerisasi.</param>
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

        
        /// Fungsi ini dipanggil saat tombol "Invert / Negate" diklik.
        /// Ia langsung memanggil fungsi `ApplyNegation` untuk membalikkan warna gambar.
        
        private void BtnNegate_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Silakan pilih gambar terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyNegation();
        }

        
        /// Fungsi inti untuk menerapkan efek negasi (invert warna).
        /// Nilai setiap channel warna (R, G, B) dari setiap piksel diubah
        /// menjadi `255 - nilai_asli`.
        
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
        
        /// Fungsi ini dipanggil saat tombol "Brightness Level" diklik.
        /// Ia berfungsi sebagai toggle untuk menampilkan atau menyembunyikan
        /// panel kontrol `TrackBar` untuk penyesuaian kecerahan. Jika panel lain aktif,
        /// panel itu akan disembunyikan terlebih dahulu.
        
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

        
        /// Fungsi ini dipanggil setiap kali slider (TrackBar) untuk Brightness digerakkan.
        /// Ia memperbarui label level dan memanggil fungsi `ApplyBrightness`
        /// secara real-time untuk menyesuaikan kecerahan gambar.
        
        private void BrTrackBar_Scroll(object sender, EventArgs e)
        {
            int level = brTrackBar.Value;
            lblBRLevel.Text = $"Level: {level}";
            // Panggil fungsi pemrosesan gambar
            ApplyBrightness(level);
        }
        
        /// Fungsi inti untuk menyesuaikan kecerahan gambar.
        /// Nilai `level` (bisa positif atau negatif) ditambahkan ke setiap channel warna (R, G, B)
        /// dari setiap piksel. Hasilnya dijaga agar tetap dalam rentang 0-255 (clamping)
        /// untuk menghindari warna yang tidak valid.
        
        /// <param name="level">Tingkat kecerahan yang akan ditambahkan (-255 hingga +255).</param>
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
        
        /// Fungsi ini dipanggil saat tombol "Interactive Color Select" diklik.
        /// Ia mengaktifkan atau menonaktifkan mode pemilihan warna. Saat aktif,
        /// pengguna dapat mengklik gambar untuk memilih warna, dan kursor akan berubah.
        /// Panel kontrol lain akan disembunyikan saat mode ini aktif.
        
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

        
        /// Fungsi ini dipanggil saat pengguna mengklik area `pictureBox`.
        /// Jika mode `isColorSelectActive` aktif, fungsi ini akan menghitung posisi piksel
        /// yang diklik pada gambar asli (memperhitungkan zoom), mengambil warnanya,
        /// dan memanggil `ApplyColorSelection` untuk memproses gambar.
        
        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isColorSelectActive || originalImage == null || pictureBox.Image == null)
            {
                return;
            }

            // Koordinat gambar asli
            int targetX, targetY;

            if (pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            {
                // Logika untuk mode AutoSize (saat di-zoom in/out)
                // Koordinat e.X dan e.Y sudah relatif terhadap gambar yang di-zoom.
                // Cukup dibagi dengan zoomFactor untuk mendapatkan koordinat di gambar asli.
                targetX = (int)(e.X / zoomFactor);
                targetY = (int)(e.Y / zoomFactor);
            }
            else // PictureBoxSizeMode.Zoom (saat fit to screen)
            {
                // Logika lama yang sudah benar untuk mode Zoom
                int x = e.X;
                int y = e.Y;

                if (originalImage.Width <= 0 || originalImage.Height <= 0) return;

                float imgRatio = (float)originalImage.Width / originalImage.Height;
                float boxRatio = (float)pictureBox.ClientSize.Width / pictureBox.ClientSize.Height;

                int drawWidth, drawHeight, offsetX, offsetY;

                if (imgRatio > boxRatio) // Dibatasi oleh lebar
                {
                    drawWidth = pictureBox.ClientSize.Width;
                    drawHeight = (int)(drawWidth / imgRatio);
                    offsetX = 0;
                    offsetY = (pictureBox.ClientSize.Height - drawHeight) / 2;
                }
                else // Dibatasi oleh tinggi
                {
                    drawWidth = (int)(pictureBox.ClientSize.Height * imgRatio);
                    drawHeight = pictureBox.ClientSize.Height;
                    offsetX = (pictureBox.ClientSize.Width - drawWidth) / 2;
                    offsetY = 0;
                }

                if (x < offsetX || x >= offsetX + drawWidth || y < offsetY || y >= offsetY + drawHeight)
                {
                    lblInfo.Text = "Clicked outside the image area. Please click on the picture.";
                    return;
                }

                targetX = (int)(((float)(x - offsetX) / drawWidth) * originalImage.Width);
                targetY = (int)(((float)(y - offsetY) / drawHeight) * originalImage.Height);
            }
            
            {
                // Fallback jika ada mode lain yang tidak terduga
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
        
        /// Fungsi inti untuk mengisolasi warna tertentu pada gambar.
        /// Hanya piksel yang warnanya "mirip" dengan `targetColor` (dalam rentang `tolerance`)
        //  yang akan dipertahankan. Piksel lain akan diubah menjadi hitam.
        
        /// <param name="targetColor">Warna yang ingin diisolasi.</param>
        /// <param name="tolerance">Toleransi perbedaan warna.</param>
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
        
        /// Helper function untuk menyembunyikan semua panel kontrol interaktif
        /// (Black & White, Brightness) dan menonaktifkan mode Color Select.
        
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

            // Sembunyikan panel Rotasi jika aktif
            if (isRotationActive)
            {
                isRotationActive = false;
                rotationControlPanel.Visible = false;
                btnRotateFree.BackColor = Colors.DarkTertiary;
            }

            // Sembunyikan panel Translasi jika aktif
            if (isTranslationActive)
            {
                isTranslationActive = false;
                translationControlPanel.Visible = false;
                btnTranslate.BackColor = Colors.DarkTertiary;
            }

            // Sembunyikan panel Ripple jika aktif
            if (isRippleEffectActive)
            {
                isRippleEffectActive = false;
                rippleControlPanel.Visible = false;
                btnRippleEffect.BackColor = Colors.DarkTertiary;
            }

            // Sembunyikan panel Konvolusi jika aktif
            if (isConvolutionActive)
            {
                isConvolutionActive = false;
                convolutionControlPanel.Visible = false;
                btnCustomKernel.BackColor = Colors.DarkTertiary;
            }

        }

        private Panel CreateRotationControlPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(30, 10, 30, 10),
                Visible = false
            };

            rotationTrackBar = new TrackBar
            {
                Dock = DockStyle.Fill,
                Minimum = -180,
                Maximum = 180,
                Value = 0,
                TickFrequency = 15,
                SmallChange = 5,
                LargeChange = 45,
                BackColor = Colors.DarkSecondary,
                Cursor = Cursors.Hand,
                Height = 40
            };
            rotationTrackBar.Scroll += RotationTrackBar_Scroll;

            lblRotationAngle = new Label
            {
                Text = "Angle: 0°",
                Width = 120,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            Label lblLeft = new Label { Text = "-180°", Width = 80, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };
            Label lblRight = new Label { Text = "+180°", Width = 100, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0), Margin = new Padding(0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(lblLeft, 0, 0);
            layout.Controls.Add(rotationTrackBar, 1, 0);
            layout.Controls.Add(lblRight, 2, 0);
            layout.Controls.Add(lblRotationAngle, 3, 0);

            panel.Controls.Add(layout);
            panel.Paint += (s, e) => { using (Pen borderPen = new Pen(Colors.Border, 1)) { e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0); } };

            return panel;
        }

        private void BtnRotateFree_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            HideControlPanels();

            isRotationActive = !isRotationActive;
            rotationControlPanel.Visible = isRotationActive;

            if (isRotationActive)
            {
                BtnReset_Click(null, null);
                rotationTrackBar.Value = 0;
                lblRotationAngle.Text = "Angle: 0°";
                RotateImage(0);
                btnRotateFree.BackColor = Colors.DarkHover;
                Controls.SetChildIndex(rotationControlPanel, 1);
            }
            else
            {
                BtnReset_Click(null, null);
                btnRotateFree.BackColor = Colors.DarkTertiary;
            }
        }

        private void RotationTrackBar_Scroll(object sender, EventArgs e)
        {
            int angle = rotationTrackBar.Value;
            lblRotationAngle.Text = $"Angle: {angle}°";
            RotateImage(angle);
        }

        private void RotateImage(float angle)
        {
            if (originalImage == null) return;

            // Sembunyikan panel lain jika tidak dalam mode rotasi bebas
            if (!isRotationActive)
            {
                HideControlPanels();
            }

            try
            {
                Bitmap originalBmp = new Bitmap(originalImage);

                // Konversi sudut ke radian
                double angleRad = angle * Math.PI / 180.0;

                // Hitung ukuran bounding box baru untuk menampung gambar yang diputar
                double cos = Math.Abs(Math.Cos(angleRad));
                double sin = Math.Abs(Math.Sin(angleRad));
                int newWidth = (int)(originalBmp.Width * cos + originalBmp.Height * sin);
                int newHeight = (int)(originalBmp.Width * sin + originalBmp.Height * cos);

                // Buat bitmap baru dengan ukuran yang sudah dihitung
                Bitmap rotatedBmp = new Bitmap(newWidth, newHeight);
                rotatedBmp.SetResolution(originalBmp.HorizontalResolution, originalBmp.VerticalResolution);

                using (Graphics g = Graphics.FromImage(rotatedBmp))
                {
                    // Atur latar belakang transparan
                    g.Clear(Color.Transparent);

                    // Pindahkan titik pivot rotasi ke tengah gambar baru
                    g.TranslateTransform(newWidth / 2f, newHeight / 2f);

                    // Lakukan rotasi
                    g.RotateTransform(angle);

                    // Pindahkan kembali titik pivot agar gambar asli tergambar di tengah
                    g.TranslateTransform(-originalBmp.Width / 2f, -originalBmp.Height / 2f);

                // Gambar citra asli ke atas kanvas yang sudah diputar dengan interpolasi yang baik
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(originalBmp, new Point(0, 0));
                }

            UpdatePictureBoxImage(rotatedBmp);

                if (isRotationActive)
                {
                    lblInfo.Text = $"Image rotated by {angle} degrees.";
                }
                else
                {
                    lblInfo.Text = $"Applied {angle}° rotation.";
                }

                originalBmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during rotation: {ex.Message}", "Rotation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BtnReset_Click(null, null);
            }
        }

        private Panel CreateTranslationControlPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(30, 10, 30, 10),
                Visible = false
            };

            xTranslationTrackBar = new TrackBar { Dock = DockStyle.Fill, Minimum = -500, Maximum = 500, Value = 0, TickFrequency = 50, BackColor = Colors.DarkSecondary, Cursor = Cursors.Hand };
            yTranslationTrackBar = new TrackBar { Dock = DockStyle.Fill, Minimum = -500, Maximum = 500, Value = 0, TickFrequency = 50, BackColor = Colors.DarkSecondary, Cursor = Cursors.Hand };

            xTranslationTrackBar.Scroll += (s, e) => TranslationTrackBar_Scroll();
            yTranslationTrackBar.Scroll += (s, e) => TranslationTrackBar_Scroll();

            lblXOffset = new Label { Text = "X: 0px", Width = 100, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Colors.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblYOffset = new Label { Text = "Y: 0px", Width = 100, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Colors.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            Label lblX = new Label { Text = "X-Axis", Width = 50, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };
            Label lblY = new Label { Text = "Y-Axis", Width = 50, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(0), Margin = new Padding(0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            layout.Controls.Add(lblX, 0, 0);
            layout.Controls.Add(xTranslationTrackBar, 1, 0);
            layout.Controls.Add(lblXOffset, 2, 0);
            layout.Controls.Add(lblY, 0, 1);
            layout.Controls.Add(yTranslationTrackBar, 1, 1);
            layout.Controls.Add(lblYOffset, 2, 1);

            panel.Controls.Add(layout);
            panel.Paint += (s, e) => { using (Pen borderPen = new Pen(Colors.Border, 1)) { e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0); } };

            return panel;
        }

        private void BtnTranslate_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            HideControlPanels();

            isTranslationActive = !isTranslationActive;
            translationControlPanel.Visible = isTranslationActive;

            if (isTranslationActive)
            {
                // Set trackbar range based on image size
                xTranslationTrackBar.Minimum = -originalImage.Width;
                xTranslationTrackBar.Maximum = originalImage.Width;
                yTranslationTrackBar.Minimum = -originalImage.Height;
                yTranslationTrackBar.Maximum = originalImage.Height;

                xTranslationTrackBar.Value = 0;
                yTranslationTrackBar.Value = 0;

                TranslationTrackBar_Scroll(); // Apply initial state (0,0)

                btnTranslate.BackColor = Colors.DarkHover;
                Controls.SetChildIndex(translationControlPanel, 1);
            }
            else
            {
                BtnReset_Click(null, null);
                btnTranslate.BackColor = Colors.DarkTertiary;
            }
        }

        private void TranslationTrackBar_Scroll()
        {
            int xOffset = xTranslationTrackBar.Value;
            int yOffset = yTranslationTrackBar.Value;

            lblXOffset.Text = $"X: {xOffset}px";
            lblYOffset.Text = $"Y: {yOffset}px";

            TranslateImage(xOffset, yOffset);
        }

        private void TranslateImage(int offsetX, int offsetY)
        {
            if (originalImage == null) return;

            try
            {
                Bitmap originalBmp = new Bitmap(originalImage);
                Bitmap translatedBmp = new Bitmap(originalBmp.Width, originalBmp.Height, originalBmp.PixelFormat);

                using (Graphics g = Graphics.FromImage(translatedBmp))
                {
                    // Atur latar belakang transparan atau warna lain
                    g.Clear(Color.Transparent);

                    // Gambar citra asli dengan offset
                    g.DrawImage(originalBmp, new Rectangle(offsetX, offsetY, originalBmp.Width, originalBmp.Height));
                }

                UpdatePictureBoxImage(translatedBmp);

                lblInfo.Text = $"Image translated by (X:{offsetX}, Y:{offsetY}).";
                originalBmp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during translation: {ex.Message}", "Translation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BtnReset_Click(null, null);
            }
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            zoomFactor *= 1.25f; // Perbesar 25%
            if (zoomFactor > 10.0f) zoomFactor = 10.0f; // Batas zoom maksimal 1000%
            ApplyZoom();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            zoomFactor /= 1.25f; // Perkecil 25%
            if (zoomFactor < 0.1f) zoomFactor = 0.1f; // Batas zoom minimal 10%
            ApplyZoom();
        }

        private void BtnFitToScreen_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            FitImageToScreen();
        }

        private void ApplyZoom()
        {
            if (originalImage == null) return;

            // Ubah SizeMode ke AutoSize agar ukuran PictureBox bisa diubah
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;

            // Hitung ukuran baru berdasarkan faktor zoom
            int newWidth = (int)(originalImage.Width * zoomFactor);
            int newHeight = (int)(originalImage.Height * zoomFactor);

            // Buat bitmap baru dengan ukuran yang sudah di-zoom
            Bitmap zoomedBmp = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(zoomedBmp))
            {
                // Gunakan kualitas interpolasi tinggi untuk hasil yang lebih baik
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            // Terapkan gambar baru ke PictureBox
            UpdatePictureBoxImage(zoomedBmp);

            // Update label
            lblZoom.Text = $"Zoom: {zoomFactor:P0}";
            lblInfo.Text = $"Image zoomed to {zoomFactor:P0}.";
        }

        private void FitImageToScreen()
        {
            if (originalImage == null) return;

            // Kembalikan SizeMode ke Zoom agar pas di layar
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            UpdatePictureBoxImage(new Bitmap(originalImage)); // Tampilkan gambar asli

            // Hitung zoom factor yang efektif saat 'Fit to Screen'
            Panel canvas = pictureBox.Parent as Panel;
            if (canvas != null)
            {
                float ratioX = (float)canvas.ClientSize.Width / originalImage.Width;
                float ratioY = (float)canvas.ClientSize.Height / originalImage.Height;
                zoomFactor = Math.Min(ratioX, ratioY); // Faktor zoom adalah rasio terkecil
            }

            lblZoom.Text = $"Zoom: {zoomFactor:P0}";
            lblInfo.Text = "Image fit to screen.";
        }

        
        /// Event handler untuk tombol tambah (+).
        /// Ini adalah shortcut yang langsung memanggil `BtnMathOperation_Click`
        /// dengan operasi `Add`.
        
        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            BtnMathOperation_Click(MathOperation.Add);
        }
        
        /// Fungsi inti untuk menjumlahkan dua gambar.
        /// Menjumlahkan nilai R, G, B dari setiap piksel yang bersesuaian dari kedua gambar.
        /// Hasilnya di-clamp pada 255 untuk mencegah overflow.
        
        /// <returns>Bitmap baru yang merupakan hasil penjumlahan.</returns>
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
        
        /// Fungsi generik yang menangani semua operasi matematika (Tambah, Kali, Bagi).
        /// Ia membuka dialog untuk memilih gambar kedua, memastikan dimensinya sama,
        /// memanggil fungsi matematika yang sesuai, dan menampilkan hasilnya di `ImageMathForm`.
        
        /// <param name="operation">Jenis operasi matematika yang akan dilakukan.</param>
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
        
        /// Fungsi inti untuk mengalikan dua gambar.
        /// Mengalikan nilai R, G, B dari setiap piksel yang bersesuaian.
        /// Hasilnya di-clamp pada 255.
        
        /// <returns>Bitmap baru yang merupakan hasil perkalian.</returns>
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

        
        /// Fungsi inti untuk membagi dua gambar.
        /// Membagi nilai R, G, B dari piksel gambar pertama dengan piksel gambar kedua.
        /// Menangani kasus pembagian dengan nol dengan menghasilkan nilai 255.
        
        /// <returns>Bitmap baru yang merupakan hasil pembagian.</returns>
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
        
        /// Fungsi ini dipanggil saat tombol "A/B" di grup Numeric Analysis diklik.
        /// Ia meminta pengguna memilih gambar kedua, lalu memanggil `AnalyzeImageDivision`
        /// untuk melakukan analisis numerik pada hasil pembagian.
        
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

        
        /// Melakukan operasi pembagian gambar, kemudian menghitung dan menampilkan
        /// statistik numerik (rata-rata intensitas per channel) dari gambar hasil
        /// dalam sebuah MessageBox, tanpa menampilkan gambar hasilnya secara visual.
        
        /// <param name="bmp2">Bitmap kedua yang akan menjadi pembagi.</param>
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
        
        /// Fungsi ini dipanggil saat tombol "A*B" di grup Numeric Analysis diklik.
        /// Ia meminta pengguna memilih gambar kedua, lalu memanggil `AnalyzeImageMultiplication`
        /// untuk melakukan analisis numerik pada hasil perkalian.
        
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

        
        /// Melakukan operasi perkalian gambar, kemudian menghitung dan menampilkan
        /// statistik numerik (rata-rata intensitas per channel) dari gambar hasil
        /// dalam sebuah MessageBox.
        
        /// <param name="bmp2">Bitmap kedua yang akan menjadi pengali.</param>
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
        
        /// Fungsi generik yang menangani operasi logika bitwise (AND, OR, XOR).
        /// Ia membuka dialog untuk memilih gambar kedua, memastikan dimensinya sama,
        /// memanggil `ApplyLogicalOperation` untuk memproses, dan menampilkan hasilnya
        /// di `ImageLogicForm`.
        
        /// <param name="operation">Jenis operasi logika yang akan dilakukan.</param>
        private void BtnLogicalOperation_Click(LogicalOperation operation)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Citra utama belum dimuat. Silakan buka citra terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HideControlPanels();

            // Operasi NOT (unary) ditangani oleh event handler-nya sendiri
            if (operation == LogicalOperation.NOT)
            {
                // Kode ini seharusnya tidak pernah dijangkau karena event handler NOT diubah
                ApplyLogicalNot(); return;
            }
            using (OpenFileDialog ofd = new OpenFileDialog
            {
                Title = $"Select Second Image for {operation} Operation",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                FilterIndex = 1
            })
            {
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

                    if (originalImage.Width != secondImage.Width || originalImage.Height != secondImage.Height)
                    {
                        MessageBox.Show("Operasi logika gagal. Kedua citra harus memiliki dimensi yang sama.", "Error Dimensi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        secondImage.Dispose();
                        return;
                    }

                    // Lakukan operasi logika untuk mendapatkan gambar hasil
                    Bitmap resultImage = ApplyLogicalOperation((Bitmap)originalImage, (Bitmap)secondImage, operation);

                    if (resultImage != null)
                    {
                        // Tampilkan hasil dalam Form baru (ImageLogicForm)
                        using (ImageLogicForm logicForm = new ImageLogicForm(originalImage, secondImage, resultImage, operation))
                        {
                            logicForm.ShowDialog();
                        }
                        BtnReset_Click(null, null);
                    }
                }
            }
        }

        
        /// Fungsi inti yang melakukan operasi logika bitwise (AND, OR, XOR) per piksel
        /// antara dua gambar.
        
        /// <param name="operation">Operasi logika yang akan diterapkan.</param>
        /// <returns>Bitmap baru yang merupakan hasil dari operasi logika.</returns>
        private Bitmap ApplyLogicalOperation(Bitmap bmp1, Bitmap bmp2, LogicalOperation operation)
        {
            Bitmap resultBmp = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format32bppArgb);
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);

            BitmapData data1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData data2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dataR = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int totalBytes = data1.Stride * bmp1.Height;
            byte[] rgbValues1 = new byte[totalBytes];
            byte[] rgbValues2 = new byte[totalBytes];
            byte[] rgbValuesR = new byte[totalBytes];

            Marshal.Copy(data1.Scan0, rgbValues1, 0, totalBytes);
            Marshal.Copy(data2.Scan0, rgbValues2, 0, totalBytes);

            for (int i = 0; i < totalBytes; i += 4)
            {
                switch (operation)
                {
                    case LogicalOperation.AND:
                        rgbValuesR[i] = (byte)(rgbValues1[i] & rgbValues2[i]);         // B
                        rgbValuesR[i + 1] = (byte)(rgbValues1[i + 1] & rgbValues2[i + 1]); // G
                        rgbValuesR[i + 2] = (byte)(rgbValues1[i + 2] & rgbValues2[i + 2]); // R
                        break;
                    case LogicalOperation.OR:
                        rgbValuesR[i] = (byte)(rgbValues1[i] | rgbValues2[i]);         // B
                        rgbValuesR[i + 1] = (byte)(rgbValues1[i + 1] | rgbValues2[i + 1]); // G
                        rgbValuesR[i + 2] = (byte)(rgbValues1[i + 2] | rgbValues2[i + 2]); // R
                        break;
                    case LogicalOperation.XOR:
                        rgbValuesR[i] = (byte)(rgbValues1[i] ^ rgbValues2[i]);         // B
                        rgbValuesR[i + 1] = (byte)(rgbValues1[i + 1] ^ rgbValues2[i + 1]); // G
                        rgbValuesR[i + 2] = (byte)(rgbValues1[i + 2] ^ rgbValues2[i + 2]); // R
                        break;
                }
                rgbValuesR[i + 3] = 255; // Alpha
            }

            Marshal.Copy(rgbValuesR, 0, dataR.Scan0, totalBytes);

            bmp1.UnlockBits(data1);
            bmp2.UnlockBits(data2);
            resultBmp.UnlockBits(dataR);

            return resultBmp;
        }

        
        /// Fungsi ini dipanggil oleh tombol "NOT".
        /// Ia menerapkan operasi negasi bitwise (bitwise NOT atau `~`) pada setiap channel warna
        /// dari gambar yang sedang ditampilkan di `pictureBox`.
        
        private void ApplyLogicalNot()
        {
            HideControlPanels();
            if (pictureBox.Image is not Bitmap currentBmp) return;

            Bitmap resultBmp = new Bitmap(currentBmp); // Buat salinan untuk diubah
            Rectangle rect = new Rectangle(0, 0, resultBmp.Width, resultBmp.Height);
            BitmapData data = resultBmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            int totalBytes = data.Stride * resultBmp.Height;
            byte[] rgbValues = new byte[totalBytes];
            Marshal.Copy(data.Scan0, rgbValues, 0, totalBytes);

            for (int i = 0; i < totalBytes; i++)
            {
                if ((i + 1) % 4 != 0) // Abaikan channel Alpha
                    rgbValues[i] = (byte)~rgbValues[i];
            }

            Marshal.Copy(rgbValues, 0, data.Scan0, totalBytes);
            resultBmp.UnlockBits(data);

            UpdatePictureBoxImage(resultBmp);
            lblInfo.Text = "Logical NOT (bitwise negation) operation completed.";
        }

        
        /// Override metode Dispose untuk memastikan semua resource gambar
        /// (terutama `originalImage` dan gambar di `pictureBox`) dibebaskan dari memori saat form ditutup.
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                originalImage?.Dispose();
                pictureBox.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        // --- Fitur Distorsi Citra (Ripple Effect) ---

        private Panel CreateRippleControlPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = Colors.DarkSecondary,
                Padding = new Padding(30, 10, 30, 10),
                Visible = false
            };

            amplitudeTrackBar = new TrackBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 50, Value = 10, TickFrequency = 5, BackColor = Colors.DarkSecondary, Cursor = Cursors.Hand };
            frequencyTrackBar = new TrackBar { Dock = DockStyle.Fill, Minimum = 5, Maximum = 100, Value = 20, TickFrequency = 10, BackColor = Colors.DarkSecondary, Cursor = Cursors.Hand };

            amplitudeTrackBar.Scroll += (s, e) => RippleTrackBar_Scroll();
            frequencyTrackBar.Scroll += (s, e) => RippleTrackBar_Scroll();

            lblAmplitude = new Label { Text = "Amplitude: 10", Width = 120, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Colors.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblFrequency = new Label { Text = "Frequency: 20", Width = 120, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Colors.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            Label lblAmp = new Label { Text = "Amplitude", Width = 70, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };
            Label lblFreq = new Label { Text = "Frequency", Width = 70, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextMuted, Font = new Font("Segoe UI", 9f) };

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(0), Margin = new Padding(0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            layout.Controls.Add(lblAmp, 0, 0);
            layout.Controls.Add(amplitudeTrackBar, 1, 0);
            layout.Controls.Add(lblAmplitude, 2, 0);
            layout.Controls.Add(lblFreq, 0, 1);
            layout.Controls.Add(frequencyTrackBar, 1, 1);
            layout.Controls.Add(lblFrequency, 2, 1);

            panel.Controls.Add(layout);
            panel.Paint += (s, e) => { using (Pen borderPen = new Pen(Colors.Border, 1)) { e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0); } };

            return panel;
        }

        private void BtnRippleEffect_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            HideControlPanels();

            isRippleEffectActive = !isRippleEffectActive;
            rippleControlPanel.Visible = isRippleEffectActive;

            if (isRippleEffectActive)
            {
                BtnReset_Click(null, null);
                RippleTrackBar_Scroll(); // Terapkan dengan nilai default
                btnRippleEffect.BackColor = Colors.DarkHover;
                Controls.SetChildIndex(rippleControlPanel, 1);
            }
            else
            {
                BtnReset_Click(null, null);
                btnRippleEffect.BackColor = Colors.DarkTertiary;
            }
        }

        private void RippleTrackBar_Scroll()
        {
            int amplitude = amplitudeTrackBar.Value;
            int frequency = frequencyTrackBar.Value;

            lblAmplitude.Text = $"Amplitude: {amplitude}";
            lblFrequency.Text = $"Frequency: {frequency}";

            ApplyRippleEffect(amplitude, frequency);
        }

        private void ApplyRippleEffect(int amplitude, int frequency)
        {
            if (originalImage is not Bitmap originalBmp) return;

            // Jangan proses jika frekuensi 0 untuk menghindari division by zero
            if (frequency == 0) return;

            Bitmap resultBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
            BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resultData = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytesPerPixel = 4;
            int byteCount = originalData.Stride * originalData.Height;
            byte[] originalBytes = new byte[byteCount];
            byte[] resultBytes = new byte[byteCount];

            Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

            int width = originalBmp.Width;
            int height = originalBmp.Height;
            int stride = originalData.Stride;

            double centerX = width / 2.0;
            double centerY = height / 2.0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // Hitung offset berdasarkan gelombang sinus dari jarak ke pusat
                    double offset = amplitude * Math.Sin(distance / frequency);

                    // Hitung posisi sumber piksel (source pixel)
                    // Tambahkan offset ke posisi x dan y saat ini
                    int srcX = (int)(x + offset);
                    int srcY = (int)(y + offset);

                    int currentPos = y * stride + x * bytesPerPixel;

                    // Pastikan koordinat sumber berada dalam batas gambar
                    if (srcX >= 0 && srcX < width && srcY >= 0 && srcY < height)
                    {
                        int srcPos = srcY * stride + srcX * bytesPerPixel;
                        resultBytes[currentPos] = originalBytes[srcPos];         // Blue
                        resultBytes[currentPos + 1] = originalBytes[srcPos + 1]; // Green
                        resultBytes[currentPos + 2] = originalBytes[srcPos + 2]; // Red
                        resultBytes[currentPos + 3] = originalBytes[srcPos + 3]; // Alpha
                    }
                    else
                    {
                        // Jika di luar batas, buat piksel menjadi transparan atau hitam
                        resultBytes[currentPos] = 0;
                        resultBytes[currentPos + 1] = 0;
                        resultBytes[currentPos + 2] = 0;
                        resultBytes[currentPos + 3] = 0; // Transparan
                    }
                }
            }

            Marshal.Copy(resultBytes, 0, resultData.Scan0, byteCount);

            originalBmp.UnlockBits(originalData);
            resultBmp.UnlockBits(resultData);

            UpdatePictureBoxImage(resultBmp);
            lblInfo.Text = $"Ripple effect applied with Amplitude: {amplitude}, Frequency: {frequency}.";
        }

        /// <summary>
        /// Helper method to safely update the PictureBox image, disposing the old one if necessary.
        /// </summary>
        /// <param name="newImage">The new image to display.</param>
        private void UpdatePictureBoxImage(Image newImage)
        {
            Image oldImage = pictureBox.Image;
            pictureBox.Image = newImage;
            oldImage?.Dispose();
        }
    

    // --- Fitur Konvolusi ---

    private Panel CreateConvolutionControlPanel()
    {
        Panel panel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 160,
            BackColor = Colors.DarkSecondary,
            Padding = new Padding(20),
            Visible = false
        };

        Label title = new Label { Text = "Custom 3x3 Kernel Matrix", Dock = DockStyle.Top, Height = 25, ForeColor = Colors.TextPrimary, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };

        TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        TableLayoutPanel kernelGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
        for (int i = 0; i < 3; i++)
        {
            kernelGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            kernelGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        }

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                customKernelInputs[x, y] = new NumericUpDown
                {
                    Dock = DockStyle.Fill,
                    Minimum = -255,
                    Maximum = 255,
                    Value = (x == 1 && y == 1) ? 1 : 0,
                    TextAlign = HorizontalAlignment.Center,
                    Font = new Font("Segoe UI", 9f),
                    Margin = new Padding(3)
                };
                kernelGrid.Controls.Add(customKernelInputs[x, y], x, y);
            }
        }

        TableLayoutPanel settingsLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Label lblFactor = new Label { Text = "Factor (Divisor):", Anchor = AnchorStyles.Left, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextSecondary };
        nudFactor = new NumericUpDown { Minimum = 1, Maximum = 1000, Value = 1, Dock = DockStyle.Fill, Margin = new Padding(3) };

        Label lblBias = new Label { Text = "Bias (Offset):", Anchor = AnchorStyles.Left, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Colors.TextSecondary };
        nudBias = new NumericUpDown { Minimum = -255, Maximum = 255, Value = 0, Dock = DockStyle.Fill, Margin = new Padding(3) };

        btnApplyCustomKernel = CreatePrimaryButton("APPLY KERNEL", Colors.Primary);
        btnApplyCustomKernel.Click += BtnApplyCustomKernel_Click;

        settingsLayout.Controls.Add(lblFactor, 0, 0);
        settingsLayout.Controls.Add(nudFactor, 1, 0);
        settingsLayout.Controls.Add(lblBias, 0, 1);
        settingsLayout.Controls.Add(nudBias, 1, 1);
        settingsLayout.Controls.Add(btnApplyCustomKernel, 0, 2);
        settingsLayout.SetColumnSpan(btnApplyCustomKernel, 2);

        mainLayout.Controls.Add(kernelGrid, 0, 0);
        mainLayout.Controls.Add(settingsLayout, 1, 0);

        panel.Controls.Add(mainLayout);
        panel.Controls.Add(title);
        panel.Paint += (s, e) => { using (Pen borderPen = new Pen(Colors.Border, 1)) { e.Graphics.DrawLine(borderPen, 0, 0, panel.Width, 0); } };

        return panel;
    }

    private void BtnCustomKernel_Click(object sender, EventArgs e)
    {
        if (originalImage == null) return;
        HideControlPanels();

        isConvolutionActive = !isConvolutionActive;
        convolutionControlPanel.Visible = isConvolutionActive;

        if (isConvolutionActive)
        {
            btnCustomKernel.BackColor = Colors.DarkHover;
            Controls.SetChildIndex(convolutionControlPanel, 1);
        }
        else
        {
            BtnReset_Click(null, null);
            btnCustomKernel.BackColor = Colors.DarkTertiary;
        }
    }

    private void BtnApplyCustomKernel_Click(object sender, EventArgs e)
    {
        float[,] kernel = new float[3, 3];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                kernel[x, y] = (float)customKernelInputs[x, y].Value;
            }
        }

        float factor = (float)nudFactor.Value;
        int bias = (int)nudBias.Value;

        ApplyConvolution(kernel, factor, bias);
        lblInfo.Text = "Custom 3x3 convolution kernel applied.";
    }

    private void ApplySobelEdgeDetection()
    {
        if (originalImage is not Bitmap originalBmp) return;

        HideControlPanels();
        Bitmap resultBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);

        Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
        BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData resultData = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        int byteCount = originalData.Stride * originalData.Height;
        byte[] originalBytes = new byte[byteCount];
        byte[] resultBytes = new byte[byteCount];

        Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

        int stride = originalData.Stride;
        int width = originalBmp.Width;
        int height = originalBmp.Height;
        int bytesPerPixel = 4;

        // Definisi Kernel Sobel X dan Sobel Y
        int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                double sumX_B = 0, sumX_G = 0, sumX_R = 0;
                double sumY_B = 0, sumY_G = 0, sumY_R = 0;

                // Konvolusi terpisah untuk Sobel X dan Sobel Y
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int pos = (y + ky) * stride + (x + kx) * bytesPerPixel;
                        byte b = originalBytes[pos];
                        byte g = originalBytes[pos + 1];
                        byte r = originalBytes[pos + 2];

                        // Hitung gradien X
                        sumX_B += b * gx[ky + 1, kx + 1];
                        sumX_G += g * gx[ky + 1, kx + 1];
                        sumX_R += r * gx[ky + 1, kx + 1];

                        // Hitung gradien Y
                        sumY_B += b * gy[ky + 1, kx + 1];
                        sumY_G += g * gy[ky + 1, kx + 1];
                        sumY_R += r * gy[ky + 1, kx + 1];
                    }
                }

                // Satukan kembali (Magnitude = sqrt(Gx^2 + Gy^2))
                int currentPos = y * stride + x * bytesPerPixel;
                resultBytes[currentPos] = (byte)Math.Min(255, Math.Sqrt(sumX_B * sumX_B + sumY_B * sumY_B));     // B
                resultBytes[currentPos + 1] = (byte)Math.Min(255, Math.Sqrt(sumX_G * sumX_G + sumY_G * sumY_G)); // G
                resultBytes[currentPos + 2] = (byte)Math.Min(255, Math.Sqrt(sumX_R * sumX_R + sumY_R * sumY_R)); // R
                resultBytes[currentPos + 3] = 255; // Alpha
            }
        }

        Marshal.Copy(resultBytes, 0, resultData.Scan0, byteCount);
        originalBmp.UnlockBits(originalData);
        resultBmp.UnlockBits(resultData);

        UpdatePictureBoxImage(resultBmp);
        lblInfo.Text = "Sobel Edge Detection applied (Combined X & Y).";
    }

    private void ApplyRobertsEdgeDetection()
    {
        if (originalImage is not Bitmap originalBmp) return;

        HideControlPanels();
        Bitmap resultBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);

        Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
        BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData resultData = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        int byteCount = originalData.Stride * originalData.Height;
        byte[] originalBytes = new byte[byteCount];
        byte[] resultBytes = new byte[byteCount];

        Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

        int stride = originalData.Stride;
        int width = originalBmp.Width;
        int height = originalBmp.Height;
        int bytesPerPixel = 4;

        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int currentPos = y * stride + x * bytesPerPixel;
                int nextXPos = y * stride + (x + 1) * bytesPerPixel;
                int nextYPos = (y + 1) * stride + x * bytesPerPixel;
                int nextXYPos = (y + 1) * stride + (x + 1) * bytesPerPixel;

                for (int i = 0; i < 3; i++) // B, G, R
                {
                    int p00 = originalBytes[currentPos + i];
                    int p01 = originalBytes[nextXPos + i];
                    int p10 = originalBytes[nextYPos + i];
                    int p11 = originalBytes[nextXYPos + i];

                    // Roberts Cross
                    // Gx = [[1, 0], [0, -1]] => p(x,y) - p(x+1,y+1)
                    // Gy = [[0, 1], [-1, 0]] => p(x+1,y) - p(x,y+1)
                    
                    int gx = p00 - p11;
                    int gy = p01 - p10;

                    int magnitude = (int)Math.Sqrt(gx * gx + gy * gy);
                    resultBytes[currentPos + i] = (byte)Math.Min(255, magnitude);
                }
                resultBytes[currentPos + 3] = 255; // Alpha
            }
        }

        Marshal.Copy(resultBytes, 0, resultData.Scan0, byteCount);
        originalBmp.UnlockBits(originalData);
        resultBmp.UnlockBits(resultData);

        UpdatePictureBoxImage(resultBmp);
        lblInfo.Text = "Roberts Cross Edge Detection applied.";
    }

    private void ApplyCannyEdgeDetection()
    {
        if (originalImage is not Bitmap originalBmp) return;

        HideControlPanels();
        
        int width = originalBmp.Width;
        int height = originalBmp.Height;
        
        // 1. Grayscale Conversion
        int[,] gray = new int[width, height];
        Rectangle rect = new Rectangle(0, 0, width, height);
        BitmapData srcData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int bytes = srcData.Stride * height;
        byte[] srcBytes = new byte[bytes];
        Marshal.Copy(srcData.Scan0, srcBytes, 0, bytes);
        originalBmp.UnlockBits(srcData);
        
        int stride = srcData.Stride;
        for(int y=0; y<height; y++) {
            for(int x=0; x<width; x++) {
                int idx = y*stride + x*4;
                byte b = srcBytes[idx];
                byte g = srcBytes[idx+1];
                byte r = srcBytes[idx+2];
                gray[x,y] = (int)(r*0.299 + g*0.587 + b*0.114);
            }
        }
        
        // 2. Gaussian Blur (5x5)
        int[,] blurred = new int[width, height];
        int[,] gaussianKernel = { {2,4,5,4,2}, {4,9,12,9,4}, {5,12,15,12,5}, {4,9,12,9,4}, {2,4,5,4,2} };
        int kernelSum = 159;
        
        for(int y=2; y<height-2; y++) {
            for(int x=2; x<width-2; x++) {
                int sum = 0;
                for(int ky=-2; ky<=2; ky++) {
                    for(int kx=-2; kx<=2; kx++) {
                        sum += gray[x+kx, y+ky] * gaussianKernel[kx+2, ky+2];
                    }
                }
                blurred[x,y] = sum / kernelSum;
            }
        }
        
        // 3. Sobel Gradients
        double[,] gradient = new double[width, height];
        double[,] angle = new double[width, height];
        int[,] gx = {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}};
        int[,] gy = {{1, 2, 1}, {0, 0, 0}, {-1, -2, -1}};
        
        for(int y=1; y<height-1; y++) {
            for(int x=1; x<width-1; x++) {
                double sumX = 0, sumY = 0;
                for(int ky=-1; ky<=1; ky++) {
                    for(int kx=-1; kx<=1; kx++) {
                        int val = blurred[x+kx, y+ky];
                        sumX += val * gx[ky+1, kx+1];
                        sumY += val * gy[ky+1, kx+1];
                    }
                }
                gradient[x,y] = Math.Sqrt(sumX*sumX + sumY*sumY);
                double theta = Math.Atan2(sumY, sumX) * 180.0 / Math.PI;
                if (theta < 0) theta += 180;
                angle[x,y] = theta;
            }
        }
        
        // 4. Non-Maximum Suppression
        int[,] nms = new int[width, height];
        for(int y=1; y<height-1; y++) {
            for(int x=1; x<width-1; x++) {
                double q = 255, r = 255;
                double ang = angle[x,y];
                
                if ((ang >= 0 && ang < 22.5) || (ang >= 157.5 && ang <= 180)) {
                    q = gradient[x+1, y]; r = gradient[x-1, y];
                } else if (ang >= 22.5 && ang < 67.5) {
                    q = gradient[x+1, y+1]; r = gradient[x-1, y-1];
                } else if (ang >= 67.5 && ang < 112.5) {
                    q = gradient[x, y+1]; r = gradient[x, y-1];
                } else if (ang >= 112.5 && ang < 157.5) {
                    q = gradient[x-1, y+1]; r = gradient[x+1, y-1];
                }
                
                if (gradient[x,y] >= q && gradient[x,y] >= r) nms[x,y] = (int)gradient[x,y];
                else nms[x,y] = 0;
            }
        }
        
        // 5. Double Threshold & Hysteresis
        int lowThreshold = 30;
        int highThreshold = 90;
        int[,] edges = new int[width, height];
        
        for(int y=1; y<height-1; y++) {
            for(int x=1; x<width-1; x++) {
                if (nms[x,y] >= highThreshold) edges[x,y] = 255;
                else if (nms[x,y] >= lowThreshold) edges[x,y] = 128;
                else edges[x,y] = 0;
            }
        }
        
        bool changed = true;
        while(changed) {
            changed = false;
            for(int y=1; y<height-1; y++) {
                for(int x=1; x<width-1; x++) {
                    if (edges[x,y] == 128) {
                        if (edges[x-1,y-1]==255 || edges[x,y-1]==255 || edges[x+1,y-1]==255 ||
                            edges[x-1,y]==255   ||                      edges[x+1,y]==255 ||
                            edges[x-1,y+1]==255 || edges[x,y+1]==255 || edges[x+1,y+1]==255) {
                                edges[x,y] = 255;
                                changed = true;
                        }
                    }
                }
            }
        }
        
        Bitmap resultBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        BitmapData resData = resultBmp.LockBits(new Rectangle(0,0,width,height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        byte[] resBytes = new byte[bytes];
        
        for(int y=0; y<height; y++) {
            for(int x=0; x<width; x++) {
                int val = (edges[x,y] == 255) ? 255 : 0;
                int idx = y*stride + x*4;
                resBytes[idx] = (byte)val; resBytes[idx+1] = (byte)val; resBytes[idx+2] = (byte)val; resBytes[idx+3] = 255;
            }
        }
        
        Marshal.Copy(resBytes, 0, resData.Scan0, bytes);
        resultBmp.UnlockBits(resData);
        UpdatePictureBoxImage(resultBmp);
        lblInfo.Text = "Canny Edge Detection applied.";
    }

    private void ApplyPredefinedConvolution(string type)
    {
        float[,] kernel;
        float factor = 1;
        int bias = 0;

        switch (type)
        {
            case "GaussianBlur":
                kernel = new float[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
                factor = 16;
                break;
            case "Sharpen":
                kernel = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
                break;
            case "EdgeDetection": // Laplacian
                kernel = new float[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
                break;
            default:
                return;
        }

        ApplyConvolution(kernel, factor, bias);
        lblInfo.Text = $"{type} filter applied.";
    }

    private void ApplyConvolution(float[,] kernel, float factor = 1.0f, int bias = 0)
    {
        if (originalImage is not Bitmap originalBmp) return;

        HideControlPanels();
        Bitmap resultBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);

        Rectangle rect = new Rectangle(0, 0, originalBmp.Width, originalBmp.Height);
        BitmapData originalData = originalBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData resultData = resultBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        int byteCount = originalData.Stride * originalData.Height;
        byte[] originalBytes = new byte[byteCount];
        byte[] resultBytes = new byte[byteCount];

        Marshal.Copy(originalData.Scan0, originalBytes, 0, byteCount);

        int stride = originalData.Stride;
        int width = originalBmp.Width;
        int height = originalBmp.Height;
        int bytesPerPixel = 4;

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float sumR = 0, sumG = 0, sumB = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int pixelPosX = x + kx;
                        int pixelPosY = y + ky;
                        int pos = pixelPosY * stride + pixelPosX * bytesPerPixel;

                        float kernelVal = kernel[kx + 1, ky + 1];

                        sumB += originalBytes[pos] * kernelVal;
                        sumG += originalBytes[pos + 1] * kernelVal;
                        sumR += originalBytes[pos + 2] * kernelVal;
                    }
                }

                int currentPos = y * stride + x * bytesPerPixel;
                byte finalB = (byte)Math.Max(0, Math.Min(255, (sumB / factor) + bias));
                byte finalG = (byte)Math.Max(0, Math.Min(255, (sumG / factor) + bias));
                byte finalR = (byte)Math.Max(0, Math.Min(255, (sumR / factor) + bias));

                resultBytes[currentPos] = finalB;
                resultBytes[currentPos + 1] = finalG;
                resultBytes[currentPos + 2] = finalR;
                resultBytes[currentPos + 3] = 255; // Alpha
            }
        }

        Marshal.Copy(resultBytes, 0, resultData.Scan0, byteCount);

        originalBmp.UnlockBits(originalData);
        resultBmp.UnlockBits(resultData);

        UpdatePictureBoxImage(resultBmp);
    }
    }
}