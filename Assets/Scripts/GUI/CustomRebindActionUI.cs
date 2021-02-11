using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace NotZombiller
{
    /// <summary>
    /// Since unity's sample is riddled with bugs, this is my attempt at creating a rebinding operation
    /// The binding index refers to the index of the binding in a composite group
    /// To know the specific index of a binding, check the input action asset
    /// </summary>
    public class CustomRebindActionUI : MonoBehaviour
    {
        #region Serializable Fields

        [Header("Bound Action")]
        [Tooltip("Which action does this binding reference")]
        [SerializeField] private InputActionReference actionReference;

        [Header("Composite Bindings")]
        [Tooltip("Only tick this in case of a composite binding., likely only keyboard movement requires this")]
        [SerializeField] private bool isComposite;
        [Tooltip("If the action is composite, which binding to change")]
        [SerializeField] private int bindingIndex;

        [Header("Controller Type")]
        [Tooltip("For which controller type does this binding show")]
        [SerializeField] private ControllerType controllerType;

        [Header("UI Elements")]
        [Tooltip("The text displaying the name of the action")]
        [SerializeField] private TMP_Text actionNameText;
        [Tooltip("The text displaying the current binding of the action")]
        [SerializeField] private TMP_Text bindingNameText;

        #endregion

        #region Internal Fields

        private enum ControllerType { Keyboard, Gamepad }

        // The rebinding operation
        InputActionRebindingExtensions.RebindingOperation operation;

        #endregion

        #region Events

        public static event Action OnRebindComplete;

        #endregion

        #region Monobehaviour Callbacks

        private void Start()
        {
            UpdateUIText();
        }

        private void OnDisable()
        {
            if (operation != null)
            {
                operation.Dispose();
            }
        }

        #endregion

        #region Private Methods

        // Changes the UI Text to represent the current binding
        private void UpdateUIText()
        {
            // If composite, only display the binding that correlates with the index
            if (isComposite)
            {
                bindingNameText.text = actionReference.action.bindings[bindingIndex].ToDisplayString();
            }
            else
            {
                // If it is a composite binding, the label won't be correct and needs to be put manually in the editor
                actionNameText.text = actionReference.action.name;
                foreach (InputBinding binding in actionReference.action.bindings)
                {
                    // Show only the binding for the selected controllertype
                    if (controllerType == ControllerType.Keyboard)
                    {
                        if (binding.path.Contains("Keyboard"))
                        {
                            bindingNameText.text = binding.ToDisplayString();
                        }
                    }
                    else
                    {
                        if (binding.path.Contains("Gamepad"))
                        {
                            bindingNameText.text = binding.ToDisplayString();
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        // Called by pressing the button
        public void StartRebinding()
        {
            // Disable the action, else we cannot rebind it
            actionReference.action.Disable();

            bindingNameText.text = string.Empty;

            // Configure the operation based on the controller type
            if (controllerType == ControllerType.Keyboard)
            {
                // Use binding index or not, depending on composite status
                if (isComposite)
                {
                    operation = InputActionRebindingExtensions.PerformInteractiveRebinding(actionReference.action, bindingIndex).WithBindingGroup("Keyboard");
                }
                else
                {
                    operation = InputActionRebindingExtensions.PerformInteractiveRebinding(actionReference.action).WithBindingGroup("Keyboard");
                }
            }
            else
            {
                operation = InputActionRebindingExtensions.PerformInteractiveRebinding(actionReference.action).WithBindingGroup("Gamepad");
            }

            operation.OnCancel(operation =>
            {
                actionReference.action.Enable();
                operation.Dispose();
                UpdateUIText();
            });

            operation.OnComplete(operation =>
            {
                actionReference.action.Enable();
                operation.Dispose();
                UpdateUIText();
                var rebinds = actionReference.asset.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("rebinds", rebinds);
                OnRebindComplete?.Invoke();
            });

            operation.Start();
        }

        #endregion
    }
}