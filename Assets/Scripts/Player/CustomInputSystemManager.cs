using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MoreMountains.TopDownEngine;
using NotZombiller;

/// <summary>
/// Extension on the base class to allow for rebinds and switching between mouse aiming / joystick aiming
/// </summary>
public class CustomInputSystemManager : InputSystemManager
{
    protected override void OnEnable()
    {
        base.OnEnable();
        CustomRebindActionUI.OnRebindComplete += HandleRebinds;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CustomRebindActionUI.OnRebindComplete -= HandleRebinds;
    }

    private void HandleRebinds()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            InputActions.LoadBindingOverridesFromJson(rebinds);
    }


    // These two are called by the buttons in the binding tab
    // We set forceweaponmode to be true so that even in the case of forgetting to set it up in the editor
    // it still works correctly
    
    public void ChangeWeaponModeToGamepad()
    {
        ForceWeaponMode = true;
        WeaponForcedMode = WeaponAim.AimControls.SecondaryMovement;
    }

    public void ChangeWeaponModeToKeyboard()
    {
        ForceWeaponMode = true;
        WeaponForcedMode = WeaponAim.AimControls.Mouse;
    }
}