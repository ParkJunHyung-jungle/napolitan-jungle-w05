using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class ButtonPanelManager : MonoBehaviour
{
    public Action<bool> OnButtonPressed;
    
    public ButtonManager[] buttonList;
    public ButtonGuideManager buttonGuideScript;

    private ButtonStatus[] GuideImage;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GuideImage = new ButtonStatus[16];

        foreach (ButtonManager button in buttonList)
        {
            button.PressCompleted += OnButtonPressCompleted;
        }

        setRandomImage();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize()
    {

    }

    public void resetButtonStatus()
    {
        for (int i = 0; i < 16; i++)
        {
            buttonList[i].status = ButtonStatus.Deactivated;
            buttonList[i].setCurrentColor();
        }
    }

    public void setRandomImage()
    {
        for (int i = 0; i < 16; i++)
        {
            GuideImage[i] = (ButtonStatus)Random.Range(0, 3);
        }
        buttonGuideScript.setGuide(GuideImage);
    }

    public bool CheckAnswer()
    {
        bool solved = true;
        for (int i = 0; i < 16; i++)
        {
            if (buttonList[i].status != GuideImage[i])
            {
                solved = false;
                break;
            }
        }
        return solved;
    }
    void OnDestroy()
    {
        foreach (ButtonManager button in buttonList)
            button.PressCompleted -= OnButtonPressCompleted;
    }

    private void OnButtonPressCompleted(ButtonManager button)
    {
        OnButtonPressed?.Invoke(CheckAnswer());

        if (CheckAnswer())
        {
            Debug.Log("정답!");
            // 성공 UI 표시나 다음 단계 진행
        }
    }
    

    public void MakeFault()
    {
        setRandomImage();

    }
}
