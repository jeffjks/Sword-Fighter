using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

public struct ClientInput
{
    public long timestamp;
    public Vector2 movementRaw;
    public Vector3 forwardDirection;
    public Vector3 deltaPos;
}

public enum PlayerSkill
{
    Dead = -1,
    Idle,
    Move,
    Block,
    Attack1,
    Attack2,
    Roll
}

// 서버 내 캐릭터 시뮬레이션

namespace SwordFighterServer
{
    public class Player
    {
        public int id;
        public string username;

        public Vector3 position;
        public Vector3 direction; // 방향 벡터
        public Vector3 deltaPos;
        public Vector2 movementRaw;

        public int hitPoints_max;
        public int hitPoints;
        public PlayerSkill state;
        public PositionHistory positionHistory = new PositionHistory();

        private Queue<ClientInput> _clientInputs = new Queue<ClientInput>();

        private ClientInput _lastClientInput;
        private Vector3 _lastPosition;

        private readonly Dictionary<PlayerSkill, int> _skillDuration = new Dictionary<PlayerSkill, int>();
        private readonly List<ScheduledTask> _scheduledTasks = new List<ScheduledTask>();

        private const float AttackRadius = 2.5f;
        private const int AttackDamage = 20;

        public class ScheduledTask
        {
            public long ExecuteAt;
            public Action Task;
        }

        public Player(int id, string username, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            position = spawnPosition;
            //rotation = Quaternion.Identity;
            direction = new Vector3(0, 0, 1);
            hitPoints_max = 100;
            hitPoints = hitPoints_max;
            state = PlayerSkill.Idle;

            _skillDuration.Add(PlayerSkill.Attack1, 800);
            _skillDuration.Add(PlayerSkill.Block, 1500);
            _skillDuration.Add(PlayerSkill.Roll, 1000);
        }

        public void Update() // 스레드에 의해 실행. 클라이언트로부터 패킷을 받았을 때마다가 아닌 일정 시간마다 broadcast
        {
            ExecuteSchedule();
            Move();
            positionHistory.RecordPosition(Server.GetUnixTime(), position);
        }

        public void AddSchedule(Action action, int delayMs)
        {
            var task = new ScheduledTask
            {
                ExecuteAt = Server.GetUnixTime() + delayMs,
                Task = action
            };
            _scheduledTasks.Add(task);
        }

        private void ExecuteSchedule()
        {
            long now = Server.GetUnixTime();

            for (int i = _scheduledTasks.Count - 1; i >= 0; i--)
            {
                if (_scheduledTasks[i].ExecuteAt <= now)
                {
                    _scheduledTasks[i].Task.Invoke();
                    _scheduledTasks.RemoveAt(i);
                }
            }
        }

        public void ExecutePlayerSkill(long timestamp, PlayerSkill playerSkill, Vector3 direction)
        {
            this.direction = direction;

            if (state == PlayerSkill.Idle || state == PlayerSkill.Move)
            {
                state = playerSkill;
                var serverTime = Server.GetUnixTime();
                AddSchedule(() => UpdateState(PlayerSkill.Idle), _skillDuration[playerSkill]);

                switch (playerSkill)
                {
                    case PlayerSkill.Roll:
                        position = GetRollDestination(position);
                        _lastPosition = position;
                        ServerSend.UpdatePlayer(id, this, timestamp);
                        break;

                    case PlayerSkill.Attack1:
                        AddSchedule(() => PlayerAttack(), 500);
                        break;
                }

                ServerSend.PlayerState(this);
            }
        }

        private void UpdateState(PlayerSkill state)
        {
            this.state = state;
            ServerSend.PlayerState(this);
        }

        private bool IsBlocking(int fromId)
        {
            if (state != PlayerSkill.Block)
            {
                return false;
            }
            Vector3 oppositeDir = position - Server.clients[fromId].player.position;
            float dot = Vector3.Dot(direction, oppositeDir);
            return (dot < 0); // 캐릭터의 방향을 계산하여 막기 판정
        }

        public void PlayerAttack() // 피격 판정
        {
            foreach (int targetPlayerID in Server.spawnedPlayers)
            {
                if (targetPlayerID == id) // 자기자신 제외
                    continue;

                var otherPlayer = Server.clients[targetPlayerID].player;

                Vector3? otherPosition = otherPlayer.positionHistory.GetPositionAt(Server.GetUnixTime());
                Vector3? myPosition = positionHistory.GetPositionAt(Server.GetUnixTime());

                if (otherPosition == null || myPosition == null)
                    continue;

                Console.WriteLine($"otherPosition: {otherPosition}");
                Console.WriteLine($"myPosition: {myPosition}");

                float distance_squared = Vector3.DistanceSquared(myPosition.Value, otherPosition.Value);

                if (distance_squared < AttackRadius * AttackRadius) // 거리 계산
                {
                    if (Vector3.Dot(direction, myPosition.Value - otherPosition.Value) < 0) // 방향 계산
                    {
                        otherPlayer.ChangePlayerHp(id, -AttackDamage);
                    }
                }
            }
        }

        private void Move()
        {
            while (_clientInputs.Count > 0)
            {
                ClientInput clientInput = _clientInputs.Dequeue();

                var deltaTime = (clientInput.timestamp - _lastClientInput.timestamp);

                if (deltaTime <= 0)
                    continue;

                position = _lastPosition + _lastClientInput.deltaPos * (deltaTime * Constants.TICKS_PER_SEC / 1000f);

                direction = clientInput.forwardDirection;
                movementRaw = clientInput.movementRaw;

                _lastPosition = position;
                _lastClientInput.timestamp = clientInput.timestamp;
                deltaPos = clientInput.deltaPos;

                ServerSend.UpdatePlayer(id, this, clientInput.timestamp);

                _lastClientInput = clientInput;
            }

            position += _lastClientInput.deltaPos;

            position = ClampPosition(position);
        }

        public void BroadcastPlayer()
        {
            ServerSend.BroadcastPlayer(this);
        }

        public void SetMovement(ClientInput clientInput, Vector3 position)
        {
            //this.position = position;
            _clientInputs.Enqueue(clientInput);
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
                if (state != PlayerSkill.Roll && !IsBlocking(fromClient))
                {
                    this.hitPoints += hitPoints;
                    ServerSend.PlayerHp(this);
                }
            }

            if (this.hitPoints <= 0)
            {
                state = PlayerSkill.Dead;
                ServerSend.PlayerState(this); // 캐릭터 사망 판정
            }
        }
    }
}
