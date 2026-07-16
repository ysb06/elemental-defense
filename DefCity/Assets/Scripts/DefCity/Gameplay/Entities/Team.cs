using UnityEngine;

namespace DefCity.Gameplay.Entities
{
    public enum TeamKind
    {
        Player,
        Enemy
    }

    public class Team : MonoBehaviour
    {
        [SerializeField] private string teamName;
        [SerializeField] private TeamKind kind;

        public string TeamName => teamName;
        public TeamKind Kind => kind;

        public bool IsAlliedWith(Team otherTeam)
        {
            return this == otherTeam;
        }
    }
}
