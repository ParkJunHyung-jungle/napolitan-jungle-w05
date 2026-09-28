using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FacilityButton : MonoBehaviour, IInteractable
{
    public Action OnButtonPressed;
    
    [SerializeField] private Material greenColor;
    [SerializeField] private Material redColor;
    [SerializeField] private Material grayColor;
    
    private Animator _animator;
    private MeshRenderer mesh;
    
    private ButtonStatus _status;
    private IInteractable _interactableImplementation;

    public ButtonStatus Status => _status;
    
    public void Initialize()
    {
        _animator = GetComponent<Animator>();
        mesh = GetComponent<MeshRenderer>();
        
        _status = ButtonStatus.Deactivate;
    }
    

    public void Interact()
    {
        _animator.SetTrigger("ButtonPressed");
        ChangeStatus();
        OnButtonPressed?.Invoke();
    }

    private void ChangeStatus()
    {
        _status = _status.Next();
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
}