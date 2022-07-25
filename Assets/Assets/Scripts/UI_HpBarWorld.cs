using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HpBarWorld : UI_HpBar
{
    public Text m_UI_UserNameText;
    public float m_MaxDistance;

    private const float maxScale = 0.25f;
    private const float minScale = 0.1f;

    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(m_PlayerManager.transform.position + new Vector3(0, 2.4f, 0));

        Vector3 localPlayerPosition = GameManager.players[Client.instance.myId].transform.position;
        float distance = Vector3.Distance(m_PlayerManager.transform.position, localPlayerPosition);

        if (distance < m_MaxDistance) {
            float scale = Mathf.Lerp(maxScale, minScale, distance/m_MaxDistance);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else {
            transform.localScale = Vector3.zero;
        }
    }

    public override void SetUserName(string username)
    {
        m_UI_UserNameText.text = username;
    }
}
