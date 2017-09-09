using System;
using System.Collections.Generic;
using System.IO;
using BytesRoad.Net.Ftp;

namespace Licence_Data_Base
{
    class Program
    {
        enum LicenceTypes
        {
            Developer = 0,
            Tester,
            TesterYoutube,
            Youtuber,
            Player,
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(165, 35);
            Console.WriteLine("Reading configs...");
            string Path_B = ReadDBPath();
            CFGBundle FTPInfo = ReadFTPcfg();

            if (!String.IsNullOrWhiteSpace(Path_B))
            {
                if (!FTPInfo.isLocalDB)
                {
                    Console.WriteLine("Downloading databases...");
                    ReadFTP(FTPInfo, Path_B);
                }

                List<DataBase> LicenceList = new List<DataBase>();

                Console.WriteLine("Reading databases...");
                LicenceList = ReadDB(Path_B);

                string Input = "";
                string[] InputStrings = { " " };

                Console.Write("\nReady!\n\n|--------------------------------------|\n|Licence Data Base Access Terminal v1.2|\n|       Made by Nikolay Repin          | \n|            Backlink 2017             |\n|--------------------------------------|\n");

                while (InputStrings[0].ToLower() != "quit")
                {
                    Console.Write("> ");

                    Input = Console.ReadLine();

                    InputStrings = Input.Split(' ');

                    if (InputStrings[0].ToLower() == "database" && InputStrings[1].ToLower() == "add" && InputStrings.Length == 8)
                    {
                        Console.Write("Adding new record: \n\t\tName: " + Convert.ToString(InputStrings[2]) + "\n\t\tLicence Type: " + Convert.ToString(InputStrings[3]) + "\n\t\tDate of Issue: " + Convert.ToString(InputStrings[4]) + "\n\t\tDate of Expiry: " + Convert.ToString(InputStrings[5]) + "\n\t\tUsername: " + Convert.ToString(InputStrings[6]) + "\n\t\tPassword: " + Convert.ToString(InputStrings[7]) + "\n\n");
                        LicenceList.Add(new DataBase(LicenceList, InputStrings[2], Convert.ToInt16(InputStrings[3]), InputStrings[4], InputStrings[5], UIDGen(LicenceList), InputStrings[6], InputStrings[7]));
                    }

                    if (InputStrings[0].ToLower() == "database" && InputStrings[1].ToLower() == "list" && InputStrings.Length == 2)
                    {
                        Console.Write("Listing all records...\n\n|№\t|Name\t\t\t|Licence Type\t\t|Date of Issue\t|Date of Expiry\t|UID\t\t|Username\t|Password|\n");
                        foreach (DataBase DB in LicenceList)
                        {
                            Console.WriteLine(" " + Convert.ToString(DB.Num) + "\t " + DB.Name + "\t " + DB.LicenceType + " (" + (LicenceTypes)DB.LicenceType + ")\t " + DB.DeclarationDate + "\t " + DB.ExpiryDate + "\t " + DB.UserID + "\t " + DB.Username + "\t " + DB.UserPassword);
                        }
                    }

                    if (InputStrings[0].ToLower() == "database" && InputStrings[1].ToLower() == "remove" && InputStrings.Length == 3)
                    {
                        Console.Write("Removing record at slot " + InputStrings[2] + "...\n\n");
                        LicenceList.RemoveAt(Convert.ToInt16(InputStrings[2]) - 1);

                        for (int i = 0; i < LicenceList.Count; ++i)
                        {
                            LicenceList[i].Num = i + 1;
                        }
                    }

                    if (InputStrings[0].ToLower() == "colorscheme" && InputStrings.Length == 3)
                    {
                        if (Enum.IsDefined(typeof(ConsoleColor), Convert.ToInt32(InputStrings[1])))
                        {
                            Console.ForegroundColor = (ConsoleColor)Convert.ToInt32(InputStrings[1]);
                        }
                        if (Enum.IsDefined(typeof(ConsoleColor), Convert.ToInt32(InputStrings[2])))
                        {
                            Console.BackgroundColor = (ConsoleColor)Convert.ToInt32(InputStrings[2]);
                        }

                        Console.Clear();
                    }

                    if (InputStrings[0].ToLower() == "database" && InputStrings[1].ToLower() == "edit" && InputStrings.Length == 3)
                    {
                        int TargetNumber = Convert.ToInt16(InputStrings[2]) - 1;

                        Console.WriteLine(">Enter new parameters: ");
                        Console.Write("> ");

                        Input = Console.ReadLine();

                        InputStrings = Input.Split(' ');

                        try
                        {
                            LicenceList[TargetNumber].Name = InputStrings[0];
                            LicenceList[TargetNumber].LicenceType = Convert.ToInt16(InputStrings[1]);
                            LicenceList[TargetNumber].DeclarationDate = InputStrings[2];
                            LicenceList[TargetNumber].ExpiryDate = InputStrings[3];
                            LicenceList[TargetNumber].Username = InputStrings[4];
                            LicenceList[TargetNumber].UserPassword = InputStrings[5];

                            Console.WriteLine("Record in slot " + Convert.ToString(TargetNumber + 1) + " successfully edited!\n");
                        }
                        catch { Console.Write("\n\nError.\n\n"); }
                    }
                    if (InputStrings[0].ToLower() == "clear" && InputStrings.Length == 1)
                        Console.Clear();
                }

                FileStream FS = new FileStream(Path_B, FileMode.Create);
                StreamWriter SW = new StreamWriter(FS);

                foreach (DataBase DB in LicenceList)
                {
                    Console.WriteLine("Writing down database file...");
                    SW.WriteLine(Convert.ToString(DB.Num) + "\t" + DB.Name + "\t" + DB.LicenceType + "\t" + DB.DeclarationDate + "\t" + DB.ExpiryDate + "\t" + (DB.UserID + 71) + "\t" + DB.Username + "\t" + DB.UserPassword);
                }

                SW.Close();

                if (!FTPInfo.isLocalDB)
                {
                    Console.WriteLine("Uploading database...");
                    WriteFTP(FTPInfo, Path_B);
                    File.Delete(Path_B);
                }

                Console.Clear();
                Console.WriteLine("Hit any key to exit...");
                Console.ReadKey();
            }
            else
            {
                Console.Write("No Database Selected, please enter database path: ");
                FileStream FS = new FileStream("Config/Databases.cfg", FileMode.OpenOrCreate);
                StreamWriter SW = new StreamWriter(FS);
                SW.WriteLine("Databases/" + Console.ReadLine() + ".bcldb");
                SW.Close();
            }
        }

        private static List<DataBase> ReadDB(string Path)
        {
            List<DataBase> LicenceList_B = new List<DataBase>();
            string[] FileStrings;

            FileStream FS = new FileStream(Path, FileMode.OpenOrCreate);
            StreamReader SR = new StreamReader(FS);

            while (!SR.EndOfStream)
            {
                FileStrings = SR.ReadLine().Split('\t');

                if (FileStrings.Length == 8)
                    LicenceList_B.Add(new DataBase(LicenceList_B, FileStrings[1], Convert.ToInt16(FileStrings[2]), FileStrings[3], FileStrings[4], (Convert.ToInt32(FileStrings[5]) - 71), FileStrings[6], FileStrings[7]));
            }

            SR.Close();

            return LicenceList_B;
        }

        private static string ReadDBPath()
        {
            FileStream FS = new FileStream("Config/Databases.cfg", FileMode.OpenOrCreate);
            StreamReader SR = new StreamReader(FS);
            string path = SR.ReadLine();
            SR.Close();
            return path;
        }

        private static CFGBundle ReadFTPcfg()
        {
            FileStream FS = new FileStream("Config/ftpConfig.cfg", FileMode.OpenOrCreate);
            StreamReader SR = new StreamReader(FS);

            CFGBundle OutOfCfg = new CFGBundle();

            OutOfCfg.isLocalDB = Convert.ToBoolean(SR.ReadLine());
            OutOfCfg.PassiveMode = Convert.ToBoolean(SR.ReadLine());
            OutOfCfg.TimeoutFTP = Convert.ToInt16(SR.ReadLine());
            OutOfCfg.FTP_SERVER = SR.ReadLine();
            OutOfCfg.FTP_PORT = Convert.ToInt16(SR.ReadLine());
            OutOfCfg.FTP_USER = SR.ReadLine();
            OutOfCfg.FTP_PASSWORD = SR.ReadLine();

            SR.Close();

            return OutOfCfg;
        }

        private static int UIDGen(List<DataBase> LicenceList)
        {
            Random rnd = new Random();
            int UID_B = rnd.Next();
            bool isOK = false;

            while (!isOK)
            {
                isOK = true;
                foreach (DataBase DB in LicenceList)
                {
                    if (UID_B == DB.UserID)
                    {
                        isOK = false;
                        UID_B = rnd.Next();
                    }
                }
            }

            return UID_B;
        }

        private static void ReadFTP(CFGBundle FTPInfo, string Path)
        {
            FtpClient client = new FtpClient();

            //Задаём параметры клиента.
            //client.PassiveMode = true; //Включаем пассивный режим.
            //int TimeoutFTP = 30000; //Таймаут.
            //string FTP_SERVER = "адрес фтп сервера";
            //int FTP_PORT = 9999;
            //string FTP_USER = "пользователь";
            //string FTP_PASSWORD = "пароль";

            try
            {
                client.Connect(FTPInfo.TimeoutFTP, FTPInfo.FTP_SERVER, FTPInfo.FTP_PORT);
                client.Login(FTPInfo.TimeoutFTP, FTPInfo.FTP_USER, FTPInfo.FTP_PASSWORD);

                client.GetFile(FTPInfo.TimeoutFTP, Path, "/public_html/Databases/MANDB.bcldb");

                client.Disconnect(FTPInfo.TimeoutFTP);
            }
            catch (FtpTimeoutException)
            {
                Console.WriteLine("Error: timed out");
                Console.WriteLine("\nHit any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
                return;
            }
            catch
            {
                Console.WriteLine("Error: unidentified error");
                Console.WriteLine("\nHit any key to exit...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        private static void WriteFTP(CFGBundle FTPInfo, string Path)
        {
            FtpClient client = new FtpClient();

            client.Connect(FTPInfo.TimeoutFTP, FTPInfo.FTP_SERVER, FTPInfo.FTP_PORT);
            client.Login(FTPInfo.TimeoutFTP, FTPInfo.FTP_USER, FTPInfo.FTP_PASSWORD);

            client.PutFile(FTPInfo.TimeoutFTP, "/public_html/Databases/MANDB.bcldb", Path);

            client.Disconnect(FTPInfo.TimeoutFTP);
        }


        /*private static void RandDBGen()
        {
            FileStream FS = new FileStream("Databases/ExDb.bcldb", FileMode.OpenOrCreate);
            StreamWriter SW = new StreamWriter(FS);
            Random rnd = new Random();

            for (int i = 0; i < 1000000; ++i)
            {
                SW.WriteLine(Convert.ToString(rnd.Next()) + "\t" + Convert.ToString(rnd.Next()) + "\t" + Convert.ToString(rnd.Next()) + "\t" + Convert.ToString(rnd.Next()) + "\t" + Convert.ToString(rnd.Next()));
            }

            SW.Close();
        }*/
    }

    class DataBase
    {
        public int Num;
        public string Name;
        public int LicenceType;
        public string DeclarationDate;
        public string ExpiryDate;
        public long UserID;
        public string Username;
        public string UserPassword;

        public DataBase(List<DataBase> LicenceList, string Name, int LicenceType, string DeclarationDate, string ExpiryDate, long UID, string Username, string Pass)
        {
            Num = LicenceList.Count + 1;
            this.Name = Name;
            this.LicenceType = LicenceType;
            this.DeclarationDate = DeclarationDate;
            this.ExpiryDate = ExpiryDate;
            UserID = UID;
            this.Username = Username;
            UserPassword = Pass;
        }
    }

    struct CFGBundle
    {
        public bool isLocalDB;
        public bool PassiveMode;
        public int TimeoutFTP;
        public string FTP_SERVER;
        public int FTP_PORT;
        public string FTP_USER;
        public string FTP_PASSWORD;
    }
}
