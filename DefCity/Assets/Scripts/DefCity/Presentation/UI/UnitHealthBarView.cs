using System;
using DefCore.Gameplay.Combat;
using UnityEngine;

namespace DefCity.Presentation.UI
{
    [DisallowMultipleComponent]
    public class UnitHealthBarView : MonoBehaviour
    {
        [SerializeField] private ProgressBarViewController progressBarViewController;
        [SerializeField] private Vector2 screenOffset = new(0f, 20f);

        public Health TargetDamageable => targetDamageable;
        public bool IsBound => isBound;

        private Health targetDamageable;
        private Camera targetCamera;
        private bool isBound;

        private void Awake()
        {
            if (progressBarViewController == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a ProgressBarViewController.");
            }

            SetVisible(false);
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void LateUpdate()
        {
            if (!isBound)
            {
                return;
            }

            if (targetDamageable == null)
            {
                Unbind();
                return;
            }

            UpdateScreenPosition();
        }

        public void Bind(Health damageable, Camera targetCamera)
        {
            if (damageable == null)
            {
                throw new ArgumentNullException(nameof(damageable));
            }

            if (targetCamera == null)
            {
                throw new ArgumentNullException(nameof(targetCamera));
            }

            if (!damageable.IsAlive)
            {
                throw new InvalidOperationException($"Cannot bind {name} to a dead Damageable.");
            }

            if (damageable.MaxHealth <= 0f)
            {
                throw new InvalidOperationException($"{damageable.name} must have a positive MaxHealth.");
            }

            if (damageable.DamageCollider == null)
            {
                throw new InvalidOperationException($"{damageable.name} requires a DamageCollider.");
            }

            Unbind();

            targetDamageable = damageable;
            this.targetCamera = targetCamera;
            isBound = true;

            targetDamageable.OnDamaged.AddListener(OnTargetDamaged);
            targetDamageable.OnDeath.AddListener(OnTargetDeath);

            RefreshHealth();
            UpdateScreenPosition();
        }

        public void Unbind()
        {
            if (targetDamageable != null)
            {
                targetDamageable.OnDamaged.RemoveListener(OnTargetDamaged);
                targetDamageable.OnDeath.RemoveListener(OnTargetDeath);
            }

            targetDamageable = null;
            targetCamera = null;
            isBound = false;
            SetVisible(false);
        }

        private void OnTargetDamaged(GameObject sender, DamageEventArgs args)
        {
            RefreshHealth();
        }

        private void OnTargetDeath(GameObject sender, DamageEventArgs args)
        {
            Unbind();
        }

        private void RefreshHealth()
        {
            float healthRatio = Mathf.Clamp01(
                targetDamageable.CurrentHealth / targetDamageable.MaxHealth);
            progressBarViewController.SetValue(healthRatio * 100f);
        }

        private void UpdateScreenPosition()
        {
            Bounds bounds = targetDamageable.DamageCollider.bounds;
            Vector3 anchorPosition = bounds.center + Vector3.up * bounds.extents.y;
            Vector3 screenPosition = targetCamera.WorldToScreenPoint(anchorPosition);
            screenPosition.x += screenOffset.x;
            screenPosition.y += screenOffset.y;
            bool isVisible = screenPosition.z > 0f &&
                             targetCamera.pixelRect.Contains(screenPosition);

            if (isVisible)
            {
                progressBarViewController.transform.position = screenPosition;
            }

            SetVisible(isVisible);
        }

        private void SetVisible(bool isVisible)
        {
            progressBarViewController.gameObject.SetActive(isVisible);
        }
    }
}
