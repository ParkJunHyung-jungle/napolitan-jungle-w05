using UnityEngine;

public class NewLeverFacility : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        //
    }
    private bool _isDamaged;
    private DateManager _dateManager;
    private float eventTime;
    float limitTime;

    public void Start()
    {
        _dateManager = Managers.Date;
        _isDamaged = false;
        eventTime = 60;
        limitTime = eventTime + 15;
    }

    public void DoSomeThingPer1Hour()
    {
        float currentMinute = _dateManager.CurrentMinute;
        float currentDay = _dateManager.CurrentDay;
        float totalTime = ((currentDay - 1) * 1440 + currentMinute);



        if (totalTime >= eventTime)
        {
            //1시간 마다 실행 되는 것

            eventTime = eventTime + 60;
            limitTime = 0;
            limitTime = eventTime + 15;
        }
        if (totalTime >= limitTime)
        {
            //데미지를 줌

            limitTime = limitTime + 15;

        }


    }

    private void Update()
    {
        DoSomeThingPer1Hour();

    }


}

