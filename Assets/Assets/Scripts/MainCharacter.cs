using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
struct Unit {
    public Vector3Int pos;
    public int direction;

    public Unit(Vector3Int pos, int direction) {
        this.pos = pos;
        this.direction = direction;
    }
};
*/

public class MainCharacter : MonoBehaviour
{
    public PlayerManager m_PlayerManager;
    public Animator m_Animator;
    public float m_CameraRotationSpeed;
    public Transform m_CameraObject;
    public Transform m_CharacterModel;
    public int m_Speed;
    
    private float m_CameraRotation;
    //Unit unit;

    private void SendMovementDataToServer() {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        ClientSend.PlayerMovement(movement);
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

    void Update()
    {
        float xRot = Input.GetAxis("Mouse X");
        float yRot = Input.GetAxis("Mouse Y");

        PreformCameraRotation(xRot, yRot);

        if (m_PlayerManager.m_State == -1) {
            return;
        }

        int horizontal_raw = (int) Input.GetAxisRaw("Horizontal");
        int vertical_raw = (int) Input.GetAxisRaw("Vertical");
        
        if (m_PlayerManager.m_State <= 1) {
            if (horizontal_raw != 0 || vertical_raw != 0) { // 방향키 입력시 이동
                m_PlayerManager.m_State = 1;
                float dt = (float) Time.deltaTime*m_Speed / 256;
                Vector3 cam_forward = Vector3.Normalize(new Vector3(m_CameraObject.forward.x, 0, m_CameraObject.forward.z));
                Vector3 cam_right = Vector3.Cross(cam_forward, Vector3.down);
                transform.position += cam_right*horizontal_raw*dt + cam_forward*vertical_raw*dt;
                
                Quaternion rot = Quaternion.LookRotation(cam_forward);
                m_CharacterModel.rotation = rot;
            }
            else {
                m_PlayerManager.m_State = 0;
            }
        }

        transform.position = ClampPosition(transform.position);

        SendMovementDataToServer();
        SendInputDataToServer();

        //Debug.Log(m_State);
    }

    private void PreformCameraRotation(float xRot, float yRot) {
        Vector2 mouseDelta = new Vector2(xRot, yRot) * m_CameraRotationSpeed * Time.deltaTime;
        Vector3 camAngle = m_CameraObject.rotation.eulerAngles;
        camAngle += new Vector3(-mouseDelta.y, mouseDelta.x, 0);

        if (camAngle.x < 180f) {
            camAngle.x = Mathf.Clamp(camAngle.x, 0f, 70f);
            }
        else {
            camAngle.x = Mathf.Clamp(camAngle.x, 335f, 360f);
        }

        m_CameraObject.rotation = Quaternion.Euler(camAngle);
    }

    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3
        (
            Mathf.Clamp(position.x, -50f, 50f),
            position.y,
            Mathf.Clamp(position.z, -50f, 50f)
        );
    }
}
