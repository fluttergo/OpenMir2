﻿using SystemModule;
using M2Server.CommandSystem;

namespace M2Server
{
    [GameCommand("FireBurn", "", 10)]
    public class FireBurnCommand : BaseCommond
    {
        [DefaultCommand]
        public void FireBurn(string[] @Params, TPlayObject PlayObject)
        {
            if (@Params == null)
            {
                return;
            }
            var nInt = @Params.Length > 0 ? int.Parse(@Params[0]) : 0;
            var nTime = @Params.Length > 1 ? int.Parse(@Params[1]) : 0;
            var nN = @Params.Length > 2 ? int.Parse(@Params[2]) : 0;
            if (PlayObject.m_btPermission < 6)
            {
                return;
            }
            if (nInt == 0 || nTime == 0 || nN == 0)
            {
                PlayObject.SysMsg("命令格式: @" + M2Share.g_GameCommand.FIREBURN.sCmd + " nInt nTime nN", TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            var FireBurnEvent = new TFireBurnEvent(PlayObject, PlayObject.m_nCurrX, PlayObject.m_nCurrY, nInt, nTime, nN);
            M2Share.EventManager.AddEvent(FireBurnEvent);
        }
    }
}