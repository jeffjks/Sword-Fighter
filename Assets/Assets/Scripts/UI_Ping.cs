using UnityEngine;
using TMPro;

public class UI_Ping : MonoBehaviour
{
    public TextMeshProUGUI pingText;

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
        pingText.SetText($"{ping}ms");
    }
}
