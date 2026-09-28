using UnityEngine;

public class ButtonGuideManager : MonoBehaviour
{
    public Material red;
    public Material green;
    public Material gray;
    public MeshRenderer[] spheres;

    //고장 내면 flag on
    private bool isFault;
    
    public bool IsFault => isFault;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setGuide(ButtonStatus[] statusList)
    {
        for (int i = 0; i < 16; i++)
        {
            switch (statusList[i])
            {
                case ButtonStatus.Deactivated:
                    spheres[i].material = gray;
                    break;
                case ButtonStatus.Red:
                    spheres[i].material = red;
                    break;
                case ButtonStatus.Green:
                    spheres[i].material = green;
                    break;

            }
        }
    }
}
