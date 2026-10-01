using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FacilityButton : MonoBehaviour, IInteractable
{
    public Action OnButtonPressed;

    [SerializeField] private Material greenColor;
    [SerializeField] private Material redColor;
    [SerializeField] private Material grayColor;

    [SerializeField] private float animationDuration = 0.25f;

    private Animator _animator;
    private MeshRenderer mesh;

    private bool _interactable = true;

    private ButtonStatus _status;
    private IInteractable _interactableImplementation;

    public ButtonStatus Status => _status;

    public void Initialize()
    {
        _animator = GetComponent<Animator>();
        var clickable = transform.Find("Clickable");
        mesh = clickable.GetComponent<MeshRenderer>();
        _status = ButtonStatus.Deactivate;
    }


    public void Interact()
    {
        if (_interactable)
        {
            _interactable = false;
            _status = _status.Next();
            _animator.SetTrigger("ButtonPressed");
            StartCoroutine(WaitForAnimation());

            OnButtonPressed?.Invoke();
        }

    }

    private void ChangeStatus()
    {

        ChangeColor();
    }

    private void ChangeColor()
    {
        switch (_status)
        {
            case ButtonStatus.Deactivate:
                mesh.material = grayColor;
                break;
            case ButtonStatus.Red:
                mesh.material = redColor;
                break;
            case ButtonStatus.Green:
                mesh.material = greenColor;
                break;
        }
    }

    public void Clear()
    {
        _status = ButtonStatus.Deactivate;
        ChangeColor();
    }

    IEnumerator WaitForAnimation()
    {

        yield return new WaitForSeconds(animationDuration / 2);
        Managers.Sound.FacilityButtonClickSound();
        ChangeColor();
        yield return new WaitForSeconds(animationDuration / 2);
        _interactable = true;

    }
}
