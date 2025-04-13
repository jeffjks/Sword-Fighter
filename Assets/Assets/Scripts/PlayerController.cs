using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
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
    
    private UIManager m_UIManager;
    private CancellationTokenSource _cts;

    private const float TICKS_PER_SEC = 30f;
    private const float MS_PER_TICK = 1000f / TICKS_PER_SEC;
    private const int BUFFER_SIZE = 1024;
    private const float SPEED = 4.8f;

    void Awake() {
        m_UIManager = GameManager.instance.m_UIManager;

        File.WriteAllText("Assets/Resources/send.txt", string.Empty); // DEBUG
        File.WriteAllText("Assets/Resources/received.txt", string.Empty); // DEBUG
    }

    private void SendMovementDataToServer(ClientInput clientInput) {
        if (m_PlayerMe.m_State == PlayerSkill.Dead)
            return;

        if (clientInput.deltaPos == Vector3.zero)
            return;

        ClientSend.PlayerMovement(clientInput, m_PlayerMe.realPosition);
        
        m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);
    }

    private void SendInputDataToServer() {
        if (m_PlayerMe.m_State > PlayerSkill.Move) {
            return;
        }

        _cts?.Cancel(); // 이전 예약 취소
        _cts = new CancellationTokenSource();
        
        var timestamp = TimeSync.GetSyncTime();
        if (Input.GetButtonDown("Attack1"))
        {
            m_PlayerMe.ExecutePlayerSkill(PlayerSkill.Attack1, GetForwardDirection());
            IdleAfterDelay(800, _cts.Token).Forget();
            ClientSend.PlayerSkill(timestamp, PlayerSkill.Attack1, GetForwardDirection());
        }
        else if (Input.GetButtonDown("Block"))
        {
            m_PlayerMe.ExecutePlayerSkill(PlayerSkill.Block, GetForwardDirection());
            IdleAfterDelay(1500, _cts.Token).Forget();
            ClientSend.PlayerSkill(timestamp, PlayerSkill.Block, GetForwardDirection());
        }
        else if (Input.GetButtonDown("Roll"))
        {
            var forwardDirection = GetForwardDirection();
            var rollInput = new ClientInput() {
                timestamp = TimeSync.GetSyncTime(),
                movementRaw = inputVector_raw,
                forwardDirection = forwardDirection,
                deltaPos = forwardDirection * PlayerManager.ROLL_DISTANCE
            };
            m_PlayerMe.m_ClientInputQueue.Enqueue(rollInput);
            m_PlayerMe.ExecutePlayerSkill(PlayerSkill.Roll, GetForwardDirection());
            IdleAfterDelay(1000, _cts.Token).Forget();
            ClientSend.PlayerSkill(timestamp, PlayerSkill.Roll, GetForwardDirection());
        }
        else if (Input.GetButtonDown("Jump")) // DEBUG
        {
            m_PlayerMe.realPosition = new Vector3(m_PlayerMe.realPosition.x + 12f, m_PlayerMe.realPosition.y, m_PlayerMe.realPosition.z);
        }
    }

    private async UniTaskVoid IdleAfterDelay(int delayMilliseconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(delayMilliseconds, cancellationToken: token);
            m_PlayerMe.m_State = PlayerSkill.Idle;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ControlPlayerMovement() {
        m_PlayerMe.m_Movement = inputVector;
        m_PlayerMe.m_MovementRaw = inputVector_raw;
    }

    private Vector3 GetForwardDirection()
    {
        return Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));
    }

    private void FixedUpdate() // Camera
    {
        Vector3 forwardDirection = GetForwardDirection();

        var clientInput = new ClientInput {
            timestamp = TimeSync.GetSyncTime(),
            movementRaw = inputVector_raw,
            forwardDirection = forwardDirection,
            deltaPos = GetDeltaPosition(forwardDirection, inputVector_raw)
        };
        /*
        if (clientInput.deltaPos != Vector3.zero) {
            Debug.Log($"{clientInput.deltaPos.x}, {clientInput.deltaPos.y}, {clientInput.deltaPos.z}");
        }*/
        SendMovementDataToServer(clientInput);
        
        SimulateMove(clientInput);

        if (clientInput.deltaPos != Vector3.zero)
        {
            using (StreamWriter writer = new ("Assets/Resources/send.txt", append: true))
            {
                writer.WriteLine($"[{clientInput.timestamp}] ClientSend: {clientInput.deltaPos} (position: {m_PlayerMe.realPosition})");
            }
        }
    }

    void Update()
    {
        if (GameManager.instance.m_UIManager.m_UI_ChatInputField.GetWritingChat() || m_PlayerMe.m_State > PlayerSkill.Move) {
            inputVector_raw = Vector2Int.zero;
            inputVector = Vector2.zero;
        }
        else {
            inputVector_raw = new Vector2Int((int) Input.GetAxisRaw("Horizontal"), (int) Input.GetAxisRaw("Vertical"));
            inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        ControlPlayerMovement();
        

        if (m_PlayerMe.m_State == PlayerSkill.Dead) {
            return;
        }

        //isReady = true; // TEMP
        if (!m_UIManager.m_UI_ChatInputField.GetWritingChat()) {
            SendInputDataToServer();
        }

        //Debug.Log(m_State);
    }

    private Vector3 GetDeltaPosition(Vector3 forwardDirection, Vector2Int movementRaw) { // 이동벡터 반환 및 이동
        if (movementRaw == Vector2Int.zero) {
            return Vector3.zero;
        }
        
        //m_PlayerMe.m_State = 1;
        Vector3 cam_right = Vector3.Cross(forwardDirection, Vector3.down);
        Vector3 deltaPos = (cam_right*movementRaw.x + forwardDirection*movementRaw.y)*SPEED / 30f;
        
        return deltaPos;
    }

    private void SimulateMove(ClientInput clientInput)
    {
        if (m_PlayerMe.m_State == PlayerSkill.Idle || m_PlayerMe.m_State == PlayerSkill.Move)
        {
            Quaternion rot = Quaternion.LookRotation(clientInput.forwardDirection);
            m_CharacterModel.rotation = rot;
        }

        m_PlayerMe.deltaPos = clientInput.deltaPos;
        m_PlayerMe.realPosition += clientInput.deltaPos;
        m_PlayerMe.realPosition = m_PlayerMe.ClampPosition(m_PlayerMe.realPosition);
        //transform.position = m_PlayerMe.realPosition;
    }
}
