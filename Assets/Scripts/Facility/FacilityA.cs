using UnityEngine;

public class FacilityA : Facility
{
    [SerializeField]
    private FacilityButton[] facilityButtons;
    [SerializeField]
    private ButtonGuideManager buttonGuideManager;

    [Header("Observation")] 
    [SerializeField]private bool isFault;
    private ButtonStatus[] goalButtonStatuses;

    public override void Initialize()
    {
        foreach (FacilityButton facilityButton in facilityButtons)
        {
            facilityButton.Initialize();
            facilityButton.OnButtonPressed += OnButtonClicked;
        }
        
        goalButtonStatuses = new ButtonStatus[facilityButtons.Length];
        for (int i = 0; i < facilityButtons.Length; i++)
        {
            goalButtonStatuses[i] = ButtonStatus.Deactivate;
           
        }
       
        buttonGuideManager.SetGuide(goalButtonStatuses);
    }

    public override bool IsFault()
    {
        for (int i = 0; i < facilityButtons.Length; i++)
        {
            if (facilityButtons[i].Status != goalButtonStatuses[i])
            {
                return true;
            }
        }

        return false;
    }

    public override void MakeFault()
    {
        for (int i = 0; i < 16; i++)
        {
            goalButtonStatuses[i] = (ButtonStatus)Random.Range(0, 3);
        }
        buttonGuideManager.SetGuide(goalButtonStatuses);
    }

    public override void Clear()
    {
        foreach (FacilityButton facilityButton in facilityButtons)
        {
            facilityButton.Clear();
        }

        for (int i = 0; i < goalButtonStatuses.Length; i++)
        {
            goalButtonStatuses[i] = ButtonStatus.Deactivate;
        }
        buttonGuideManager.SetGuide(goalButtonStatuses);
        
    }

    private void OnButtonClicked()
    {
        OnFacilityInteracted?.Invoke(IsFault(), facilityID);
        isFault =IsFault();
    }
}