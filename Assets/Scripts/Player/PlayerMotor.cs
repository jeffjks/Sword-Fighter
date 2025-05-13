using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Enums;

public class PlayerMotor : MonoBehaviour
{
    public PlayerController m_PlayerController;
    public PlayerMe m_PlayerMe;

    private Vector2 _prevInputVector;
    private const float SPEED = 4.8f;

    private void FixedUpdate() // Camera
    {
        SimulateMove();
    }

    private void SimulateMove()
    {
        var inputVector = m_PlayerController.InputVector;

        if (inputVector == Vector2.zero && _prevInputVector == Vector2.zero)
        {
            if (m_PlayerMe.IsCurrentState(PlayerState.Move))
            {
                m_PlayerMe.CurrentStateMachine.SetState(PlayerState.Idle);
            }
            return;
        }
        _prevInputVector = inputVector;

        if ((m_PlayerMe.IsCurrentState(PlayerState.Idle) || m_PlayerMe.IsCurrentState(PlayerState.Move)) == false)
            return;
        if (!m_PlayerMe.m_IsMovable)
            return;
        
        m_PlayerMe.CurrentStateMachine.SetState(PlayerState.Move);
        Vector3 forwardDirection = m_PlayerController.GetForwardDirection();

        var timestamp = TimeSync.GetSyncTime();

        var deltaPos = GetDeltaPosition();

        m_PlayerMe.m_DeltaPos = deltaPos;
        m_PlayerMe.m_RealPosition = m_PlayerMe.ClampPosition(m_PlayerMe.m_RealPosition + deltaPos);
        transform.position = m_PlayerMe.ClampPosition(transform.position + deltaPos);
        
        var clientInput = new ClientInput(timestamp, deltaPos);
        ClientSend.PlayerMovement(timestamp, forwardDirection, deltaPos, inputVector);
        m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);
    }

    private Vector3 GetDeltaPosition()
    {
        var forwardDirection = m_PlayerController.GetForwardDirection();
        
        //m_PlayerMe.m_State = 1;
        Vector3 cam_right = Vector3.Cross(forwardDirection, Vector3.down);
        Vector3 deltaPos = (cam_right*m_PlayerController.InputVector.x + forwardDirection*m_PlayerController.InputVector.y)*SPEED / 30f;
        
        return deltaPos;
    }
}
