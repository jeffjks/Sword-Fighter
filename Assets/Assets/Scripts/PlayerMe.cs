using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMe : PlayerManager
{
    void Awake() {
        m_UI_HpBar = GameManager.instance.m_UIManager.m_UI_HpBarMain;
    }

    public override void Start_DealDamage_Attack1() {
        m_Sword.StartDeal();
    }

    public override void Finish_DealDamage_Attack1() {
        m_Sword.FinishDeal();
    }
}
