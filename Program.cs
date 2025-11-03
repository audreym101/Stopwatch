using System;
using System.Windows.Forms;

namespace Stopwatch
{
    /// <summary>
    /// Main entry point for the Stopwatch application
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}