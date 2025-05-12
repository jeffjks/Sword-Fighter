using System.Collections.Generic;
using UnityEngine;
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
        if (m_PlayerMe.m_SkillDurations.ContainsKey(playerSkill) == false)
            return;

        var timestamp = TimeSync.GetSyncTime();
        var facingDirection = GetForwardDirection();

        if (_skillDistances.TryGetValue(playerSkill, out var distance)) // 임시로 모든 스킬 일괄처리
        {
            var result = m_PlayerMe.ExecutePlayerSkill(timestamp, playerSkill, facingDirection);
            if (result == false)
                return;

            var clientInput = new ClientInput(timestamp, facingDirection * distance);
            m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);
        }
    }

    public void OnJump()
    {
        if (CanUseSkill() == false)
            return;

        m_PlayerMe.m_RealPosition = new Vector3(m_PlayerMe.m_RealPosition.x + 12f, m_PlayerMe.m_RealPosition.y, m_PlayerMe.m_RealPosition.z);
    }

    private bool CanUseSkill()
    {
        if (m_UIManager.m_UI_ChatInputField.IsWritingChat)
            return false;
        if (IsPlayerControllable() == false)
            return false;
        return true;
    }

    private bool IsPlayerControllable()
    {
        return !m_PlayerMe.IsCurrentState(PlayerState.Dead) && !m_PlayerMe.IsCurrentState(PlayerState.UsingSkill);
    }

    public Vector3 GetForwardDirection()
    {
        return Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));
    }

    private void FixedUpdate() // Camera
    {
        if (m_PlayerMe.IsCurrentState(PlayerState.Idle) || m_PlayerMe.IsCurrentState(PlayerState.Move))
        {
            m_PlayerMe.SetRotation(m_CameraObject.forward);
        }
    }
}
