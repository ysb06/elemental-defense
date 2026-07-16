using UnityEngine;

namespace DefCity.Gameplay.Entities
{
    /// <summary>
    /// 게임 내 모든 유닛과 건물의 기본 클래스입니다. 팀 정보를 포함하여, 향후 공통 속성이나 기능이 추가될 수 있습니다.
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [SerializeField] private Team team;
        public Team Team
        {
            get { return team; }
            set
            {
                if (team == null)
                    team = value;
                else
                    Debug.LogWarning($"Team is already set for {gameObject.name}. Changing team is not allowed.");
            }
        }
    }
}