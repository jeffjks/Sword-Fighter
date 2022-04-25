using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ClientInput
{
    public int seqNum;
    public int horizontal_raw;
    public int vertical_raw;
    public Vector3 cam_forward;
}

public class MainCharacter : MonoBehaviour
{
    public PlayerManager m_PlayerManager;
    public Animator m_Animator;
    public Transform m_CameraObject;
    public Transform m_CharacterModel;
    
    // Shared
    private float timer;
    private int currentTick = 0;
    private const float TICKS_PER_SEC = 30f;
    private const float MS_PER_TICK = 1000f / TICKS_PER_SEC;
    private const int BUFFER_SIZE = 1024;
    private const float SPEED = 0.16f;

    // Client specific
    private readonly Queue<ClientInput> inputTimeline = new Queue<ClientInput>();

    private void SendMovementDataToServer(ClientInput clientInput) {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (m_PlayerManager.m_State > 1) {
            clientInput.horizontal_raw = 0;
            clientInput.vertical_raw = 0;
        }
        transform.position = ProcessMovement(transform.position, clientInput);

        ClientSend.PlayerMovement(movement, clientInput);
        
        this.inputTimeline.Enqueue(clientInput);
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

    void FixedUpdate()
    {
        int horizontal_raw = (int) Input.GetAxisRaw("Horizontal");
        int vertical_raw = (int) Input.GetAxisRaw("Vertical");
        Vector3 cam_forward = Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));

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

        SendInputDataToServer();

        //Debug.Log(m_State);
    }

    public void OnStateReceived(int receivedSeqNum, Vector3 receivedState) {
        // Remove inputs which arrived at server.
        while (inputTimeline.Count > 0 && inputTimeline.Peek().seqNum <= receivedSeqNum) {
            inputTimeline.Dequeue();
        }

        Vector3 newState = receivedState;

        // Re-apply unacknowledged inputs.
        foreach (var input in inputTimeline) {
            newState = ProcessMovement(newState, input);
        }

        // Set new move state.
        if (Vector3.Distance(newState, transform.position) > 0f) {
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
