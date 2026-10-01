using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "GameInfo", menuName = "Scriptable Objects/GameInfo")]
public class GameInfo : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int time;
        [TextArea(6, 10)]
        public string rule;
    }

    [SerializeField]
    private float _secondsPerDay;
    [SerializeField]
    private float _startTime = 0f;
    [SerializeField]
    private float _endTime = 6f;
    [SerializeField]
    private List<Entry> _instructions = new();

    public float SecondsPerDay => _secondsPerDay;
    public float StartTime => _startTime;
    public float EndTime => _endTime;
    public List<Entry> Instructions => _instructions;

    public string GetInstruction(int currentMinute)
    {
        foreach (var entry in _instructions)
        {
            if (entry.time == currentMinute)
            {
                if (entry.time == 240)
                {
                    string hardCodedRule = entry.rule;
                    if (Managers.Game.IsLeverPulledAtTwo)
                        hardCodedRule = hardCodedRule.Replace("<s color=#8B0000>왜 안 당겼어?</s>", "고마워");
                    return hardCodedRule;
                }

                return entry.rule;
            }
        }
        return string.Empty;
    }
}
