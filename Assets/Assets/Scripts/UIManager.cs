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
    public UI_ChatInputField m_UI_ChatInputField;
    public UI_ChatWindow m_UI_ChatWindow;
    public InputField m_IpAdressField;
    public InputField m_UsernameField;
    public UI_HpBarMain m_UI_HpBarMain;
    public Text m_ErrorText;

    private ObjectPooling m_ObjectPooling;
    private bool m_Ingame = false;

    private void Awake() // Singleton
    {
        m_ObjectPooling = GameManager.instance.m_ObjectPooling;
        
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
        m_IpAdressField.text = ChatClient.instance.defaultIp;
    }

    private bool ConnetcToServer() {
        try {
            if (Client.instance.enabled) {
                Client.instance.ConnectToServer(m_IpAdressField.text);
            }
        }
        catch (FormatException e) {
            m_ErrorText.text = "Invalid IP Adress!";
            Debug.LogError(e);
            return false;
        }
        catch (TimeoutException e) {
            m_ErrorText.text = "Failed to connect server.";
            Debug.LogError(e);
            return false;
        }
        catch (Exception e) {
            m_ErrorText.text = "Unknown error has occured";
            Debug.LogError(e);
            return false;
        }
        return true;
    }

    private bool ConnetcToChatServer() {
        try {
            if (ChatClient.instance.enabled) {
                ChatClient.instance.ConnectToServer(m_IpAdressField.text);
            }
        }
        catch (FormatException e) {
            //m_ErrorText.text = "Invalid IP Adress!";
            Debug.LogError(e);
            return false;
        }
        catch (TimeoutException e) {
            //m_ErrorText.text = "Failed to connect chat server.";
            Debug.LogError(e);
            return false;
        }
        catch (Exception e) {
            //m_ErrorText.text = "Unknown error has occured";
            Debug.LogError(e);
            return false;
        }
        return true;
    }

    public void ButtonConnectToServer() {
        if (m_UsernameField.text.Length < 4) {
            m_ErrorText.text = "Username is too short!";
            return;
        }

        if (!ConnetcToServer()) {
            Client.instance.Disconnect();
            return;
        }
        m_StartMenu.SetActive(false);
        m_InGameMenu.SetActive(true);
        m_UsernameField.interactable = false;
        m_UI_HpBarMain.FillMainHpBar(1f);
        m_ErrorText.text = string.Empty;
        m_Ingame = true;
        m_ObjectPooling.Init(3);

        if (!ConnetcToChatServer()) {
            ChatClient.instance.Disconnect();
            m_UI_ChatWindow.PushTextMessage(-1, "채팅 서버에 접속할 수 없습니다.");
        }
    }

    private void ReturnToMainMenu() {
        Client.instance.Disconnect();
        ChatClient.instance.Disconnect();

        m_UI_ChatWindow.ClearChatWindow();
        m_UI_ChatInputField.WritingChatOff();

        m_StartMenu.SetActive(true);
        m_InGameMenu.SetActive(false);
        m_UsernameField.interactable = true;
        m_Ingame = false;
    }

    

    void Update() {
        if (m_Ingame) {
            if (Client.instance.enabled && !Client.instance.IsConnected()) {
                ReturnToMainMenu();
            }
            else if (Input.GetButtonDown("Exit")) {
                ReturnToMainMenu();
            }

            if (Input.GetButtonDown("Submit")) {
                m_UI_ChatInputField.ToggleWritingChat();
            }
        }
    }
}
