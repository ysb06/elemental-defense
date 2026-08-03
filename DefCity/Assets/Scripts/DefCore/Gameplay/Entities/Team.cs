using UnityEngine;

namespace DefCore.Gameplay.Entities
{
    public class Team : MonoBehaviour
    {
        [SerializeField] private int teamId;

        public int TeamId => teamId;
        public string TeamName => gameObject.name;

        /// <summary>
        /// 현재는 단순 Team ID 비교로 적인지 플레이어인지 판단함. 추후 동맹과 같은 개념이 생기면 이 부분을 확장해야 함.
        /// </summary>
        /// <param name="other">비교할 엔티티</param>
        /// <returns>동맹 여부</returns>
        public bool IsAlliedWith(Team other)
        {
            if (other == null)
            {
                return false;
            }

            return other != null && other.TeamId == teamId;
        }
    }
}
