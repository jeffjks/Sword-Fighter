using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public GameObject m_InGameMenu;

    private void OnEnable()
    {
        m_InGameMenu.SetActive(true);
    }

    private void OnDisable()
    {
        m_InGameMenu.SetActive(false);
    }
}
