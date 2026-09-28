using System.Collections;
using UnityEngine;
public enum ButtonStatus
{
    Deactivated,
    Red,
    Green
}

public class ButtonManager : MonoBehaviour
{
    public bool isClickable;
    public bool isAnimating;
    public ButtonStatus status;
    public Transform animatedChild;
    public Vector3 localOffset = new Vector3(0f, -0.4f, 0f);
    private Vector3 InitialPosition;
    public float animationDuration = 0.3f;
    public ChangeButtonColor changeColorScript;

    public event System.Action<ButtonManager> PressCompleted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitialPosition = animatedChild.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClickButton()
    {
        if (!isAnimating)
            StartCoroutine(MoveAndReturn());
    }

    private IEnumerator MoveAndReturn()
    {
        isAnimating = true;
        yield return MoveTo(InitialPosition + localOffset);
        status = (ButtonStatus)(((int)status + 1) % 3);
        changeColorScript.changeColor(status);
        yield return MoveTo(InitialPosition);
        isAnimating = false;
        PressCompleted?.Invoke(this);
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start = animatedChild.localPosition;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            animatedChild.localPosition = Vector3.Lerp(start, target, elapsed / animationDuration);
            yield return null;
        }
        animatedChild.localPosition = target;
    }
    public void setCurrentColor()
    {
        changeColorScript.changeColor(status);
    }
}
