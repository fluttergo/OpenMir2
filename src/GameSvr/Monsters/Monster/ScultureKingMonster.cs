﻿using SystemModule;

namespace GameSvr
{
    public class ScultureKingMonster : Monster
    {
        private int m_nDangerLevel = 0;
        private readonly IList<TBaseObject> m_SlaveObjectList = null;

        public ScultureKingMonster() : base()
        {
            m_dwSearchTime = M2Share.RandomNumber.Random(1500) + 1500;
            m_nViewRange = 8;
            m_boStoneMode = true;
            m_nCharStatusEx = Grobal2.STATE_STONE_MODE;
            m_btDirection = 5;
            m_nDangerLevel = 5;
            m_SlaveObjectList = new List<TBaseObject>();
        }

        private void MeltStone()
        {
            m_nCharStatusEx = 0;
            m_nCharStatus = GetCharStatus();
            SendRefMsg(Grobal2.RM_DIGUP, m_btDirection, m_nCurrX, m_nCurrY, 0, "");
            m_boStoneMode = false;
            var stoneEvent = new Event(m_PEnvir, m_nCurrX, m_nCurrY, 6, 5 * 60 * 1000, true);
            M2Share.EventManager.AddEvent(stoneEvent);
        }

        private void CallSlave()
        {
            short nX = 0;
            short nY = 0;
            TBaseObject BaseObject;
            var nCount = M2Share.RandomNumber.Random(6) + 6;
            GetFrontPosition(ref nX, ref nY);
            for (var i = 0; i <= nCount; i++)
            {
                if (m_SlaveObjectList.Count >= 30)
                {
                    break;
                }
                BaseObject = M2Share.UserEngine.RegenMonsterByName(m_sMapName, nX, nY, M2Share.g_Config.sZuma[M2Share.RandomNumber.Random(4)]);
                if (BaseObject != null)
                {
                    m_SlaveObjectList.Add(BaseObject);
                }
            }
        }

        public override void Attack(TBaseObject TargeTBaseObject, byte nDir)
        {
            var WAbil = m_WAbil;
            int nPower = GetAttackPower(HUtil32.LoWord(WAbil.DC), HUtil32.HiWord(WAbil.DC) - HUtil32.LoWord(WAbil.DC));
            HitMagAttackTarget(TargeTBaseObject, 0, nPower, true);
        }

        public override void Run()
        {
            TBaseObject BaseObject;
            if (!m_boGhost && !m_boDeath && m_wStatusTimeArr[Grobal2.POISON_STONE] == 0 && (HUtil32.GetTickCount() - m_dwWalkTick) >= m_nWalkSpeed)
            {
                if (m_boStoneMode)
                {
                    for (var i = 0; i < m_VisibleActors.Count; i++)
                    {
                        BaseObject = m_VisibleActors[i].BaseObject;
                        if (BaseObject.m_boDeath)
                        {
                            continue;
                        }
                        if (IsProperTarget(BaseObject))
                        {
                            if (!BaseObject.m_boHideMode || m_boCoolEye)
                            {
                                if (Math.Abs(m_nCurrX - BaseObject.m_nCurrX) <= 2 && Math.Abs(m_nCurrY - BaseObject.m_nCurrY) <= 2)
                                {
                                    MeltStone();
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if ((HUtil32.GetTickCount() - m_dwSearchEnemyTick) > 8000 || (HUtil32.GetTickCount() - m_dwSearchEnemyTick) > 1000 && m_TargetCret == null)
                    {
                        m_dwSearchEnemyTick = HUtil32.GetTickCount();
                        SearchTarget();
                        if (m_nDangerLevel > m_WAbil.HP / m_WAbil.MaxHP * 5 && m_nDangerLevel > 0)
                        {
                            m_nDangerLevel -= 1;
                            CallSlave();
                        }
                        if (m_WAbil.HP == m_WAbil.MaxHP)
                        {
                            m_nDangerLevel = 5;
                        }
                    }
                }
                for (var i = m_SlaveObjectList.Count - 1; i >= 0; i--)
                {
                    BaseObject = m_SlaveObjectList[i];
                    if (BaseObject.m_boDeath || BaseObject.m_boGhost)
                    {
                        m_SlaveObjectList.RemoveAt(i);
                    }
                }
            }
            base.Run();
        }
    }
}

