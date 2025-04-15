using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerMotor : MonoBehaviour
{
    public PlayerController m_PlayerController;
    public PlayerMe m_PlayerMe;

    private const float SPEED = 4.8f;
    
    private void Awake()
    {
        File.WriteAllText("Assets/Resources/send.txt", string.Empty); // DEBUG
        File.WriteAllText("Assets/Resources/received.txt", string.Empty); // DEBUG
    }

    private void FixedUpdate() // Camera
    {
        SimulateMove();
    }

    private void SendMovementDataToServer(ClientInput clientInput) {
        if (m_PlayerMe.CurrentState != PlayerState.Move)
            return;

        if (clientInput.deltaPos == Vector3.zero)
            return;

        ClientSend.PlayerMovement(clientInput, m_PlayerMe.m_RealPosition);
        
        m_PlayerMe.m_ClientInputQueue.Enqueue(clientInput);
    }
    

    private void SimulateMove()
    {
        if (m_PlayerMe.CurrentState != PlayerState.Move)
            return;
        
        Vector3 forwardDirection = m_PlayerController.GetForwardDirection();

        var clientInput = new ClientInput(TimeSync.GetSyncTime(), m_PlayerController.InputVector, forwardDirection, GetDeltaPosition());

        m_PlayerMe.m_DeltaPos = clientInput.deltaPos;
        m_PlayerMe.m_RealPosition += clientInput.deltaPos;
        m_PlayerMe.m_RealPosition = m_PlayerMe.ClampPosition(m_PlayerMe.m_RealPosition);
        //transform.position = m_PlayerMe.realPosition;
        
        SendMovementDataToServer(clientInput);

#if UNITY_EDITOR
        if (clientInput.deltaPos != Vector3.zero)
        {
            using (StreamWriter writer = new ("Assets/Resources/send.txt", append: true))
            {
                writer.WriteLine($"[{clientInput.timestamp}] ClientSend: {clientInput.deltaPos} (position: {m_PlayerMe.m_RealPosition})");
            }
        }
#endif
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
