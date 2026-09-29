using System;
using System.Collections;
using UnityEngine;

public class SystemTimer : MonoBehaviour
{
    public Action OnTimerEnd;

    [Tooltip("고장 스케줄에 제한 시간이 없을 때(0 이하) 쓰는 기본값")]
    [SerializeField] private float fatalTime = 30;
    private GameManager _gameManager;

    private bool _isActive = false;
    private float _currentTime = 0;
    private float _currentFatalTime;


    public bool IsActive => _isActive;
    public float CurrentTime => _currentTime;
    /// <summary>지금 돌고 있는(마지막으로 켠) 타이머의 제한 시간.</summary>
    public float FatalTime => _currentFatalTime;
    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        _currentFatalTime = fatalTime;
    }

    /// <summary>제한 시간(초)으로 타이머를 켠다. 0 이하면 기본값을 쓴다.</summary>
    public void SetTimer(float fatalTime)
    {
        _currentFatalTime = fatalTime > 0 ? fatalTime : this.fatalTime;
        _currentTime = _currentFatalTime;
        _isActive = true;
    }
    public void Tick()
    {
        if (_isActive)
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime <= 0) TimeEnd();
        }


    }

    private void TimeEnd()
    {
        _currentTime = _currentFatalTime;
        _isActive = true;
        OnTimerEnd?.Invoke();
    }

    public void SetTimerEnd()
    {
        _isActive = false;
        _currentTime = 0;
    }

}
