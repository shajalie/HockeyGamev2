using System;
using UnityEngine;

namespace HockeyGame.Input
{
    public class AIInputProvider : IInputProvider
    {
        public Vector2 MoveInput { get; set; }
        public Vector2 LookInput { get; set; }
        public bool ShootPressed { get; set; }
        public bool ShootHeld { get; set; }
        public bool PassPressed { get; set; }
        public bool SprintHeld { get; set; }
        public bool SwitchPlayerPressed { get; set; }

        public event Action OnShootPerformed;
        public event Action OnPassPerformed;
        public event Action OnSwitchPlayerPerformed;

        public void TriggerShoot()
        {
            ShootPressed = true;
            OnShootPerformed?.Invoke();
        }

        public void TriggerPass()
        {
            PassPressed = true;
            OnPassPerformed?.Invoke();
        }

        public void TriggerSwitchPlayer()
        {
            SwitchPlayerPressed = true;
            OnSwitchPlayerPerformed?.Invoke();
        }

        public void Reset()
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            ShootPressed = false;
            ShootHeld = false;
            PassPressed = false;
            SprintHeld = false;
            SwitchPlayerPressed = false;
        }
    }
}
