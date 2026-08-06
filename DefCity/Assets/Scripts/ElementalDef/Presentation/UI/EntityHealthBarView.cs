using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class EntityHealthBarView : MonoBehaviour
    {
        private RectTransform rootRectTransform;
        private RectTransform fillRectTransform;

        public RectTransform RectTransform => rootRectTransform;

        public static EntityHealthBarView Create(
            Transform owner,
            Vector2 sizeInPixels,
            float worldUnitsPerPixel,
            float borderThicknessInPixels,
            Color backgroundColor,
            Color fillColor,
            int sortingOrder)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            GameObject root = new(
                "Entity Health Bar",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(EntityHealthBarView));
            root.layer = owner.gameObject.layer;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(owner, false);
            rootRect.sizeDelta = sizeInPixels;
            rootRect.localScale = Vector3.one * worldUnitsPerPixel;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            Image background = CreateImage("Background", rootRect, backgroundColor);
            Stretch(background.rectTransform, Vector2.zero, Vector2.zero);

            GameObject fillAreaObject = new("Fill Area", typeof(RectTransform));
            fillAreaObject.layer = root.layer;
            RectTransform fillArea = fillAreaObject.GetComponent<RectTransform>();
            fillArea.SetParent(background.rectTransform, false);
            Stretch(
                fillArea,
                Vector2.one * borderThicknessInPixels,
                -Vector2.one * borderThicknessInPixels);

            Image fill = CreateImage("Fill", fillArea, fillColor);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            EntityHealthBarView view = root.GetComponent<EntityHealthBarView>();
            view.rootRectTransform = rootRect;
            view.fillRectTransform = fillRect;
            view.SetNormalizedValue(1f);
            view.SetVisible(false);
            return view;
        }

        public void SetNormalizedValue(float normalizedValue)
        {
            if (fillRectTransform == null)
            {
                return;
            }

            if (float.IsNaN(normalizedValue) || float.IsInfinity(normalizedValue))
            {
                normalizedValue = 0f;
            }

            Vector2 anchorMax = fillRectTransform.anchorMax;
            anchorMax.x = Mathf.Clamp01(normalizedValue);
            fillRectTransform.anchorMax = anchorMax;
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            GameObject imageObject = new(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.layer = parent.gameObject.layer;

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(
            RectTransform rectTransform,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
