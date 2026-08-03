using System.Collections.Generic;
using System.Text;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.StageMaps.Generation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Testing.Editor
{
    [CustomEditor(typeof(StageMapGeneratorTester))]
    public sealed class StageMapGeneratorTesterEditor : UnityEditor.Editor
    {
        private const float CellOverlayHeight = 0.56f;
        private const float RouteOverlayHeight = 0.64f;

        private static readonly Color BoundsColor =
            new(0.65f, 0.65f, 0.65f, 0.9f);
        private static readonly Color RoadFillColor =
            new(0.28f, 0.32f, 0.35f, 0.42f);
        private static readonly Color RoadOutlineColor =
            new(0.62f, 0.72f, 0.76f, 0.9f);
        private static readonly Color WaterColor =
            new(0.12f, 0.55f, 1f, 1f);
        private static readonly Color FireColor =
            new(1f, 0.24f, 0.08f, 1f);
        private static readonly Color EarthColor =
            new(0.18f, 0.78f, 0.3f, 1f);
        private static readonly Color HeadquartersColor =
            new(1f, 0.1f, 0.8f, 1f);
        private static readonly Color FullPathColor =
            new(0f, 0.9f, 1f, 1f);
        private static readonly Color PatternColor =
            new(1f, 0.65f, 0f, 1f);
        private static readonly Color CrossingColor =
            new(1f, 0.9f, 0.1f, 1f);
        private static readonly Color BlockedMarkColor =
            new(0.03f, 0.03f, 0.03f, 1f);

        private StageMapGeneratorTester EditTarget =>
            (StageMapGeneratorTester)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Stage Map Preview"))
            {
                EditTarget.GeneratePreview();
                Repaint();
                SceneView.RepaintAll();
            }

            using (new EditorGUI.DisabledScope(
                       !EditTarget.HasPreview &&
                       string.IsNullOrEmpty(EditTarget.LastMessage)))
            {
                if (GUILayout.Button("Clear Preview"))
                {
                    EditTarget.ClearPreview();
                    Repaint();
                    SceneView.RepaintAll();
                }
            }

            DrawGenerationStatus();
        }

        private void OnSceneGUI()
        {
            if (!EditTarget.HasPreview || EditTarget.GroundTilemap == null)
            {
                return;
            }

            GeneratedStageMap map = EditTarget.PreviewMap;
            GeneratedStageRoute route = EditTarget.PreviewRoute;
            Tilemap tilemap = EditTarget.GroundTilemap;
            GridGeometry geometry = GridGeometry.Create(tilemap);

            DrawBounds(tilemap, map.Bounds, geometry);
            DrawCells(tilemap, map, geometry);
            DrawOrderedPath(tilemap, route, geometry);
            DrawPatterns(tilemap, route, geometry);
            DrawEndpoints(tilemap, map, geometry);
            DrawCrossings(tilemap, route, geometry);
        }

        private void DrawGenerationStatus()
        {
            if (string.IsNullOrEmpty(EditTarget.LastMessage))
            {
                return;
            }

            StageMapGenerationResult result = EditTarget.LastResult;
            if (!EditTarget.HasPreview)
            {
                StringBuilder failure = new(EditTarget.LastMessage);
                if (result != null)
                {
                    failure.AppendLine();
                    failure.AppendLine(
                        $"Ground cells: {result.GroundCellCount}");
                    failure.AppendLine(
                        $"Blocked requested/placed: " +
                        $"{result.RequestedBlockedCellCount}/" +
                        $"{result.BlockedCellCount}");
                    AppendRouteStatistics(failure, result.RouteResult);
                }

                EditorGUILayout.HelpBox(
                    failure.ToString().TrimEnd(),
                    MessageType.Error);
                return;
            }

            CellStatistics statistics =
                CellStatistics.From(EditTarget.PreviewMap);
            GeneratedStageMap map = EditTarget.PreviewMap;
            GeneratedStageRoute route = EditTarget.PreviewRoute;
            StringBuilder summary = new();
            summary.AppendLine(EditTarget.LastMessage);
            summary.AppendLine($"Seed: {map.Seed}");
            summary.AppendLine($"Pattern ID: {map.PatternId}");
            summary.AppendLine($"Generator: {map.GeneratorVersion}");
            summary.AppendLine(
                $"Patterns / Road / Ground: " +
                $"{route.PatternPlacements.Count} / " +
                $"{statistics.Road} / {result.GroundCellCount}");
            summary.AppendLine(
                $"Deployable: {statistics.Deployable} " +
                $"(Water {statistics.WaterDeployable}, " +
                $"Fire {statistics.FireDeployable}, " +
                $"Earth {statistics.EarthDeployable})");
            summary.AppendLine(
                $"Blocked: {statistics.Blocked} " +
                $"(Water {statistics.WaterBlocked}, " +
                $"Fire {statistics.FireBlocked}, " +
                $"Earth {statistics.EarthBlocked})");
            summary.AppendLine(
                $"Blocked requested/placed: " +
                $"{result.RequestedBlockedCellCount}/" +
                $"{result.BlockedCellCount}");
            summary.AppendLine(
                $"Validation errors: " +
                $"{result.ValidationReport.Errors.Count}");
            summary.AppendLine(
                $"Passage order: {BuildPassageOrder(route)}");
            AppendRouteStatistics(summary, result.RouteResult);

            EditorGUILayout.HelpBox(
                summary.ToString().TrimEnd(),
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "Scene overlay: blue/red/green cells are deployable Water/Fire/Earth. " +
                "Darker cells with an X keep their element but are blocked. " +
                "Gray is Road; magenta is Headquarters.",
                MessageType.None);
        }

        private void AppendRouteStatistics(
            StringBuilder text,
            StageRouteGenerationResult routeResult)
        {
            if (routeResult == null)
            {
                return;
            }

            StageRouteGenerationDiagnosticsFormatter.Append(
                text,
                routeResult,
                EditTarget.HasGenerationTiming,
                EditTarget.LastGenerationElapsedMilliseconds);
        }

        private static string BuildPassageOrder(GeneratedStageRoute route)
        {
            StringBuilder order = new("Spawn");
            for (int index = 0;
                 index < route.OrderedPatternPassages.Count;
                 index++)
            {
                order.Append(" -> #")
                    .Append(index + 1)
                    .Append(' ')
                    .Append(GetPassageDisplayName(
                        route,
                        route.OrderedPatternPassages[index]));
            }

            return order.Append(" -> Route Goal").ToString();
        }

        private static void DrawCells(
            Tilemap tilemap,
            GeneratedStageMap map,
            GridGeometry geometry)
        {
            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                GetCellColors(
                    entry.Cell,
                    out Color fill,
                    out Color outline);
                Vector3[] corners = geometry.GetCellCorners(
                    tilemap,
                    entry.Coordinates,
                    CellOverlayHeight,
                    inset: 0.06f);
                Handles.DrawSolidRectangleWithOutline(
                    corners,
                    fill,
                    outline);

                if (IsBlockedCell(entry.Cell))
                {
                    DrawBlockedMark(
                        tilemap,
                        entry.Coordinates,
                        geometry);
                }
            }
        }

        private static void GetCellColors(
            StageMapCell cell,
            out Color fill,
            out Color outline)
        {
            if (cell.Marker == StageCellMarker.Headquarters)
            {
                fill = WithAlpha(HeadquartersColor, 0.5f);
                outline = HeadquartersColor;
                return;
            }

            if (cell.Terrain == StageTerrainKind.Road)
            {
                fill = RoadFillColor;
                outline = RoadOutlineColor;
                return;
            }

            Color elementColor = GetElementColor(cell.Element);
            if (cell.Terrain == StageTerrainKind.Object)
            {
                fill = WithAlpha(
                    Color.Lerp(elementColor, Color.black, 0.48f),
                    0.72f);
                outline = WithAlpha(elementColor, 0.95f);
                return;
            }

            fill = WithAlpha(elementColor, 0.28f);
            outline = WithAlpha(elementColor, 0.88f);
        }

        private static Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Water:
                    return WaterColor;
                case ElementType.Fire:
                    return FireColor;
                case ElementType.Earth:
                    return EarthColor;
                default:
                    return Color.gray;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static bool IsBlockedCell(StageMapCell cell)
        {
            return cell.Terrain == StageTerrainKind.Object &&
                   cell.Marker == StageCellMarker.None;
        }

        private static void DrawBlockedMark(
            Tilemap tilemap,
            Vector2Int cell,
            GridGeometry geometry)
        {
            Vector3 center = geometry.GetCellCenter(
                tilemap,
                cell,
                CellOverlayHeight + 0.015f);
            Vector3 diagonalA =
                geometry.CellX * 0.27f + geometry.CellY * 0.27f;
            Vector3 diagonalB =
                geometry.CellX * 0.27f - geometry.CellY * 0.27f;

            Handles.color = BlockedMarkColor;
            Handles.DrawAAPolyLine(
                3f,
                center - diagonalA,
                center + diagonalA);
            Handles.DrawAAPolyLine(
                3f,
                center - diagonalB,
                center + diagonalB);
        }

        private static void DrawOrderedPath(
            Tilemap tilemap,
            GeneratedStageRoute route,
            GridGeometry geometry)
        {
            IReadOnlyList<Vector2Int> path = route.OrderedPath;
            if (path.Count < 2)
            {
                return;
            }

            HashSet<Vector2Int> crossingCells = new();
            foreach (RouteCrossingDefinition crossing in
                     route.DisconnectedCrossings)
            {
                crossingCells.Add(crossing.Cell);
            }

            Handles.color = FullPathColor;
            for (int index = 1; index < path.Count; index++)
            {
                Vector2Int fromCell = path[index - 1];
                Vector2Int toCell = path[index];
                Vector3 from = geometry.GetCellCenter(
                    tilemap,
                    fromCell,
                    RouteOverlayHeight);
                Vector3 to = geometry.GetCellCenter(
                    tilemap,
                    toCell,
                    RouteOverlayHeight);

                if (fromCell.x == toCell.x)
                {
                    if (crossingCells.Contains(fromCell))
                    {
                        from = Vector3.Lerp(from, to, 0.32f);
                    }

                    if (crossingCells.Contains(toCell))
                    {
                        to = Vector3.Lerp(to, from, 0.32f);
                    }
                }

                Handles.DrawAAPolyLine(4f, from, to);
            }
        }

        private static void DrawPatterns(
            Tilemap tilemap,
            GeneratedStageRoute route,
            GridGeometry geometry)
        {
            foreach (StageRoutePatternPlacement placement in
                     route.PatternPlacements)
            {
                Vector3 position = geometry.GetCellCenter(
                    tilemap,
                    placement.AnchorCell,
                    RouteOverlayHeight + 0.025f);
                Handles.color = PatternColor;
                Handles.DrawWireDisc(
                    position,
                    geometry.Normal,
                    geometry.MarkerSize * 0.58f);
                Handles.Label(
                    position + geometry.CellY.normalized *
                    geometry.MarkerSize * 0.85f,
                    GetPatternVisitLabel(route, placement));
            }
        }

        private static void DrawEndpoints(
            Tilemap tilemap,
            GeneratedStageMap map,
            GridGeometry geometry)
        {
            DrawEndpoint(
                tilemap,
                map.Spawns[0].Cell,
                Color.green,
                "Spawn",
                geometry);
            DrawEndpoint(
                tilemap,
                map.RouteGoalCell,
                Color.red,
                "Route Goal",
                geometry);
            DrawEndpoint(
                tilemap,
                map.HeadquartersCell,
                HeadquartersColor,
                "Headquarters (Blocked / Neutral)",
                geometry);
        }

        private static void DrawEndpoint(
            Tilemap tilemap,
            Vector2Int cell,
            Color color,
            string label,
            GridGeometry geometry)
        {
            Vector3 position = geometry.GetCellCenter(
                tilemap,
                cell,
                RouteOverlayHeight + 0.04f);
            Handles.color = color;
            Handles.SphereHandleCap(
                0,
                position,
                Quaternion.identity,
                geometry.MarkerSize,
                EventType.Repaint);
            Handles.Label(
                position + geometry.CellY.normalized *
                geometry.MarkerSize * 0.85f,
                label);
        }

        private static void DrawCrossings(
            Tilemap tilemap,
            GeneratedStageRoute route,
            GridGeometry geometry)
        {
            foreach (RouteCrossingDefinition crossing in
                     route.DisconnectedCrossings)
            {
                StageRoutePatternPlacement placement =
                    FindCrossingPlacement(route, crossing.Cell);
                string visitText = placement == null
                    ? ""
                    : $"Visits H#{GetPassageVisitNumber(route, placement.Passages[0].PassageId)} / " +
                      $"V#{GetPassageVisitNumber(route, placement.Passages[1].PassageId)} - ";
                Vector3 position = geometry.GetCellCenter(
                    tilemap,
                    crossing.Cell,
                    RouteOverlayHeight + 0.05f);
                Handles.color = CrossingColor;
                Handles.DrawWireDisc(
                    position,
                    geometry.Normal,
                    geometry.MarkerSize * 0.7f);
                Handles.Label(
                    position - geometry.CellY.normalized *
                    geometry.MarkerSize * 0.9f,
                    $"No turn - {visitText}Nodes H:{crossing.HorizontalNodeId} / " +
                    $"V:{crossing.VerticalNodeId}");
            }
        }

        private static string GetPatternVisitLabel(
            GeneratedStageRoute route,
            StageRoutePatternPlacement placement)
        {
            if (placement.Kind != StageRoutePatternKind.DisconnectedCross)
            {
                int visit = GetPassageVisitNumber(
                    route,
                    placement.Passages[0].PassageId);
                return $"#{visit} {placement.Slot}: {placement.Kind}";
            }

            int horizontalVisit = GetPassageVisitNumber(
                route,
                placement.Passages[0].PassageId);
            int verticalVisit = GetPassageVisitNumber(
                route,
                placement.Passages[1].PassageId);
            return $"{placement.Slot}: Cross " +
                   $"H#{horizontalVisit} / V#{verticalVisit}";
        }

        private static string GetPassageDisplayName(
            GeneratedStageRoute route,
            StageRoutePatternPassage passage)
        {
            StageRoutePatternPlacement placement =
                FindPlacement(route, passage.PlacementId);
            if (placement == null)
            {
                return passage.PassageId;
            }

            if (placement.Kind != StageRoutePatternKind.DisconnectedCross)
            {
                return $"{placement.Slot}:{placement.Kind}";
            }

            string axis = passage.Axis == StageRoutePassageAxis.Horizontal
                ? "H"
                : "V";
            return $"{placement.Slot}:Cross-{axis}";
        }

        private static int GetPassageVisitNumber(
            GeneratedStageRoute route,
            string passageId)
        {
            for (int index = 0;
                 index < route.OrderedPatternPassages.Count;
                 index++)
            {
                if (string.Equals(
                        route.OrderedPatternPassages[index].PassageId,
                        passageId,
                        System.StringComparison.Ordinal))
                {
                    return index + 1;
                }
            }

            return 0;
        }

        private static StageRoutePatternPlacement FindPlacement(
            GeneratedStageRoute route,
            string placementId)
        {
            foreach (StageRoutePatternPlacement placement in
                     route.PatternPlacements)
            {
                if (string.Equals(
                        placement.Id,
                        placementId,
                        System.StringComparison.Ordinal))
                {
                    return placement;
                }
            }

            return null;
        }

        private static StageRoutePatternPlacement FindCrossingPlacement(
            GeneratedStageRoute route,
            Vector2Int cell)
        {
            foreach (StageRoutePatternPlacement placement in
                     route.PatternPlacements)
            {
                if (placement.Kind == StageRoutePatternKind.DisconnectedCross &&
                    placement.AnchorCell == cell)
                {
                    return placement;
                }
            }

            return null;
        }

        private static void DrawBounds(
            Tilemap tilemap,
            RectInt bounds,
            GridGeometry geometry)
        {
            Vector3[] corners = geometry.GetBoundsCorners(
                tilemap,
                bounds,
                RouteOverlayHeight);
            Vector3[] closedCorners =
            {
                corners[0],
                corners[1],
                corners[2],
                corners[3],
                corners[0],
            };

            Handles.color = BoundsColor;
            Handles.DrawAAPolyLine(2f, closedCorners);
        }

        private readonly struct GridGeometry
        {
            public Vector3 CellX { get; }
            public Vector3 CellY { get; }
            public Vector3 Normal { get; }
            public float MarkerSize { get; }

            private GridGeometry(
                Vector3 cellX,
                Vector3 cellY,
                Vector3 normal)
            {
                CellX = cellX;
                CellY = cellY;
                Normal = normal;
                MarkerSize = Mathf.Min(cellX.magnitude, cellY.magnitude) * 0.32f;
            }

            public static GridGeometry Create(Tilemap tilemap)
            {
                Vector3 origin = tilemap.GetCellCenterWorld(Vector3Int.zero);
                Vector3 cellX =
                    tilemap.GetCellCenterWorld(Vector3Int.right) - origin;
                Vector3 cellY =
                    tilemap.GetCellCenterWorld(Vector3Int.up) - origin;
                Vector3 normal = Vector3.Cross(cellX, cellY).normalized;
                if (normal.sqrMagnitude <= Mathf.Epsilon)
                {
                    normal = Vector3.up;
                }

                Camera sceneCamera =
                    SceneView.currentDrawingSceneView?.camera;
                Vector3 referenceDirection = sceneCamera != null
                    ? sceneCamera.transform.position - origin
                    : Vector3.up;
                if (Vector3.Dot(normal, referenceDirection) < 0f)
                {
                    normal = -normal;
                }

                return new GridGeometry(cellX, cellY, normal);
            }

            public Vector3 GetCellCenter(
                Tilemap tilemap,
                Vector2Int cell,
                float height)
            {
                return tilemap.GetCellCenterWorld(
                           new Vector3Int(cell.x, cell.y, 0)) +
                       Normal * height;
            }

            public Vector3[] GetCellCorners(
                Tilemap tilemap,
                Vector2Int cell,
                float height,
                float inset)
            {
                Vector3 center = GetCellCenter(tilemap, cell, height);
                Vector3 halfX = CellX * (0.5f - inset);
                Vector3 halfY = CellY * (0.5f - inset);
                return new[]
                {
                    center - halfX - halfY,
                    center + halfX - halfY,
                    center + halfX + halfY,
                    center - halfX + halfY,
                };
            }

            public Vector3[] GetBoundsCorners(
                Tilemap tilemap,
                RectInt bounds,
                float height)
            {
                Vector3 minimumCenter = GetCellCenter(
                    tilemap,
                    bounds.min,
                    height);
                Vector3 halfX = CellX * 0.5f;
                Vector3 halfY = CellY * 0.5f;
                Vector3 minimumCorner = minimumCenter - halfX - halfY;
                Vector3 widthOffset = CellX * bounds.width;
                Vector3 heightOffset = CellY * bounds.height;
                return new[]
                {
                    minimumCorner,
                    minimumCorner + widthOffset,
                    minimumCorner + widthOffset + heightOffset,
                    minimumCorner + heightOffset,
                };
            }
        }

        private readonly struct CellStatistics
        {
            public int Road { get; }
            public int Deployable { get; }
            public int Blocked { get; }
            public int WaterDeployable { get; }
            public int FireDeployable { get; }
            public int EarthDeployable { get; }
            public int WaterBlocked { get; }
            public int FireBlocked { get; }
            public int EarthBlocked { get; }

            private CellStatistics(
                int road,
                int deployable,
                int blocked,
                int waterDeployable,
                int fireDeployable,
                int earthDeployable,
                int waterBlocked,
                int fireBlocked,
                int earthBlocked)
            {
                Road = road;
                Deployable = deployable;
                Blocked = blocked;
                WaterDeployable = waterDeployable;
                FireDeployable = fireDeployable;
                EarthDeployable = earthDeployable;
                WaterBlocked = waterBlocked;
                FireBlocked = fireBlocked;
                EarthBlocked = earthBlocked;
            }

            public static CellStatistics From(GeneratedStageMap map)
            {
                int road = 0;
                int deployable = 0;
                int blocked = 0;
                int waterDeployable = 0;
                int fireDeployable = 0;
                int earthDeployable = 0;
                int waterBlocked = 0;
                int fireBlocked = 0;
                int earthBlocked = 0;

                foreach (StageMapCellEntry entry in map.EnumerateCells())
                {
                    StageMapCell cell = entry.Cell;
                    if (cell.Terrain == StageTerrainKind.Road)
                    {
                        road++;
                        continue;
                    }

                    bool isDeployable = cell.IsDeployable;
                    bool isBlocked = IsBlockedCell(cell);
                    if (isDeployable)
                    {
                        deployable++;
                    }
                    else if (isBlocked)
                    {
                        blocked++;
                    }

                    switch (cell.Element)
                    {
                        case ElementType.Water when isDeployable:
                            waterDeployable++;
                            break;
                        case ElementType.Fire when isDeployable:
                            fireDeployable++;
                            break;
                        case ElementType.Earth when isDeployable:
                            earthDeployable++;
                            break;
                        case ElementType.Water when isBlocked:
                            waterBlocked++;
                            break;
                        case ElementType.Fire when isBlocked:
                            fireBlocked++;
                            break;
                        case ElementType.Earth when isBlocked:
                            earthBlocked++;
                            break;
                    }
                }

                return new CellStatistics(
                    road,
                    deployable,
                    blocked,
                    waterDeployable,
                    fireDeployable,
                    earthDeployable,
                    waterBlocked,
                    fireBlocked,
                    earthBlocked);
            }
        }
    }
}
