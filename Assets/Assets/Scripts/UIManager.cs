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
    public Image m_OppositeHpBar;
    public GameObject m_OppositeHpUI;
    public Text m_MessageText;

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
    }

    public void ConnectToServer() {
        try {
            Client.instance.ConnectToServer(m_IpAdressField.text);
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
        m_StartMenu.SetActive(false);
        m_InGameMenu.SetActive(true);
        m_UsernameField.interactable = false;
        m_MyHpBar.fillAmount = 1f;
        m_OppositeHpBar.fillAmount = 1f;
    }

    void Update() {
        if (GameManager.players.ContainsKey(Client.instance.myId)) {
            int my_current_hp = GameManager.players[Client.instance.myId].m_CurrentHp;
            int my_max_hp = GameManager.players[Client.instance.myId].m_MaxHp;
            SetMyHpBar(my_current_hp, my_max_hp);
        }
        
        if (GameManager.players.ContainsKey(Client.instance.oppositeId)) {
            if (!m_OppositeHpUI.activeSelf) {
                m_OppositeHpUI.SetActive(true);
            }
            int opposite_current_hp = GameManager.players[Client.instance.oppositeId].m_CurrentHp;
            int opposite_max_hp = GameManager.players[Client.instance.oppositeId].m_MaxHp;
            SetOppositeHpBar(opposite_current_hp, opposite_max_hp);
        }
        else {
            m_OppositeHpBar.fillAmount = 1f;
            m_OppositeHpUI.SetActive(false);
        }
    }

    public void SetMyHpBar(int current_hp, int max_hp) {
        m_MyHpBar.fillAmount = (float) current_hp/max_hp;
    }

    public void SetOppositeHpBar(int current_hp, int max_hp) {
        m_OppositeHpBar.fillAmount = (float) current_hp/max_hp;
    }
}
