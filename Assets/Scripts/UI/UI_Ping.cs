using UnityEngine;
using TMPro;

public class UI_Ping : MonoBehaviour
{
    public TextMeshProUGUI m_PingText;

    private void OnEnable()
    {
        TimeSync.Action_OnPingUpdate += UpdatePingText;
    }

    private void OnDisable()
    {
        TimeSync.Action_OnPingUpdate -= UpdatePingText;
    }

    private void UpdatePingText(int ping)
    {
        m_PingText.SetText($"{ping}ms");
    }
}
