// MainForm.cs - Основная форма приложения
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MetroMapApp
{
    public partial class MainForm : Form
    {
        private GraphCanvas canvas;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ComboBox cmbLines;

        public MainForm()
        {
            InitializeComponent();
            InitializeMetroLines();
        }

        private void InitializeComponent()
        {
            this.Text = "Схема метро";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Меню
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Файл");
            var editMenu = new ToolStripMenuItem("Правка");

            fileMenu.DropDownItems.Add("Новая схема", null, (s, e) => canvas.ClearAll());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

            editMenu.DropDownItems.Add("Добавить линию", null, AddNewLine);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            this.Controls.Add(menuStrip);

            // Панель инструментов
            toolStrip = new ToolStrip();
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;

            cmbLines = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLines.SelectedIndexChanged += (s, e) =>
            {
                if (cmbLines.SelectedItem is MetroLine line)
                    canvas.SelectedLine = line;
            };
            toolStrip.Items.Add(new ToolStripLabel("Линия:"));
            toolStrip.Items.Add(new ToolStripControlHost(cmbLines));

            toolStrip.Items.Add(new ToolStripSeparator());

            var btnAddStation = new ToolStripButton("Добавить станцию", null, (s, e) => SetMode(EditMode.AddStation))
            {
                CheckOnClick = true,
                Image = CreateColorBitmap(Color.Green)
            };
            var btnAddTrack = new ToolStripButton("Добавить перегон", null, (s, e) => SetMode(EditMode.AddTrack))
            {
                CheckOnClick = true,
                Image = CreateColorBitmap(Color.Blue)
            };
            var btnMove = new ToolStripButton("Перемещать", null, (s, e) => SetMode(EditMode.MoveStation))
            {
                CheckOnClick = true,
                Image = CreateColorBitmap(Color.Orange)
            };
            var btnSelect = new ToolStripButton("Выбор", null, (s, e) => SetMode(EditMode.None))
            {
                CheckOnClick = true,
                Checked = true,
                Image = CreateColorBitmap(Color.Gray)
            };

            toolStrip.Items.Add(btnSelect);
            toolStrip.Items.Add(btnAddStation);
            toolStrip.Items.Add(btnAddTrack);
            toolStrip.Items.Add(btnMove);

            this.Controls.Add(toolStrip);

            // Canvas
            canvas = new GraphCanvas
            {
                Location = new Point(10, 55),
                Size = new Size(1160, 680),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            canvas.StatusChanged += (s, e) => UpdateStatus();
            this.Controls.Add(canvas);

            // Status bar
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Готово");
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // Обработка кнопок
            btnSelect.Click += (s, e) => { UncheckOthers(btnSelect); };
            btnAddStation.Click += (s, e) => { UncheckOthers(btnAddStation); };
            btnAddTrack.Click += (s, e) => { UncheckOthers(btnAddTrack); };
            btnMove.Click += (s, e) => { UncheckOthers(btnMove); };
        }

        private void InitializeMetroLines()
        {
            // Предустановленные линии метро
            canvas.AddLine(new MetroLine("Красная линия", Color.Red));
            canvas.AddLine(new MetroLine("Синяя линия", Color.Blue));
            canvas.AddLine(new MetroLine("Зеленая линия", Color.Green));
            canvas.AddLine(new MetroLine("Оранжевая линия", Color.Orange));
            canvas.AddLine(new MetroLine("Фиолетовая линия", Color.Purple));

            UpdateLinesCombo();
            if (cmbLines.Items.Count > 0)
                cmbLines.SelectedIndex = 0;
        }

        private void UpdateLinesCombo()
        {
            cmbLines.Items.Clear();
            foreach (var line in canvas.Lines)
            {
                cmbLines.Items.Add(line);
            }
            cmbLines.DisplayMember = "Name";
        }

        private void AddNewLine(object sender, EventArgs e)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Новая линия";
                dialog.Size = new Size(350, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;

                var lblName = new Label { Text = "Название:", Location = new Point(10, 10) };
                var txtName = new TextBox { Location = new Point(10, 35), Size = new Size(300, 20) };

                var lblColor = new Label { Text = "Цвет:", Location = new Point(10, 65) };
                var colorDialog = new ColorDialog();
                var btnColor = new Button { Text = "Выбрать цвет", Location = new Point(10, 90), Size = new Size(100, 25) };
                var selectedColor = Color.Gray;

                btnColor.Click += (s, ev) =>
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedColor = colorDialog.Color;
                        btnColor.BackColor = selectedColor;
                    }
                };

                var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(130, 130) };

                dialog.Controls.Add(lblName);
                dialog.Controls.Add(txtName);
                dialog.Controls.Add(lblColor);
                dialog.Controls.Add(btnColor);
                dialog.Controls.Add(btnOk);
                dialog.AcceptButton = btnOk;

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    canvas.AddLine(new MetroLine(txtName.Text, selectedColor));
                    UpdateLinesCombo();
                    cmbLines.SelectedIndex = cmbLines.Items.Count - 1;
                }
            }
        }

        private void SetMode(EditMode mode)
        {
            canvas.CurrentMode = mode;
            UpdateStatus();
        }

        private void UncheckOthers(ToolStripButton checkedButton)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripButton btn && btn != checkedButton && btn.CheckOnClick)
                {
                    btn.Checked = false;
                }
            }
        }

        private void UpdateStatus()
        {
            switch (canvas.CurrentMode)
            {
                case EditMode.AddStation:
                    statusLabel.Text = "Режим: добавление станции. Кликните на поле для создания станции.";
                    break;
                case EditMode.AddTrack:
                    statusLabel.Text = canvas.SelectedLine == null ?
                        "Выберите линию метро" :
                        "Режим: добавление перегона. Выберите первую станцию.";
                    break;
                case EditMode.MoveStation:
                    statusLabel.Text = "Режим: перемещение. Перетаскивайте станции мышью.";
                    break;
                default:
                    statusLabel.Text = "Режим: выбор. Кликните на объект для выделения. Правый клик - меню.";
                    break;
            }
        }

        private System.Drawing.Bitmap CreateColorBitmap(Color color)
        {
            var bmp = new System.Drawing.Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, 16, 16);
                g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
            }
            return bmp;
        }
    }
}