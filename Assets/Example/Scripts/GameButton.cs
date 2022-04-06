using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameButton : MonoBehaviour
{
    public enum ButtonState
    {
        Interatable,
        Disabled,
        Waiting,
    }

    public Button button { get; private set; } = null;

    public ButtonState buttonState
    {
        get { return _buttonState; }
    }
    [SerializeField]
    private ButtonState _buttonState = ButtonState.Interatable;

    [SerializeField]
    private GameLoadingSpinner loadingSpinner = null;

    [SerializeField]
    private Text mainButtonText = null;

    protected virtual void OnValidate()
    {
        Initialise();
    }

    protected virtual void Awake()
    {
        Initialise();
    }

    private void Initialise()
    {
        AssignButton();
        SetButtonState(_buttonState);
    }

    private void AssignButton()
    {
        button = GetComponentInChildren<Button>();
    }

    public void SetButtonState(ButtonState state)
    {
        _buttonState = state;
        switch (_buttonState)
        {
            case ButtonState.Interatable:
                button.interactable = true;
                button.image.raycastTarget = true;
                SetSpinnerVisibility(false);
                SetTextVisibility(true);
                break;
            case ButtonState.Disabled:
                button.interactable = false;
                button.image.raycastTarget = true;
                SetSpinnerVisibility(false);
                SetTextVisibility(true);
                break;
            case ButtonState.Waiting:
                UnityEngine.EventSystems.BaseEventData data = new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current);
                button.OnDeselect(data);
                button.interactable = true;
                button.image.raycastTarget = false;
                SetSpinnerVisibility(true);
                SetTextVisibility(false);
                break;
        }
    }

    protected virtual bool SetMainButtonText(string text)
    {
        if (mainButtonText != null)
        {
            mainButtonText.text = text;
            return true;
        }
        return false;
    }

    protected virtual void SetSpinnerVisibility(bool visible)
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.gameObject.SetActive(visible);
        }
    }

    protected virtual void SetTextVisibility(bool visible)
    {
        if (mainButtonText != null)
        {
            mainButtonText.gameObject.SetActive(visible);
        }
    }
}