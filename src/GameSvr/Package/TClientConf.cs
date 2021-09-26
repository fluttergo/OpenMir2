﻿using System.IO;
using SystemModule;

namespace GameSvr
{
    public class TClientConf : Packets
    {
        public bool boClientCanSet;
        public bool boRunHuman;
        public bool boRunMon;
        public bool boRunNpc;
        public bool boWarRunAll;
        public byte btDieColor;
        public ushort wSpellTime;
        public ushort wHitIime;
        public ushort wItemFlashTime;
        public byte btItemSpeed;
        public bool boCanStartRun;
        public bool boParalyCanRun;
        public bool boParalyCanWalk;
        public bool boParalyCanHit;
        public bool boParalyCanSpell;
        public bool boShowRedHPLable;
        public bool boShowHPNumber;
        public bool boShowJobLevel;
        public bool boDuraAlert;
        public bool boMagicLock;
        public bool boAutoPuckUpItem;

        public byte[] GetPacket()
        {
            using var memoryStream = new MemoryStream();
            var backingStream = new BinaryWriter(memoryStream);

            backingStream.Write(boClientCanSet);
            backingStream.Write(boRunHuman);
            backingStream.Write(boRunMon);
            backingStream.Write(boRunNpc);
            backingStream.Write(boWarRunAll);
            backingStream.Write(btDieColor);
            backingStream.Write(wSpellTime);
            backingStream.Write(wHitIime);
            backingStream.Write(wItemFlashTime);
            backingStream.Write(btItemSpeed);
            backingStream.Write(boCanStartRun);
            backingStream.Write(boParalyCanRun);
            backingStream.Write(boParalyCanWalk);
            backingStream.Write(boParalyCanHit);
            backingStream.Write(boParalyCanSpell);
            backingStream.Write(boShowRedHPLable);
            backingStream.Write(boShowHPNumber);
            backingStream.Write(boShowJobLevel);
            backingStream.Write(boDuraAlert);
            backingStream.Write(boMagicLock);
            backingStream.Write(boAutoPuckUpItem);

            var stream = backingStream.BaseStream as MemoryStream;
            return stream.ToArray();
        }
    }
}