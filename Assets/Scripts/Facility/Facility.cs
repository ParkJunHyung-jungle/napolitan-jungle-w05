using System;
using UnityEngine;

public abstract class Facility  : MonoBehaviour
{
    public Action OnFacilityInteracted;
    
    [SerializeField]
    protected int facilityID;

    public int FacilityID => facilityID;
    

    public abstract void Initialize();
    public abstract bool IsFault();
    public abstract void MakeFault();

    
}