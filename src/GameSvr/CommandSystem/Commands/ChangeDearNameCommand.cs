﻿using SystemModule;
using GameSvr.CommandSystem;

namespace GameSvr
{
    /// <summary>
    /// 调整指定玩家配偶名称
    /// </summary>
    [GameCommand("ChangeDearName", "调整指定玩家配偶名称", help: "人物名称 配偶名称(如果为 无 则清除)", 10)]
    public class ChangeDearNameCommand : BaseCommond
    {
        [DefaultCommand]
        public void ChangeDearName(string[] @Params, TPlayObject PlayObject)
        {
            var sHumanName = @Params.Length > 0 ? @Params[0] : "";
            var sDearName = @Params.Length > 1 ? @Params[1] : "";
            if (string.IsNullOrEmpty(sHumanName) || sDearName == "")
            {
                PlayObject.SysMsg(CommandAttribute.CommandHelp(), TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            var m_PlayObject = M2Share.UserEngine.GetPlayObject(sHumanName);
            if (m_PlayObject != null)
            {
                if (sDearName.ToLower().CompareTo("无".ToLower()) == 0)
                {
                    m_PlayObject.m_sDearName = "";
                    m_PlayObject.RefShowName();
                    PlayObject.SysMsg(sHumanName + " 的配偶名清除成功。", TMsgColor.c_Green, TMsgType.t_Hint);
                }
                else
                {
                    m_PlayObject.m_sDearName = sDearName;
                    m_PlayObject.RefShowName();
                    PlayObject.SysMsg(sHumanName + " 的配偶名更改成功。", TMsgColor.c_Green, TMsgType.t_Hint);
                }
            }
            else
            {
                PlayObject.SysMsg(string.Format(M2Share.g_sNowNotOnLineOrOnOtherServer, sHumanName), TMsgColor.c_Red, TMsgType.t_Hint);
            }
        }
    }
}