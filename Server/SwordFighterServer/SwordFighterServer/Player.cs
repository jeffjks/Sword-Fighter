﻿using System;
using System.Collections.Generic;
using System.Numerics;

public struct ClientInput
{
    public long timestamp;
    public int horizontal_raw;
    public int vertical_raw;
    public Vector3 cam_forward;
    public Vector3 deltaPos;
}

// 서버 내 캐릭터 시뮬레이션

namespace SwordFighterServer
{
    class Player
    {
        public int id;
        public string username;

        public bool[] inputs;
        public Vector3 position;
        public Vector3 direction; // 방향 벡터
        public Vector3 deltaPos;
        //public Quaternion rotation;
        public int hitPoints_max;
        public int hitPoints;
        public int state;

        private Queue<ClientInput> clientInputs = new Queue<ClientInput>();

        private ClientInput lastClientInput;
        private Vector3 lastPosition;

        private LinkedList<DateTime> stateLinkedList = new LinkedList<DateTime>(); // 스킬 사용 후 종료 시점 기록용
        private int[] duration;
        private int[] input_state;

        public Player(int id, string username, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            position = spawnPosition;
            //rotation = Quaternion.Identity;
            direction = new Vector3(0, 0, 1);
            hitPoints_max = 100;
            hitPoints = hitPoints_max;
            state = 0;

            inputs = new bool[4];
            duration = new int[4] { 1500, 800, 800, 1000 };
            input_state = new int[4] { 2, 3, 4, 5 };
        }

        public void Update() // 스레드에 의해 실행. 클라이언트로부터 패킷을 받았을 때마다가 아닌 일정 시간마다 broadcast
        {
            UpdateState();
            Move();
        }

        private void UpdateState()
        {
            if (stateLinkedList.Count > 0)
            {
                if (stateLinkedList.First.Value <= DateTime.Now) { // 스킬 종료 시 state를 0으로 만들고 클라이언트에게 전달
                    state = 0;
                    stateLinkedList.RemoveFirst();
                    //stateLinkedList.RemoveAt(0);
                    ServerSend.PlayerState(this);
                }
            }
        }

        private void InputToState(long timestamp) // 클라이언트의 input을 감지하면 스킬 사용
        {
            if (0 <= state && state <= 1)
            {
                SetState(timestamp, inputs);

                if (state > 1)
                {
                    ServerSend.PlayerState(this);
                    //Console.WriteLine($"State send: {state}");
                }
            }
        }

        private void AddStateToStateLinkedList(DateTime dateTime)
        {
            var currentNode = stateLinkedList.First;

            if (currentNode == null)
            {
                stateLinkedList.AddFirst(dateTime);
                return;
            }

            while (true)
            {
                Console.WriteLine("State 0");
                DateTime currentDateTime = currentNode.Value;
                if (currentDateTime < dateTime)
                {
                    stateLinkedList.AddAfter(currentNode, dateTime);
                    return;
                }
                currentNode = currentNode.Next;

                if (currentNode == null)
                {
                    stateLinkedList.AddLast(dateTime);
                    return;
                }
            }
        }

        private void SetState(long timestamp, bool[] inputs) { // input에 따라 스킬 사용
            for (int i = 0; i < inputs.Length; ++i)
            {
                if (inputs[i])
                {
                    AddStateToStateLinkedList(DateTime.Now.AddMilliseconds(duration[i])); // 스킬 종료 시점 추가
                    //stateList.Add();
                    //stateList.Sort();
                    state = input_state[i];

                    if (i == 3) // Roll
                    {
                        position = GetRollDestination(position);
                        lastPosition = position;
                        ServerSend.UpdatePlayer(this, timestamp);
                    }
                    return;
                }
            }
        }

        private bool IsBlocking(int fromId)
        {
            if (state != 2)
            {
                return false;
            }
            Vector3 oppositeDir = position - Server.clients[fromId].player.position;
            float dot = Vector3.Dot(direction, oppositeDir);
            return (dot < 0); // 캐릭터의 방향을 계산하여 막기 판정
        }

        public void SetInput(long timestamp, bool[] inputs)
        {
            this.inputs = inputs;
            InputToState(timestamp);
        }

        private void Move()
        {
            while (clientInputs.Count > 0)
            {
                ClientInput clientInput = clientInputs.Dequeue();

                var deltaTime = (clientInput.timestamp - lastClientInput.timestamp);

                if (deltaTime <= 0)
                    continue;

                position = lastPosition + lastClientInput.deltaPos * (deltaTime * Constants.TICKS_PER_SEC / 1000f);

                lastPosition = position;
                lastClientInput.timestamp = clientInput.timestamp;
                deltaPos = clientInput.deltaPos;

                ServerSend.UpdatePlayer(this, clientInput.timestamp);

                lastClientInput = clientInput;
            }

            position += lastClientInput.deltaPos;

            position = ClampPosition(position);
        }

        public void SetMovement(ClientInput clientInput, Vector3 position, Vector3 direction)
        {
            //this.position = position;
            clientInputs.Enqueue(clientInput);
            this.direction = direction;
        }

        private Vector3 GetRollDestination(Vector3 position)
        {
            position += direction * Constants.ROLL_DISTANCE;
            return ClampPosition(position);
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            if (position.X < -50f)
            {
                position.X = -50f;
            }
            else if (position.X > 50f)
            {
                position.X = 50f;
            }
            if (position.Z < -50f)
            {
                position.Z = -50f;
            }
            else if (position.Z > 50f)
            {
                position.Z = 50f;
            }
            return position;
        }

        public void ChangePlayerHp(int fromClient, int hitPoints) // 데미지 판정 (hitPoints : 체력 변화량)
        {
            if (hitPoints <= 0)
            {
                if (state != 5 && !IsBlocking(fromClient))
                {
                    this.hitPoints += hitPoints;
                    ServerSend.PlayerHp(this);
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
