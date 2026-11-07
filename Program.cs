using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

class PosterForm : Form
{
    Image posterImg;
    Image daddyImg;
    bool isDaddyMode = false;

    public PosterForm()
    {
        // 1. Check conditions before loading the UI
        CheckTicketConditions();

        // 2. Load embedded resources
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("poster.jpg"))
            posterImg = Image.FromStream(s);
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("daddy.png"))
            daddyImg = Image.FromStream(s);

        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        KeyPreview = true;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;

        // 3. Determine initial visual layout based on settings and current date
        DateTime today = DateTime.Today;
        if (Properties.Settings.Default.HasBoughtTickets && today.Month == 11 && today.Day == 15)
        {
            SetupFullScreenDaddyLayout();
        }
        else
        {
            SetupPosterLayout();
        }
    }

    private void CheckTicketConditions()
    {
        // If the user already bought tickets but it's NOT November 15 yet, block access
        if (Properties.Settings.Default.HasBoughtTickets)
        {
            DateTime today = DateTime.Today;
            if (today.Month != 11 || today.Day != 15)
            {
                MessageBox.Show("Open in November 15.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0); // Safely close the app immediately
            }
        }
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
        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
        Location = new Point(screenBounds.Left, screenBounds.Top);
        ClientSize = new Size(screenBounds.Width, screenBounds.Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        if (isDaddyMode)
        {
            e.Graphics.DrawImage(daddyImg, 0, 0, ClientSize.Width, ClientSize.Height);
        }
        else
        {
            e.Graphics.DrawImage(posterImg, 0, 0, ClientSize.Width, ClientSize.Height);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (isDaddyMode) return;

        double fx = (double)e.X / ClientSize.Width;
        double fy = (double)e.Y / ClientSize.Height;

        // Close button (X)
        if (fx > 0.93 && fy < 0.07)
        {
            Close();
            return;
        }

        // BUY TICKETS button
        if (fx > 0.17 && fx < 0.83 && fy > 0.92 && fy < 0.995)
        {
            MessageBox.Show("Билеты закончились! Все раскупил Медведь 🐻 , Деревня Петухи", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Save transaction state to application configuration settings
            Properties.Settings.Default.HasBoughtTickets = true;
            Properties.Settings.Default.Save();

            // Evaluate state depending on what day they clicked it
            DateTime today = DateTime.Today;
            if (today.Month == 11 && today.Day == 15)
            {
                SetupFullScreenDaddyLayout();
                Invalidate();
            }
            else
            {
                MessageBox.Show("Open in November 15.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
        }
        base.OnMouseClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
        base.OnKeyDown(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new PosterForm());
    }
}
