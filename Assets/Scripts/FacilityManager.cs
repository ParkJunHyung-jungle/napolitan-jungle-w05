using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{

    public Action<bool> OnFacilityStatusChanged;

    private GameManager _gameManager;
    //일단 퍼즐 A,B,C가 프리팹이라는 전재
    [SerializeField] private List<GameObject> _facilityList = new List<GameObject>();
    [SerializeField] private ButtonPanelManager _buttonPanelManager;


    public ButtonPanelManager ButtonPanelManager => _buttonPanelManager;

    public int FaultCount
    {
        get
        {
            var count = 0;
            foreach(var f in _facilityList)
            {
                if(f.activeSelf ) count++;
            }
            return count;
        }
    }
    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;

        _buttonPanelManager.Initialize();

        _buttonPanelManager.OnButtonPressed += gameManager.OnFacilityInteracted;

    }
    //0은 하나 1은 둘 2는 셋의 고장을 일으킴
    public void MakeFault(int count)
    {
        _buttonPanelManager.MakeFault();

        ////count만큼 반복해야함
        //_gameManager._brokenStack++;
        //int randomIndex = Random.Range(0, _facilityList.Count);

        ////여긴 SetActive가 아닌 고장실행 스크립트를 부르는 걸로 바꿔야함
        //_facilityList[randomIndex].SetActive(true);

        ////중복되는 인덱스가 나올 시 다시 실행하도록 해야함
        ///

        OnFacilityStatusChanged(false);

    }

}
