using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private SystemTimer _systemTimer;
    private float _gaugeValue;
    private bool _isRunning;
    private int _brokenStack;
    public int _hp;
    //is~per는 이 상황에서 중복되지 않게 실행될 수 있도록 플래그
    private bool _is20per;
    private bool _is40per;
    private bool _is45per;
    private bool _is60per;
    private bool _is70per;
    private bool _is80per;

    void Awake()
    {
        _gaugeValue = 0;
        _brokenStack = 0;
        _isRunning = false;
        _hp = 5;
    }

    void Update()
    {
        if(_gaugeValue == 100)
        {
            //게임 승리 결과 표시
        }

        if (_hp <= 0)
        {
            //게임 패배 결과 표시

        }

        //100까지 늘어나는데 고장이 한번일어나면 코루틴 중단후 30초 카운트
        if (!_isRunning && _brokenStack == 0)
        {
            StartCoroutine(IncreaseGauge());

        }
        if (_isRunning && _brokenStack == 1)
        {
            StopCoroutine(IncreaseGauge());
            _isRunning = false;

            //고장 스케쥴러 스크립트를 여기서 부르고 그곳에 넣어야함
            _systemTimer.LimitTimer();

        }
        if (!_isRunning && _brokenStack == 2)
        {
            //추가 사운드

        }

    }

    //0.5초당 게이지 1 증가
    IEnumerator IncreaseGauge()
    {
        while (true)
        {
            _isRunning = true;
            _gaugeValue++;
            yield return new WaitForSeconds(0.5f);
            Debug.Log(_gaugeValue);
            
        }
    }
}

