using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    private Animator animator;
    private bool isDown;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("IsDown", isDown);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact()
    {
        isDown = !isDown;
        animator.SetBool("IsDown", isDown);
    }
}
