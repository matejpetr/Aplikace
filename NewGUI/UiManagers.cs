using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NewGUI
{
    public class ImageManager
    {
        private readonly PictureBox _pictureBox;
        private readonly string[] _exts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

        public ImageManager(PictureBox pictureBox)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));
        }

        public void UpdateImageForLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                SetImage(null);
                return;
            }

            try
            {
                string baseDir = Application.StartupPath;
                var parent = Directory.GetParent(baseDir);
                if (parent != null) baseDir = parent.FullName;
                string sensorsDir = Path.Combine(baseDir, "Senzory");

                string foundPath = null;
                foreach (var ext in _exts)
                {
                    var p = Path.Combine(sensorsDir, label + ext);
                    if (File.Exists(p)) { foundPath = p; break; }
                }

                if (foundPath == null)
                {
                    SetImage(null);
                    return;
                }

                using (var fs = new FileStream(foundPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var img = Image.FromStream(fs);
                    SetImage((Image)img.Clone());
                }
            }
            catch
            {
                SetImage(null);
            }
        }

        private void SetImage(Image img)
        {
            if (_pictureBox.InvokeRequired)
            {
                try { _pictureBox.BeginInvoke((Action)(() => SwapImage(img))); } catch { }
            }
            else
            {
                SwapImage(img);
            }
        }

        private void SwapImage(Image img)
        {
            try
            {
                var old = _pictureBox.Image;
                _pictureBox.Image = img;
                if (old != null && old != img) old.Dispose();
            }
            catch { }
        }
    }

    public class ValueDisplayManager
    {
        private readonly Control _target;

        public ValueDisplayManager(Control target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public void UpdateValueText(string text)
        {
            if (_target.InvokeRequired)
            {
                try { _target.BeginInvoke((Action)(() => _target.Text = text ?? string.Empty)); } catch { }
            }
            else
            {
                _target.Text = text ?? string.Empty;
            }
        }
    }
}