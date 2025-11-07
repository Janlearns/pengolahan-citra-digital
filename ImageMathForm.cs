using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyApps
{
    public class ImageMathForm : Form
    {
        private PictureBox pbA, pbB, pbResult;
        private string operationSymbol;
        private readonly Image imgA, imgB, imgResult; // Gunakan readonly untuk imgA, imgB, imgResult

        // Konstruktor default (dipanggil saat MathOperation.Add digunakan di Dashboard)
        public ImageMathForm(Image imageA, Image imageB, Image imageResult)
            : this(imageA, imageB, imageResult, Dashboard.MathOperation.Add) 
        {
        }

        // Konstruktor Utama (NEW: Dipanggil oleh Dashboard)
        public ImageMathForm(Image imageA, Image imageB, Image imageResult, Dashboard.MathOperation operation)
        {
            this.imgA = imageA;
            this.imgB = imageB;
            this.imgResult = imageResult;

            // Tentukan simbol berdasarkan enum
            if (operation == Dashboard.MathOperation.Multiply)
                operationSymbol = "x";
            else if (operation == Dashboard.MathOperation.Divide)
                operationSymbol = "÷";
            else
                operationSymbol = "+";

            InitializeMathComponents();
        }

        private void InitializeMathComponents()
        {
            Text = $"Image Math Operation: A {operationSymbol} B = Result";
            ClientSize = new Size(1250, 600);
            BackColor = Colors.DarkPrimary;
            Font = new Font("Segoe UI", 10f);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 500);

            // Layout utama: 3 kolom untuk 3 gambar
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); // Simbol Operasi
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); // Simbol '='
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content

            // --- Header Labels ---
            mainLayout.Controls.Add(CreateHeaderLabel("IMAGE A (Original)"), 0, 0);
            mainLayout.Controls.Add(CreateHeaderLabel("IMAGE B (Operand)"), 2, 0);
            mainLayout.Controls.Add(CreateHeaderLabel("RESULT (A B)"), 4, 0); // Diubah agar lebih umum

            // --- Symbols ---
            mainLayout.Controls.Add(CreateSymbolLabel(operationSymbol), 1, 1); // Hanya tambahkan sekali
            mainLayout.Controls.Add(CreateSymbolLabel("="), 3, 1);

            // --- PictureBoxes ---
            pbA = CreatePictureBox(imgA);
            pbB = CreatePictureBox(imgB);
            pbResult = CreatePictureBox(imgResult);

            mainLayout.Controls.Add(pbA, 0, 1);
            mainLayout.Controls.Add(pbB, 2, 1);
            mainLayout.Controls.Add(pbResult, 4, 1);

            Controls.Add(mainLayout);
        }

        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Colors.Primary
            };
        }

        private Label CreateSymbolLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Colors.TextSecondary
            };
        }

        private PictureBox CreatePictureBox(Image img)
        {
            Panel p = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BackColor = Colors.DarkPrimary
            };

            PictureBox pb = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Colors.DarkSecondary,
                BorderStyle = BorderStyle.None
            };

            pb.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Colors.Border, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, pb.Width - 1, pb.Height - 1);
                }
            };

            p.Controls.Add(pb);
            return pb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Hapus citra hasil dan citra kedua yang kepemilikannya dialihkan ke form ini
                imgResult?.Dispose();
                imgB?.Dispose();
                // imgA adalah originalImage dari Dashboard, jangan dispose di sini
            }
            base.Dispose(disposing);
        }
    }
}