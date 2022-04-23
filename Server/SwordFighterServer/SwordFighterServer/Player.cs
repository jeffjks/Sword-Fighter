using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

// 서버 내 캐릭터 시뮬레이션

namespace SwordFighterServer
{
    class Player
    {
        public int id;
        public string username;

        public Vector2 movement;
        public bool[] inputs;
        public Vector3 position;
        public Vector3 direction; // 방향 벡터
        //public Quaternion rotation;
        public int state;
        public int hitPoints_max;
        public int hitPoints;

        private int idleTicks; // 스킬 사용 후 스킬 종료까지 남은 시간
        private bool invincibility;
        private bool isBlocking;

        public Player(int id, string username, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            position = spawnPosition;
            //rotation = Quaternion.Identity;
            direction = new Vector3(0, 0, 1);
            state = 0;
            hitPoints_max = 100;
            hitPoints = hitPoints_max;

            movement = new Vector2(0, 0);
            inputs = new bool[4];
        }

        public void Update() // 스레드에 의해 실행
        {
            EndOfState();
            Move();
        }

        private void EndBlocking() // Blocking으로 인한 막기 판정 종료
        {
            if (state == 2) // Blocking
            {
                if (isBlocking && idleTicks <= 0)
                {
                    isBlocking = false;
                }
            }
        }

        private void EndInvincibility() // Roll로 인한 무적 판정 종료
        {
            if (state == 5) // Roll
            {
                if (invincibility && idleTicks <= 6)
                {
                    invincibility = false;
                }
            }
        }

        private void EndOfState()
        {
            if (idleTicks > 0)
            {
                idleTicks--;

                EndBlocking();
                EndInvincibility();

                if (idleTicks == 0)
                {
                    if (state > 1) // 스킬 종료 시 state를 0으로 만들고 클라이언트에게 전달
                    {
                        state = 0;
                        ServerSend.PlayerState(this);
                    }
                }
            }
        }

        private void Move()
        {
            ServerSend.PlayerMovement(this);
            //ServerSend.PlayerRotation(this);
        }

        private void InputToState() // 클라이언트의 Input으로 스킬 사용
        {
            if (0 <= state && state <= 1 && idleTicks == 0)
            {
                if (inputs[0]) // Blocking
                {
                    state = 2;
                    idleTicks = 45;
                    isBlocking = true;
                }
                else if (inputs[1]) // Attack1
                {
                    state = 3;
                    idleTicks = 24;
                }
                else if (inputs[2]) // Attack2
                {
                    state = 4;
                    idleTicks = 24;
                }
                else if (inputs[3]) // Roll
                {
                    state = 5;
                    idleTicks = 30;
                    invincibility = true;
                }
                if (state > 1)
                {
                    ServerSend.PlayerState(this);
                    //Console.WriteLine($"State send: {state}");
                }
            }
        }

        private bool IsBlocking(int fromId)
        {
            if (!isBlocking)
            {
                return false;
            }
            Vector3 oppositeDir = position - Server.clients[fromId].player.position;
            float dot = Vector3.Dot(direction, oppositeDir);
            return (dot < 0); // 캐릭터의 방향을 계산하여 막기 판정
        }

        private bool CheckDistance(int fromId)
        {
            float distance = Vector3.Distance(Server.clients[fromId].player.position, position);
            return (distance < 2.5f);
        }

        public void SetInput(bool[] inputs)
        {
            this.inputs = inputs;
            InputToState();
        }

        public void SetMovement(Vector2 movement, Vector3 position, Vector3 direction)
        {
            this.movement = movement;
            this.position = position;
            this.direction = direction;
        }

        public void ChangePlayerHp(int fromClient, int hitPoints) // 데미지 판정
        {
            if (hitPoints <= 0)
            {
                if (!invincibility && !IsBlocking(fromClient))
                {
                    if (CheckDistance(fromClient))
                    {
                        this.hitPoints += hitPoints;
                        ServerSend.PlayerHp(this);
                    }
                }
            }

            if (this.hitPoints <= 0)
            {
                state = -1;
                ServerSend.PlayerState(this); // 캐릭터 사망 판정
            }
        }
    }
}
