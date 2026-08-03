using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.World
{
    public class MapPatternGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2Int mapOrigin = new(1, 1);
        [SerializeField] private RectInt mapBounds = new(0, 0, 20, 20);
        [SerializeField] private RectInt routeGenerationBounds = new(0, 0, 10, 10);
    }
}