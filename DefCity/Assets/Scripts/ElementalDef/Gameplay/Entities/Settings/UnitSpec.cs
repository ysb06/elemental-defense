using System;
using UnityEngine;
using ElementalDef.Gameplay.Combat;

namespace ElementalDef.Gameplay.Entities.Settings
{
    [Serializable]
    public struct AttackStats
    {
        public ElementType Element;
        public float Power;
        public float Range;
        public float Cooldown;
    }

    [Serializable]
    public struct DefenseStats
    {
        public ElementType Element;
        public float MaxHealth;
        public float Defense;
    }

    [Serializable]
    public struct ScannerStats
    {
        public float AcquisitionPadding;
        public float Interval;
    }

    [Serializable]
    public struct MovementStats
    {
        public float Speed;
        public float Acceleration;
        public float AngularSpeed;
        public float StoppingDistance;
    }

    public abstract class UnitSpec : ScriptableObject
    {
        [SerializeField] private AttackStats attack;
        [SerializeField] private DefenseStats defense;
        [SerializeField] private ScannerStats scanner;

        public AttackStats Attack => attack;
        public DefenseStats Defense => defense;
        public ScannerStats Scanner => scanner;
    }
}
