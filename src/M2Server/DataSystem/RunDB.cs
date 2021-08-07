using System;
using System.Diagnostics;
using SystemModule;
using SystemModule.Packages;

namespace M2Server
{
    public class RunDB
    {
        public static bool DBSocketConnected()
        {
            return true;
        }

        private static bool GetDBSockMsg(int nQueryID, ref int nIdent, ref int nRecog, ref string sStr, int dwTimeOut, bool boLoadRcd)
        {
            bool result = false;
            bool boLoadDBOK = false;
            string s24 = string.Empty;
            string s28 = string.Empty;
            string s2C = string.Empty;
            string sCheckFlag = string.Empty;
            string sDefMsg = string.Empty;
            string s38 = string.Empty;
            int nLen;
            int nCheckCode;
            TDefaultMessage DefMsg;
            const string sLoadDBTimeOut = "[RunDB] 读取人物数据超时...";
            const string sSaveDBTimeOut = "[RunDB] 保存人物数据超时...";
            int dwTimeOutTick = HUtil32.GetTickCount();
            while (true)
            {
                if (HUtil32.GetTickCount() - dwTimeOutTick > dwTimeOut)
                {
                    //M2Share.n4EBB6C = M2Share.n4EBB68;
                    break;
                }
                s24 = "";
                HUtil32.EnterCriticalSection(M2Share.UserDBSection);
                try
                {
                    if (M2Share.g_Config.sDBSocketRecvText.IndexOf("!", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        s24 = M2Share.g_Config.sDBSocketRecvText;
                        M2Share.g_Config.sDBSocketRecvText = string.Empty;
                    }
                }
                finally
                {
                    HUtil32.LeaveCriticalSection(M2Share.UserDBSection);
                }
                if (!string.IsNullOrEmpty(s24))
                {
                    s28 = "";
                    s24 = HUtil32.ArrestStringEx(s24, '#', '!', ref s28);
                    if (s28 != "")
                    {
                        s28 = HUtil32.GetValidStr3(s28, ref s2C, new[] { "/" });
                        nLen = s28.Length;
                        unsafe
                        {
                            if (nLen >= 12 && HUtil32.Str_ToInt(s2C, 0) == nQueryID) //sizeof(TDefaultMessage)
                            {
                                nCheckCode = HUtil32.MakeLong(HUtil32.Str_ToInt(s2C, 0) ^ 170, nLen);
                                byte[] data = new byte[sizeof(int)];
                                fixed (byte* by = data)
                                {
                                    *(int*)by = nCheckCode;
                                }
                                sCheckFlag = EDcode.EncodeBuffer(data, data.Length);
                                if (HUtil32.CompareBackLStr(s28, sCheckFlag, sCheckFlag.Length))
                                {
                                    if (nLen == Grobal2.DEFBLOCKSIZE)
                                    {
                                        sDefMsg = s28;
                                        s38 = "";
                                    }
                                    else
                                    {
                                        sDefMsg = s28.Substring(0, Grobal2.DEFBLOCKSIZE);
                                        s38 = s28.Substring(Grobal2.DEFBLOCKSIZE, s28.Length - Grobal2.DEFBLOCKSIZE - 6);
                                    }
                                    DefMsg = EDcode.DecodeMessage(sDefMsg);
                                    nIdent = DefMsg.Ident;
                                    nRecog = DefMsg.Recog;
                                    sStr = s38;
                                    boLoadDBOK = true;
                                    result = true;
                                    break;
                                }
                                else
                                {
                                    M2Share.g_Config.nLoadDBErrorCount++;
                                    break;
                                }
                            }
                            else
                            {
                                M2Share.g_Config.nLoadDBErrorCount++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Thread.CurrentThread.Sleep(1);
                }
            }
            if (!boLoadDBOK)
            {
                if (boLoadRcd)
                {
                    M2Share.ErrorMessage(sLoadDBTimeOut);
                }
                else
                {
                    M2Share.ErrorMessage(sSaveDBTimeOut);
                }
            }
            if (HUtil32.GetTickCount() - dwTimeOutTick > M2Share.dwRunDBTimeMax)
            {
                M2Share.dwRunDBTimeMax = HUtil32.GetTickCount() - dwTimeOutTick;
            }
            M2Share.g_Config.boDBSocketWorking = false;
            return result;
        }

        public static bool MakeHumRcdFromLocal(ref THumDataInfo HumanRcd)
        {
            bool result;
            //FillChar(HumanRcd, sizeof(THumDataInfo), '\0');
            HumanRcd.Data.Abil.Level = 30;
            result = true;
            return result;
        }

        public static bool LoadHumRcdFromDB(string sAccount, string sCharName, string sStr, ref THumDataInfo HumanRcd, int nCertCode)
        {
            bool result = false;
            //FillChar(HumanRcd, sizeof(THumDataInfo), '\0');
            HumanRcd = new THumDataInfo();
            if (LoadRcd(sAccount, sCharName, sStr, nCertCode, ref HumanRcd))
            {
                HumanRcd.Data.sChrName = sCharName;
                HumanRcd.Data.sAccount = sAccount;
                if (HumanRcd.Data.sChrName == sCharName && (HumanRcd.Data.sAccount == "" || HumanRcd.Data.sAccount == sAccount))
                {
                    result = true;
                }
            }
            M2Share.g_Config.nLoadDBCount++;
            return result;
        }

        public static bool SaveHumRcdToDB(string sAccount, string sCharName, int nSessionID, ref THumDataInfo HumanRcd)
        {
            bool result = SaveRcd(sAccount, sCharName, nSessionID, ref HumanRcd);
            M2Share.g_Config.nSaveDBCount ++;
            return result;
        }

        private static bool SaveRcd(string sAccount, string sCharName, int nSessionID, ref THumDataInfo HumanRcd)
        {
            int nIdent = 0;
            int nRecog = 0;
            string sStr = string.Empty;
            int nQueryID = GetQueryID(M2Share.g_Config);
            bool result = false;
            M2Share.DataServer.SendMessage(nQueryID, EDcode.EncodeBuffer(Grobal2.MakeDefaultMsg(Grobal2.DB_SAVEHUMANRCD, nSessionID, 0, 0, 0)) +
                EDcode.EncodeString(sAccount) + "/" + EDcode.EncodeString(sCharName) + "/" + EDcode.EncodeBuffer(HumanRcd));
            if (GetDBSockMsg(nQueryID, ref nIdent, ref nRecog, ref sStr, 5000, false))
            {
                if (nIdent == Grobal2.DBR_SAVEHUMANRCD && nRecog == 1)
                {
                    Debug.WriteLine("[RunDB] 保存人物({0})数据成功", sCharName);
                    result = true;
                }
                else
                {
                    Debug.WriteLine("[RunDB] 保存人物({0})数据失败", sCharName);
                    M2Share.ErrorMessage(string.Format("[RunDB] 保存人物({0})数据失败", sCharName));
                }
            }
            return result;
        }

        private static bool LoadRcd(string sAccount, string sCharName, string sStr, int nCertCode, ref THumDataInfo HumanRcd)
        {
            bool result = true;
            int nIdent = 0;
            int nRecog = 0;
            string sHumanRcdStr = string.Empty;
            int nQueryID = GetQueryID(M2Share.g_Config);
            TLoadHuman loadHuman = new TLoadHuman();
            loadHuman.sAccount = sAccount;
            loadHuman.sChrName = sCharName;
            loadHuman.sUserAddr = sStr;
            loadHuman.nSessionID = nCertCode;
            TDefaultMessage defmsg = Grobal2.MakeDefaultMsg(Grobal2.DB_LOADHUMANRCD, 0, 0, 0, 0);
            string sDBMsg = EDcode.EncodeBuffer(defmsg) + EDcode.EncodeBuffer(loadHuman);
            M2Share.DataServer.SendMessage(nQueryID, sDBMsg);
            if (GetDBSockMsg(nQueryID, ref nIdent, ref nRecog, ref sHumanRcdStr, 5000, true))
            {
                if (nIdent == Grobal2.DBR_LOADHUMANRCD)
                {
                    if (nRecog == 1)
                    {
                        sHumanRcdStr = HUtil32.GetValidStr3(sHumanRcdStr, ref sDBMsg, '/');
                        string sDBCharName = EDcode.DeCodeString(sDBMsg, true);
                        if (sDBCharName == sCharName)
                        {
                            var dataBuff = EDcode.DecodeBuffer(sHumanRcdStr);
                            HumanRcd = new THumDataInfo(dataBuff);
                            result = true;
                        }
                    }
                    else {
                        result = false;
                    }
                }
            }
            return result;
        }

        private static int GetQueryID(TM2Config Config)
        {
            Config.nDBQueryID ++;
            if (Config.nDBQueryID > int.MaxValue - 1)
            {
                Config.nDBQueryID = 1;
            }
            return Config.nDBQueryID;
        }
    } 
}

