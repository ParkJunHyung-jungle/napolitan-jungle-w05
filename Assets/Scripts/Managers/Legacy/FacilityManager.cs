using System;

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
            foreach (var f in _facilities)
            {
                if (f.IsFault()) count++;
            }
            return count;
        }
    }

    /// <summary>
    /// 전달된 gameManager의 씬에서 설비와 고장 스케줄 이벤트를 초기화한다.
    /// 각 Facility의 guideLight를 시설 ID와 함께 전역 조명 어댑터에 등록한다.
    /// </summary>
    public void Initialize(LegacyGameManager gameManager)
    {

        _faultScheduler = GetComponent<FaultScheduler>();

        foreach (Facility facility in _facilities)
        {
            facility.Initialize();
            facility.OnFacilityInteracted += OnFacilityInteracted;

            if (facility.guideLight == null) continue;

            LightController lightController = facility.guideLight.GetComponent<LightController>();
            if (lightController == null) continue;

            //lightController.BindFacility(facility.FacilityID);
        }


        _faultScheduler.OnFaultMade += OnFaultMade;

    }


    /// <summary>
    /// 설비 수리 콜백의 isFault와 facilityID를 받아 완료 상태 변경을 알린다.
    /// 설비 장치 상태는 변경하지 않고 OnFacilityStatusChanged만 호출한다.
    /// </summary>
    private void OnFacilityInteracted(bool isFault, int facilityID)
    {
        OnFacilityStatusChanged?.Invoke(!isFault);
    }

    /// <summary>
    /// schedule의 시설 ID 목록을 조회해 아직 정상인 설비에 고장을 발생시킨다.
    /// 새 고장이 하나 이상이면 고장 스케줄과 상태 변경 이벤트를 호출한다.
    /// </summary>
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

    /// <summary>
    /// facilityID와 일치하는 씬 설비를 직렬화된 배열에서 검색한다.
    /// 일치하는 Facility를 반환하고 없으면 null을 반환한다.
    /// </summary>
    private Facility FindFacility(int facilityID)
    {
        foreach (var f in _facilities)
        {
            if (f.FacilityID == facilityID) return f;
        }
        return null;
    }

}
