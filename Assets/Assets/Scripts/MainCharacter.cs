using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ClientInput // 클라이언트측 예측 방식
{
    public int seqNum;
    public int horizontal_raw;
    public int vertical_raw;
    public Vector3 cam_forward;
}

public class MainCharacter : MonoBehaviour
{
    public PlayerManager m_PlayerManager;
    //public Animator m_Animator;
    public Transform m_CameraObject;
    public Transform m_CharacterModel;
    
    private UIManager m_UIManager;
    private float timer;
    private int currentTick = 0;
    //private bool isReady = false;
    private const float TICKS_PER_SEC = 30f;
    private const float MS_PER_TICK = 1000f / TICKS_PER_SEC;
    private const int BUFFER_SIZE = 1024;
    private const float SPEED = 0.16f;

    private readonly Queue<ClientInput> inputTimeline = new Queue<ClientInput>();

    void Awake() {
        m_UIManager = GameManager.instance.m_UIManager;
    }

    private void SendMovementDataToServer(ClientInput clientInput) {
        if (m_PlayerManager.m_State == -1) {
            return;
        }
        Vector2 movement = m_PlayerManager.inputVector;

        if (m_PlayerManager.m_State > 1) {
            clientInput.horizontal_raw = 0;
            clientInput.vertical_raw = 0;
        }
        transform.position = ProcessMovement(transform.position, clientInput);

        ClientSend.PlayerMovement(movement, clientInput);
        //StartCoroutine(PlayerMovementDelay(movement, clientInput));
        
        this.inputTimeline.Enqueue(clientInput);
    }

    private IEnumerator PlayerMovementDelay(Vector2 movement, ClientInput clientInput) {
        yield return new WaitForSeconds(0.5f);
        ClientSend.PlayerMovement(movement, clientInput);
        //Debug.Log("B");
        yield break;
    }

    private void SendInputDataToServer() {
        if (m_PlayerManager.m_State > 1) {
            return;
        }
        bool[] inputs = new bool[]
        {
            Input.GetButtonDown("Block"),
            Input.GetButtonDown("Attack1"),
            Input.GetButtonDown("Attack2"),
            Input.GetButtonDown("Roll")
        };

        bool tmp = false;
        foreach (bool input in inputs) {
            if (input) {
                tmp = true;
                break;
            }
        }
        if (tmp) {
            ClientSend.PlayerInput(inputs);
        }
    }

    void FixedUpdate() // Camera
    {
        int horizontal_raw;
        int vertical_raw;
        Vector3 cam_forward = Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));

        if (m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            horizontal_raw = 0;
            vertical_raw = 0;
        }
        else {
            horizontal_raw = (int) m_PlayerManager.inputVector_raw.x;
            vertical_raw = (int) m_PlayerManager.inputVector_raw.y;
        }

        ClientInput clientInput = new ClientInput {
            seqNum = this.currentTick++,
            horizontal_raw = horizontal_raw,
            vertical_raw = vertical_raw,
            cam_forward = cam_forward
        };

        SendMovementDataToServer(clientInput);
    }

    void Update()
    {
        if (m_PlayerManager.m_State == -1) {
            return;
        }

        //isReady = true; // TEMP
        if (!m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            SendInputDataToServer();
        }

        //Debug.Log(m_State);
    }

    public void OnStateReceived(int receivedSeqNum, Vector3 receivedState) {
        while (inputTimeline.Count > 0 && inputTimeline.Peek().seqNum <= receivedSeqNum) { // 처리된 요청은 삭제
            inputTimeline.Dequeue();
        }

        Vector3 newState = receivedState; // 서버로부터 받은 가장 최신 좌표

        foreach (var input in inputTimeline) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            newState = ProcessMovement(newState, input);
        }
        
        if (Vector3.Distance(newState, transform.position) > 0f) { // 계산한 좌표가 맞는지 확인
            transform.position = newState;
        }
    }

    private Vector3 ProcessMovement(Vector3 pos, ClientInput clientInput) {
        if ((clientInput.horizontal_raw == 0 && clientInput.vertical_raw == 0)) {
            return pos;
        }
        
        //m_PlayerManager.m_State = 1;
        Vector3 cam_right = Vector3.Cross(clientInput.cam_forward, Vector3.down);
        pos += (cam_right*clientInput.horizontal_raw + clientInput.cam_forward*clientInput.vertical_raw)*SPEED;
        
        Quaternion rot = Quaternion.LookRotation(clientInput.cam_forward);
        m_CharacterModel.rotation = rot;
        
        pos = m_PlayerManager.ClampPosition(pos);
        return pos;
    }
}
