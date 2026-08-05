using UnityEngine;
using UnityEngine.UI;
using ElementalDef.Gameplay.StageMaps.Runtime;

namespace ElementalDef.Presentation.UI
{
    public class StageStartButtonController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private StageMapRuntimeController stageMapRuntimeController;

        public void OnClickStartButton()
        {
            stageMapRuntimeController.ActivateGameplay();
            startButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}