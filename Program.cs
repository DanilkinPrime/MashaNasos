using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

class PosterForm : Form
{
    Image posterImg;
    Image daddyImg;
    bool isDaddyMode = false; // Tracks whether full-screen image is active

    public PosterForm()
    {
        // Load the poster image
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("poster.jpg"))
            posterImg = Image.FromStream(s);

        // Load the daddy image
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("daddy.png"))
            daddyImg = Image.FromStream(s);

        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        KeyPreview = true;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;

        // Initial setup for the poster mode
        SetupPosterLayout();
    }

    private void SetupPosterLayout()
    {
        isDaddyMode = false;
        Rectangle area = Screen.PrimaryScreen.WorkingArea;
        int h = (int)(area.Height * 0.75);
        int w = h * posterImg.Width / posterImg.Height;
        ClientSize = new Size(w, h);
        Location = new Point(area.Right - w - 20, area.Top + (area.Height - h) / 2);
    }

    private void SetupFullScreenDaddyLayout()
    {
        isDaddyMode = true;
        // Cover the literal entire screen bounds (ignoring taskbar)
        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
        Location = new Point(screenBounds.Left, screenBounds.Top);
        ClientSize = new Size(screenBounds.Width, screenBounds.Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        if (isDaddyMode)
        {
            // Draw full-screen image stretching to fill the display
            e.Graphics.DrawImage(daddyImg, 0, 0, ClientSize.Width, ClientSize.Height);
        }
        else
        {
            // Draw original poster
            e.Graphics.DrawImage(posterImg, 0, 0, ClientSize.Width, ClientSize.Height);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (isDaddyMode)
        {
            // Optional: Click anywhere on full screen to go back or close
            SetupPosterLayout();
            Invalidate();
            return;
        }

        double fx = (double)e.X / ClientSize.Width;
        double fy = (double)e.Y / ClientSize.Height;

        // little X in the top-right corner of the poster
        if (fx > 0.93 && fy < 0.07)
        {
            Close();
            return;
        }

        // КУПИТЬ БИЛЕТЫ button
        if (fx > 0.17 && fx < 0.83 && fy > 0.92 && fy < 0.995)
        {
            MessageBox.Show("Билеты закончились! Все раскупил Медведь 🐻 , Деревня Петухи", 
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Go full screen with daddy.png
            SetupFullScreenDaddyLayout();
            Invalidate(); // Forces the form to redraw itself
        }

        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            if (isDaddyMode)
            {
                // Escape exits full screen back to poster mode
                SetupPosterLayout();
                Invalidate();
            }
            else
            {
                Close();
            }
        }
        base.OnKeyDown(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new PosterForm());
    }
}
