using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

// Fullscreen jump scare: shows daddy.png with a sound, closes after 3 sec,
// or earlier on click / Esc.
class ScareForm : Form
{
    Image pic;
    Timer sound = new Timer { Interval = 120 };
    Timer autoClose = new Timer { Interval = 3000 };

    public ScareForm()
    {
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("daddy.png"))
            pic = Image.FromStream(s);

        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        DoubleBuffered = true;
        KeyPreview = true;

        sound.Tick += (o, e) => System.Media.SystemSounds.Hand.Play();
        autoClose.Tick += (o, e) => Close();
        sound.Start();
        autoClose.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // fit the picture to the screen, keeping proportions
        float k = Math.Min((float)ClientSize.Width / pic.Width, (float)ClientSize.Height / pic.Height);
        int w = (int)(pic.Width * k), h = (int)(pic.Height * k);
        e.Graphics.DrawImage(pic, (ClientSize.Width - w) / 2, (ClientSize.Height - h) / 2, w, h);
    }

    protected override void OnMouseClick(MouseEventArgs e) { Close(); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        sound.Stop();
        autoClose.Stop();
        base.OnFormClosed(e);
    }
}

class PosterForm : Form
{
    Image img;

    public PosterForm()
    {
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("poster.jpg"))
            img = Image.FromStream(s);

        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        KeyPreview = true;

        // Poster takes 75% of screen height, placed on the RIGHT side
        Rectangle area = Screen.PrimaryScreen.WorkingArea;
        int h = (int)(area.Height * 0.75);
        int w = h * img.Width / img.Height;
        ClientSize = new Size(w, h);
        Location = new Point(area.Right - w - 20, area.Top + (area.Height - h) / 2);

        DoubleBuffered = true;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.DrawImage(img, 0, 0, ClientSize.Width, ClientSize.Height);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        double fx = (double)e.X / ClientSize.Width;
        double fy = (double)e.Y / ClientSize.Height;

        // little X in the top-right corner of the poster
        if (fx > 0.93 && fy < 0.07) { Close(); return; }

        // "КУПИТЬ БИЛЕТЫ" button
        if (fx > 0.17 && fx < 0.83 && fy > 0.92 && fy < 0.995)
        {
            using (var scare = new ScareForm()) scare.ShowDialog(this);
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Close();
        base.OnKeyDown(e);
    }

    [STAThread]
    static void Main(string[] args)
    {
        // Show the poster only on 15 November (any year), using the PC's clock.
        // Run "MashaPoster.exe --force" to show it on any other day (for testing).
        DateTime now = DateTime.Now;
        bool force = args.Length > 0 && args[0] == "--force";
        if (!force && !(now.Month == 11 && now.Day == 15)) return;

        Application.EnableVisualStyles();
        Application.Run(new PosterForm());
    }
}
