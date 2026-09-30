using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameInfo", menuName = "Scriptable Objects/GameInfo")]
public class GameInfo : ScriptableObject
{
    [SerializeField]
    private float _secondsPerDay;
    [SerializeField]
    private float _startTime = 0f;
    [SerializeField]
    private float _endTime = 6f;

    public float SecondsPerDay => _secondsPerDay;
    public float StartTime => _startTime;
    public float EndTime => _endTime;
}
