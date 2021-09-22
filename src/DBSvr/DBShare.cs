using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using SystemModule;
using SystemModule.Common;

namespace DBSvr
{
    public class DBShare
    {
        public static string sHumDBFilePath = ".\\FDB\\";
        public static string sDataDBFilePath = ".\\FDB\\";
        public static string sFeedPath = ".\\FDB\\";
        public static string sBackupPath = ".\\FDB\\";
        public static string sConnectPath = ".\\Connects\\";
        public static string sLogPath = ".\\Log\\";
        public static int nServerPort = 6000;
        public static string sServerAddr = "0.0.0.0";
        public static int g_nGatePort = 5100;
        public static string g_sGateAddr = "0.0.0.0";
        public static int nIDServerPort = 5600;
        public static string sIDServerAddr = "10.10.0.168";
        public static bool g_boEnglishNames = false;
        public static bool boViewHackMsg = false;
        public static int g_nQueryChrCount = 0;
        public static int nHackerNewChrCount = 0;
        public static int nHackerDelChrCount = 0;
        public static int nHackerSelChrCount = 0;
        public static int n4ADC1C = 0;
        public static int n4ADC20 = 0;
        public static int n4ADC24 = 0;
        public static int n4ADC28 = 0;
        public static int n4ADBB8 = 0;
        public static string sServerName = "热血传奇";
        public static string sConfFileName = "Dbsrc.ini";
        public static string sGateConfFileName = "!ServerInfo.txt";
        public static string sServerIPConfFileNmae = "!AddrTable.txt";
        public static string sGateIDConfFileName = "SelectID.txt";
        public static string sMapFile = String.Empty;
        public static StringList DenyChrNameList = null;
        public static Hashtable ServerIPList = null;
        public static Dictionary<string, short> GateIDList = null;
        public static long dwInterval = 3000;
        public static int nLevel1 = 1;
        public static int nLevel2 = 7;
        public static int nLevel3 = 14;
        public static int nDay1 = 14;
        public static int nDay2 = 62;
        public static int nDay3 = 124;
        public static int nMonth1 = 0;
        public static int nMonth2 = 0;
        public static int nMonth3 = 0;
        public static int g_nClearRecordCount = 0;
        public static int g_nClearIndex = 0;
        public static int g_nClearCount = 0;
        public static int g_nClearItemIndexCount = 0;
        public static bool boOpenDBBusy = false;
        public static long g_dwGameCenterHandle = 0;
        public static bool g_boDynamicIPMode = false;
        public static StringList g_ClearMakeIndex = null;
        public static TRouteInfo[] g_RouteInfo = new TRouteInfo[20];
        public static bool boDenyChrName = true;
        public static int nDELMaxLevel = 30;
        public static string DBConnection = "server=10.10.0.199;uid=root;pwd=123456;database=Mir2;";

        public static void LoadGateID()
        {
            StringList LoadList;
            string sLineText;
            string sID = string.Empty;
            string sIPaddr = string.Empty;
            GateIDList.Clear();
            if (File.Exists(sGateIDConfFileName))
            {
                LoadList = new StringList();
                LoadList.LoadFromFile(sGateIDConfFileName);
                for (var i = 0; i < LoadList.Count; i++)
                {
                    sLineText = LoadList[i];
                    if ((sLineText == "") || (sLineText[0] == ';'))
                    {
                        continue;
                    }
                    sLineText = HUtil32.GetValidStr3(sLineText, ref sID, new string[] { " ", "\09" });
                    sLineText = HUtil32.GetValidStr3(sLineText, ref sIPaddr, new string[] { " ", "\09" });
                    int nID = HUtil32.Str_ToInt(sID, -1);
                    if (nID < 0)
                    {
                        continue;
                    }
                    GateIDList.Add(sIPaddr, (short)nID);
                }
                LoadList = null;
            }
        }

        public static short GetGateID(string sIPaddr)
        {
            short result = 0;
            if (GateIDList.ContainsKey(sIPaddr))
            {
                return GateIDList[sIPaddr];
            }
            return result;
        }

        public static void LoadIPTable()
        {
            ServerIPList.Clear();
            try
            {
                var stringList = new StringList();
                stringList.LoadFromFile(sServerIPConfFileNmae);
                for (int i = 0; i < stringList.Count; i++)
                {
                    if (ServerIPList.ContainsKey(stringList[i]))
                    {
                        continue;
                    }
                    ServerIPList.Add(stringList[i], stringList[i]);
                }
            }
            catch
            {
                OutMainMessage("加载IP列表文件 " + sServerIPConfFileNmae + " 出错！！！");
            }
        }

        public static void LoadConfig()
        {
            int LoadInteger;
            IniFile Conf = new IniFile(sConfFileName);
            if (Conf != null)
            {
                sDataDBFilePath = Conf.ReadString("DB", "Dir", sDataDBFilePath);
                sHumDBFilePath = Conf.ReadString("DB", "HumDir", sHumDBFilePath);
                sFeedPath = Conf.ReadString("DB", "FeeDir", sFeedPath);
                sBackupPath = Conf.ReadString("DB", "Backup", sBackupPath);
                sConnectPath = Conf.ReadString("DB", "ConnectDir", sConnectPath);
                sLogPath = Conf.ReadString("DB", "LogDir", sLogPath);
                nServerPort = Conf.ReadInteger("Setup", "ServerPort", nServerPort);
                sServerAddr = Conf.ReadString("Setup", "ServerAddr", sServerAddr);
                g_nGatePort = Conf.ReadInteger("Setup", "GatePort", g_nGatePort);
                g_sGateAddr = Conf.ReadString("Setup", "GateAddr", g_sGateAddr);
                sIDServerAddr = Conf.ReadString("Server", "IDSAddr", sIDServerAddr);
                nIDServerPort = Conf.ReadInteger("Server", "IDSPort", nIDServerPort);
                boViewHackMsg = Conf.ReadBool("Setup", "ViewHackMsg", boViewHackMsg);
                sServerName = Conf.ReadString("Setup", "ServerName", sServerName);
                boDenyChrName = Conf.ReadBool("Setup", "DenyChrName", boDenyChrName);
                nDELMaxLevel = Conf.ReadInteger("Setup", "DELMaxLevel", nDELMaxLevel);
                dwInterval = Conf.ReadInteger<long>("DBClear", "Interval", dwInterval);
                nLevel1 = Conf.ReadInteger("DBClear", "Level1", nLevel1);
                nLevel2 = Conf.ReadInteger("DBClear", "Level2", nLevel2);
                nLevel3 = Conf.ReadInteger("DBClear", "Level3", nLevel3);
                nDay1 = Conf.ReadInteger("DBClear", "Day1", nDay1);
                nDay2 = Conf.ReadInteger("DBClear", "Day2", nDay2);
                nDay3 = Conf.ReadInteger("DBClear", "Day3", nDay3);
                nMonth1 = Conf.ReadInteger("DBClear", "Month1", nMonth1);
                nMonth2 = Conf.ReadInteger("DBClear", "Month2", nMonth2);
                nMonth3 = Conf.ReadInteger("DBClear", "Month3", nMonth3);
                LoadInteger = Conf.ReadInteger("Setup", "DynamicIPMode", -1);
                if (LoadInteger < 0)
                {
                    Conf.WriteBool("Setup", "DynamicIPMode", g_boDynamicIPMode);
                }
                else
                {
                    g_boDynamicIPMode = LoadInteger == 1;
                }
                g_boEnglishNames = Conf.ReadBool("Setup", "EnglishNameOnly", g_boEnglishNames);
                //Conf.Free;
            }
            LoadIPTable();
            LoadGateID();
        }

        public static bool CheckChrName(string sChrName)
        {
            char Chr;
            bool result = true;
            bool boIsTwoByte = false;
            char FirstChr = '\0';
            for (var i = 0; i <= sChrName.Length; i++)
            {
                Chr = sChrName[i];
                if (boIsTwoByte)
                {
                    if (!((FirstChr <= '÷') && (Chr >= '@') && (Chr <= 't')))
                    {
                        if (!((FirstChr > '÷') && (Chr >= '@') && (Chr <= '?')))
                        {
                            result = false;
                        }
                    }
                    boIsTwoByte = false;
                }
                else
                {
                    if ((Chr >= '?') && (Chr <= 't'))
                    {
                        boIsTwoByte = true;
                        FirstChr = Chr;
                    }
                    else
                    {
                        if (!((Chr >= '0') && (Chr <= '9')) && !((Chr >= 'a') && (Chr <= 'z')) && !((Chr >= 'A') && (Chr <= 'Z')))
                        {
                            result = false;
                        }
                    }
                }
                if (!result)
                {
                    break;
                }
            }
            return result;
        }

        public static bool InClearMakeIndexList(int nIndex)
        {
            bool result = false;
            for (var i = 0; i < g_ClearMakeIndex.Count; i++)
            {
                //if (nIndex == ((int)g_ClearMakeIndex.Values[i]))
                //{
                //    result = true;
                //    break;
                //}
            }
            return result;
        }

        public static void OutMainMessage(string sMsg)
        {
            WriteLogMsg(sMsg);
        }

        private static void WriteLogMsg(string sMsg)
        {
            Console.WriteLine(sMsg);
        }

        public static bool CheckServerIP(string sIP)
        {
            bool result = false;
            if (ServerIPList.ContainsKey(sIP))
            {
                return true;
            }
            return result;
        }

        public static void MainOutMessage(string sMsg)
        {
            Console.WriteLine(sMsg);
        }

        public static void Initialization()
        {
            //InitializeCriticalSection(HumDB_CS);
            DenyChrNameList = new StringList();
            ServerIPList = new Hashtable();
            GateIDList = new Dictionary<string, short>();
            //g_ClearMakeIndex = new object();
        }

        public void finalization()
        {
            //DenyChrNameList.Free;
            //ServerIPList.Free;
            //GateIDList.Free;
            //g_ClearMakeIndex.Free;
        }

    }

    public class TServerInfo
    {
        public int nSckHandle;
        public string sStr;
        public bool bo08;
        public Socket Socket;
    }

    public class THumSession
    {
        public string sChrName;
        public int nIndex;
        public Socket Socket;
        public bool bo24;
        public bool bo2C;
        public long dwTick30;
    }

    public class THumInfo
    {
        public TRecordHeader Header;
        public string sChrName;
        public string sAccount;
        public bool boDeleted;
        public bool boSelected;
        public DateTime dModDate;
        public byte btCount;
        public byte[] unknown2;
    }

    public class TGlobaSessionInfo
    {
        public string sAccount;
        public string sIPaddr;
        public int nSessionID;
        public long dwAddTick;
        public DateTime dAddDate;
        public bool boLoadRcd;
        public bool boStartPlay;
    }

    public class TGateInfo
    {
        public Socket Socket;
        public string sGateaddr;
        public string sText;
        public IList<TUserInfo> UserList;
        public long dwTick10;
        public short nGateID;
    }

    public class TUserInfo
    {
        public string sAccount;
        public string sUserIPaddr;
        public string sGateIPaddr;
        public string sConnID;
        public int nSessionID;
        public Socket Socket;
        public string s2C;
        public bool boChrSelected;
        public bool boChrQueryed;
        public long dwTick34;
        public long dwChrTick;
        public short nSelGateID;
    }

    public class TRouteInfo
    {
        public int nGateCount;
        public string sSelGateIP;
        public string[] sGameGateIP;
        public int[] nGameGatePort;

        public TRouteInfo()
        {
            sGameGateIP = new string[8];
            nGameGatePort = new int[8];
        }
    }

    public class TQueryChr
    {
        public byte btJob;
        public byte btHair;
        public byte btSex;
        public ushort wLevel;
        public string sName;
    }

    public class TDBHeader
    {
        public string sDesc;
        public int n24;
        public int n28;
        public int n2C;
        public int n30;
        public int n34;
        public int n38;
        public int n3C;
        public int n40;
        public int n44;
        public int n48;
        public int n4C;
        public int n50;
        public int n54;
        public int n58;
        public int nLastIndex;
        public DateTime dLastDate;
        public int nHumCount;
        public int n6C;
        public int n70;
        public DateTime dUpdateDate;
    }
}