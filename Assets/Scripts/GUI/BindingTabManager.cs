using UnityEngine;

namespace NotZombiller
{
    /// <summary>
    /// This is a simple class responsible for providing basic tab navigation in the keybind panel
    /// and communicating with the InputMode to send changes
    /// </summary>
    public class BindingTabManager : MonoBehaviour
    {
        [Header("GUI Elements")]
        [SerializeField] private GameObject keyboardPanel;
        [SerializeField] private GameObject gamepadPanel;


        private void ChangeInputMode(InputType tab)
        {
            // Sets the input mode
            InputMode.Instance.SetInputType(tab);

            ChangeTabs(tab);
        }

        // These two methods are called by buttons, which cant use enums as method parameters
        // So instead of using messy ints, we simply define two seperate methods

        public void ChangeToKeyboardTab()
        {
            ChangeInputMode(InputType.Keyboard);
        }

        public void ChangeToGamepadTab()
        {
            ChangeInputMode(InputType.Gamepad);
        }

        // Activates/Deactives the tab
        public void ChangeTabs(InputType tab)
        {
            switch (tab)
            {
                case InputType.Keyboard:
                    keyboardPanel.SetActive(true);
                    gamepadPanel.SetActive(false);
                    break;
                case InputType.Gamepad:
                    keyboardPanel.SetActive(false);
                    gamepadPanel.SetActive(true);
                    break;
            }
        }
    }
}