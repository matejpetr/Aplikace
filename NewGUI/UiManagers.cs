using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NewGUI
{
    public class ImageManager // tøída pro správu obrázkù v PictureBoxu
    {
        private readonly PictureBox _pictureBox; // PictureBox pro zobrazení obrázku
        private readonly string[] _exts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }; // podporované typy obrázkù. []jako pole stringù

        public ImageManager(PictureBox pictureBox)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox)); // inicializace PictureBoxu
        }

        public void UpdateImageForLabel(string label, string element, string baseDir) // aktualizuje obrázek podle zadaného štítku, elementu a základního adresáøe
        {
            if (string.IsNullOrWhiteSpace(label)) //Pokud je combobox prázdný, smaže obrázek
            {
                SetImage(null);
                return;
            }

            try
            {
                
                string sensorsDir = Path.Combine(baseDir, element); // Složka s obrázky senzorù + vybraný element (napø. Senzor/Aktuátor)

                string foundPath = null; // cesta k nalezenému obrázku, nejprv null
                foreach (var ext in _exts)
                {
                    var p = Path.Combine(sensorsDir, label + ext);
                    if (File.Exists(p)) { foundPath = p; break; } // pokud soubor existuje, uložíme cestu a ukonèíme smyèku
                }

                if (foundPath == null)
                {
                    SetImage(null); // pokud nebyl nalezen žádný obrázek, smaže obrázek
                    return;
                }

                using (var fs = new FileStream(foundPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // otevøe soubor s obrázkem
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

        private void SetImage(Image img) // bezpeènì nastaví obrázek v PictureBoxu
        {
            if (_pictureBox.InvokeRequired) 
            {
                try { _pictureBox.BeginInvoke((Action)(() => SwapImage(img))); } catch { } // bezpeèné volání na UI vláknì
            }
            else
            {
                SwapImage(img);
            }
        }

        private void SwapImage(Image img) // vymìní obrázek v PictureBoxu
        {
            try
            {
                var old = _pictureBox.Image; // uloží starý obrázek
                _pictureBox.Image = img; // nastaví nový obrázek
                if (old != null && old != img) old.Dispose(); // uvolní starý obrázek, pokud existuje a není stejný jako nový
            }
            catch { }
        }
    }

    public class ValueDisplayManager // tøída pro správu zobrazení textových hodnot v ovládacím prvku
    {
        private readonly Control _target; // cílový ovládací prvek pro zobrazení textu

        public ValueDisplayManager(Control target) // inicializace s cílovým ovládacím prvkem
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public void UpdateValueText(string text) // aktualizuje text v cílovém ovládacím prvku
        {
            if (_target.InvokeRequired)
            {
                try { _target.BeginInvoke((Action)(() => _target.Text = text ?? string.Empty)); } catch { } // bezpeèné volání na UI vláknì
            }
            else
            {
                _target.Text = text ?? string.Empty; // nastaví text, nebo prázdný øetìzec pokud je null
            }
        }
    }
}