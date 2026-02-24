// GraphCanvas.cs - Кастомный контрол для отрисовки графа
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MetroMapApp
{
    public enum EditMode
    {
        None,
        AddStation,
        AddTrack,
        MoveStation,
        Delete
    }

    public class GraphCanvas : Control
    {
        public List<Station> Stations { get; private set; }
        public List<Track> Tracks { get; private set; }
        public List<MetroLine> Lines { get; private set; }

        public EditMode CurrentMode { get; set; }
        public MetroLine SelectedLine { get; set; }
        public BorderStyle BorderStyle { get; }

        private Station selectedStation;
        private Station trackStartStation;
        private Station draggedStation;
        private Point dragOffset;
        private Point mousePosition;

        public event EventHandler<Station> StationSelected;
        public event EventHandler<Track> TrackSelected;
        public event EventHandler StatusChanged;

        public GraphCanvas()
        {
            Stations = new List<Station>();
            Tracks = new List<Track>();
            Lines = new List<MetroLine>();
            CurrentMode = EditMode.None;

            DoubleBuffered = true;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
        }

        public void AddLine(MetroLine line)
        {
            Lines.Add(line);
        }

        public void AddStation(Point location, string name)
        {
            if (SelectedLine == null) return;

            var station = new Station(name, location)
            {
                Line = SelectedLine,
                Color = SelectedLine.Color
            };
            Stations.Add(station);
            Invalidate();
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddTrack(Station station1, Station station2, int distance)
        {
            if (station1 == station2) return;
            if (Tracks.Any(t => (t.Station1 == station1 && t.Station2 == station2) ||
                               (t.Station1 == station2 && t.Station2 == station1)))
                return;

            var track = new Track(station1, station2, distance, SelectedLine);
            Tracks.Add(track);
            Invalidate();
        }

        public void DeleteStation(Station station)
        {
            // Удаляем все связанные перегоны
            Tracks.RemoveAll(t => t.Station1 == station || t.Station2 == station);
            Stations.Remove(station);
            selectedStation = null;
            Invalidate();
        }

        public void DeleteTrack(Track track)
        {
            Tracks.Remove(track);
            Invalidate();
        }

        public void ClearAll()
        {
            Stations.Clear();
            Tracks.Clear();
            selectedStation = null;
            trackStartStation = null;
            Invalidate();
        }

        public void EditTrackDistance(Track track, int newDistance)
        {
            track.Distance = newDistance;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем перегоны
            foreach (var track in Tracks)
            {
                DrawTrack(g, track);
            }

            // Рисуем временную линию при создании перегона
            if (CurrentMode == EditMode.AddTrack && trackStartStation != null)
            {
                g.DrawLine(new Pen(Color.Gray, 2) { DashStyle = DashStyle.Dash },
                    trackStartStation.Location, mousePosition);
            }

            // Рисуем станции
            foreach (var station in Stations)
            {
                DrawStation(g, station);
            }
        }

        private void DrawTrack(Graphics g, Track track)
        {
            var pen = new Pen(track.Line?.Color ?? Color.Gray, track.IsSelected ? 4 : 3);

            if (track.IsSelected)
            {
                pen.Color = Color.Red;
            }

            g.DrawLine(pen, track.Station1.Location, track.Station2.Location);

            // Рисуем расстояние
            if (track.Distance > 0)
            {
                var midPoint = track.GetMidPoint();
                var text = track.Distance + " мин";
                var size = g.MeasureString(text, Font);

                g.FillRectangle(Brushes.White, midPoint.X - size.Width / 2 - 2, midPoint.Y - size.Height / 2 - 2,
                    size.Width + 4, size.Height + 4);
                g.DrawString(text, Font, Brushes.Black, midPoint.X - size.Width / 2, midPoint.Y - size.Height / 2);
            }
        }

        private void DrawStation(Graphics g, Station station)
        {
            var brush = new SolidBrush(station.IsSelected ? Color.Red : (station.Line?.Color ?? station.Color));
            var borderPen = new Pen(station.IsSelected ? Color.DarkRed : Color.DarkGreen, 2);

            // Круг станции
            g.FillEllipse(brush, station.GetBounds());
            g.DrawEllipse(borderPen, station.GetBounds());

            // Название станции
            var textSize = g.MeasureString(station.Name, Font);
            g.DrawString(station.Name, Font, Brushes.Black,
                station.Location.X - textSize.Width / 2,
                station.Location.Y + station.Radius + 2);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                switch (CurrentMode)
                {
                    case EditMode.AddStation:
                        AddStationPrompt(e.Location);
                        break;

                    case EditMode.AddTrack:
                        HandleTrackCreation(e.Location);
                        break;

                    case EditMode.MoveStation:
                        StartDrag(e.Location);
                        break;

                    case EditMode.None:
                        SelectObject(e.Location);
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu(e.Location);
            }
        }

        private void AddStationPrompt(Point location)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Новая станция";
                dialog.Size = new Size(300, 150);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var lbl = new Label { Text = "Название станции:", Location = new Point(10, 10), Size = new Size(200, 20) };
                var txt = new TextBox { Location = new Point(10, 35), Size = new Size(260, 20) };
                var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(110, 80) };

                dialog.Controls.Add(lbl);
                dialog.Controls.Add(txt);
                dialog.Controls.Add(btnOk);
                dialog.AcceptButton = btnOk;

                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
                {
                    AddStation(location, txt.Text);
                }
            }
        }

        private void HandleTrackCreation(Point location)
        {
            var station = Stations.FirstOrDefault(s => s.Contains(location));

            if (station != null)
            {
                if (trackStartStation == null)
                {
                    trackStartStation = station;
                    trackStartStation.IsSelected = true;
                    Invalidate();
                }
                else if (trackStartStation != station)
                {
                    // Запрашиваем время в пути
                    using (var dialog = new Form())
                    {
                        dialog.Text = "Время в пути";
                        dialog.Size = new Size(300, 150);
                        dialog.StartPosition = FormStartPosition.CenterParent;
                        dialog.FormBorderStyle = FormBorderStyle.FixedDialog;

                        var lbl = new Label { Text = "Время в пути (мин):", Location = new Point(10, 10), Size = new Size(200, 20) };
                        var num = new NumericUpDown { Location = new Point(10, 35), Size = new Size(260, 20), Minimum = 1, Maximum = 999 };
                        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(110, 80) };

                        dialog.Controls.Add(lbl);
                        dialog.Controls.Add(num);
                        dialog.Controls.Add(btnOk);
                        dialog.AcceptButton = btnOk;

                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            AddTrack(trackStartStation, station, (int)num.Value);
                        }
                    }

                    trackStartStation.IsSelected = false;
                    trackStartStation = null;
                    Invalidate();
                }
            }
        }

        private void StartDrag(Point location)
        {
            draggedStation = Stations.FirstOrDefault(s => s.Contains(location));
            if (draggedStation != null)
            {
                dragOffset = new Point(location.X - draggedStation.Location.X, location.Y - draggedStation.Location.Y);
                draggedStation.IsSelected = true;
            }
        }

        private void SelectObject(Point location)
        {
            // Сбрасываем выделение
            foreach (var s in Stations) s.IsSelected = false;
            foreach (var t in Tracks) t.IsSelected = false;

            // Проверяем станции
            var station = Stations.FirstOrDefault(s => s.Contains(location));
            if (station != null)
            {
                station.IsSelected = true;
                selectedStation = station;
                StationSelected?.Invoke(this, station);
                Invalidate();
                return;
            }

            // Проверяем перегоны
            var track = Tracks.FirstOrDefault(t => t.Contains(location));
            if (track != null)
            {
                track.IsSelected = true;
                TrackSelected?.Invoke(this, track);
                Invalidate();
                return;
            }

            selectedStation = null;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mousePosition = e.Location;

            if (draggedStation != null && e.Button == MouseButtons.Left)
            {
                draggedStation.Location = new Point(e.Location.X - dragOffset.X, e.Location.Y - dragOffset.Y);
                Invalidate();
            }
            else if (CurrentMode == EditMode.AddTrack && trackStartStation != null)
            {
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (draggedStation != null)
            {
                draggedStation.IsSelected = false;
                draggedStation = null;
                Invalidate();
            }
        }

        private void ShowContextMenu(Point location)
        {
            var contextMenu = new ContextMenuStrip();

            // Проверяем, кликнули ли на объект
            var station = Stations.FirstOrDefault(s => s.Contains(location));
            var track = Tracks.FirstOrDefault(t => t.Contains(location));

            if (station != null)
            {
                contextMenu.Items.Add("Редактировать название", null, (s, e) => EditStationName(station));
                contextMenu.Items.Add("Удалить станцию", null, (s, e) => DeleteStation(station));
            }
            else if (track != null)
            {
                contextMenu.Items.Add("Редактировать время", null, (s, e) => EditTrackTime(track));
                contextMenu.Items.Add("Удалить перегон", null, (s, e) => DeleteTrack(track));
            }
            else
            {
                contextMenu.Items.Add("Очистить всё", null, (s, e) =>
                {
                    if (MessageBox.Show("Удалить все станции и перегоны?", "Подтверждение",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ClearAll();
                    }
                });
            }

            if (contextMenu.Items.Count > 0)
            {
                contextMenu.Show(this, location);
            }
        }

        private void EditStationName(Station station)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Редактирование";
                dialog.Size = new Size(300, 150);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;

                var txt = new TextBox { Text = station.Name, Location = new Point(10, 20), Size = new Size(260, 20) };
                var btn = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(110, 70) };

                dialog.Controls.Add(txt);
                dialog.Controls.Add(btn);
                dialog.AcceptButton = btn;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    station.Name = txt.Text;
                    Invalidate();
                }
            }
        }

        private void EditTrackTime(Track track)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Редактирование времени";
                dialog.Size = new Size(300, 150);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;

                var num = new NumericUpDown { Value = track.Distance, Location = new Point(10, 20), Size = new Size(260, 20), Minimum = 1 };
                var btn = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(110, 70) };

                dialog.Controls.Add(num);
                dialog.Controls.Add(btn);
                dialog.AcceptButton = btn;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    EditTrackDistance(track, (int)num.Value);
                }
            }
        }
    }
}