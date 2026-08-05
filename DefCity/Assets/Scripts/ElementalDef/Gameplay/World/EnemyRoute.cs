using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.World
{
    /// <summary>
    /// 순서가 정해진 적 이동 경로입니다.
    ///
    /// 맵 제작 시 아래 조건은 검증하지 않으므로 제작자가 보장해야 합니다.
    /// - 경로에는 Entry와 Exit를 포함해 최소 2개의 셀이 있어야 합니다.
    /// - 첫 번째 셀은 Entry, 마지막 셀은 Exit이며 이동 순서대로 작성해야 합니다.
    /// - Entry와 Exit는 맵의 실제 스폰 지점과 목표 지점에 대응해야 합니다.
    /// - 모든 좌표는 동일한 Grid 기준이며 실제 맵 셀 안에 있어야 합니다.
    /// - 모든 셀은 몬스터가 이동 가능한 타일과 NavMesh 위에 있어야 합니다.
    /// - 의도하지 않은 중복 셀이나 순환 경로가 없어야 합니다.
    /// - 출구에는 플레이어 본영이 있다면 본영이 있는 마지막 셀은 추가하면 안 됩니다.
    /// </summary>
    public sealed class EnemyRoute : MonoBehaviour
    {
        [SerializeField] private List<Vector2Int> fixedPath = new();

        public int PathLength => fixedPath.Count;
        public Vector2Int EntryCell => fixedPath[0];
        public Vector2Int ExitCell => fixedPath[^1];
        public Vector2Int this[int pathIndex] => fixedPath[pathIndex];

        public void Initialize(IReadOnlyList<Vector2Int> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            fixedPath = new List<Vector2Int>(path);
        }

        public bool ContainsCell(Vector2Int coordinates)
        {
            return fixedPath.Contains(coordinates);
        }

        /// <summary>
        /// 연속한 모든 셀이 상하좌우로 한 칸씩 연결되었는지만 검사합니다.
        /// </summary>
        public bool IsPathConnected()
        {
            for (int pathIndex = 1; pathIndex < fixedPath.Count; pathIndex++)
            {
                Vector2Int offset = fixedPath[pathIndex] - fixedPath[pathIndex - 1];
                int distance = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
                if (distance != 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
