using System;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.World;
using UnityEngine;
using ElementalDef.Gameplay.Combat.Settings;

namespace ElementalDef.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class ElementalDamageCalculator : MonoBehaviour
    {
        [SerializeField] private Tile3DCellManager tileManager;
        [SerializeField] private TerrainModifier terrainModifier;
        [SerializeField] private ElementalAffinitySettings elementalAffinitySettings;

        private void Awake()
        {
            EnsureConfigured();
        }

        public float CalculateDamage(
            float baseAttackPower,
            float skillMultiplier,
            ElementType attackElement,
            Vector3 attackerWorldPosition,
            ElementalCombatant defender = null)
        {
            EnsureConfigured();
            EnsureNonNegativeFinite(baseAttackPower, nameof(baseAttackPower));
            EnsureNonNegativeFinite(skillMultiplier, nameof(skillMultiplier));

            if (!Enum.IsDefined(typeof(ElementType), attackElement))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackElement),
                    attackElement,
                    "Attack element must be a defined ElementType value.");
            }

            EnsureNonNegativeFinite(defender?.Defense ?? 0f, $"{nameof(defender)}.{nameof(ElementalCombatant.Defense)}");

            ElementType attackerTerrainElement = ResolveTerrainElement(attackerWorldPosition, "attacker");
            ElementType defenderTerrainElement = ResolveTerrainElement(defender?.transform.position ?? Vector3.zero, "defender");

            ElementType defenderElement = ElementType.Neutral;
            float defenderDefense = 0;
            if (defender != null)
            {
                defenderElement = defender.DefenseElement;
                defenderDefense = defender.Defense;
            }

            float affinityMultiplier = elementalAffinitySettings.GetCombatMultiplier(attackElement, defenderElement);
            float attackTerrainMultiplier = terrainModifier.GetAttackMultiplier(attackElement, attackerTerrainElement);
            float defenseTerrainMultiplier = terrainModifier.GetDefenseMultiplier(defenderElement, defenderTerrainElement);

            EnsureNonNegativeFinite(affinityMultiplier, nameof(affinityMultiplier));
            EnsureNonNegativeFinite(attackTerrainMultiplier, nameof(attackTerrainMultiplier));
            EnsureNonNegativeFinite(defenseTerrainMultiplier, nameof(defenseTerrainMultiplier));

            float outgoingDamage = baseAttackPower * skillMultiplier * affinityMultiplier * attackTerrainMultiplier;
            float effectiveDefense = defenderDefense * defenseTerrainMultiplier;

            EnsureNonNegativeFinite(outgoingDamage, nameof(outgoingDamage));
            EnsureNonNegativeFinite(effectiveDefense, nameof(effectiveDefense));

            return Mathf.Max(0f, outgoingDamage - effectiveDefense);
        }

        private ElementType ResolveTerrainElement(Vector3 worldPosition, string participantRole)
        {
            if (!tileManager.TryGetCell(worldPosition, out CellRef cell))
            {
                throw new InvalidOperationException(
                    $"[{name}] Cannot calculate elemental damage because the {participantRole} at " +
                    $"world position {worldPosition} is not on a valid cell in '{tileManager.name}'.");
            }

            GameObject tileInstance = tileManager.GetTileInstance(cell.RefCoordinates);
            if (tileInstance == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] Cannot calculate elemental damage because cell {cell.Coordinates} for the " +
                    $"{participantRole} has no instantiated tile GameObject.");
            }

            ElementalTile elementalTile = tileInstance.GetComponent<ElementalTile>();
            if (elementalTile == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] Cannot calculate elemental damage because tile '{tileInstance.name}' at " +
                    $"cell {cell.Coordinates} for the {participantRole} has no {nameof(ElementalTile)} component.");
            }

            if (!Enum.IsDefined(typeof(ElementType), elementalTile.ElementType))
            {
                throw new InvalidOperationException(
                    $"[{name}] Tile '{tileInstance.name}' at cell {cell.Coordinates} has an undefined " +
                    $"{nameof(ElementType)} value: {(int)elementalTile.ElementType}.");
            }

            return elementalTile.ElementType;
        }

        private void EnsureConfigured()
        {
            if (tileManager == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalDamageCalculator)} requires a {nameof(Tile3DCellManager)} reference.");
            }

            if (terrainModifier == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalDamageCalculator)} requires a {nameof(TerrainModifier)} reference.");
            }

            if (elementalAffinitySettings == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalDamageCalculator)} requires an {nameof(ElementalAffinitySettings)} reference.");
            }
        }

        private static void EnsureNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Elemental damage inputs and multipliers must be finite, non-negative values.");
            }
        }
    }
}
