using System;
using System.Collections.Generic;
using System.Numerics;

public interface IClientInput
{
    public int SeqNum { get; set; }
    public long Timestamp { get; set; }
    public Vector3 FacingDirection { get; set; }
}

public class MoveInput : IClientInput
{
    public int SeqNum { get; set; }
    public long Timestamp { get; set; }
    public Vector3 FacingDirection { get; set; }

    public Vector3 deltaPos;
    public Vector2 inputVector;

    public MoveInput(long timestamp, Vector3 facingDirection, Vector3 deltaPos, Vector2 inputVector)
    {
        Timestamp = timestamp;
        FacingDirection = facingDirection;
        this.deltaPos = deltaPos;
        this.inputVector = inputVector;
    }
}

public class SkillInput : IClientInput
{
    public int SeqNum { get; set; }
    public long Timestamp { get; set; }
    public Vector3 FacingDirection { get; set; }

    public PlayerSkill playerSkill;

    public SkillInput(long timestamp, Vector3 facingDirection, PlayerSkill playerSkill)
    {
        Timestamp = timestamp;
        FacingDirection = facingDirection;
        this.playerSkill = playerSkill;
    }
}

public enum PlayerState
{
    Dead = -1,
    Idle,
    Move,
    UsingSkill
}

public enum PlayerSkill
{
    None,
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
        public Vector2 inputVector;

        public int hitPoints_max;
        public int hitPoints;
        public PlayerState currentState;
        public PlayerSkill currentSkill;
        public PositionHistory positionHistory = new PositionHistory();

        private Dictionary<int, IClientInput> _clientInputs = new Dictionary<int, IClientInput>();

        private readonly Dictionary<PlayerSkill, int> _skillDuration = new Dictionary<PlayerSkill, int>();
        private readonly List<ScheduledTask> _scheduledTasks = new List<ScheduledTask>();

        private const float AttackRadius = 2.5f;
        private const int AttackDamage = 20;
        private int _lastSeqNum;

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
            currentState = PlayerState.Idle;

            _skillDuration.Add(PlayerSkill.Attack1, 800);
            _skillDuration.Add(PlayerSkill.Block, 1500);
            _skillDuration.Add(PlayerSkill.Roll, 1000);
        }

        public void Update() // 스레드에 의해 실행. 클라이언트로부터 패킷을 받았을 때마다가 아닌 일정 시간마다 broadcast
        {
            ExecuteSchedule();
            SimulateInput();
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

        private void SimulateInput()
        {
            while (_clientInputs.TryGetValue(_lastSeqNum, out var curInput))
            {
                ExecuteInput(curInput);
                ServerSend.UpdatePlayerPosition(_lastSeqNum, this, curInput.Timestamp);
                _clientInputs.Remove(_lastSeqNum);
                _lastSeqNum++;
            }
        }

        public void AddClientInput(IClientInput clientInput)
        {
            _clientInputs.Add(clientInput.SeqNum, clientInput);
        }

        private void ExecuteInput(IClientInput clientInput)
        {
            direction = clientInput.FacingDirection;

            switch (clientInput)
            {
                case MoveInput moveInput:
                    position = ClampPosition(position + moveInput.deltaPos);
                    deltaPos = moveInput.deltaPos;
                    inputVector = moveInput.inputVector;
                    Console.WriteLine($"[{clientInput.SeqNum}, {moveInput.Timestamp}] {position}");
                    break;

                case SkillInput skillInput:
                    ExecutePlayerSkill(skillInput);
                    break;
            }
        }

        public void ExecutePlayerSkill(SkillInput skillInput)
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Move)
            {
                currentState = PlayerState.UsingSkill;
                currentSkill = skillInput.playerSkill;
                var serverTime = Server.GetUnixTime();
                AddSchedule(ReturnToIdle, _skillDuration[skillInput.playerSkill]);
                deltaPos = Vector3.Zero;
                inputVector = Vector2.Zero;

                switch (skillInput.playerSkill)
                {
                    case PlayerSkill.Roll:
                        Console.WriteLine($"[{skillInput.SeqNum}, {skillInput.Timestamp}] {skillInput.playerSkill}: {position}, {GetRollDestination(position)}");
                        position = GetRollDestination(position);
                        break;

                    case PlayerSkill.Attack1:
                        AddSchedule(() => PlayerAttack(), 500);
                        break;
                }

                ServerSend.PlayerSkill(id, skillInput.Timestamp, currentSkill, skillInput.FacingDirection);
            }
        }

        private void ReturnToIdle()
        {
            currentState = PlayerState.Idle;
            currentSkill = PlayerSkill.None;
        }

        private bool IsBlocking(int fromId)
        {
            if (currentSkill != PlayerSkill.Block)
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

                var serverTime = Server.GetUnixTime();

                var otherPlayer = Server.clients[targetPlayerID].player;

                if (otherPlayer.positionHistory.TryGetPositionAt(serverTime, out var otherPosition) == false)
                    return;
                if (positionHistory.TryGetPositionAt(serverTime, out var myPosition) == false)
                    return;

                if (otherPosition == null || myPosition == null)
                    continue;

                float distance_squared = Vector3.DistanceSquared(myPosition, otherPosition);

                if (distance_squared < AttackRadius * AttackRadius) // 거리 계산
                {
                    if (Vector3.Dot(direction, myPosition - otherPosition) < 0) // 방향 계산
                    {
                        otherPlayer.ChangePlayerHp(id, -AttackDamage);
                    }
                }
            }
        }

        public void BroadcastPlayer()
        {
            ServerSend.UpdatePlayerPosition(-1, this, Server.GetUnixTime(), true);
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
                if (currentSkill != PlayerSkill.Roll && !IsBlocking(fromClient))
                {
                    this.hitPoints += hitPoints;
                    ServerSend.PlayerHp(this);
                }
            }

            if (this.hitPoints <= 0)
            {
                currentState = PlayerState.Dead;
                ServerSend.PlayerState(this); // 캐릭터 사망 판정
            }
        }
    }
}
