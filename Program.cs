using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

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
            MessageBox.Show("Билеты закончились! Все раскупил Медведь \U0001F43B",
                "Деревня Петухи", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Close();
        base.OnKeyDown(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new PosterForm());
    }
}
