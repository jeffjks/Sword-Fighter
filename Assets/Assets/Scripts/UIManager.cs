using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject m_StartMenu;
    public GameObject m_InGameMenu;
    public InputField m_IpAdressField;
    public InputField m_UsernameField;
    public Image m_MyHpBar;
    public GameObject m_OppositeUI_prefab;
    public Text m_MessageText;
    public float m_MaxDistance;

    private Dictionary<int, OppositeUI> m_OppositeUIs = new Dictionary<int, OppositeUI>();
    private bool m_Ingame = false;

    private void Awake() // Singleton
    {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists. Destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        m_IpAdressField.text = Client.instance.defaultIp;
        //m_IpAdressField.text = ChatClient.instance.defaultIp;
    }

    public void ConnectToServer() {
        try {
            Client.instance.ConnectToServer(m_IpAdressField.text);
            ChatClient.instance.ConnectToServer(m_IpAdressField.text);
        }
        catch (FormatException e) {
            m_MessageText.text = "Invalid IP Adress!";
            Debug.LogError(e);
            return;
        }
        catch (TimeoutException e) {
            m_MessageText.text = "Failed to connect server.";
            Debug.LogError(e);
            return;
        }
        catch (Exception e) {
            m_MessageText.text = "Unknown error has occured";
            Debug.LogError(e);
            return;
        }
        m_StartMenu.SetActive(false);
        m_InGameMenu.SetActive(true);
        m_UsernameField.interactable = false;
        m_MyHpBar.fillAmount = 1f;
        //m_OppositeHpBar.fillAmount = 1f;
        m_MessageText.text = string.Empty;
        m_Ingame = true;
    }

    private void ReturnToMainMenu() {
        Client.instance.Disconnect();
        ChatClient.instance.Disconnect();
        foreach (KeyValuePair<int, PlayerManager> player in GameManager.players) {
            Destroy(GameManager.players[player.Key].gameObject);
        }
        GameManager.players.Clear();
        m_StartMenu.SetActive(true);
        m_InGameMenu.SetActive(false);
        m_UsernameField.interactable = true;
        m_Ingame = false;
    }

    void Update() {
        if (m_Ingame) {
            if (!Client.instance.IsConnected() || Input.GetButtonDown("Exit")) {
                ReturnToMainMenu();
            }
        }

        try {
            foreach (KeyValuePair<int, PlayerManager> player in GameManager.players) {
                PlayerManager myPm = GameManager.players[Client.instance.myId];
                if (player.Key == Client.instance.myId) {
                    int my_current_hp = myPm.m_CurrentHp;
                    int my_max_hp = myPm.m_MaxHp;
                    SetMyHpBar(my_current_hp, my_max_hp);
                }
                else {
                    if (!m_OppositeUIs.ContainsKey(player.Key)) { // Create New UI Object
                        GameObject oppositeUI = Instantiate(m_OppositeUI_prefab, Vector3.zero, Quaternion.identity, m_InGameMenu.transform);
                        m_OppositeUIs.Add(player.Key, oppositeUI.GetComponent<OppositeUI>());
                        m_OppositeUIs[player.Key].m_OppositeUI_Username.text = GameManager.players[player.Key].username;
                    }
                    if (m_OppositeUIs.ContainsKey(player.Key)) { // Update UI Object
                        PlayerManager opPm = GameManager.players[player.Key];
                        int opposite_current_hp = opPm.m_CurrentHp;
                        int opposite_max_hp = opPm.m_MaxHp;
                        m_OppositeUIs[player.Key].transform.position = Camera.main.WorldToScreenPoint(opPm.transform.position + new Vector3(0, 2.4f, 0));

                        float distance = Vector3.Distance(myPm.transform.position, opPm.transform.position);
                        if (distance < m_MaxDistance) {
                            float scale = Mathf.Lerp(0.5f, 0.2f, distance/m_MaxDistance);
                            m_OppositeUIs[player.Key].transform.localScale = new Vector3(scale, scale, scale);
                            SetOppositeHpBar(m_OppositeUIs[player.Key], opposite_current_hp, opposite_max_hp);
                        }
                        else {
                            m_OppositeUIs[player.Key].transform.localScale = Vector3.zero;
                        }
                    }
                }
            }
        }
        catch (KeyNotFoundException e) {
            Debug.LogError(e);
        }
    }

    public void DestroyUI(int id) {
        Destroy(m_OppositeUIs[id].gameObject);
        m_OppositeUIs.Remove(id);
    }

    public void SetMyHpBar(int current_hp, int max_hp) {
        m_MyHpBar.fillAmount = (float) current_hp/max_hp;
    }

    public void SetOppositeHpBar(OppositeUI oppositeHP, int current_hp, int max_hp) {
        oppositeHP.m_OppositeUI_HpBar.fillAmount = (float) current_hp/max_hp;
    }
}
