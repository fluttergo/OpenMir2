﻿using SystemModule;
using GameSvr.CommandSystem;

namespace GameSvr.Command
{
    /// <summary>
    /// 调整指定物品名称
    /// </summary>
    [GameCommand("ChangeItemName", "调整指定物品名称", 10, Help = "物品编号 物品ID号 物品名称")]
    public class ChangeItemNameCommand : BaseCommond
    {
        [DefaultCommand]
        public void ChangeItemName(string[] @params, TPlayObject PlayObject)
        {
            if (@params == null)
            {
                return;
            }
            int nMakeIndex;
            int nItemIndex;
            var sMakeIndex = @params.Length > 0 ? @params[0] : "";
            var sItemIndex = @params.Length > 1 ? @params[1] : "";
            var sItemName = @params.Length > 2 ? @params[2] : "";
            if (sMakeIndex == "" || sItemIndex == "" || string.IsNullOrEmpty(sItemName))
            {
                PlayObject.SysMsg(CommandAttribute.CommandHelp(), TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            nMakeIndex = HUtil32.Str_ToInt(sMakeIndex, -1);
            nItemIndex = HUtil32.Str_ToInt(sItemIndex, -1);
            if (nMakeIndex <= 0 || nItemIndex < 0)
            {
                PlayObject.SysMsg(CommandAttribute.CommandHelp(), TMsgColor.c_Red, TMsgType.t_Hint);
                return;
            }
            if (M2Share.ItemUnit.AddCustomItemName(nMakeIndex, nItemIndex, sItemName))
            {
                M2Share.ItemUnit.SaveCustomItemName();
                PlayObject.SysMsg("物品名称设置成功。", TMsgColor.c_Green, TMsgType.t_Hint);
                return;
            }
            PlayObject.SysMsg("此物品，已经设置了其它的名称!!!", TMsgColor.c_Red, TMsgType.t_Hint);
        }
    }
}