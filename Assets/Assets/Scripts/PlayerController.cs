using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct ClientInput
{
    public long timestamp;
    public Vector2Int movementRaw;
    public Vector3 forwardDirection;
    public Vector3 deltaPos;

    public ClientInput(long timestamp, Vector2Int movementRaw, Vector3 forwardDirection, Vector3 deltaPos)
    {
        this.timestamp = timestamp;
        this.movementRaw = movementRaw;
        this.forwardDirection = forwardDirection;
        this.deltaPos = deltaPos;
    }
}

public class PlayerController : MonoBehaviour
{
    public PlayerMe m_PlayerMe;
    //public Animator m_Animator;
    public Transform m_CameraObject;
    public Transform m_CharacterModel;
    
    private Vector2Int inputVector_raw;
    private Vector2 inputVector;

    private Vector3 previousDeltaPos;
    
    private UIManager m_UIManager;
    //private bool isReady = false;
    private const float TICKS_PER_SEC = 30f;
    private const float MS_PER_TICK = 1000f / TICKS_PER_SEC;
    private const int BUFFER_SIZE = 1024;
    private const float SPEED = 4.8f;

    void Awake() {
        m_UIManager = GameManager.instance.m_UIManager;
    }

    private void SendMovementDataToServer(ClientInput clientInput) {
        if (m_PlayerMe.m_State == -1) {
            return;
        }

        if (m_PlayerMe.m_State > 1) {
            clientInput.movementRaw = Vector2Int.zero;
        }

        var deltaPos = GetDeltaPosition(clientInput);
        m_PlayerMe.deltaPos = deltaPos;
        clientInput.deltaPos = deltaPos;

        if (Mathf.Abs(Vector3.Distance(deltaPos, previousDeltaPos)) > 0f) { // 변화가 있을때만 전송
            ClientSend.PlayerMovement(clientInput, m_PlayerMe.realPosition);

            previousDeltaPos = clientInput.deltaPos;
        }
        
        m_PlayerMe._clientInputQueue.Enqueue(clientInput);
        
        ProcessMovement(clientInput, m_PlayerMe.deltaPos);
    }

    private void SendInputDataToServer() {
        if (m_PlayerMe.m_State > 1) {
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
            var timestamp = TimeSync.GetSyncTime();
            ClientSend.PlayerInput(timestamp, inputs);
        }

        // DEBUG
        if (Input.GetButtonDown("Jump"))
        {
            m_PlayerMe.realPosition = new Vector3(m_PlayerMe.realPosition.x + 12f, m_PlayerMe.realPosition.y, m_PlayerMe.realPosition.z);
        }
    }

    private void ControlPlayerMovement() {
        m_PlayerMe.m_Movement = inputVector;
        m_PlayerMe.m_MovementRaw = inputVector_raw;
    }

    void FixedUpdate() // Camera
    {
        Vector3 forwardDirection = Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));

        ClientInput clientInput = new ClientInput {
            timestamp = TimeSync.GetSyncTime(),
            movementRaw = inputVector_raw,
            forwardDirection = forwardDirection,
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
        

        if (m_PlayerMe.m_State == -1) {
            return;
        }

        //isReady = true; // TEMP
        if (!m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            SendInputDataToServer();
        }

        //Debug.Log(m_State);
    }

    private Vector3 GetDeltaPosition(ClientInput clientInput) { // 이동벡터 반환 및 이동
        if ((clientInput.movementRaw == Vector2Int.zero)) {
            return Vector3.zero;
        }
        
        //m_PlayerMe.m_State = 1;
        Vector3 cam_right = Vector3.Cross(clientInput.forwardDirection, Vector3.down);
        Vector3 deltaPos = (cam_right*clientInput.movementRaw.x + clientInput.forwardDirection*clientInput.movementRaw.y)*SPEED / 30f;
        
        return deltaPos;
    }

    private void ProcessMovement(ClientInput clientInput, Vector3 deltaPos)
    {
        Quaternion rot = Quaternion.LookRotation(clientInput.forwardDirection);
        m_CharacterModel.rotation = rot;

        m_PlayerMe.realPosition += deltaPos;
        m_PlayerMe.realPosition = m_PlayerMe.ClampPosition(m_PlayerMe.realPosition);
        //transform.position = m_PlayerMe.realPosition;
    }
}
