using UnityEngine;

namespace DefCity.Presentation.UI
{
    public class ProgressBarViewController : MonoBehaviour
    {
        [SerializeField] private RectTransform barRectTransform;
        [SerializeField, Range(0f, 100f)] private float value = 100f;

        private void OnValidate()
        {
            SetValue(value);
        }

        private void Awake()
        {
            SetValue(value);
        }

        public void SetValue(float value)
        {
            this.value = Mathf.Clamp(value, 0f, 100f);

            if (barRectTransform == null)
            {
                return;
            }

            Vector2 offsetMax = barRectTransform.offsetMax;
            offsetMax.x = -50f + this.value / 2f;
            barRectTransform.offsetMax = offsetMax;
        }
    }
}
