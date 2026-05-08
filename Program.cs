using System;
using System.IO;
using System.Windows.Forms;

namespace OSPWebScraper;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            File.WriteAllText("crash.log", ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }
}