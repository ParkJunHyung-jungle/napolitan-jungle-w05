using UnityEngine;

public class ButtonGuideManager : MonoBehaviour
{
    public Material red;
    public Material green;
    public Material gray;
    public MeshRenderer[] spheres;

    public void SetGuide(ButtonStatus[] statusList)
    {
        for (int i = 0; i < 16; i++)
        {
            switch (statusList[i])
            {
                case ButtonStatus.Deactivate:
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
