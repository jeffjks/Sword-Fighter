using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public GameObject m_InGameMenu;

    private void OnEnable()
    {
        if (m_InGameMenu != null)
            m_InGameMenu.SetActive(true);
    }

    private void OnDisable()
    {
        if (m_InGameMenu != null)
            m_InGameMenu.SetActive(false);
    }
}
