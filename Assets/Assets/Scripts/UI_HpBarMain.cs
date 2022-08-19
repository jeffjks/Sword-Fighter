using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_HpBarMain : UI_HpBar
{
    public void FillMainHpBar(float value) {
        m_HpBarFillImage.fillAmount = value;
    }

    public override void SetUserNameUI(string username)
    {
        return;
    }
}
