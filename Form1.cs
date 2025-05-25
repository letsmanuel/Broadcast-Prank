using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace broadcast
{
    public partial class Form1 : Form
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private string _tempVideoPath;

        public Form1()
        {
            InitializeComponent();

            // Fullscreen and topmost
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            var videoView = new VideoView
            {
                MediaPlayer = _mediaPlayer,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(videoView);

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Find the single embedded mp4 resource
            var mp4ResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));

            if (mp4ResourceName == null)
            {
                MessageBox.Show("No embedded MP4 resource found!");
                this.Close();
                return;
            }

            // Extract to temp file
            _tempVideoPath = Path.Combine(Path.GetTempPath(), "embeddedVideo.mp4");
            ExtractEmbeddedResource(mp4ResourceName, _tempVideoPath);

            // Play the video
            var media = new Media(_libVLC, _tempVideoPath, FromType.FromPath);
            _mediaPlayer.Play(media);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(_tempVideoPath))
            {
                try { File.Delete(_tempVideoPath); } catch { }
            }
        }

        private void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new Exception($"Resource '{resourceName}' not found.");

            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }
    }
}