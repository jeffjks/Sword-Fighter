using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.InputSystem;

public struct ClientInput
{
    public long timestamp;
    public Vector3 deltaPos;

    public ClientInput(long timestamp, Vector3 deltaPos)
    {
        this.timestamp = timestamp;
        this.deltaPos = deltaPos;
    }
}

public class PlayerController : MonoBehaviour
{
    public PlayerMe m_PlayerMe;
    //public Animator m_Animator;
    public Transform m_CameraObject;
    
    public Vector2 InputVector { get; private set; }
    
    private UIManager m_UIManager;
    private CancellationTokenSource _cts;

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

    private void Awake()
    {
        m_UIManager = GameManager.instance.m_UIManager;
    }

    public void OnMove(InputValue inputValue)
    {
        InputVector = inputValue.Get<Vector2>();

        m_PlayerMe.SetMovementAnimation(InputVector);
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

        if (_skillDistances.TryGetValue(playerSkill, out var distance) && distance != 0f) // 임시로 모든 스킬 일괄처리
        {
            var clientInput = new ClientInput(timestamp, forwardDirection * distance);
            m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);

            m_PlayerMe.ExecutePlayerSkill(playerSkill, forwardDirection);
            IdleAfterDelay(duration, _cts.Token).Forget();
            ClientSend.PlayerSkill(timestamp, forwardDirection, playerSkill);
        }
    }

    public void OnJump()
    {
        if (CanUseSkill() == false)
            return;
        
        _cts?.Cancel(); // 이전 예약 취소
        _cts = new CancellationTokenSource();

        m_PlayerMe.m_RealPosition = new Vector3(m_PlayerMe.m_RealPosition.x + 12f, m_PlayerMe.m_RealPosition.y, m_PlayerMe.m_RealPosition.z);
    }

    private bool CanUseSkill() // 최적화 예정
    {
        if (m_UIManager.m_UI_ChatInputField.GetWritingChat())
            return false;
        if (m_PlayerMe.CurrentState == PlayerState.Dead || m_PlayerMe.CurrentState == PlayerState.UsingSkill)
            return false;
        return true;
    }

    private async UniTaskVoid IdleAfterDelay(int delayMilliseconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(delayMilliseconds, cancellationToken: token);
            m_PlayerMe.CurrentState = PlayerState.Idle;
        }
        catch (OperationCanceledException)
        {
        }
    }

    public Vector3 GetForwardDirection()
    {
        return Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));
    }

    private void FixedUpdate() // Camera
    {
        if (m_PlayerMe.CurrentState == PlayerState.Idle || m_PlayerMe.CurrentState == PlayerState.Move)
        {
            m_PlayerMe.SetRotation(m_CameraObject.forward);
        }
    }
}
