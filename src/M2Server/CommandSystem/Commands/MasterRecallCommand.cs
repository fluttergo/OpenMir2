﻿using SystemModule;
using M2Server.CommandSystem;

namespace M2Server
{
    /// <summary>
    /// 师徒传送，师父可以将徒弟传送到自己身边，徒弟必须允许传送。
    /// </summary>
    [GameCommand("MasterRecall", "师徒传送，师父可以将徒弟传送到自己身边，徒弟必须允许传送。", 10)]
    public class MasterRecallCommand : BaseCommond
    {
        [DefaultCommand]
        public void MasterRecall(string[] @Params, TPlayObject PlayObject)
        {
            var sParam = @Params.Length > 0 ? @Params[0] : "";
            TPlayObject MasterHuman;
            if (sParam != "" && sParam[0] == '?')
            {
                PlayObject.SysMsg("命令格式: @" + this.Attributes.Name + " (师徒传送，师父可以将徒弟传送到自己身边，徒弟必须允许传送。)", TMsgColor.c_Green, TMsgType.t_Hint);
                return;
            }
            if (!PlayObject.m_boMaster)
            {
                PlayObject.SysMsg("只能师父才能使用此功能！！！", TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            if (PlayObject.m_MasterList.Count == 0)
            {
                PlayObject.SysMsg("你的徒弟一个都不在线！！！", TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            //if (PlayObject.m_PEnvir.m_boNOMASTERRECALL)
            //{
            //    PlayObject.SysMsg("本地图禁止师徒传送！！！", TMsgColor.c_Red, TMsgType.t_Hint);
            //    return;
            //}
            if (HUtil32.GetTickCount() - PlayObject.m_dwMasterRecallTick < 10000)
            {
                PlayObject.SysMsg("稍等伙才能再次使用此功能！！！", TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            for (var i = 0; i < PlayObject.m_MasterList.Count; i++)
            {
                MasterHuman = PlayObject.m_MasterList[i] as TPlayObject;
                if (MasterHuman.m_boCanMasterRecall)
                {
                    PlayObject.RecallHuman(MasterHuman.m_sCharName);
                }
                else
                {
                    PlayObject.SysMsg(MasterHuman.m_sCharName + " 不允许传送！！！", TMsgColor.c_Red, TMsgType.t_Hint);
                }
            }
        }
    }
}