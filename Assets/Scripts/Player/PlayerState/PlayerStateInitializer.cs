// This is Auto Generated Code by (Editors.PlayerStateScriptMaker). Do not modify the code.
using System.Collections.Generic;
using Shared.Enums;

public static class PlayerStateInitializer
{
    public static Dictionary<PlayerState, PlayerStateBase> GetPlayerStateDictionary(PlayerManager manager)
    {
        var playerStates = new Dictionary<PlayerState, PlayerStateBase>
        {
            { PlayerState.Idle, new IdleState(manager) },
            { PlayerState.Move, new MoveState(manager) },
            { PlayerState.UsingSkill, new UsingSkillState(manager) },
            { PlayerState.Dead, new DeadState(manager) },
        };

        return playerStates;
    }
}

