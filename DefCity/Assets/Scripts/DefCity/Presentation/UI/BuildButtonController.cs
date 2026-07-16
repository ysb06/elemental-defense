using UnityEngine;
using DefCity.Gameplay.City.Construction;
using DefCity.Gameplay.Entities;

namespace DefCity.Presentation.UI
{
    public class BuildButtonController : MonoBehaviour
    {
        [SerializeField] private Entity target;
        [SerializeField] private Builder builder;

        public void OnBuildButtonClick()
        {
            if (target != null && builder != null)
            {
                builder.BeginBuild(target);
            }
        }
    }
}