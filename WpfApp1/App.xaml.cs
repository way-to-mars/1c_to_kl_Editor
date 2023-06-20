using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.IO;
using System.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string file_from_args = null;
        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Debug.WriteLine("==== START UP ====");

            try
            {
                // Early check for input file_name
                // if input args and corresponding file exist but the file doesn't starts with "1CClientBankExchange"
                // then open it in notepad and close the App
                string[] args = Environment.GetCommandLineArgs();
                if (args != null && args.Length > 1)
                {
                    // The difference to File.ReadAllLines is that File.ReadLines makes use of lazy evaluation and doesn't read the whole file into an array of lines first.
                    string first_line = File.ReadLines(args[1], Encoding.GetEncoding("windows-1251")).First();
                    if (first_line != null && !first_line.StartsWith("1CClientBankExchange"))
                    {
                        Process.Start("notepad.exe", args[1]);
                        this.Shutdown();
                    }
                    // if File contains 1CClientBankExchange make sure it has exactly format needed
                    if (!Read1c.CheckFormat(
                            File.ReadAllLines(args[1], Encoding.GetEncoding("windows-1251"))
                        )) {
                        Process.Start("notepad.exe", args[1]);
                        this.Shutdown();
                    }
                }
            } catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
