using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

public static class DialogueInputResolver
{
    private static readonly Regex PlaceholderPattern = new Regex(@"\{(?<action>[A-Za-z0-9_/]+)(:(?<part>[A-Za-z0-9_]+))?\}", RegexOptions.Compiled);
    private static FlightSchooledPlayerControls fallbackControls;

    public static string ResolvePlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return PlaceholderPattern.Replace(text, match =>
        {
            string actionName = match.Groups["action"].Value;
            string compositePart = match.Groups["part"].Success ? match.Groups["part"].Value : null;
            string resolved = GetBindingDisplayText(actionName, compositePart);
            return string.IsNullOrWhiteSpace(resolved) ? match.Value : resolved;
        });
    }

    public static bool WasDialogueAdvancePressedThisFrame()
    {
        return WasActionPressedThisFrame("PickupItem");
    }

    public static string GetBindingDisplayText(string actionName, string compositePart = null)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return string.Empty;

        PlayerInput playerInput = GetActivePlayerInput();
        InputAction action = FindAction(playerInput?.actions, actionName);
        string currentControlScheme = playerInput != null ? playerInput.currentControlScheme : null;

        if (action == null)
        {
            EnsureFallbackControls();
            action = FindAction(fallbackControls?.asset, actionName);
        }

        if (action == null)
            return actionName;

        if (TryGetBindingIndex(action, currentControlScheme, compositePart, out int bindingIndex))
        {
            string display = action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            if (!string.IsNullOrWhiteSpace(display))
                return display;
        }

        if (TryGetBindingIndex(action, "Keyboard&Mouse", compositePart, out bindingIndex))
        {
            string display = action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            if (!string.IsNullOrWhiteSpace(display))
                return display;
        }

        return actionName;
    }

    private static bool WasActionPressedThisFrame(string actionName)
    {
        PlayerInput playerInput = GetActivePlayerInput();
        InputAction action = FindAction(playerInput?.actions, actionName);

        if (action == null)
        {
            EnsureFallbackControls();
            action = FindAction(fallbackControls?.asset, actionName);
            if (fallbackControls != null)
                fallbackControls.Enable();
        }

        if (action == null)
            return false;

        if (!action.enabled)
            action.Enable();

        return action.WasPressedThisFrame();
    }

    private static PlayerInput GetActivePlayerInput()
    {
        PlayerInput[] playerInputs = Object.FindObjectsByType<PlayerInput>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (PlayerInput playerInput in playerInputs)
        {
            if (playerInput != null && playerInput.actions != null)
                return playerInput;
        }

        return null;
    }

    private static InputAction FindAction(InputActionAsset actionAsset, string actionName)
    {
        if (actionAsset == null || string.IsNullOrWhiteSpace(actionName))
            return null;

        if (actionName.Contains("/"))
            return actionAsset.FindAction(actionName, false);

        return actionAsset.FindAction($"Player/{actionName}", false) ?? actionAsset.FindAction(actionName, false);
    }

    private static bool TryGetBindingIndex(InputAction action, string preferredGroup, string compositePart, out int bindingIndex)
    {
        if (TryGetBindingIndex(action, preferredGroup, compositePart, requirePreferredGroup: true, out bindingIndex))
            return true;

        return TryGetBindingIndex(action, preferredGroup, compositePart, requirePreferredGroup: false, out bindingIndex);
    }

    private static bool TryGetBindingIndex(InputAction action, string preferredGroup, string compositePart, bool requirePreferredGroup, out int bindingIndex)
    {
        bindingIndex = -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            string effectivePath = binding.effectivePath;

            if (string.IsNullOrWhiteSpace(effectivePath))
                continue;

            if (!string.IsNullOrEmpty(compositePart))
            {
                if (!binding.isPartOfComposite || !string.Equals(binding.name, compositePart, System.StringComparison.OrdinalIgnoreCase))
                    continue;
            }
            else if (binding.isComposite || binding.isPartOfComposite)
            {
                continue;
            }

            if (requirePreferredGroup && !BindingMatchesGroup(binding, preferredGroup))
                continue;

            bindingIndex = i;
            return true;
        }

        return false;
    }

    private static bool BindingMatchesGroup(InputBinding binding, string preferredGroup)
    {
        if (string.IsNullOrWhiteSpace(preferredGroup))
            return false;

        return !string.IsNullOrWhiteSpace(binding.groups) &&
               binding.groups.IndexOf(preferredGroup, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void EnsureFallbackControls()
    {
        if (fallbackControls != null)
            return;

        fallbackControls = new FlightSchooledPlayerControls();
        InputBindingOverrides.ApplySavedOverrides(fallbackControls.asset);
    }
}
