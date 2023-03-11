using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOthers : PlayerManager
{
    protected override void Update() {
        base.Update();
        SetRotation();
    }

    private void SetRotation() {
        m_CharacterModel.rotation = Quaternion.LookRotation(direction);
    }

    public override void Start_DealDamage_Attack1() {
        return;
    }

    public override void Finish_DealDamage_Attack1() {
        return;
    }

    void FixedUpdate() // Camera
    {
        ProcessMovement();
    }

    private Vector3 ProcessMovement() { // deltaPos에 기반한 이동
        realPosition += deltaPos;
        realPosition = ClampPosition(realPosition);
        //transform.position = realPosition;

        if (deltaPos == Vector3.zero) {
            m_Movement = Vector2.zero;
        }
        return deltaPos;
    }
}
