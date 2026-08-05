using System;
using System.Collections.Generic;
using System.Text;
using ElementalDef.Gameplay.StageMaps.Generation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Testing.Editor
{
    internal static class StageRouteGenerationDiagnosticsFormatter
    {
        public static void Append(
            StringBuilder text,
            StageRouteGenerationResult result,
            bool hasElapsedTime,
            double elapsedMilliseconds)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (result == null)
            {
                return;
            }

            if (hasElapsedTime)
            {
                text.AppendLine(
                    $"Generation time: {elapsedMilliseconds:F2} ms");
            }

            StageRouteGenerationDiagnostics diagnostics = result.Diagnostics;
            if (diagnostics.HasPreferredPatternComposition)
            {
                text.AppendLine(
                    $"Equal-probability target mix: " +
                    $"Straight {diagnostics.PreferredStraightPatternCount} / " +
                    $"Corner {diagnostics.PreferredCornerPatternCount} / " +
                    $"Cross {diagnostics.PreferredCrossPatternCount}");
            }

            if (!result.Succeeded)
            {
                text.AppendLine($"Route failure: {result.FailureReason}");
            }
            else
            {
                AppendSelectedPatternMix(
                    text,
                    result.Route.PatternPlacements,
                    diagnostics);
            }

            text.AppendLine(
                $"Physical layouts: {diagnostics.AcceptedPhysicalLayoutCount} " +
                $"accepted / {diagnostics.PhysicalLayoutDrawCount} draws " +
                $"({diagnostics.PhysicalPlacementRejectedCount} rejected, " +
                $"{diagnostics.DuplicatePhysicalLayoutCount} duplicate, " +
                $"{diagnostics.UnselectedPhysicalLayoutCount} unselected)");
            text.AppendLine(
                $"Passage variants: " +
                $"{diagnostics.AcceptedPassageOrderVariantCount} accepted / " +
                $"{diagnostics.PassageOrderDrawCount} draws " +
                $"({diagnostics.DuplicatePassageOrderVariantCount} duplicate, " +
                $"{diagnostics.LayoutsWithoutValidOrderCount} layout(s) without order)");
            text.AppendLine(
                $"Candidates: {diagnostics.GeneratedCandidateCount} generated / " +
                $"{diagnostics.CandidatesAttempted} attempted / " +
                $"{diagnostics.CandidatesSearched} searched / " +
                $"{diagnostics.CandidatesNotAttempted} not attempted");
            text.AppendLine(
                $"Prevalidation rejects: " +
                $"{diagnostics.PrevalidationRejectedCandidateCount} " +
                $"(entry {GetRejectionCount(diagnostics, StageRouteCandidateRejectionReason.EntryPortUnavailable)}, " +
                $"exit {GetRejectionCount(diagnostics, StageRouteCandidateRejectionReason.ExitPortUnavailable)}, " +
                $"fixed {GetRejectionCount(diagnostics, StageRouteCandidateRejectionReason.FixedPassageConflict)}, " +
                $"reachability {GetRejectionCount(diagnostics, StageRouteCandidateRejectionReason.ResidualConnectivityUnavailable)}, " +
                $"capacity {GetRejectionCount(diagnostics, StageRouteCandidateRejectionReason.InsufficientResidualCells)})");
            text.AppendLine(
                $"Search outcomes: {diagnostics.PathNotFoundCandidateCount} no path / " +
                $"{diagnostics.PerCandidateBudgetExceededCount} candidate limit / " +
                $"{(diagnostics.TotalSearchBudgetExceeded ? "yes" : "no")} total limit");
            text.AppendLine(
                $"Search limits: " +
                $"work {GetSearchLimitCount(diagnostics, StageRouteSearchLimitKind.PerCandidateWork)}, " +
                $"open set {GetSearchLimitCount(diagnostics, StageRouteSearchLimitKind.OpenSetCapacity)}, " +
                $"alternatives {GetSearchLimitCount(diagnostics, StageRouteSearchLimitKind.ConnectorAlternativeCount)}");
            text.AppendLine(
                $"Work units: {diagnostics.TotalWorkUnits} total / " +
                $"{diagnostics.MaximumCandidateWorkUnits} max candidate");
            text.AppendLine(
                $"A* / connectors / backtracks: " +
                $"{diagnostics.AStarNodeExpansionCount} / " +
                $"{diagnostics.ConnectorAlternativeCount} / " +
                $"{diagnostics.BacktrackCount}");
            text.AppendLine(
                $"Reachability: {diagnostics.ReachabilityCheckCount} checks / " +
                $"{diagnostics.ReachabilityVisitedCellCount} visited cells");

            if (diagnostics.SelectedCandidateIndex >= 0)
            {
                text.Append(
                    $"Selected: candidate {diagnostics.SelectedCandidateIndex}, " +
                    $"physical layout {diagnostics.SelectedPhysicalLayoutIndex}, " +
                    $"variant {diagnostics.SelectedVariantIndex}");
            }
        }

        private static int GetRejectionCount(
            StageRouteGenerationDiagnostics diagnostics,
            StageRouteCandidateRejectionReason reason)
        {
            return diagnostics.GetPrevalidationRejectionCount(reason);
        }

        private static int GetSearchLimitCount(
            StageRouteGenerationDiagnostics diagnostics,
            StageRouteSearchLimitKind limitKind)
        {
            return diagnostics.GetSearchLimitCount(limitKind);
        }

        private static void AppendSelectedPatternMix(
            StringBuilder text,
            IReadOnlyList<StageRoutePatternPlacement> placements,
            StageRouteGenerationDiagnostics diagnostics)
        {
            int straightCount = 0;
            int cornerCount = 0;
            int crossCount = 0;
            for (int index = 0; index < placements.Count; index++)
            {
                switch (placements[index].Kind)
                {
                    case StageRoutePatternKind.Straight:
                        straightCount++;
                        break;
                    case StageRoutePatternKind.Corner:
                        cornerCount++;
                        break;
                    case StageRoutePatternKind.DisconnectedCross:
                        crossCount++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(placements),
                            placements[index].Kind,
                            "Pattern kind is not defined.");
                }
            }

            bool matchesPreferred =
                diagnostics.HasPreferredPatternComposition &&
                straightCount == diagnostics.PreferredStraightPatternCount &&
                cornerCount == diagnostics.PreferredCornerPatternCount &&
                crossCount == diagnostics.PreferredCrossPatternCount;
            string selectionKind = diagnostics.HasPreferredPatternComposition
                ? matchesPreferred
                    ? "target matched"
                    : "fallback"
                : "no target";
            text.AppendLine(
                $"Selected pattern mix: Straight {straightCount} / " +
                $"Corner {cornerCount} / Cross {crossCount} " +
                $"({selectionKind})");
        }
    }

    [CustomEditor(typeof(StageMapPathTester))]
    public sealed class StageMapPathTesterEditor : UnityEditor.Editor
    {
        private static readonly Color BoundsColor =
            new(0.65f, 0.65f, 0.65f, 0.8f);
        private static readonly Color FullPathColor =
            new(0f, 0.75f, 1f, 0.9f);
        private static readonly Color PatternPathColor =
            new(1f, 0.65f, 0f, 1f);
        private static readonly Color CrossUnderpassColor =
            new(0.55f, 0.25f, 1f, 1f);
        private static readonly Color HeadquartersColor =
            new(1f, 0.1f, 0.8f, 1f);
        private static readonly Color CrossingColor =
            new(1f, 0.9f, 0.1f, 1f);

        private StageMapPathTester EditTarget =>
            (StageMapPathTester)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Path Preview"))
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

            GeneratedStageRoute route = EditTarget.PreviewRoute;
            Tilemap tilemap = EditTarget.GroundTilemap;

            DrawBounds(tilemap, route.Bounds);
            DrawOrderedPath(tilemap, route, FullPathColor, 5f);

            foreach (StageRoutePatternPlacement placement in
                     route.PatternPlacements)
            {
                DrawPattern(tilemap, route, placement);
            }

            DrawEndpoint(
                tilemap,
                route.Spawn.Cell,
                Color.green,
                "Spawn");
            DrawEndpoint(
                tilemap,
                route.RouteGoalCell,
                Color.red,
                "Route Goal");
            DrawHeadquartersFootprint(
                tilemap,
                route.HeadquartersFootprint);

            foreach (RouteCrossingDefinition crossing in
                     route.DisconnectedCrossings)
            {
                DrawCrossing(tilemap, route, crossing);
            }
        }

        private void DrawGenerationStatus()
        {
            if (string.IsNullOrEmpty(EditTarget.LastMessage))
            {
                return;
            }

            if (!EditTarget.HasPreview)
            {
                StringBuilder failureSummary = new(EditTarget.LastMessage);
                if (EditTarget.LastResult != null)
                {
                    failureSummary.AppendLine();
                    AppendGenerationStatistics(
                        failureSummary,
                        EditTarget.LastResult);
                }

                EditorGUILayout.HelpBox(
                    failureSummary.ToString().TrimEnd(),
                    MessageType.Error);
                return;
            }

            GeneratedStageRoute route = EditTarget.PreviewRoute;
            StageRouteGenerationResult result = EditTarget.LastResult;
            StringBuilder summary = new();
            summary.AppendLine(EditTarget.LastMessage);
            summary.AppendLine($"Seed: {route.Seed}");
            summary.AppendLine($"Pattern ID: {route.PatternId}");
            summary.AppendLine(
                $"Patterns: {route.PatternPlacements.Count}");
            summary.AppendLine($"Road cells: {route.RoadCells.Count}");
            summary.AppendLine(
                $"Crossings: {route.DisconnectedCrossings.Count}");
            summary.AppendLine(
                $"Headquarters: origin {route.HeadquartersFootprint.position}, " +
                $"size {route.HeadquartersFootprint.size}");
            summary.AppendLine(
                $"Passage order: {BuildPassageOrder(route)}");
            AppendGenerationStatistics(summary, result);

            EditorGUILayout.HelpBox(
                summary.ToString().TrimEnd(),
                MessageType.Info);
        }

        private void AppendGenerationStatistics(
            StringBuilder text,
            StageRouteGenerationResult result)
        {
            StageRouteGenerationDiagnosticsFormatter.Append(
                text,
                result,
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

        private static void DrawBounds(Tilemap tilemap, RectInt bounds)
        {
            Vector3[] corners =
            {
                GetCellCorner(tilemap, bounds.xMin, bounds.yMin),
                GetCellCorner(tilemap, bounds.xMax, bounds.yMin),
                GetCellCorner(tilemap, bounds.xMax, bounds.yMax),
                GetCellCorner(tilemap, bounds.xMin, bounds.yMax),
                GetCellCorner(tilemap, bounds.xMin, bounds.yMin),
            };

            Handles.color = BoundsColor;
            Handles.DrawAAPolyLine(2f, corners);
        }

        private static void DrawPattern(
            Tilemap tilemap,
            GeneratedStageRoute route,
            StageRoutePatternPlacement placement)
        {
            if (placement.Kind == StageRoutePatternKind.DisconnectedCross)
            {
                DrawDisconnectedCrossPattern(tilemap, route, placement);
                return;
            }

            foreach (StageRoutePatternPassage passage in placement.Passages)
            {
                DrawPath(
                    tilemap,
                    passage.Cells,
                    PatternPathColor,
                    9f);
            }

            Vector3 anchorPosition = GetCellTop(tilemap, placement.AnchorCell);
            Handles.color = PatternPathColor;
            Handles.SphereHandleCap(
                0,
                anchorPosition,
                Quaternion.identity,
                0.2f,
                EventType.Repaint);
            Handles.Label(
                anchorPosition + Vector3.up * 0.2f,
                $"#{GetPassageVisitNumber(route, placement.Passages[0].PassageId)} " +
                $"{placement.Slot}: {placement.Kind}");
        }

        private static void DrawDisconnectedCrossPattern(
            Tilemap tilemap,
            GeneratedStageRoute route,
            StageRoutePatternPlacement placement)
        {
            StageRoutePatternPassage horizontal = placement.Passages[0];
            StageRoutePatternPassage vertical = placement.Passages[1];
            DrawPath(
                tilemap,
                horizontal.Cells,
                PatternPathColor,
                9f);
            DrawGappedPath(
                tilemap,
                vertical.Cells,
                placement.AnchorCell,
                CrossUnderpassColor,
                9f);

            Vector3 anchorPosition = GetCellTop(tilemap, placement.AnchorCell);
            int horizontalVisit = GetPassageVisitNumber(
                route,
                horizontal.PassageId);
            int verticalVisit = GetPassageVisitNumber(
                route,
                vertical.PassageId);
            Handles.Label(
                anchorPosition + Vector3.up * 0.2f,
                $"{placement.Slot}: Cross " +
                $"H#{horizontalVisit} / V#{verticalVisit}");
        }

        private static void DrawCrossing(
            Tilemap tilemap,
            GeneratedStageRoute route,
            RouteCrossingDefinition crossing)
        {
            Vector3 position = GetCellTop(tilemap, crossing.Cell);
            StageRoutePatternPlacement placement =
                FindCrossingPlacement(route, crossing.Cell);
            string visitText = placement == null
                ? ""
                : $"Visits H#{GetPassageVisitNumber(route, placement.Passages[0].PassageId)} / " +
                  $"V#{GetPassageVisitNumber(route, placement.Passages[1].PassageId)} — ";
            Handles.color = CrossingColor;
            Handles.DrawWireDisc(
                position,
                GetTilemapPlaneNormal(tilemap),
                0.3f);
            Handles.Label(
                position - Vector3.up * 0.22f,
                $"No turn — {visitText}Nodes H:{crossing.HorizontalNodeId} / " +
                $"V:{crossing.VerticalNodeId}");
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

        private static void DrawOrderedPath(
            Tilemap tilemap,
            GeneratedStageRoute route,
            Color color,
            float width)
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

            Handles.color = color;
            for (int index = 1; index < path.Count; index++)
            {
                Vector2Int fromCell = path[index - 1];
                Vector2Int toCell = path[index];
                Vector3 from = GetCellTop(tilemap, fromCell);
                Vector3 to = GetCellTop(tilemap, toCell);

                if (fromCell.x == toCell.x)
                {
                    ShortenSegmentAtGap(
                        crossingCells,
                        fromCell,
                        toCell,
                        ref from,
                        ref to);
                }

                Handles.DrawAAPolyLine(width, from, to);
            }
        }

        private static void DrawGappedPath(
            Tilemap tilemap,
            IReadOnlyList<Vector2Int> path,
            Vector2Int gapCell,
            Color color,
            float width)
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            Vector3 gapPosition = GetCellTop(tilemap, gapCell);
            Handles.color = color;
            for (int index = 1; index < path.Count; index++)
            {
                Vector2Int fromCell = path[index - 1];
                Vector2Int toCell = path[index];
                Vector3 from = GetCellTop(tilemap, fromCell);
                Vector3 to = GetCellTop(tilemap, toCell);
                ShortenSegmentAtGap(
                    gapCell,
                    gapPosition,
                    fromCell,
                    toCell,
                    ref from,
                    ref to);

                Handles.DrawAAPolyLine(width, from, to);
            }
        }

        private static void ShortenSegmentAtGap(
            HashSet<Vector2Int> gapCells,
            Vector2Int fromCell,
            Vector2Int toCell,
            ref Vector3 from,
            ref Vector3 to)
        {
            if (gapCells.Contains(fromCell))
            {
                from = Vector3.Lerp(from, to, 0.32f);
            }

            if (gapCells.Contains(toCell))
            {
                to = Vector3.Lerp(to, from, 0.32f);
            }
        }

        private static void ShortenSegmentAtGap(
            Vector2Int gapCell,
            Vector3 gapPosition,
            Vector2Int fromCell,
            Vector2Int toCell,
            ref Vector3 from,
            ref Vector3 to)
        {
            if (fromCell == gapCell)
            {
                from = Vector3.Lerp(gapPosition, to, 0.32f);
            }

            if (toCell == gapCell)
            {
                to = Vector3.Lerp(gapPosition, from, 0.32f);
            }
        }

        private static void DrawPath(
            Tilemap tilemap,
            IReadOnlyList<Vector2Int> path,
            Color color,
            float width)
        {
            if (path == null || path.Count < 2)
            {
                return;
            }

            Vector3[] points = new Vector3[path.Count];
            for (int index = 0; index < path.Count; index++)
            {
                points[index] = GetCellTop(tilemap, path[index]);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(width, points);
        }

        private static void DrawEndpoint(
            Tilemap tilemap,
            Vector2Int cell,
            Color color,
            string label)
        {
            Vector3 position = GetCellTop(tilemap, cell);
            Handles.color = color;
            Handles.SphereHandleCap(
                0,
                position,
                Quaternion.identity,
                0.32f,
                EventType.Repaint);
            Handles.Label(position + Vector3.up * 0.2f, label);
        }

        private static void DrawHeadquartersFootprint(
            Tilemap tilemap,
            RectInt footprint)
        {
            Vector3 center = Vector3.zero;
            int cellCount = 0;
            Handles.color = HeadquartersColor;
            for (int y = footprint.yMin; y < footprint.yMax; y++)
            {
                for (int x = footprint.xMin; x < footprint.xMax; x++)
                {
                    Vector3[] cellCorners =
                    {
                        GetCellCorner(tilemap, x, y),
                        GetCellCorner(tilemap, x + 1, y),
                        GetCellCorner(tilemap, x + 1, y + 1),
                        GetCellCorner(tilemap, x, y + 1),
                    };
                    Handles.DrawSolidRectangleWithOutline(
                        cellCorners,
                        new Color(
                            HeadquartersColor.r,
                            HeadquartersColor.g,
                            HeadquartersColor.b,
                            0.16f),
                        HeadquartersColor);
                    center += GetCellTop(tilemap, new Vector2Int(x, y));
                    cellCount++;
                }
            }

            if (cellCount == 0)
            {
                return;
            }

            Vector3[] outline =
            {
                GetCellCorner(tilemap, footprint.xMin, footprint.yMin),
                GetCellCorner(tilemap, footprint.xMax, footprint.yMin),
                GetCellCorner(tilemap, footprint.xMax, footprint.yMax),
                GetCellCorner(tilemap, footprint.xMin, footprint.yMax),
                GetCellCorner(tilemap, footprint.xMin, footprint.yMin),
            };
            Handles.color = HeadquartersColor;
            Handles.DrawAAPolyLine(4f, outline);

            center /= cellCount;
            Handles.SphereHandleCap(
                0,
                center,
                Quaternion.identity,
                0.32f,
                EventType.Repaint);
            Handles.Label(
                center + Vector3.up * 0.2f,
                $"Headquarters Center ({footprint.width}x{footprint.height})");
        }

        private static Vector3 GetCellTop(
            Tilemap tilemap,
            Vector2Int cell)
        {
            return tilemap.GetCellCenterWorld(
                new Vector3Int(cell.x, cell.y, 0)) + Vector3.up * 0.55f;
        }

        private static Vector3 GetCellCorner(
            Tilemap tilemap,
            int x,
            int y)
        {
            return tilemap.CellToWorld(new Vector3Int(x, y, 0)) +
                   Vector3.up * 0.55f;
        }

        private static Vector3 GetTilemapPlaneNormal(Tilemap tilemap)
        {
            Vector3 origin = tilemap.CellToWorld(Vector3Int.zero);
            Vector3 cellX = tilemap.CellToWorld(Vector3Int.right) - origin;
            Vector3 cellY = tilemap.CellToWorld(Vector3Int.up) - origin;
            Vector3 normal = Vector3.Cross(cellX, cellY).normalized;
            return normal.sqrMagnitude > 0f ? normal : Vector3.forward;
        }
    }
}
