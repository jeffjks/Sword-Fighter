using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ClientInput // 클라이언트측 예측 방식
{
    public int seqNum;
    public int horizontal_raw;
    public int vertical_raw;
    public Vector3 cam_forward;
    public Vector3 deltaPos;
}

public class PlayerController : MonoBehaviour
{
    public PlayerManager m_PlayerManager;
    //public Animator m_Animator;
    public Transform m_CameraObject;
    public Transform m_CharacterModel;
    
    private Vector2Int inputVector_raw;
    private Vector2 inputVector;

    private Vector3 previousDeltaPos;
    
    private UIManager m_UIManager;
    private float timer;
    private int currentTick = 0;
    //private bool isReady = false;
    private const float TICKS_PER_SEC = 30f;
    private const float MS_PER_TICK = 1000f / TICKS_PER_SEC;
    private const int BUFFER_SIZE = 1024;
    private const float SPEED = 4.8f;

    private readonly Queue<ClientInput> inputTimeline = new Queue<ClientInput>();

    void Awake() {
        m_UIManager = GameManager.instance.m_UIManager;
    }

    private void SendMovementDataToServer(ClientInput clientInput) {
        if (m_PlayerManager.m_State == -1) {
            return;
        }

        if (m_PlayerManager.m_State > 1) {
            clientInput.horizontal_raw = 0;
            clientInput.vertical_raw = 0;
        }

        m_PlayerManager.deltaPos = ProcessMovement(clientInput);
        clientInput.deltaPos = m_PlayerManager.deltaPos;

        if (Mathf.Abs(Vector3.Distance(clientInput.deltaPos, previousDeltaPos)) > 0f) { // 변화가 있을때만 전송
            ClientSend.PlayerMovement(inputVector, clientInput);
            //StartCoroutine(PlayerMovementDelay(inputVector, clientInput));

            previousDeltaPos = clientInput.deltaPos;
        }
        
        inputTimeline.Enqueue(clientInput);
    }

    private IEnumerator PlayerMovementDelay(Vector2 movement, ClientInput clientInput) {
        yield return new WaitForSeconds(2.0f);
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

    private void ControlPlayerMovement() {
        m_PlayerManager.m_Movement = inputVector;
        m_PlayerManager.m_MovementRaw = inputVector_raw;
    }

    void FixedUpdate() // Camera
    {
        Vector3 cam_forward = Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));

        ClientInput clientInput = new ClientInput {
            seqNum = this.currentTick++,
            horizontal_raw = inputVector_raw.x,
            vertical_raw = inputVector_raw.y,
            cam_forward = cam_forward,
            deltaPos = Vector3.zero
        };
        /*
        if (clientInput.deltaPos != Vector3.zero) {
            Debug.Log($"{clientInput.deltaPos.x}, {clientInput.deltaPos.y}, {clientInput.deltaPos.z}");
        }*/
        SendMovementDataToServer(clientInput);
    }

    void Update()
    {
        if (GameManager.instance.m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            inputVector_raw = Vector2Int.zero;
            inputVector = Vector2.zero;
        }
        else {
            inputVector_raw = new Vector2Int((int) Input.GetAxisRaw("Horizontal"), (int) Input.GetAxisRaw("Vertical"));
            inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        ControlPlayerMovement();
        

        if (m_PlayerManager.m_State == -1) {
            return;
        }

        //isReady = true; // TEMP
        if (!m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            SendInputDataToServer();
        }

        //Debug.Log(m_State);
    }

    public void OnStateReceived(int receivedSeqNum, Vector3 receivedState) { // *** 추가 작업 필요 (멈췄을때만 위치 보정?)
        if (receivedState != Vector3.zero) {
            return;
        }
        while (inputTimeline.Count > 0 && inputTimeline.Peek().seqNum <= receivedSeqNum) { // 처리된 요청은 삭제
            inputTimeline.Dequeue();
        }
        int seqNumTemp = inputTimeline.Peek().seqNum;

        Vector3 newState = receivedState; // 서버로부터 받은 가장 최신 좌표

        foreach (var input in inputTimeline) { // 지금까지 input기록에 따라 시뮬레이션하여 현재 좌표 계산
            newState += input.deltaPos;
            //newState = ProcessMovement(newState, input);
        }

        //Debug.Log(receivedSeqNum + " : " + receivedState);
        //Debug.Log(seqNumTemp + ", " + newState);
        
        if (Vector3.Distance(newState, m_PlayerManager.realPosition) > 0f) { // 계산한 좌표가 맞는지 확인
            m_PlayerManager.realPosition = newState;
        }
    }

    private Vector3 ProcessMovement(ClientInput clientInput) { // 이동벡터 반환 및 이동
        if ((clientInput.horizontal_raw == 0 && clientInput.vertical_raw == 0)) {
            return Vector3.zero;
        }
        
        //m_PlayerManager.m_State = 1;
        Vector3 cam_right = Vector3.Cross(clientInput.cam_forward, Vector3.down);
        Vector3 deltaPos = (cam_right*clientInput.horizontal_raw + clientInput.cam_forward*clientInput.vertical_raw)*SPEED / 30f;
        
        Quaternion rot = Quaternion.LookRotation(clientInput.cam_forward);
        m_CharacterModel.rotation = rot;
        
        m_PlayerManager.realPosition += deltaPos;
        m_PlayerManager.realPosition = m_PlayerManager.ClampPosition(m_PlayerManager.realPosition);
        transform.position = m_PlayerManager.realPosition;
        return deltaPos;
    }
}
