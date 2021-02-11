using System;
using UnityEngine;

namespace NotZombiller
{
    // The enum defining the possible types of input
    public enum InputType { Keyboard, Gamepad }


    /// <summary>
    /// This class is responsible for keeping track of which input type the player is
    /// </summary>
    public class InputMode : MonoBehaviour
    {
        // instance for easy global access
        public static InputMode Instance { get; set; }

        // Set up static instance
        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else Destroy(this);

            TryGetPreviousSetup();
        }

        // The input type currently in use
        private InputType currentInputType;
        public InputType CurrentInputType { get { return currentInputType; } }

        public void SetInputType(InputType newType)
        {
            if (currentInputType != newType)
            {
                currentInputType = newType;
                CustomInputSystemManager inputManager = FindObjectOfType<CustomInputSystemManager>();
                if (currentInputType == InputType.Keyboard) { inputManager.ChangeWeaponModeToKeyboard(); }
                else { inputManager.ChangeWeaponModeToGamepad(); }
            }
        }

        // Remember what mode is selected
        private void TryGetPreviousSetup()
        {
            if (PlayerPrefs.HasKey("InputMode"))
            {
                try
                {
                    string inputMode = PlayerPrefs.GetString("InputMode");
                    InputType savedType = (InputType)Enum.Parse(typeof(InputType), inputMode, true);
                    if (Enum.IsDefined(typeof(InputType), savedType))
                    {
                        SetInputType(savedType);
                        InitializeBindingTabs(savedType);
                        Debug.Log($"Converted {inputMode} to {savedType}");
                    }
                    else
                    {
                        Debug.Log($"{inputMode} is not a valid member of InputType");
                    }
                }
                catch (ArgumentException)
                {
                    Debug.LogError($"Parsing is not valid");
                }
            }
        }

        // Used to set the starting tab of the binding manager
        private void InitializeBindingTabs(InputType newType)
        {
            BindingTabManager bindingTabManager = FindObjectOfType<BindingTabManager>();
            bindingTabManager.ChangeTabs(newType);
        }

        // Save what mode is selected
        private void OnDisable()
        {
            PlayerPrefs.SetString("InputMode", currentInputType.ToString());
        }
    }
}