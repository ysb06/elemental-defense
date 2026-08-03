using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DefCore.Gameplay.Navigation;
using ElementalDef.Gameplay.World;

namespace ElementalDef.Gameplay.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SampleAI : MonoBehaviour
    {
        [SerializeField] private UnitMovement movement;
        private void Start()
        {
            movement.MoveToCell(new Vector2Int(5, 1));
        }

    }
}
