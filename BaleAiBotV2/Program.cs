using System.Runtime.InteropServices;

namespace BaleAiBotV2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;
        private static Mutex mutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "BaleAiBotV2_SingleInstance_App";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {

                IntPtr hWnd = FindWindow(null, "Bale Ai Bot");

                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);    
                }
                return; 
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());

            GC.KeepAlive(mutex);
        }
    }
}