using System;
using UnityEngine;

namespace HockeyGame.Input
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool ShootPressed { get; }
        bool ShootHeld { get; }
        bool PassPressed { get; }
        bool SprintHeld { get; }
        bool SwitchPlayerPressed { get; }

        event Action OnShootPerformed;
        event Action OnPassPerformed;
        event Action OnSwitchPlayerPerformed;
    }
}
