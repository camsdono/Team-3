using UnityEngine;
using UnityEngine.UI;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace NotZombiller
{
    /// <summary>
    /// This class lives on the recticle. Its goal is to enable or disable the image of the recticle depending
    /// on the input mode (mouse/keyboard)
    /// </summary>
    public class RecticleSetter : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        // Reference to image that lives on child
        private Image image;

        private void Start()
        {
            image = GetComponentInChildren<Image>();
            if (IsInKeyboardMode()) { ToggleImage(true); }
            else ToggleImage(false);
        }

        private void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }

        // Returns true if we are using keyboard as selected input. Else returns false
        private bool IsInKeyboardMode()
        {
            if (InputMode.Instance.CurrentInputType == InputType.Gamepad) { return false; }
            else return true;
        }

        void MMEventListener<TopDownEngineEvent>.OnMMEvent(TopDownEngineEvent eventType)
        {
            if (eventType.EventType == TopDownEngineEventTypes.Pause)
            {
                ToggleImage(false);
            }
            else if (eventType.EventType == TopDownEngineEventTypes.UnPause || eventType.EventType == TopDownEngineEventTypes.LevelStart)
            {
                if (IsInKeyboardMode()) { ToggleImage(true); }
                else ToggleImage(false);
            }
        }

        // Checks if the image is null before toggling. Important for end-of-game cases
        private void ToggleImage(bool toggleOn)
        {
            if (image != null)
            {
                image.enabled = toggleOn;
            }
        }
    }
}