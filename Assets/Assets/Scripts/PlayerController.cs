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
    private const float SPEED = 4.8f;

    private readonly Dictionary<PlayerSkill, int> _skillDurations = new()
    {
        { PlayerSkill.Attack1, 800 },
        { PlayerSkill.Block, 1500 },
        { PlayerSkill.Roll, 1000 }
    };

    private readonly Dictionary<PlayerSkill, float> _skillDistances = new()
    {
        { PlayerSkill.Attack1, 0f },
        { PlayerSkill.Block, 0f },
        { PlayerSkill.Roll, 5f }
    };

    private void Awake() {
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

    public void OnAttack() => UsePlayerSkill(PlayerSkill.Attack1);

    public void OnBlock() => UsePlayerSkill(PlayerSkill.Block);

    public void OnRoll() => UsePlayerSkill(PlayerSkill.Roll);

    private void UsePlayerSkill(PlayerSkill playerSkill)
    {
        if (CanUseSkill() == false)
            return;
        if (_skillDurations.TryGetValue(playerSkill, out int duration) == false)
            return;
        
        _cts?.Cancel(); // 이전 예약 취소
        _cts = new CancellationTokenSource();

        var timestamp = TimeSync.GetSyncTime();
        var forwardDirection = GetForwardDirection();

        if (_skillDistances.TryGetValue(playerSkill, out var distance) && distance != 0f)
        {
            var clientInput = new ClientInput() {
                timestamp = timestamp,
                movementRaw = inputVector_raw,
                forwardDirection = forwardDirection,
                deltaPos = forwardDirection * distance
            };
            m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);
        }

        m_PlayerMe.ExecutePlayerSkill(playerSkill, forwardDirection);
        IdleAfterDelay(duration, _cts.Token).Forget();
        ClientSend.PlayerSkill(timestamp, playerSkill, forwardDirection);
    }

    public void OnJump()
    {
        if (CanUseSkill() == false)
            return;
        
        _cts?.Cancel(); // 이전 예약 취소
        _cts = new CancellationTokenSource();

        m_PlayerMe.realPosition = new Vector3(m_PlayerMe.realPosition.x + 12f, m_PlayerMe.realPosition.y, m_PlayerMe.realPosition.z);
    }

    private bool CanUseSkill() // 최적화 예정
    {
        if (m_UIManager.m_UI_ChatInputField.GetWritingChat())
            return false;
        if (m_PlayerMe.m_State == PlayerSkill.Dead)
            return false;
        if (m_PlayerMe.m_State > PlayerSkill.Move)
            return false;
        return true;
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

#if UNITY_EDITOR
        if (clientInput.deltaPos != Vector3.zero)
        {
            using (StreamWriter writer = new ("Assets/Resources/send.txt", append: true))
            {
                writer.WriteLine($"[{clientInput.timestamp}] ClientSend: {clientInput.deltaPos} (position: {m_PlayerMe.realPosition})");
            }
        }
#endif
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
    }

    private Vector3 GetDeltaPosition(Vector3 forwardDirection, Vector2Int movementRaw) { // 이동벡터 반환
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
