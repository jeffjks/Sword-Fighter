using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float m_CameraRotationSpeed;

    void Update()
    {
        float xRot = Input.GetAxis("Mouse X");
        float yRot = Input.GetAxis("Mouse Y");

        PreformCameraRotation(xRot, yRot);
    }

    private void PreformCameraRotation(float xRot, float yRot) {
        Vector2 mouseDelta = new Vector2(xRot, yRot) * m_CameraRotationSpeed * Time.deltaTime;
        Vector3 camAngle = transform.rotation.eulerAngles;
        camAngle += new Vector3(-mouseDelta.y, mouseDelta.x, 0);

        if (camAngle.x < 180f) {
            camAngle.x = Mathf.Clamp(camAngle.x, 0f, 70f);
            }
        else {
            camAngle.x = Mathf.Clamp(camAngle.x, 335f, 360f);
        }

        transform.rotation = Quaternion.Euler(camAngle);
    }
}
