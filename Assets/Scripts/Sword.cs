using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public Collider m_SwordCollider;
    private List<int> m_DamagedPlayers = new List<int>(); // 피격된 상대 플레이어들

    public void StartDeal() { // 판정 시작
        m_SwordCollider.enabled = true;
    }

    public void FinishDeal() { // 판정 종료
        m_SwordCollider.enabled = false;
        m_DamagedPlayers.Clear();
    }

    void OnTriggerEnter(Collider other) // 상대 타격시 List에 상대 id 저장 (중복 피격 방지)
    {
        if (other.gameObject.CompareTag("Opposite")) {
            int oppositeId = other.gameObject.GetComponent<PlayerManager>().id;

            if (!m_DamagedPlayers.Contains(oppositeId)) {
                m_DamagedPlayers.Add(oppositeId);
                ClientSend.ChangeHp(-20, oppositeId); // 서버에 데미지 전달
                //Debug.Log($"Deal: {oppositeId}");
            }
        }
    }
}
