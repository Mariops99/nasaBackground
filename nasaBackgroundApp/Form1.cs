using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace nasaBackgroundApp
{
    public partial class Form1 : Form
    {

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private DateTime fecha;


        public Form1()
        {

            InitializeComponent();
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            MenuItem openMenuItem = new MenuItem("Open", new EventHandler(Open));
            notifyIcon1.ContextMenu = new ContextMenu(new MenuItem[] { openMenuItem, exitMenuItem });

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fecha = dateTimePicker1.Value;
            descargarImagen(fecha.Day, fecha.Month, fecha.Year);
        }

        void Open(object sender, EventArgs e)
        { 
            notifyIcon1.Visible = true;
            new Form1();
            this.Show();
        }

        void Exit(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void settings_Click(object sender, EventArgs e)
        {
            if (settings.Text == "ON")
            {
                settings.Text = "OFF";
                rkApp.DeleteValue("nasaBackgroundApp", false);
            }
            else if (settings.Text == "OFF")
            {
                settings.Text = "ON";
                rkApp.SetValue("nasaBackgroundApp", Application.ExecutablePath);
                descargarImagen(fecha.Day, fecha.Month, fecha.Year);
            }
        }

        private void setwallpaper_Click(object sender, EventArgs e)
        {
            descargarImagen(fecha.Day, fecha.Month, fecha.Year);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            fecha = dateTimePicker1.Value;
        }

        private void descargarImagen(int dia, int mes, int año)
        {
            var url = $"https://api.nasa.gov/planetary/apod?api_key=dTdzuePtEEAMLfZ9EDehuxy2FOhbpMnEBFABwxyG&date=" + año + "-" + mes + "-" + dia;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream strReader = response.GetResponseStream())
                    {
                        if (strReader == null) return;
                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            string responseBody = objReader.ReadToEnd();
                            var json = JsonConvert.DeserializeObject<dynamic>(responseBody);
                            string imagenUrl = "";


                            if (!json.ContainsKey("hdurl"))
                            {
                                if (!json.ContainsKey("url"))
                                {
                                    descargarImagen(15, 04, 2003);
                                    return;
                                } else
                                {
                                    imagenUrl = json.url.ToString();
                                    if(imagenUrl.Contains("panorama"))
                                    {
                                        descargarImagen(15, 04, 2003);
                                        return;
                                    }
                                }
                            } else
                            {
                               imagenUrl = json.hdurl.ToString();
                            }
                            
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(new Uri(imagenUrl), @"C:\nasaBackground\imageDay.png");
                            }
                        }
                    }
                }

                // Sets the actual wallpaper
                WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 1, @"C:\nasaBackground\imageDay.png", WinAPI.SPIF_SENDCHANGE);

            }
            catch (WebException ex)
            {
                // Handle error
                Console.WriteLine(ex);
            }
        }

        public class WinAPI
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
            public const int SPI_SETDESKWALLPAPER = 20;
            public const int SPIF_SENDCHANGE = 0x2;
        }

        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
