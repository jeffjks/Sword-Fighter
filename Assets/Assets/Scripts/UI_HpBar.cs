using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public abstract class UI_HpBar : MonoBehaviour
{
    public PlayerManager m_PlayerManager;
    public Image m_HpBarFillImage;

    public void UpdateHpBarFill() {
        int current_hp = m_PlayerManager.m_CurrentHp;
        int max_hp = m_PlayerManager.m_MaxHp;

        m_HpBarFillImage.fillAmount = (float) current_hp/max_hp;
    }

    public abstract void SetUserName(string username);
}
