using System;
using System.Windows.Forms;

namespace IconCarousel
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, "IconCarousel_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("托盘图标轮播软件已在运行中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                Application.Run(new TrayIconCarousel());
            }
        }
    }
}
