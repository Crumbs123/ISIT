
using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Newtonsoft.Json.Linq;

namespace WorldMapApp
{
    public class MainForm : Form
    {
        private GMapControl map;
        private Panel infoPanel;
        private Label lblName, lblCurrency, lblPopulation;
        private PictureBox picFlag;
        private GMapOverlay overlay;

        public MainForm()
        {
            Text = "World Map Country Information";
            Width = 1200;
            Height = 700;

            map = new GMapControl
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(map);

            infoPanel = new Panel
            {
                Width = 260,
                Height = 220,
                BackColor = Color.White,
                Location = new Point(20, 20)
            };
            Controls.Add(infoPanel);
            infoPanel.BringToFront();

            lblName = new Label { Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            picFlag = new PictureBox { Location = new Point(10, 45), Size = new Size(80, 50), SizeMode = PictureBoxSizeMode.StretchImage };
            lblCurrency = new Label { Location = new Point(10, 110), AutoSize = true };
            lblPopulation = new Label { Location = new Point(10, 140), AutoSize = true };

            infoPanel.Controls.AddRange(new Control[] { lblName, picFlag, lblCurrency, lblPopulation });

            InitMap();
        }

        private void InitMap()
        {
            map.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            map.Position = new PointLatLng(20, 0);
            map.MinZoom = 2;
            map.MaxZoom = 18;
            map.Zoom = 3;
            map.OnMarkerClick += Map_OnMarkerClick;

            overlay = new GMapOverlay("markers");
            map.Overlays.Add(overlay);

            AddMarker("RU", 55.7558, 37.6176);
            AddMarker("FR", 48.8566, 2.3522);
            AddMarker("IT", 41.9028, 12.4964);
            AddMarker("ES", 40.4168, -3.7038);
        }

        private void AddMarker(string code, double lat, double lng)
        {
            var m = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.blue_dot);
            m.Tag = code;
            m.ToolTipText = code;
            overlay.Markers.Add(m);
        }

        private async void Map_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            string code = item.Tag.ToString();
            using HttpClient client = new HttpClient();
            string json = await client.GetStringAsync($"https://restcountries.com/v3.1/alpha/{code}");
            var obj = JArray.Parse(json)[0];

            lblName.Text = obj["translations"]["rus"]["common"].ToString();
            lblPopulation.Text = "Население: " + string.Format("{0:N0}", (long)obj["population"]);

            foreach (var c in obj["currencies"])
            {
                lblCurrency.Text = "Валюта: " + c.First["name"];
                break;
            }

            picFlag.Load(obj["flags"]["png"].ToString());
        }
    }
}
