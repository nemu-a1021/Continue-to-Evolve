/*using System;
using System.Linq;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace ContinuetoEvolve.Roles;

public abstract class RoleBase : IDisposable
{
    public PlayerControl Player;
    public RoleTypes Role;
    public RoleHelper.Team Team;

    public RoleBase(PlayerControl player, RoleTypes role, RoleHelper.Team team)
    {
        Player = player;
        Role = role;
        Team = team;
    }

    public void Dispose()
    {
        Player = null;
        //GC.SuppressFinalize(this); 多分いらない
    }

    public virtual void Load()
    { }

    public virtual void OnDestroy()
    { }

    public virtual bool CheckMurderAsKiller(PlayerControl target)
    => true;

    public virtual bool CheckMurderAsTarget(PlayerControl killer)
    => true;

    public virtual void MurderPlayer(PlayerControl killer, PlayerControl target)
    { }

    public virtual void OnFixedUpdate(PlayerControl player)
    { }

    public virtual void OnReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    { }

    public virtual bool OnEnterVent(PlayerPhysics physics, int ventId)
    => true;

    public virtual void OnStartMeeting()
    { }

    public virtual void OnButtonClick()
    { }



}*/