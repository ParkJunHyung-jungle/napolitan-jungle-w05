using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{

    public Action<bool> OnFacilityStatusChanged;

    /// <summary>스케줄대로 고장이 났을 때. 제한 시간(FatalTime)은 스케줄에 들어 있다.</summary>
    public Action<FaultSchedule> OnFaultOccurred;

    
    [SerializeField] private Facility[] _facilities;
    

    private FaultScheduler _faultScheduler;
    
    
    public FaultScheduler FaultScheduler => _faultScheduler;
    
    public Facility[] Facilities => _facilities;
    
    public int FaultCount
    {
        get
        {
            var count = 0;
            foreach(var f in _facilities)
            {
                if(f.IsFault() ) count++;
            }
            return count;
        }
    }
    
    public void Initialize(GameManager gameManager)
    {
        
        _faultScheduler = GetComponent<FaultScheduler>();

        foreach (var f in _facilities)
        {
            f.Initialize();
            f.OnFacilityInteracted += OnFacilityInteracted;
        }
        
        
        _faultScheduler.OnFaultMade += OnFaultMade;

    }
    

    // 설비가 수리를 완료했을 때만 온다 (isFault = false). 장치 초기화는 설비가 스스로 한다
    private void OnFacilityInteracted(bool isFault, int facilityID)
    {
        OnFacilityStatusChanged?.Invoke(!isFault);
    }
    
    // 스케줄에 적힌 설비를 고장 낸다. 새로 고장 난 설비가 있을 때만 알린다
    private void OnFaultMade(FaultSchedule schedule)
    {
        var faultCount = 0;
        if (schedule.FaultFacilityIDs != null)
        {
            foreach (var id in schedule.FaultFacilityIDs)
            {
                var facility = FindFacility(id);
                if (facility == null)
                {
                    Debug.LogWarning($"[FacilityManager] 고장 스케줄의 설비 ID {id}에 해당하는 설비가 없습니다.", this);
                    continue;
                }

                if (facility.IsFault()) continue;
                facility.MakeFault();
                faultCount++;
            }
        }

        if (faultCount == 0) return;

        OnFaultOccurred?.Invoke(schedule);
        OnFacilityStatusChanged?.Invoke(false);
    }

    private Facility FindFacility(int facilityID)
    {
        foreach (var f in _facilities)
        {
            if (f.FacilityID == facilityID) return f;
        }
        return null;
    }
    
}
