﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Permissions;
using System.Net.Sockets;
using System.Configuration;
using System.Web.Script.Serialization;

namespace AthanManager
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private bool hasBeenLoaded = false;

        private string fajr, dhuhr, asr, maghrib, isha;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!hasBeenLoaded)
            {
                // Create a simple tray menu with only one item.
                trayMenu = new ContextMenu();
                trayMenu.MenuItems.Add("إعدادات التطبيق", OnConfig);
                trayMenu.MenuItems.Add("إغلاق التطبيق", OnExit);

                // Create a tray icon. In this example we use a
                // standard system icon for simplicity, but you
                // can of course use your own custom icon too.
                trayIcon = new NotifyIcon();
                trayIcon.Text = "مذكّر أوقات الصلاة الإصدار الأول";
                trayIcon.Icon = this.Icon;

                // Add menu to tray icon and show it.
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;

                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.
                hasBeenLoaded = true;
                //base.OnLoad(e);

            }

            currentCityTxt.Text = Properties.Settings.Default["current_city"].ToString();

            fajr = Properties.Settings.Default["fajr"].ToString();
            dhuhr = Properties.Settings.Default["dhuhr"].ToString();
            asr = Properties.Settings.Default["asr"].ToString();
            maghrib = Properties.Settings.Default["maghrib"].ToString();
            isha = Properties.Settings.Default["isha"].ToString();

            //MessageBox.Show(fajr + dhuhr + asr + maghrib + isha);
        }

        // Define the event handlers. 
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            //Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            //Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        private void OnExit(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("هل أنت متأكّد؟", "مذكّر أوقات الصلاة الإصدار الأول", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (dr == DialogResult.Yes)
            {
                Environment.Exit(0);
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
            ShowInTaskbar = false;
            this.Hide();
            //this.WindowState = FormWindowState.Normal;
        }

        private void OnConfig(object sender, EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private static int WM_QUERYENDSESSION = 0x11;
        private static bool systemShutdown = false;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
            {
                Environment.Exit(0);
            }

            // If this is WM_QUERYENDSESSION, the closing event should be
            // raised in the base WndProc.
            base.WndProc(ref m);

        } //WndProc 

        private void Form1_Closing(
            System.Object sender,
            System.ComponentModel.CancelEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string city = currentCityTxt.Text;
            if(city != "" && city!=string.Empty)
            {
                label2.Text = "جاري التحميل ...";
                string response = myVars.update_pray_times(city.ToLower());
                //MessageBox.Show(response);
                var json_serializer = new JavaScriptSerializer();
                var response_object = (IDictionary<string, object>)json_serializer.DeserializeObject(response.Replace("\"items\":[{","").Replace("\"}],","\","));
                string output = "";
                try
                {
                    fajr = response_object["fajr"].ToString();
                    dhuhr = response_object["dhuhr"].ToString();
                    asr = response_object["asr"].ToString();
                    maghrib = response_object["maghrib"].ToString();
                    isha = response_object["isha"].ToString();

                    output += "الفجر: " + fajr;
                    output += " / الظهر: " + dhuhr;
                    output += " / العصر: " + asr;
                    output += " / المغرب: " + maghrib;
                    output += " / العشاء: " + isha;

                    Properties.Settings.Default["current_city"] = city;
                    Properties.Settings.Default["fajr"] = fajr;
                    Properties.Settings.Default["dhuhr"] = dhuhr;
                    Properties.Settings.Default["asr"] = asr;
                    Properties.Settings.Default["maghrib"] = maghrib;
                    Properties.Settings.Default["isha"] = isha;
                    Properties.Settings.Default.Save();

                }
                catch(KeyNotFoundException ex)
                {
                    output = "المدينة المدخلة غير صالحة .. يرجى إدخال اسم مدينة صحيح";
                }
                label2.Text = output;
            }
            else
            {
                MessageBox.Show("يرجى إدخال المدينة أولاً");
                return;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Text = "الساعة الآن: " + DateTime.Now.ToString("hh:mm:ss tt").ToLower();
        }

        private void currentCityTxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}