using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Threading;
//for windows table mode

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            base.ServiceName = "Auto";
        }
        /*
        static private string RunCmd(string command)
        {
            //例Process  
            Process p = new Process();

            p.StartInfo.FileName = "cmd.exe";           //确定程序名  
            p.StartInfo.Arguments = "/c " + command;    //确定程式命令行  
            p.StartInfo.UseShellExecute = false;        //Shell的使用  
            p.StartInfo.RedirectStandardInput = true;   //重定向输入  
            p.StartInfo.RedirectStandardOutput = true; //重定向输出  
            p.StartInfo.RedirectStandardError = true;   //重定向输出错误  
            //p.StartInfo.CreateNoWindow = true;          //设置置不显示示窗口
            p.StartInfo.CreateNoWindow = false;
            p.Start();   //00  

            //p.StandardInput.WriteLine(command);       //也可以用这种方式输入入要行的命令  

            //p.StandardInput.WriteLine("exit");        //要得加上Exit要不然下一行程式  

            return p.StandardOutput.ReadToEnd();        //输出出流取得命令行结果果  

        }  
         */
        public void readProcess(String path)
        {
            System.Diagnostics.Process[] processList = System.Diagnostics.Process.GetProcesses();
            if (processList != null)
            {
                foreach (System.Diagnostics.Process process in processList)
                {
                    WriteTxt(path, process.ProcessName.ToString());
                }
            }
        }
        protected override void OnStart(string[] args)
        {
            //弹出对话框
            /*Interop.ShowMessageBox("This a message from AutoService.",
                          "AutoService Message");
             */
            /* audit模式判断
            String value = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Setup\\State").GetValue("ImageState").ToString();
            if (value.Equals("IMAGE_STATE_GENERALIZE_RESEAL_TO_AUDIT"))
            {
                Interop.ShowMessageBox("IMAGE_STATE_GENERALIZE_RESEAL_TO_AUDIT ", "Registry");
            }
            else if (value.Equals("IMAGE_STATE_COMPLETE"))
            {
                //Interop.ShowMessageBox("IMAGE_STATE_COMPLETE", "Registry");
                Interop.CreateProcess(null, @"wscript.exe C:\WindowService\media.vbs", null);
            }
            else
            {
                Interop.ShowMessageBox(value, "Registry");
            }
            */
            
            //方案一
            /* 
            Interop.CreateProcess(null, @"C:\\Program Files\\Windows Media Player\\wmplayer.exe C:\Windows\ANTHEM.mp4 -fullscreen", null);
             */
            //方案二
            //Interop.CreateProcess(null, @"wscript.exe C:\WindowService\media.vbs", null);

            //失败尝试
            //Interop.CreateProcess("cmd.exe", @"C:\Windows\System32\");
            //Interop.CreateProcess("wscript.exe",@"C:\media.vbs");
            //读取count.txt计数
            System.Diagnostics.Process[] processList = null;
            bool flag = false;
            String name = null;
            while (!flag)
            {
                processList = System.Diagnostics.Process.GetProcesses();
                if (processList != null)
                {
                    foreach (System.Diagnostics.Process process in processList)
                    {
                        if (process.ProcessName.ToLower() == "searchui")
                        {
                            flag = true;
                            name = process.ProcessName.ToString();
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            
            int count = Read();
            //判断是否是第一次
            if (count == 0)
            {
                //readProcess("C:\\WindowService\\beforeProcess.txt");
                while (flag)
                {
                    processList = System.Diagnostics.Process.GetProcesses();
                    if (processList != null)
                    {
                        flag = false;
                        foreach (System.Diagnostics.Process process in processList)
                        {
                            if (process.ProcessName.ToString() == "FirstLogonAnim" || process.ProcessName.ToString() == "LogonUI")
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
                //增加计数
                Write(count + 1);
                //Interop.ShowMessageBox(count.ToString()+",name = "+name, "Count");
                Interop.CreateProcess(null, @"wscript.exe C:\WindowService\first.vbs", null);
                //readProcess("C:\\WindowService\\afterProcess.txt");
            }
            else
            {
                //Interop.ShowMessageBox(count.ToString(), "Count");
                //调用中间进程
                Interop.CreateProcess(@"C:\WindowService\playConsole.exe", String.Empty, null);
                //Interop.CreateProcess(null, @"wscript.exe C:\WindowService\tab.vbs", null);
                Thread.Sleep(127000);
                Interop.CreateProcess(null, @"taskkill /f /im wmplayer.exe", null);
                //Interop.CreateProcess(null, @"wscript.exe C:\WindowService\media.vbs", null);
               /*
                Interop.CreateProcess(null, @"C:\\Program Files\\Windows Media Player\\wmplayer.exe C:\WindowService\ANTHEM.mp4 -fullscreen", null);
                Thread.Sleep(127000);
                Interop.CreateProcess(null, @"taskkill /f /im wmplayer.exe", null);
                */
                
            }
        }
        public int Read()
        {
            int result = 0;
            try
            {
                StreamReader readStream = new StreamReader(@"C:\\WindowService\\count.txt", System.Text.Encoding.Default);
                string line;
                line = readStream.ReadLine();
                result = int.Parse(line);
                readStream.Close();
            }
            catch (IOException e)
            {
                Interop.ShowMessageBox(e.ToString(),"Exception");
            }
            return result;
        }
        public void Write(int count)
        {
            try
            {
                FileStream fs = new FileStream(@"C:\\WindowService\\count.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writeStream = new StreamWriter(fs);
                //writeStream.BaseStream.Seek(0, SeekOrigin.Begin);
                writeStream.WriteLine(count.ToString());
                writeStream.Flush();
                writeStream.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Interop.ShowMessageBox(e.ToString(), "Exception");
            }
        }
        public void WriteTxt(String path, String content)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                StreamWriter writeStream = new StreamWriter(fs);
                writeStream.WriteLine(content);
                writeStream.Flush();
                writeStream.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Interop.ShowMessageBox(e.ToString(), "Exception");
            }
        }

        protected override void OnStop()
        {
        }
    }
}
