using System;
using System.Collections;
using UnityEngine;

public class SystemTimer : MonoBehaviour
{
    public Action OnTimerEnd;

    [SerializeField] private float fatalTime = 30;
    private GameManager _gameManager;

    private bool _isActive = false;
    private float _currentTime = 0;

    
    public bool IsActive  => _isActive;
    public float CurrentTime => _currentTime;
    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void SetTimer()
    {
        _currentTime = fatalTime;
        _isActive = true;
    }
    public void Tick()
    {
        if (_isActive)
        {
            _currentTime -= Time.deltaTime;
            if( _currentTime <= 0) TimeEnd();
        }
        
        
    }

    private void TimeEnd()
    {
        _currentTime = fatalTime;
        _isActive = true;
        OnTimerEnd?.Invoke();
    }

}
