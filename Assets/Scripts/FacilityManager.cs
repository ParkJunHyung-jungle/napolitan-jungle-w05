using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{

    public Action<bool> OnFacilityStatusChanged;

    
    [SerializeField] private Facility[] _facilities;
    
    private GameManager _gameManager;
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
        _gameManager = gameManager;
        _faultScheduler = GetComponent<FaultScheduler>();

        foreach (var f in _facilities)
        {
            f.Initialize();
            f.OnFacilityInteracted += OnFacilityInteracted;
        }
        
        
        _faultScheduler.OnFaultMade += OnFaultMade;

    }
    

    private void OnFacilityInteracted()
    {
        
    }
    
    private void OnFaultMade(int count)
    {
        //임시 코드
        _facilities[0].MakeFault();
        
        OnFacilityStatusChanged?.Invoke(false);
    }
    
}
