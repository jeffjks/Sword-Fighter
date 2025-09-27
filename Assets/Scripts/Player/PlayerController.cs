using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Shared.Enums;

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
    public PlayerInput m_PlayerInput;

    public Vector2 InputVector { get; private set; }
    
    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = GameManager.instance.m_UIManager;

        _uiManager.m_UI_ChatInputField.Init(m_PlayerInput);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        InputVector = context.ReadValue<Vector2>();

        m_PlayerMe.SetMovementAnimation(InputVector);
    }

    public void OnAttack(InputAction.CallbackContext context) => UsePlayerSkill(PlayerSkill.Basic);

    public void OnBlock(InputAction.CallbackContext context) => UsePlayerSkill(PlayerSkill.Block);

    public void OnRoll(InputAction.CallbackContext context) => UsePlayerSkill(PlayerSkill.Roll);

    private void UsePlayerSkill(PlayerSkill playerSkill)
    {
        if (CanUseSkill() == false)
            return;

        var timestamp = TimeSync.GetSyncTime();
        var facingDirection = GetForwardDirection();

        if (m_PlayerMe.CurrentState == PlayerState.UsingSkill)
            return;


#if UNITY_EDITOR
        var debugStr = string.Empty;
        for (var i = 1; i <= 4; ++i)
        {
            if (GameManager.players.ContainsKey(i) == false)
                continue;
            var pos = GameManager.players[i].m_RealPosition;
            debugStr += $"\n\tPlayer {i} position: {pos}";
        }
#endif

        var result = m_PlayerMe.ExecuteSkill(timestamp, playerSkill, facingDirection, Vector3.zero);
        if (result == false)
        {
            Debug.LogWarning($"Failed to use skill: {playerSkill}");
            return;
        }

        ClientSend.PlayerSkill(timestamp, facingDirection, playerSkill);

#if UNITY_EDITOR
        Debug.Log($"UseSkill: ({timestamp}) {playerSkill}{debugStr}");
#endif
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (CanUseSkill() == false)
            return;

        m_PlayerMe.m_RealPosition = new Vector3(m_PlayerMe.m_RealPosition.x + 12f, m_PlayerMe.m_RealPosition.y, m_PlayerMe.m_RealPosition.z);
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.canceled)
            return;
        _uiManager.m_UI_ChatInputField.HandleSubmitInput();
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        if (!context.canceled)
            return;
        _uiManager.OnExit();
    }

    private bool CanUseSkill()
    {
        //if (_uiManager.m_UI_ChatInputField.IsWritingChat)
        //    return false;
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

    private void Update() // Camera
    {
        if (m_PlayerMe.IsCurrentState(PlayerState.Idle) || m_PlayerMe.IsCurrentState(PlayerState.Move))
        {
            m_PlayerMe.SetRotation(m_CameraObject.forward);
        }
    }
}
