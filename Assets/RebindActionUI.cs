using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindActionUI : MonoBehaviour
{
    private readonly struct BindingEntry
    {
        public BindingEntry(string label, string actionName, string compositePart = null)
        {
            Label = label;
            ActionName = actionName;
            CompositePart = compositePart;
        }

        public string Label { get; }
        public string ActionName { get; }
        public string CompositePart { get; }
    }

    private sealed class BindingRow
    {
        public BindingEntry Entry;
        public TextMeshProUGUI BindingValueText;
    }

    private static readonly BindingEntry[] Entries =
    {
        new BindingEntry("Move Up", "Move", "up"),
        new BindingEntry("Move Down", "Move", "down"),
        new BindingEntry("Move Left", "Move", "left"),
        new BindingEntry("Move Right", "Move", "right"),
        new BindingEntry("Jump", "Jump"),
        new BindingEntry("Tail Whip", "TailWhip"),
        new BindingEntry("Ground Pound", "GroundPound"),
        new BindingEntry("Pickup Item", "PickupItem")
    };

    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private int labelFontSize = 24;

    private FlightSchooledPlayerControls controls;
    private InputActionAsset actionsAsset;
    private RectTransform root;
    private readonly List<BindingRow> rows = new List<BindingRow>();
    private TextMeshProUGUI statusText;
    private InputActionRebindingExtensions.RebindingOperation activeRebind;

    private void Awake()
    {
        controls = new FlightSchooledPlayerControls();
        actionsAsset = controls.asset;

        InputBindingOverrides.ApplySavedOverrides(actionsAsset);
        BuildUi();
        RefreshBindingTexts();
    }

    private void OnDisable()
    {
        if (actionsAsset != null)
            InputBindingOverrides.SaveOverrides(actionsAsset);
    }

    private void OnDestroy()
    {
        activeRebind?.Dispose();

        if (controls != null)
        {
            controls.Disable();
            controls.Dispose();
            controls = null;
        }
    }

    private void BuildUi()
    {
        if (root != null)
            return;

        root = CreateRect("BindingsRoot", transform as RectTransform);
        root.anchorMin = new Vector2(0.1f, 0.17f);
        root.anchorMax = new Vector2(0.9f, 0.79f);
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        var fitter = root.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (var entry in Entries)
        {
            rows.Add(CreateBindingRow(entry));
        }

        CreateResetAllButton();
        statusText = CreateLabel("StatusText", "Select a control to rebind", root, 20);
        statusText.alignment = TextAlignmentOptions.Center;
    }

    private BindingRow CreateBindingRow(BindingEntry entry)
    {
        var rowRoot = CreateRect($"{entry.ActionName}_{entry.CompositePart}_Row", root);
        var layout = rowRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        var rowLayout = rowRoot.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 48f;

        var label = CreateLabel("Label", entry.Label, rowRoot, labelFontSize);
        var labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 260f;

        var bindingText = CreateLabel("Value", "-", rowRoot, labelFontSize);
        bindingText.alignment = TextAlignmentOptions.Midline;
        var bindingLayout = bindingText.gameObject.AddComponent<LayoutElement>();
        bindingLayout.preferredWidth = 180f;

        CreateButton("Rebind", rowRoot, () => StartRebind(entry));
        CreateButton("Reset", rowRoot, () => ResetBinding(entry));

        return new BindingRow
        {
            Entry = entry,
            BindingValueText = bindingText
        };
    }

    private void CreateResetAllButton()
    {
        var buttonRoot = CreateRect("ResetAllRow", root);
        var layout = buttonRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        CreateButton("Reset All", buttonRoot, ResetAllBindings);
    }

    private void StartRebind(BindingEntry entry)
    {
        if (!TryResolveBinding(entry, out var action, out var bindingIndex))
        {
            statusText.text = $"Binding not found for {entry.Label}";
            return;
        }

        activeRebind?.Cancel();

        action.Disable();
        statusText.text = $"Press a key for {entry.Label} (Esc to cancel)";

        activeRebind = action
            .PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .WithControlsExcluding("<Gamepad>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                action.Enable();
                operation.Dispose();
                activeRebind = null;
                InputBindingOverrides.SaveOverrides(actionsAsset);
                RefreshBindingTexts();
                statusText.text = $"Updated {entry.Label}";
            })
            .OnCancel(operation =>
            {
                action.Enable();
                operation.Dispose();
                activeRebind = null;
                statusText.text = "Rebind cancelled";
            });

        activeRebind.Start();
    }

    private void ResetBinding(BindingEntry entry)
    {
        if (!TryResolveBinding(entry, out var action, out var bindingIndex))
            return;

        action.RemoveBindingOverride(bindingIndex);
        InputBindingOverrides.SaveOverrides(actionsAsset);
        RefreshBindingTexts();
        statusText.text = $"Reset {entry.Label}";
    }

    private void ResetAllBindings()
    {
        actionsAsset.RemoveAllBindingOverrides();
        InputBindingOverrides.SaveOverrides(actionsAsset);
        RefreshBindingTexts();
        statusText.text = "Reset all bindings";
    }

    private void RefreshBindingTexts()
    {
        foreach (var row in rows)
        {
            row.BindingValueText.text = GetDisplayText(row.Entry);
        }
    }

    private string GetDisplayText(BindingEntry entry)
    {
        if (!TryResolveBinding(entry, out var action, out var bindingIndex))
            return "Unassigned";

        var display = action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        return string.IsNullOrWhiteSpace(display) ? "Unassigned" : display;
    }

    private bool TryResolveBinding(BindingEntry entry, out InputAction action, out int bindingIndex)
    {
        action = actionsAsset?.FindAction($"Player/{entry.ActionName}", false);
        bindingIndex = -1;

        if (action == null)
            return false;

        for (var i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (!string.IsNullOrEmpty(entry.CompositePart))
            {
                if (!binding.isPartOfComposite || !string.Equals(binding.name, entry.CompositePart, StringComparison.OrdinalIgnoreCase))
                    continue;
            }
            else
            {
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;
            }

            if (binding.path.Contains("<Keyboard>"))
            {
                bindingIndex = i;
                return true;
            }
        }

        return false;
    }

    private Button CreateButton(string text, Transform parent, Action onClick)
    {
        var buttonRect = CreateRect($"{text}Button", parent as RectTransform);
        var buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.95f);

        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => onClick?.Invoke());

        var buttonLayout = buttonRect.gameObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = text == "Reset All" ? 220f : 130f;
        buttonLayout.preferredHeight = 42f;

        var label = CreateLabel("Text", text, buttonRect, labelFontSize - 4);
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private TextMeshProUGUI CreateLabel(string objectName, string text, Transform parent, int fontSize)
    {
        var textRect = CreateRect(objectName, parent as RectTransform);
        var textComponent = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.black;
        textComponent.faceColor = Color.black;
        textComponent.alignment = TextAlignmentOptions.MidlineLeft;

        textComponent.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        if (textComponent.font != null)
            textComponent.fontSharedMaterial = textComponent.font.material;

        return textComponent;
    }

    private static RectTransform CreateRect(string name, RectTransform parent)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        var rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }
}

public static class InputBindingOverrides
{
    public const string PlayerPrefsKey = "input_binding_overrides";

    private static bool subscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (subscribed)
            return;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        subscribed = true;

        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            ApplyToScenePlayerInputs(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
        }
    }

    public static void ApplySavedOverrides(InputActionAsset actionAsset)
    {
        if (actionAsset == null)
            return;

        var savedOverrides = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(savedOverrides))
            return;

        actionAsset.LoadBindingOverridesFromJson(savedOverrides);
    }

    public static void SaveOverrides(InputActionAsset actionAsset)
    {
        if (actionAsset == null)
            return;

        var serialized = actionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PlayerPrefsKey, serialized);
        PlayerPrefs.Save();
    }

    public static void ClearOverrides()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ApplyToScenePlayerInputs(scene);
    }

    private static void ApplyToScenePlayerInputs(UnityEngine.SceneManagement.Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var playerInputs = rootGameObject.GetComponentsInChildren<PlayerInput>(true);
            foreach (var playerInput in playerInputs)
            {
                ApplySavedOverrides(playerInput.actions);
            }
        }
    }
}
