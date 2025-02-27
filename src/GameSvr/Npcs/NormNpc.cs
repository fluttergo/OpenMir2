﻿using SystemModule;

namespace GameSvr
{
    /// <summary>
    /// 管理类NPC
    /// 如 月老
    /// </summary>
    public partial class NormNpc : AnimalObject
    {
        /// <summary>
        /// 用于标识此NPC是否有效，用于重新加载NPC列表(-1 为无效)
        /// </summary>
        public short m_nFlag = 0;
        public int[] FGotoLable;
        public IList<TScript> m_ScriptList = null;
        public string m_sFilePath = string.Empty;
        /// <summary>
        /// 此NPC是否是隐藏的，不显示在地图中
        /// </summary>
        public bool m_boIsHide = false;
        /// <summary>
        /// NPC类型为地图任务型的，加载脚本时的脚本文件名为 角色名-地图号.txt
        /// </summary>
        public bool m_boIsQuest = false;
        protected string m_sPath = string.Empty;
        private IList<TScriptParams> BatchParamsList;

        public virtual void ClearScript()
        {
            m_ScriptList.Clear();
        }

        public virtual void Click(TPlayObject PlayObject)
        {
            PlayObject.m_nScriptGotoCount = 0;
            PlayObject.m_sScriptGoBackLable = "";
            PlayObject.m_sScriptCurrLable = "";
            GotoLable(PlayObject, "@main", false);
        }

        public NormNpc() : base()
        {
            this.m_boSuperMan = true;
            this.m_btRaceServer = Grobal2.RC_NPC;
            this.m_nLight = 2;
            this.m_btAntiPoison = 99;
            this.m_ScriptList = new List<TScript>();
            this.m_boStickMode = true;
            this.m_sFilePath = "";
            this.m_boIsHide = false;
            this.m_boIsQuest = true;
            this.FGotoLable = new int[100];
        }

        ~NormNpc()
        {
            ClearScript();
        }

        private void ExeAction(TPlayObject PlayObject, string sParam1, string sParam2, string sParam3, int nParam1, int nParam2, int nParam3)
        {
            int nInt1;
            int dwInt;
            // ================================================
            // 更改人物当前经验值
            // EXEACTION CHANGEEXP 0 经验数  设置为指定经验值
            // EXEACTION CHANGEEXP 1 经验数  增加指定经验
            // EXEACTION CHANGEEXP 2 经验数  减少指定经验
            // ================================================
            if (string.Compare(sParam1, "CHANGEEXP", StringComparison.OrdinalIgnoreCase) == 0)
            {
                nInt1 = HUtil32.Str_ToInt(sParam2, -1);
                switch (nInt1)
                {
                    case 0:
                        if (nParam3 >= 0)
                        {
                            PlayObject.m_Abil.Exp = nParam3;
                            PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        }
                        break;
                    case 1:
                        if (PlayObject.m_Abil.Exp >= nParam3)
                        {
                            if ((PlayObject.m_Abil.Exp - nParam3) > (int.MaxValue - PlayObject.m_Abil.Exp))
                            {
                                dwInt = int.MaxValue - PlayObject.m_Abil.Exp;
                            }
                            else
                            {
                                dwInt = nParam3;
                            }
                        }
                        else
                        {
                            if ((nParam3 - PlayObject.m_Abil.Exp) > (int.MaxValue - nParam3))
                            {
                                dwInt = int.MaxValue - nParam3;
                            }
                            else
                            {
                                dwInt = nParam3;
                            }
                        }
                        PlayObject.m_Abil.Exp += dwInt;
                        PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        break;
                    case 2:
                        if (PlayObject.m_Abil.Exp > nParam3)
                        {
                            PlayObject.m_Abil.Exp -= nParam3;
                        }
                        else
                        {
                            PlayObject.m_Abil.Exp = 0;
                        }
                        PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        break;
                }
                PlayObject.SysMsg("您当前经验点数为: " + PlayObject.m_Abil.Exp + '/' + PlayObject.m_Abil.MaxExp, MsgColor.Green, MsgType.Hint);
                return;
            }
            // ================================================
            // 更改人物当前等级
            // EXEACTION CHANGELEVEL 0 等级数  设置为指定等级
            // EXEACTION CHANGELEVEL 1 等级数  增加指定等级
            // EXEACTION CHANGELEVEL 2 等级数  减少指定等级
            // ================================================
            if (string.Compare(sParam1, "CHANGELEVEL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                nInt1 = HUtil32.Str_ToInt(sParam2, -1);
                switch (nInt1)
                {
                    case 0:
                        if (nParam3 >= 0)
                        {
                            PlayObject.m_Abil.Level = (ushort)nParam3;
                            PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        }
                        break;
                    case 1:
                        if (PlayObject.m_Abil.Level >= nParam3)
                        {
                            if ((PlayObject.m_Abil.Level - nParam3) > (short.MaxValue - PlayObject.m_Abil.Level))
                            {
                                dwInt = short.MaxValue - PlayObject.m_Abil.Level;
                            }
                            else
                            {
                                dwInt = nParam3;
                            }
                        }
                        else
                        {
                            if ((nParam3 - PlayObject.m_Abil.Level) > (int.MaxValue - nParam3))
                            {
                                dwInt = int.MaxValue - nParam3;
                            }
                            else
                            {
                                dwInt = nParam3;
                            }
                        }
                        PlayObject.m_Abil.Level += (ushort)dwInt;
                        PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        break;
                    case 2:
                        if (PlayObject.m_Abil.Level > nParam3)
                        {
                            PlayObject.m_Abil.Level -= (ushort)nParam3;
                        }
                        else
                        {
                            PlayObject.m_Abil.Level = 0;
                        }
                        PlayObject.HasLevelUp(PlayObject.m_Abil.Level - 1);
                        break;
                }
                PlayObject.SysMsg("您当前等级为: " + PlayObject.m_Abil.Level, MsgColor.Green, MsgType.Hint);
                return;
            }
            // ================================================
            // 杀死人物
            // EXEACTION KILL 0 人物死亡,不显示凶手信息
            // EXEACTION KILL 1 人物死亡不掉物品,不显示凶手信息
            // EXEACTION KILL 2 人物死亡,显示凶手信息为NPC
            // EXEACTION KILL 3 人物死亡不掉物品,显示凶手信息为NPC
            // ================================================
            if (string.Compare(sParam1, "KILL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                nInt1 = HUtil32.Str_ToInt(sParam2, -1);
                switch (nInt1)
                {
                    case 1:
                        PlayObject.m_boNoItem = true;
                        PlayObject.Die();
                        break;
                    case 2:
                        PlayObject.SetLastHiter(this);
                        PlayObject.Die();
                        break;
                    case 3:
                        PlayObject.m_boNoItem = true;
                        PlayObject.SetLastHiter(this);
                        PlayObject.Die();
                        break;
                    default:
                        PlayObject.Die();
                        break;
                }
                return;
            }
            // ================================================
            // 踢人物下线
            // EXEACTION KICK
            // ================================================
            if (string.Compare(sParam1, "KICK", StringComparison.OrdinalIgnoreCase) == 0)
            {
                PlayObject.m_boKickFlag = true;
                return;
            }
        }

        public string GetLineVariableText(TPlayObject PlayObject, string sMsg)
        {
            var nC = 0;
            var s10 = string.Empty;
            while (true)
            {
                if (HUtil32.TagCount(sMsg, '>') < 1)
                {
                    break;
                }
                HUtil32.ArrestStringEx(sMsg, '<', '>', ref s10);
                GetVariableText(PlayObject, ref sMsg, s10);
                nC++;
                if (nC >= 101)
                {
                    break;
                }
            }
            return sMsg;
        }

        /// <summary>
        /// 获取全局变量信息
        /// </summary>
        /// <param name="PlayObject"></param>
        /// <param name="sMsg"></param>
        /// <param name="sVariable"></param>
        protected virtual void GetVariableText(TPlayObject PlayObject, ref string sMsg, string sVariable)
        {
            string sText = string.Empty;
            string s14 = string.Empty;
            TDynamicVar DynamicVar;
            bool boFoundVar;
            switch (sVariable)
            {
                case "$SERVERNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$SERVERNAME>", M2Share.g_Config.sServerName);
                    return;
                case "$SERVERIP":
                    sMsg = ReplaceVariableText(sMsg, "<$SERVERIP>", M2Share.g_Config.sServerIPaddr);
                    return;
                case "$WEBSITE":
                    sMsg = ReplaceVariableText(sMsg, "<$WEBSITE>", M2Share.g_Config.sWebSite);
                    return;
                case "$BBSSITE":
                    sMsg = ReplaceVariableText(sMsg, "<$BBSSITE>", M2Share.g_Config.sBbsSite);
                    return;
                case "$CLIENTDOWNLOAD":
                    sMsg = ReplaceVariableText(sMsg, "<$CLIENTDOWNLOAD>", M2Share.g_Config.sClientDownload);
                    return;
                case "$QQ":
                    sMsg = ReplaceVariableText(sMsg, "<$QQ>", M2Share.g_Config.sQQ);
                    return;
                case "$PHONE":
                    sMsg = ReplaceVariableText(sMsg, "<$PHONE>", M2Share.g_Config.sPhone);
                    return;
                case "$BANKACCOUNT0":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT0>", M2Share.g_Config.sBankAccount0);
                    return;
                case "$BANKACCOUNT1":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT1>", M2Share.g_Config.sBankAccount1);
                    return;
                case "$BANKACCOUNT2":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT2>", M2Share.g_Config.sBankAccount2);
                    return;
                case "$BANKACCOUNT3":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT3>", M2Share.g_Config.sBankAccount3);
                    return;
                case "$BANKACCOUNT4":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT4>", M2Share.g_Config.sBankAccount4);
                    return;
                case "$BANKACCOUNT5":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT5>", M2Share.g_Config.sBankAccount5);
                    return;
                case "$BANKACCOUNT6":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT6>", M2Share.g_Config.sBankAccount6);
                    return;
                case "$BANKACCOUNT7":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT7>", M2Share.g_Config.sBankAccount7);
                    return;
                case "$BANKACCOUNT8":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT8>", M2Share.g_Config.sBankAccount8);
                    return;
                case "$BANKACCOUNT9":
                    sMsg = ReplaceVariableText(sMsg, "<$BANKACCOUNT9>", M2Share.g_Config.sBankAccount9);
                    return;
                case "$GAMEGOLDNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$GAMEGOLDNAME>", M2Share.g_Config.sGameGoldName);
                    return;
                case "$GAMEPOINTNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$GAMEPOINTNAME>", M2Share.g_Config.sGamePointName);
                    return;
                case "$USERCOUNT":
                    sText = M2Share.UserEngine.PlayObjectCount.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$USERCOUNT>", sText);
                    return;
                case "$MACRUNTIME":
                    sText = (HUtil32.GetTickCount() / (24 * 60 * 60 * 1000)).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MACRUNTIME>", sText);
                    return;
                case "$SERVERRUNTIME":
                    var nSecond = (HUtil32.GetTickCount() - M2Share.g_dwStartTick) / 1000;
                    var wHour = nSecond / 3600;
                    var wMinute = (nSecond / 60) % 60;
                    var wSecond = nSecond % 60;
                    sText = format("{0}:{1}:{2}", wHour, wMinute, wSecond);
                    sMsg = ReplaceVariableText(sMsg, "<$SERVERRUNTIME>", sText);
                    return;
                case "$DATETIME":
                    sText = DateTime.Now.ToString("dddddd,dddd,hh:mm:nn");
                    sMsg = ReplaceVariableText(sMsg, "<$DATETIME>", sText);
                    return;
                case "$HIGHLEVELINFO":
                    {
                        if (M2Share.g_HighLevelHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighLevelHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHLEVELINFO>", sText);
                        return;
                    }
                case "$HIGHPKINFO":
                    {
                        if (M2Share.g_HighPKPointHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighPKPointHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHPKINFO>", sText);
                        return;
                    }
                case "$HIGHDCINFO":
                    {
                        if (M2Share.g_HighDCHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighDCHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHDCINFO>", sText);
                        return;
                    }
                case "$HIGHMCINFO":
                    {
                        if (M2Share.g_HighMCHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighMCHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHMCINFO>", sText);
                        return;
                    }
                case "$HIGHSCINFO":
                    {
                        if (M2Share.g_HighSCHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighSCHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHSCINFO>", sText);
                        return;
                    }
                case "$HIGHONLINEINFO":
                    {
                        if (M2Share.g_HighOnlineHuman != null)
                        {
                            sText = ((TPlayObject)M2Share.g_HighOnlineHuman).GetMyInfo();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$HIGHONLINEINFO>", sText);
                        return;
                    }
                case "$RANDOMNO":
                    sMsg = ReplaceVariableText(sMsg, "<$RANDOMNO>", PlayObject.m_sRandomNo);
                    return;
                case "$RELEVEL":
                    sText = PlayObject.m_btReLevel.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$RELEVEL>", sText);
                    return;
                case "$HUMANSHOWNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$HUMANSHOWNAME>", PlayObject.GetShowName());
                    return;
                case "$MONKILLER":
                    {
                        if (PlayObject.m_LastHiter != null)
                        {
                            if (PlayObject.m_LastHiter.m_btRaceServer != Grobal2.RC_PLAYOBJECT)
                            {
                                sMsg = ReplaceVariableText(sMsg, "<$MONKILLER>", PlayObject.m_LastHiter.m_sCharName);
                            }
                        }
                        else
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$MONKILLER>", "未知");
                        }
                        return;
                    }
                case "$QUERYYBDEALLOG":// 查看元宝交易记录 
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$QUERYYBDEALLOG>", PlayObject.SelectSellDate());
                        return;
                    }
                case "$KILLER":
                    {
                        if (PlayObject.m_LastHiter != null)
                        {
                            if (PlayObject.m_LastHiter.m_btRaceServer == Grobal2.RC_PLAYOBJECT)
                            {
                                sMsg = ReplaceVariableText(sMsg, "<$KILLER>", PlayObject.m_LastHiter.m_sCharName);
                            }
                        }
                        else
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$KILLER>", "未知");
                        }
                        return;
                    }
                case "$USERNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$USERNAME>", PlayObject.m_sCharName);
                    return;
                case "$GUILDNAME":
                    {
                        if (PlayObject.m_MyGuild != null)
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$GUILDNAME>", PlayObject.m_MyGuild.sGuildName);
                        }
                        else
                        {
                            sMsg = "无";
                        }
                        return;
                    }
                case "$RANKNAME":
                    sMsg = ReplaceVariableText(sMsg, "<$RANKNAME>", PlayObject.m_sGuildRankName);
                    return;
                case "$LEVEL":
                    sText = PlayObject.m_Abil.Level.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$LEVEL>", sText);
                    return;
                case "$HP":
                    sText = PlayObject.m_WAbil.HP.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$HP>", sText);
                    return;
                case "$MAXHP":
                    sText = PlayObject.m_WAbil.MaxHP.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXHP>", sText);
                    return;
                case "$MP":
                    sText = PlayObject.m_WAbil.MP.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MP>", sText);
                    return;
                case "$MAXMP":
                    sText = PlayObject.m_WAbil.MaxMP.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXMP>", sText);
                    return;
                case "$AC":
                    sText = HUtil32.LoWord(PlayObject.m_WAbil.AC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$AC>", sText);
                    return;
                case "$MAXAC":
                    sText = HUtil32.HiWord(PlayObject.m_WAbil.AC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXAC>", sText);
                    return;
                case "$MAC":
                    sText = HUtil32.LoWord(PlayObject.m_WAbil.MAC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAC>", sText);
                    return;
                case "$MAXMAC":
                    sText = HUtil32.HiWord(PlayObject.m_WAbil.MAC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXMAC>", sText);
                    return;
                case "$DC":
                    sText = HUtil32.LoWord(PlayObject.m_WAbil.DC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$DC>", sText);
                    return;
                case "$MAXDC":
                    sText = HUtil32.HiWord(PlayObject.m_WAbil.DC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXDC>", sText);
                    return;
                case "$MC":
                    sText = HUtil32.LoWord(PlayObject.m_WAbil.MC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MC>", sText);
                    return;
                case "$MAXMC":
                    sText = HUtil32.HiWord(PlayObject.m_WAbil.MC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXMC>", sText);
                    return;
                case "$SC":
                    sText = HUtil32.LoWord(PlayObject.m_WAbil.SC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$SC>", sText);
                    return;
                case "$MAXSC":
                    sText = HUtil32.HiWord(PlayObject.m_WAbil.SC).ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXSC>", sText);
                    return;
                case "$EXP":
                    sText = PlayObject.m_Abil.Exp.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$EXP>", sText);
                    return;
                case "$MAXEXP":
                    sText = PlayObject.m_Abil.MaxExp.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXEXP>", sText);
                    return;
                case "$PKPOINT":
                    sText = PlayObject.m_nPkPoint.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$PKPOINT>", sText);
                    return;
                case "$CREDITPOINT":
                    sText = PlayObject.m_btCreditPoint.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$CREDITPOINT>", sText);
                    return;
                case "$HW":
                    sText = PlayObject.m_WAbil.HandWeight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$HW>", sText);
                    return;
                case "$MAXHW":
                    sText = PlayObject.m_WAbil.MaxHandWeight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXHW>", sText);
                    return;
                case "$BW":
                    sText = PlayObject.m_WAbil.Weight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$BW>", sText);
                    return;
                case "$MAXBW":
                    sText = PlayObject.m_WAbil.MaxWeight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXBW>", sText);
                    return;
                case "$WW":
                    sText = PlayObject.m_WAbil.WearWeight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$WW>", sText);
                    return;
                case "$MAXWW":
                    sText = PlayObject.m_WAbil.MaxWearWeight.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$MAXWW>", sText);
                    return;
                case "$GOLDCOUNT":
                    sText = PlayObject.m_nGold.ToString() + '/' + PlayObject.m_nGoldMax;
                    sMsg = ReplaceVariableText(sMsg, "<$GOLDCOUNT>", sText);
                    return;
                case "$GAMEGOLD":
                    sText = PlayObject.m_nGameGold.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$GAMEGOLD>", sText);
                    return;
                case "$GAMEPOINT":
                    sText = PlayObject.m_nGamePoint.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$GAMEPOINT>", sText);
                    return;
                case "$HUNGER":
                    sText = PlayObject.GetMyStatus().ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$HUNGER>", sText);
                    return;
                case "$LOGINTIME":
                    sText = PlayObject.m_dLogonTime.ToString();
                    sMsg = ReplaceVariableText(sMsg, "<$LOGINTIME>", sText);
                    return;
                case "$LOGINLONG":
                    sText = ((HUtil32.GetTickCount() - PlayObject.m_dwLogonTick) / 60000) + "分钟";
                    sMsg = ReplaceVariableText(sMsg, "<$LOGINLONG>", sText);
                    return;
                case "$DRESS":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_DRESS].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$DRESS>", sText);
                    return;
                case "$WEAPON":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_WEAPON].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$WEAPON>", sText);
                    return;
                case "$RIGHTHAND":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_RIGHTHAND].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$RIGHTHAND>", sText);
                    return;
                case "$HELMET":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_HELMET].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$HELMET>", sText);
                    return;
                case "$NECKLACE":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_NECKLACE].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$NECKLACE>", sText);
                    return;
                case "$RING_R":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_RINGR].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$RING_R>", sText);
                    return;
                case "$RING_L":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_RINGL].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$RING_L>", sText);
                    return;
                case "$ARMRING_R":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_ARMRINGR].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$ARMRING_R>", sText);
                    return;
                case "$ARMRING_L":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_ARMRINGL].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$ARMRING_L>", sText);
                    return;
                case "$BUJUK":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_BUJUK].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$BUJUK>", sText);
                    return;
                case "$BELT":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_BELT].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$BELT>", sText);
                    return;
                case "$BOOTS":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_BOOTS].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$BOOTS>", sText);
                    return;
                case "$CHARM":
                    sText = M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[Grobal2.U_CHARM].wIndex);
                    sMsg = ReplaceVariableText(sMsg, "<$CHARM>", sText);
                    return;
                case "$IPADDR":
                    sText = PlayObject.m_sIPaddr;
                    sMsg = ReplaceVariableText(sMsg, "<$IPADDR>", sText);
                    return;
                case "$IPLOCAL":
                    sText = PlayObject.m_sIPLocal;
                    sMsg = ReplaceVariableText(sMsg, "<$IPLOCAL>", sText);
                    return;
                case "$GUILDBUILDPOINT":
                    {
                        if (PlayObject.m_MyGuild == null)
                        {
                            sText = "无";
                        }
                        else
                        {
                            sText = PlayObject.m_MyGuild.BuildPoint.ToString();
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$GUILDBUILDPOINT>", sText);
                        return;
                    }
                case "$GUILDAURAEPOINT":
                    {
                        if (PlayObject.m_MyGuild == null)
                        {
                            sText = "无";
                        }
                        else
                        {
                            sText = PlayObject.m_MyGuild.Aurae.ToString();
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$GUILDAURAEPOINT>", sText);
                        return;
                    }
                case "$GUILDSTABILITYPOINT":
                    {
                        if (PlayObject.m_MyGuild == null)
                        {
                            sText = "无";
                        }
                        else
                        {
                            sText = PlayObject.m_MyGuild.Stability.ToString();
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$GUILDSTABILITYPOINT>", sText);
                        return;
                    }
                case "$GUILDFLOURISHPOINT":
                    {
                        if (PlayObject.m_MyGuild == null)
                        {
                            sText = "无";
                        }
                        else
                        {
                            sText = PlayObject.m_MyGuild.Flourishing.ToString();
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$GUILDFLOURISHPOINT>", sText);
                        return;
                    }
                case "$REQUESTCASTLEWARITEM":
                    sText = M2Share.g_Config.sZumaPiece;
                    sMsg = ReplaceVariableText(sMsg, "<$REQUESTCASTLEWARITEM>", sText);
                    return;
                case "$REQUESTCASTLEWARDAY":
                    sText = M2Share.g_Config.sZumaPiece;
                    sMsg = ReplaceVariableText(sMsg, "<$REQUESTCASTLEWARDAY>", sText);
                    return;
                case "$REQUESTBUILDGUILDITEM":
                    sText = M2Share.g_Config.sWomaHorn;
                    sMsg = ReplaceVariableText(sMsg, "<$REQUESTBUILDGUILDITEM>", sText);
                    return;
                case "$OWNERGUILD":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = this.m_Castle.m_sOwnGuild;
                            if (sText == "")
                            {
                                sText = "游戏管理";
                            }
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$OWNERGUILD>", sText);
                        return;
                    }
                case "$CASTLENAME":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = this.m_Castle.m_sName;
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$CASTLENAME>", sText);
                        return;
                    }
                case "$LORD":
                    {
                        if (this.m_Castle != null)
                        {
                            if (this.m_Castle.m_MasterGuild != null)
                            {
                                sText = this.m_Castle.m_MasterGuild.GetChiefName();
                            }
                            else
                            {
                                sText = "管理员";
                            }
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$LORD>", sText);
                        return;
                    }
                case "$GUILDWARFEE":
                    sMsg = ReplaceVariableText(sMsg, "<$GUILDWARFEE>", M2Share.g_Config.nGuildWarPrice.ToString());
                    return;
                case "$BUILDGUILDFEE":
                    sMsg = ReplaceVariableText(sMsg, "<$BUILDGUILDFEE>", M2Share.g_Config.nBuildGuildPrice.ToString());
                    return;
                case "$CASTLEWARDATE":
                    {
                        if (this.m_Castle == null)
                        {
                            this.m_Castle = M2Share.CastleManager.GetCastle(0);
                        }
                        if (this.m_Castle != null)
                        {
                            if (!this.m_Castle.m_boUnderWar)
                            {
                                sText = this.m_Castle.GetWarDate();
                                if (sText != "")
                                {
                                    sMsg = ReplaceVariableText(sMsg, "<$CASTLEWARDATE>", sText);
                                }
                                else
                                {
                                    sMsg = "暂时没有行会攻城 .\\ \\<返回/@main>";
                                }
                            }
                            else
                            {
                                sMsg = "正在攻城当中.\\ \\<返回/@main>";
                            }
                        }
                        else
                        {
                            sText = "????";
                        }
                        return;
                    }
                case "$LISTOFWAR":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = this.m_Castle.GetAttackWarList();
                        }
                        else
                        {
                            sText = "????";
                        }
                        if (sText != "")
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$LISTOFWAR>", sText);
                        }
                        else
                        {
                            sMsg = "暂时没有行会攻城 .\\ \\<返回/@main>";
                        }
                        return;
                    }
                case "$CASTLECHANGEDATE":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = this.m_Castle.m_ChangeDate.ToString();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$CASTLECHANGEDATE>", sText);
                        return;
                    }
                case "$CASTLEWARLASTDATE":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = this.m_Castle.m_WarDate.ToString();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$CASTLEWARLASTDATE>", sText);
                        return;
                    }
                case "$CASTLEGETDAYS":
                    {
                        if (this.m_Castle != null)
                        {
                            sText = HUtil32.GetDayCount(DateTime.Now, this.m_Castle.m_ChangeDate).ToString();
                        }
                        else
                        {
                            sText = "????";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$CASTLEGETDAYS>", sText);
                        return;
                    }
                case "$YEAR":// 服务器年
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$YEAR>", DateTime.Now.ToString("yyyy"));
                        return;
                    }
                case "$MONTH":// 服务器月
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$MONTH>", DateTime.Now.ToString("MM"));
                        return;
                    }
                case "$DAY":// 服务器日
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$DAY>", DateTime.Now.ToString("dd"));
                        return;
                    }
                case "$DATE":// 服务器日
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$DATE>", DateTime.Now.ToString("yyyy-MM-dd"));
                        return;
                    }
                case "$HOUR":// 服务器时
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$HOUR>", DateTime.Now.ToString("hh"));
                        return;
                    }
                case "$MINUTE":// 服务器分
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$MINUTE>", DateTime.Now.ToString("mm"));
                        return;
                    }
                case "$SECOND":// 服务器秒
                    {
                        sMsg = ReplaceVariableText(sMsg, "<$SECOND>", DateTime.Now.ToString("ss"));
                        return;
                    }
                case "$KILLMONNAME":// 上次杀怪的名称
                    {
                        //if ((PlayObject.m_Killmonname != ""))
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLMONNAME>", PlayObject.m_KILLMONNAME);
                        //}
                        //else
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLMONNAME>", "");
                        //}
                        return;
                    }
                case "$KILLPLAYERNAME":// 上次杀人的名称
                    {
                        //if ((PlayObject.m_KillPlayName != ""))
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLPLAYERNAME>", PlayObject.m_KILLPLAYNAME);
                        //}
                        //else
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLPLAYERNAME>", "");
                        //}
                        return;
                    }
                case "$KILLPLAYERMAP":// 上次杀人的地图名称
                    {
                        //if ((PlayObject.m_KillPlayMAP != ""))
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLPLAYERMAP>", PlayObject.m_KILLPLAYMAP);
                        //}
                        //else
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLPLAYERMAP>", "");
                        //}
                        return;
                    }
                case "$KILLMONMAP":// 上次杀怪的地图名称
                    {
                        //if (PlayObject.m_Killmonmap != "")
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLMONMAP>", PlayObject.m_KILLMONMAP);
                        //}
                        //else
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$KILLMONMAP>", "");
                        //}
                        return;
                    }
                case "$MOVEMONMAP":// 上次去过的地图名称
                    {
                        //if (PlayObject.m_MOVEMONMAP != "")
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$MOVEMONMAP>", PlayObject.m_MOVEMONMAP);
                        //}
                        //else
                        //{
                        //    sMsg = ReplaceVariableText(sMsg, "<$MOVEMONMAP>", "");
                        //}
                        return;
                    }
                case "$BSNAME":// 上次攻击我的名称
                    {
                        if ((PlayObject.m_LastHiter != null))
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$BSNAME>", PlayObject.m_LastHiter.m_sCharName);
                        }
                        else
                        {
                            sMsg = ReplaceVariableText(sMsg, "<$BSNAME>", "");
                        }
                        return;
                    }
                case "$TARGETNAME":// 正在攻击的人物或怪物名称
                    {
                        if (PlayObject.m_TargetCret != null)
                        {
                            sText = PlayObject.m_TargetCret.m_sCharName;
                        }
                        else
                        {
                            sText = "";
                        }
                        sMsg = ReplaceVariableText(sMsg, "<$TARGETNAME>", sText);
                        return;
                    }
                case "$CMD_DATE":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_DATE>", M2Share.g_GameCommand.DATA.sCmd);
                    return;
                case "$CMD_ALLOWMSG":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_ALLOWMSG>", M2Share.g_GameCommand.ALLOWMSG.sCmd);
                    return;
                case "$CMD_LETSHOUT":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_LETSHOUT>", M2Share.g_GameCommand.LETSHOUT.sCmd);
                    return;
                case "$CMD_LETTRADE":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_LETTRADE>", M2Share.g_GameCommand.LETTRADE.sCmd);
                    return;
                case "$CMD_LETGUILD":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_LETGUILD>", M2Share.g_GameCommand.LETGUILD.sCmd);
                    return;
                case "$CMD_ENDGUILD":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_ENDGUILD>", M2Share.g_GameCommand.ENDGUILD.sCmd);
                    return;
                case "$CMD_BANGUILDCHAT":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_BANGUILDCHAT>", M2Share.g_GameCommand.BANGUILDCHAT.sCmd);
                    return;
                case "$CMD_AUTHALLY":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_AUTHALLY>", M2Share.g_GameCommand.AUTHALLY.sCmd);
                    return;
                case "$CMD_AUTH":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_AUTH>", M2Share.g_GameCommand.AUTH.sCmd);
                    return;
                case "$CMD_AUTHCANCEL":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_AUTHCANCEL>", M2Share.g_GameCommand.AUTHCANCEL.sCmd);
                    return;
                case "$CMD_USERMOVE":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_USERMOVE>", M2Share.g_GameCommand.USERMOVE.sCmd);
                    return;
                case "$CMD_SEARCHING":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_SEARCHING>", M2Share.g_GameCommand.SEARCHING.sCmd);
                    return;
                case "$CMD_ALLOWGROUPCALL":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_ALLOWGROUPCALL>", M2Share.g_GameCommand.ALLOWGROUPCALL.sCmd);
                    return;
                case "$CMD_GROUPRECALLL":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_GROUPRECALLL>", M2Share.g_GameCommand.GROUPRECALLL.sCmd);
                    return;
                case "$CMD_ATTACKMODE":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_ATTACKMODE>", M2Share.g_GameCommand.ATTACKMODE.sCmd);
                    return;
                case "$CMD_REST":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_REST>", M2Share.g_GameCommand.REST.sCmd);
                    return;
                case "$CMD_STORAGESETPASSWORD":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_STORAGESETPASSWORD>", M2Share.g_GameCommand.SETPASSWORD.sCmd);
                    return;
                case "$CMD_STORAGECHGPASSWORD":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_STORAGECHGPASSWORD>", M2Share.g_GameCommand.CHGPASSWORD.sCmd);
                    return;
                case "$CMD_STORAGELOCK":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_STORAGELOCK>", M2Share.g_GameCommand.__LOCK.sCmd);
                    return;
                case "$CMD_STORAGEUNLOCK":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_STORAGEUNLOCK>", M2Share.g_GameCommand.UNLOCKSTORAGE.sCmd);
                    return;
                case "$CMD_UNLOCK":
                    sMsg = ReplaceVariableText(sMsg, "<$CMD_UNLOCK>", M2Share.g_GameCommand.UNLOCK.sCmd);
                    return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MAPMONSTERCOUNT[", "$MAPMONSTERCOUNT[".Length)) // 地图怪物数量
            {
                MonGenInfo MonGen = null;
                TBaseObject BaseObject = null;
                int MonGenCount = 0;
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                var MapName = HUtil32.GetValidStr3(s14, ref s14, new string[] { "/" });
                var MonsterName = s14;
                if (MapName.StartsWith("$")) // $MAPMOSTERCOUNT[怪物名字/地图号]
                {
                    MapName = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + MapName + ">"); // 替换变量
                }
                if (MonsterName.StartsWith("$"))
                {
                    MonsterName = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + MonsterName + ">"); // 替换变量
                }
                if (string.Compare(MapName, "ALL", StringComparison.OrdinalIgnoreCase) == 0)// 如果是全部地图
                {
                    if (string.Compare(MonsterName, "ALL", StringComparison.OrdinalIgnoreCase) == 0)// 如果是全部名字的怪物
                    {
                        for (var i = 0; i < M2Share.UserEngine.m_MonGenList.Count; i++)
                        {
                            MonGen = M2Share.UserEngine.m_MonGenList[i];
                            if (MonGen == null)
                            {
                                continue;
                            }
                            for (var j = 0; j < MonGen.CertList.Count; j++)
                            {
                                BaseObject = MonGen.CertList[j];
                                if ((BaseObject.m_Master == null) && (!BaseObject.m_boDeath) && (!BaseObject.m_boGhost))
                                {
                                    MonGenCount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < M2Share.UserEngine.m_MonGenList.Count; i++)
                        {
                            MonGen = M2Share.UserEngine.m_MonGenList[i];
                            if (MonGen == null)
                            {
                                continue;
                            }
                            for (var j = 0; j < MonGen.CertList.Count; j++)
                            {
                                BaseObject = MonGen.CertList[j];
                                if ((BaseObject.m_Master == null) && (string.Compare(BaseObject.m_sCharName, MonsterName, StringComparison.OrdinalIgnoreCase) == 0) && (!BaseObject.m_boDeath) && (!BaseObject.m_boGhost))
                                {
                                    MonGenCount++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 如果不是全部地图
                    var Envir = M2Share.MapManager.FindMap(MapName);
                    if (Envir != null)
                    {
                        if (string.Compare(MonsterName, "ALL", StringComparison.CurrentCulture) == 0)// 如果是全部名字的怪物
                        {
                            for (var i = 0; i < M2Share.UserEngine.m_MonGenList.Count; i++)
                            {
                                MonGen = M2Share.UserEngine.m_MonGenList[i];
                                if (MonGen == null)
                                {
                                    continue;
                                }
                                for (var j = 0; j < MonGen.CertList.Count; j++)
                                {
                                    BaseObject = MonGen.CertList[j];
                                    if ((BaseObject.m_Master == null) && (BaseObject.m_PEnvir == Envir) && (!BaseObject.m_boDeath) && (!BaseObject.m_boGhost))
                                    {
                                        MonGenCount++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i < M2Share.UserEngine.m_MonGenList.Count; i++)
                            {
                                MonGen = M2Share.UserEngine.m_MonGenList[i];
                                if (MonGen == null)
                                {
                                    continue;
                                }
                                for (var j = 0; j < MonGen.CertList.Count; j++)
                                {
                                    BaseObject = MonGen.CertList[j];
                                    if ((BaseObject.m_Master == null) && (BaseObject.m_PEnvir == Envir) && (string.Compare(BaseObject.m_sCharName, MonsterName, StringComparison.OrdinalIgnoreCase) == 0) && (!BaseObject.m_boDeath) && (!BaseObject.m_boGhost))
                                    {
                                        MonGenCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", MonGenCount.ToString());
            }
            //if (sVariable == "$BUTCHITEMNAME")// 显示挖到的物品名称
            //{
            //    if (PlayObject.m_sButchItem == "")
            //    {
            //        sMsg = "????";
            //    }
            //    else
            //    {
            //        sMsg = ReplaceVariableText(sMsg, "<$BUTCHITEMNAME>", PlayObject.m_sButchItem);
            //    }
            //    return;
            //}
            // -------------------------------------------------------------------------------
            if (HUtil32.CompareLStr(sVariable, "$MONKILLER[", "$MONKILLER[".Length))// $MONKILLER(怪物名称 + 地图号) 显示杀死此怪物的杀手
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">");// 替换变量
                //}
                //sText = MonDie.ReadString("杀怪人名称", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIEHOUR[", "$MONDIEHOUR[".Length)) // $MONDIEHOUR(怪物名称 + 地图号) 显示该怪物死时的小时
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">");// 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-时", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIEMIN[", "$MONDIEMIN[".Length))// $MONDIEMIN(怪物名称 + 地图号) 显示该怪物死时的分钟
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">"); // 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-分", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIESEC[", "$MONDIESEC[".Length))// $MONDIESEC(怪物名称 + 地图号) 显示该怪物死时的秒数
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">"); // 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-秒", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIEYEAR[", "$MONDIEYEAR[".Length))// $MONDIEYEAR[怪物名称 + 地图号]   显示该怪物死亡的年
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">");// 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-年", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIEMONTH[", "$MONDIEMONTH[".Length))// $MONDIEMONTH[怪物名称 + 地图号]  显示该怪物死亡的月
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">");// 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-月", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$MONDIEDAY[", "$MONDIEDAY[".Length))// $MONDIEDAY[怪物名称 + 地图号]    显示该怪物死亡的日
            {
                HUtil32.ArrestStringEx(sVariable, "[", "]", ref s14);
                //MonDie = new FileStream(M2Share.g_Config.sEnvirDir + "MonDieDataList.txt");
                //if (MonDie == null)
                //{
                //    return;
                //}
                //if ((s14[0]).CompareTo(("$")) == 0)
                //{
                //    s14 = M2Share.g_ManageNPC.GetLineVariableText(PlayObject, "<" + s14 + ">");// 替换变量
                //}
                //sText = MonDie.ReadString("怪物死亡-日", s14, "错误");
                //sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", sText);
                //MonDie.Free;
                return;
            }
            // 个人信息
            if (HUtil32.CompareLStr(sVariable, "$USEITEMMAKEINDEX(", "$USEITEMAKEINDEX(".Length))// 显示n位置的装备ID
            {
                HUtil32.ArrestStringEx(sVariable, "(", ")", ref s14);
                var n18 = HUtil32.Str_ToInt(s14, -1);
                if ((n18 >= 0 && n18 <= 15) && PlayObject.m_UseItems[n18] != null && (PlayObject.m_UseItems[n18].wIndex > 0))
                {
                    sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_UseItems[n18].MakeIndex).ToString());
                }
                else
                {
                    sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", "0");
                }
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$USEITEMNAME(", "$USEITEMNAME(".Length))// 显示n位置的装备名称
            {
                HUtil32.ArrestStringEx(sVariable, "(", ")", ref s14);
                var n18 = HUtil32.Str_ToInt(s14, -1);
                if ((n18 >= 0 && n18 <= 15) && PlayObject.m_UseItems[n18] != null && (PlayObject.m_UseItems[n18].wIndex > 0))
                {
                    sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", M2Share.UserEngine.GetStdItemName(PlayObject.m_UseItems[n18].wIndex));
                }
                else
                {
                    sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", "无");
                }
                return;
            }
            if (sVariable == "$ATTACKMODE")// 显示用户当前的攻击模式
            {
                switch (PlayObject.m_btAttatckMode)
                {
                    case M2Share.HAM_ALL: // [攻击模式: 全体攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "0");
                        break;
                    case M2Share.HAM_PEACE: // [攻击模式: 和平攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "1");
                        break;
                    case M2Share.HAM_DEAR:// [攻击模式: 夫妻攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "2");
                        break;
                    case M2Share.HAM_MASTER:// [攻击模式: 师徒攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "3");
                        break;
                    case M2Share.HAM_GROUP: // [攻击模式: 编组攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "4");
                        break;
                    case M2Share.HAM_GUILD: // [攻击模式: 行会攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "5");
                        break;
                    case M2Share.HAM_PKATTACK: // [攻击模式: 红名攻击]
                        sMsg = ReplaceVariableText(sMsg, "<$ATTACKMODE>", "6");
                        break;
                }
            }
            if (sVariable == "$CLIENTVERSION")//显示当前用户客户端版本
            {
                sMsg = ReplaceVariableText(sMsg, "<$CLIENTVERSION>", (PlayObject.m_nSoftVersionDate).ToString());
                return;
            }
            if (sVariable == "$QUERYYBDEALLOG") // 查看元宝交易记录 
            {
                sMsg = ReplaceVariableText(sMsg, "<$QUERYYBDEALLOG>", PlayObject.SelectSellDate());
                return;
            }
            if (sVariable == "$GUILDNAME")
            {
                if (PlayObject.m_MyGuild != null)
                {
                    sMsg = ReplaceVariableText(sMsg, "<$GUILDNAME>", PlayObject.m_MyGuild.sGuildName);
                }
                else
                {
                    sMsg = "无";
                }
                return;
            }
            if (sVariable == "$GOLDCOUNT")// 包裹金币数
            {
                sText = (PlayObject.m_nGold).ToString();
                sMsg = ReplaceVariableText(sMsg, "<$GOLDCOUNT>", sText);
                return;
            }
            if (sVariable == "$GOLDCOUNTX")// 包裹最多可携带金币数
            {
                sText = (PlayObject.m_nGoldMax).ToString();
                sMsg = ReplaceVariableText(sMsg, "<$GOLDCOUNTX>", sText);
                return;
            }
            if (sVariable == "$LUCKY")// 幸运  增加人物暴击
            {
                sText = (PlayObject.m_nLuck).ToString();
                sMsg = ReplaceVariableText(sMsg, "<$LUCKY>", sText);
                return;
            }
            if (sVariable == "$GAMEGOLD")// 元宝
            {
                sText = (PlayObject.m_nGameGold).ToString();
                sMsg = ReplaceVariableText(sMsg, "<$GAMEGOLD>", sText);
                return;
            }
            if (sVariable == "$REQUESTCASTLEWARITEM") // 祖玛头像
            {
                sText = M2Share.g_Config.sZumaPiece;
                sMsg = ReplaceVariableText(sMsg, "<$REQUESTCASTLEWARITEM>", sText);
                return;
            }
            if (sVariable == "$REQUESTCASTLEWARDAY")// 几天后开始攻城
            {
                sText = (M2Share.g_Config.nStartCastleWarDays).ToString();
                sMsg = ReplaceVariableText(sMsg, "<$REQUESTCASTLEWARDAY>", sText);
                return;
            }
            if (sVariable == "$REQUESTBUILDGUILDITEM")// 沃玛号角
            {
                sText = M2Share.g_Config.sWomaHorn;
                sMsg = ReplaceVariableText(sMsg, "<$REQUESTBUILDGUILDITEM>", sText);
                return;
            }
            if (sVariable == "$GUILDWARFEE") // 行会战金币数
            {
                sMsg = ReplaceVariableText(sMsg, "<$GUILDWARFEE>", (M2Share.g_Config.nGuildWarPrice).ToString());
                return;
            }
            if (sVariable == "$BUILDGUILDFEE")// 建立行会所需的金币数
            {
                sMsg = ReplaceVariableText(sMsg, "<$BUILDGUILDFEE>", (M2Share.g_Config.nBuildGuildPrice).ToString());
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$HUMAN(", "$HUMAN(".Length))
            {
                HUtil32.ArrestStringEx(sVariable, '(', ')', ref s14);
                boFoundVar = false;
                if (PlayObject.m_DynamicVarList.TryGetValue(s14, out DynamicVar))
                {
                    switch (DynamicVar.VarType)
                    {
                        case VarType.Integer:
                            sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.nInternet.ToString());
                            boFoundVar = true;
                            break;
                        case VarType.String:
                            sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.sString);
                            boFoundVar = true;
                            break;
                    }
                }
                if (!boFoundVar)
                {
                    sMsg = "??";
                }
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$GUILD(", "$GUILD(".Length))
            {
                if (PlayObject.m_MyGuild == null)
                {
                    return;
                }
                HUtil32.ArrestStringEx(sVariable, '(', ')', ref s14);
                boFoundVar = false;
                if (PlayObject.m_MyGuild.m_DynamicVarList.TryGetValue(s14, out DynamicVar))
                {
                    if (String.Compare(DynamicVar.sName, s14, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch (DynamicVar.VarType)
                        {
                            case VarType.Integer:
                                sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.nInternet.ToString());
                                boFoundVar = true;
                                break;
                            case VarType.String:
                                sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.sString);
                                boFoundVar = true;
                                break;
                        }
                    }
                }
                if (!boFoundVar)
                {
                    sMsg = "??";
                }
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$GLOBAL(", "$GLOBAL(".Length))
            {
                HUtil32.ArrestStringEx(sVariable, '(', ')', ref s14);
                boFoundVar = false;
                if (M2Share.g_DynamicVarList.TryGetValue(s14, out DynamicVar))
                {
                    switch (DynamicVar.VarType)
                    {
                        case VarType.Integer:
                            sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.nInternet.ToString());
                            boFoundVar = true;
                            break;
                        case VarType.String:
                            sMsg = ReplaceVariableText(sMsg, '<' + sVariable + '>', DynamicVar.sString);
                            boFoundVar = true;
                            break;
                    }
                }
                if (!boFoundVar)
                {
                    sMsg = "??";
                }
                return;
            }
            if (HUtil32.CompareLStr(sVariable, "$STR(", "$STR(".Length))
            {
                HUtil32.ArrestStringEx(sVariable, '(', ')', ref s14);
                var nVarValue = M2Share.GetValNameNo(s14);
                if (nVarValue >= 0)
                {
                    if (HUtil32.RangeInDefined(nVarValue, 0, 99))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_nVal[nVarValue]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 100, 199))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (M2Share.g_Config.GlobalVal[nVarValue - 100]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 200, 299))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_DyVal[nVarValue - 200]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 300, 399))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_nMval[nVarValue - 300]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 400, 499))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (M2Share.g_Config.GlobaDyMval[nVarValue - 400]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 500, 599))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_nInteger[nVarValue - 500]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 600, 699))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", PlayObject.m_sString[nVarValue - 600]);
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 700, 799))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", M2Share.g_Config.GlobalAVal[nVarValue - 700]);
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 800, 1199))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (M2Share.g_Config.GlobalVal[nVarValue - 700]).ToString());
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 1200, 1599))
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", M2Share.g_Config.GlobalAVal[nVarValue - 1100]);
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 1600, 1699)) //个人服务器字符串变量E
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", PlayObject.m_ServerStrVal[nVarValue - 1600]);
                    }
                    else if (HUtil32.RangeInDefined(nVarValue, 1700, 1799)) //个人服务器字符串变量W
                    {
                        sMsg = ReplaceVariableText(sMsg, "<" + sVariable + ">", (PlayObject.m_ServerIntVal[nVarValue - 1700]).ToString());
                    }
                }
            }
        }

        public void LoadNPCScript()
        {
            if (m_boIsQuest)
            {
                m_sPath = M2Share.sNpc_def;
                var s08 = this.m_sCharName + '-' + this.m_sMapName;
                M2Share.ScriptSystem.LoadNpcScript(this, m_sFilePath, s08);
            }
            else
            {
                m_sPath = m_sFilePath;
                M2Share.ScriptSystem.LoadNpcScript(this, m_sFilePath, this.m_sCharName);
            }
        }

        public override bool Operate(TProcessMessage ProcessMsg)
        {
            return base.Operate(ProcessMsg);
        }

        public override void Run()
        {
            if (m_Master != null)// 不允许召唤为宝宝
            {
                this.m_Master = null;
            }
            base.Run();
        }

        private void ScriptActionError(TPlayObject PlayObject, string sErrMsg, TQuestActionInfo QuestActionInfo, string sCmd)
        {
            const string sOutMessage = "[脚本错误] {0} 脚本命令:{1} NPC名称:{2} 地图:{3}({4}:{5}) 参数1:{6} 参数2:{7} 参数3:{8} 参数4:{9} 参数5:{10} 参数6:{11}";
            string sMsg = format(sOutMessage, sErrMsg, sCmd, this.m_sCharName, this.m_sMapName, this.m_nCurrX, this.m_nCurrY, QuestActionInfo.sParam1, QuestActionInfo.sParam2, QuestActionInfo.sParam3, QuestActionInfo.sParam4, QuestActionInfo.sParam5, QuestActionInfo.sParam6);
            M2Share.MainOutMessage(sMsg);
        }

        private void ScriptConditionError(TPlayObject PlayObject, TQuestConditionInfo QuestConditionInfo, string sCmd)
        {
            string sMsg = "Cmd:" + sCmd + " NPC名称:" + this.m_sCharName + " 地图:" + this.m_sMapName + " 座标:" + this.m_nCurrX + ':' + this.m_nCurrY + " 参数1:" + QuestConditionInfo.sParam1 + " 参数2:" + QuestConditionInfo.sParam2 + " 参数3:" + QuestConditionInfo.sParam3 + " 参数4:" + QuestConditionInfo.sParam4 + " 参数5:" + QuestConditionInfo.sParam5;
            M2Share.MainOutMessage("[脚本参数不正确] " + sMsg);
        }

        protected void SendMsgToUser(TPlayObject PlayObject, string sMsg)
        {
            PlayObject.SendMsg(this, Grobal2.RM_MERCHANTSAY, 0, 0, 0, 0, this.m_sCharName + '/' + sMsg);
        }

        protected string ReplaceVariableText(string sMsg, string sStr, string sText)
        {
            string result;
            var n10 = sMsg.IndexOf(sStr, StringComparison.OrdinalIgnoreCase);
            if (n10 > -1)
            {
                var s14 = sMsg.Substring(0, n10);
                var s18 = sMsg.Substring(sStr.Length + n10, sMsg.Length - (sStr.Length + n10));
                result = s14 + sText + s18;
            }
            else
            {
                result = sMsg;
            }
            return result;
        }

        public virtual void UserSelect(TPlayObject PlayObject, string sData)
        {
            string sLabel = string.Empty;
            PlayObject.m_nScriptGotoCount = 0;
            if ((!string.IsNullOrEmpty(sData)) && (sData[0] == '@'))// 处理脚本命令 @back 返回上级标签内容
            {
                HUtil32.GetValidStr3(sData, ref sLabel, new[] { '\r' });
                if (PlayObject.m_sScriptCurrLable != sLabel)
                {
                    if (sLabel != M2Share.sBACK)
                    {
                        PlayObject.m_sScriptGoBackLable = PlayObject.m_sScriptCurrLable;
                        PlayObject.m_sScriptCurrLable = sLabel;
                    }
                    else
                    {
                        if (PlayObject.m_sScriptCurrLable != "")
                        {
                            PlayObject.m_sScriptCurrLable = "";
                        }
                        else
                        {
                            PlayObject.m_sScriptGoBackLable = "";
                        }
                    }
                }
            }
        }

        protected virtual void SendCustemMsg(TPlayObject PlayObject, string sMsg)
        {
            if (!M2Share.g_Config.boSendCustemMsg)
            {
                PlayObject.SysMsg(M2Share.g_sSendCustMsgCanNotUseNowMsg, MsgColor.Red, MsgType.Hint);
                return;
            }
            if (PlayObject.m_boSendMsgFlag)
            {
                PlayObject.m_boSendMsgFlag = false;
                M2Share.UserEngine.SendBroadCastMsg(PlayObject.m_sCharName + ": " + sMsg, MsgType.Cust);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            this.m_Castle = M2Share.CastleManager.InCastleWarArea(this);
        }

        private Dictionary<string, TDynamicVar> GetDynamicVarList(TPlayObject PlayObject, string sType, ref string sName)
        {
            Dictionary<string, TDynamicVar> result = null;
            if (HUtil32.CompareLStr(sType, "HUMAN", 5))
            {
                result = PlayObject.m_DynamicVarList;
                sName = PlayObject.m_sCharName;
            }
            else if (HUtil32.CompareLStr(sType, "GUILD", 5))
            {
                if (PlayObject.m_MyGuild == null)
                {
                    return result;
                }
                result = PlayObject.m_MyGuild.m_DynamicVarList;
                sName = PlayObject.m_MyGuild.sGuildName;
            }
            else if (HUtil32.CompareLStr(sType, "GLOBAL", 6))
            {
                result = M2Share.g_DynamicVarList;
                sName = "GLOBAL";
            }
            else if (HUtil32.CompareLStr(sType, "Account", 7))
            {
                result = PlayObject.m_DynamicVarList;
                sName = PlayObject.m_sUserID;
            }
            return result;
        }

        private bool GetValValue(TPlayObject PlayObject, string sMsg, ref int nValue)
        {
            bool result = false;
            if (sMsg == "")
            {
                return result;
            }
            int n01 = M2Share.GetValNameNo(sMsg);
            if (n01 >= 0)
            {
                if (HUtil32.RangeInDefined(n01, 0, 99))
                {
                    nValue = PlayObject.m_nVal[n01];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 100, 199))
                {
                    nValue = M2Share.g_Config.GlobalVal[n01 - 100];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 200, 299))
                {
                    nValue = PlayObject.m_DyVal[n01 - 200];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 300, 399))
                {
                    nValue = PlayObject.m_nMval[n01 - 300];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 400, 499))
                {
                    nValue = M2Share.g_Config.GlobaDyMval[n01 - 400];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 500, 599))
                {
                    nValue = PlayObject.m_nInteger[n01 - 500];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 600, 699))
                {
                    nValue = HUtil32.Str_ToInt(PlayObject.m_sString[n01 - 600], 0);
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 700, 799))
                {
                    nValue = HUtil32.Str_ToInt(M2Share.g_Config.GlobalAVal[n01 - 700], 0);
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 800, 1199))
                {
                    nValue = M2Share.g_Config.GlobalVal[n01 - 700];
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 1200, 1599))
                {
                    nValue = HUtil32.Str_ToInt(M2Share.g_Config.GlobalAVal[n01 - 1100], 0);
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 1600, 1699))
                {
                    nValue = HUtil32.Str_ToInt(PlayObject.m_ServerStrVal[n01 - 1600], 0);
                    result = true;
                }
                else if (HUtil32.RangeInDefined(n01, 1700, 1799))
                {
                    nValue = PlayObject.m_ServerIntVal[n01 - 1700];
                    result = true;
                }
            }
            return result;
        }

        private bool GetValValue(TPlayObject PlayObject, string sMsg, ref string sValue)
        {
            bool result = false;
            int n01;
            try
            {
                if (sMsg == "")
                {
                    return result;
                }
                n01 = M2Share.GetValNameNo(sMsg);
                if (n01 >= 0)
                {
                    if (HUtil32.RangeInDefined(n01, 600, 699))
                    {
                        sValue = PlayObject.m_sString[n01 - 600];
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 700, 799))
                    {
                        sValue = M2Share.g_Config.GlobalAVal[n01 - 700];
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 1200, 1599))
                    {
                        sValue = M2Share.g_Config.GlobalAVal[n01 - 1100];// A变量(100-499)
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 1600, 1699))
                    {
                        sValue = PlayObject.m_ServerStrVal[n01 - 1600];
                        result = true;
                    }
                }
            }
            catch
            {
                M2Share.MainOutMessage("{异常} TNormNpc.GetValValue2");
            }
            return result;
        }

        private bool SetValValue(TPlayObject PlayObject, string sMsg, int nValue)
        {
            bool result = false;
            int n01;
            try
            {
                if (sMsg == "")
                {
                    return result;
                }
                n01 = M2Share.GetValNameNo(sMsg);
                if (n01 >= 0)
                {
                    if (HUtil32.RangeInDefined(n01, 0, 99))
                    {
                        PlayObject.m_nVal[n01] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 100, 199))
                    {
                        M2Share.g_Config.GlobalVal[n01 - 100] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 200, 299))
                    {
                        PlayObject.m_DyVal[n01 - 200] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 300, 399))
                    {
                        PlayObject.m_nMval[n01 - 300] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 400, 499))
                    {
                        M2Share.g_Config.GlobaDyMval[n01 - 400] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 500, 599))
                    {
                        PlayObject.m_nInteger[n01 - 500] = nValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 800, 1199))
                    {
                        M2Share.g_Config.GlobalVal[n01 - 700] = nValue;//G变量
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 1700, 1799))
                    {
                        PlayObject.m_ServerIntVal[n01 - 1700] = nValue;
                        result = true;
                    }
                }
            }
            catch
            {
                M2Share.MainOutMessage("{异常} TNormNpc.SetValValue1");
            }
            return result;
        }

        private bool SetValValue(TPlayObject PlayObject, string sMsg, string sValue)
        {
            bool result = false;
            int n01;
            try
            {
                if (sMsg == "")
                {
                    return result;
                }
                n01 = M2Share.GetValNameNo(sMsg);
                if (n01 >= 0)
                {
                    if (HUtil32.RangeInDefined(n01, 600, 699))
                    {
                        PlayObject.m_sString[n01 - 600] = sValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 700, 799))
                    {
                        M2Share.g_Config.GlobalAVal[n01 - 700] = sValue;
                        result = true;
                    }
                    else if (HUtil32.RangeInDefined(n01, 1200, 1599))
                    {
                        M2Share.g_Config.GlobalAVal[n01 - 1100] = sValue;// A变量(100-499)
                        result = true;
                    }
                }
            }
            catch
            {
                M2Share.MainOutMessage("{异常} TNormNpc.SetValValue2");
            }
            return result;
        }
    }
}