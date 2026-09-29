using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FacilityManager : MonoBehaviour
{

    public Action<bool> OnFacilityStatusChanged;

    
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
    

    private void OnFacilityInteracted(bool isFault, int facilityID)
    {
        OnFacilityStatusChanged?.Invoke(!isFault); 
        if(!isFault) _facilities[facilityID].Clear();   
    }
    
    private void OnFaultMade(int count)
    {
        //임시 코드
        var rand = Random.Range(0, _facilities.Length);
        _facilities[rand].MakeFault();
        
        OnFacilityStatusChanged?.Invoke(false);
    }
    
}
