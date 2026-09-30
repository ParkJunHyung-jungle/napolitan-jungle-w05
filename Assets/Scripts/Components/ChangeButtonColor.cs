using UnityEngine;

public class ChangeButtonColor : MonoBehaviour
{
    public Material red;
    public Material green;
    public Material gray;

    public MeshRenderer mesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void shiftColor()
    {

    }
    public void changeColor(ButtonStatus status)
    {
        switch (status)
        {
            case ButtonStatus.Deactivate:
                mesh.material = gray;
                break;
            case ButtonStatus.Red:
                mesh.material = red;
                break;
            case ButtonStatus.Green:
                mesh.material = green;
                break;
        }

    }
}
