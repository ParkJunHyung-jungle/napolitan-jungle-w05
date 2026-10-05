using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepFollower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField]
    private Transform _player;
    [SerializeField, Min(0.5f)]
    private float _followDistance = 2.5f;
    [SerializeField, Min(0.05f)]
    private float _sampleSpacing = 0.2f;
    [SerializeField]
    private float _footHeightOffset = 2f;
    private readonly Queue<Vector3> _trail = new();
    private Vector3 _lastSample;
    private float _pathLength;

    [Header("Footstep")]
    [SerializeField, Min(0.1f)]
    private float _strideLength = 2.1f;
    private AudioSource _audioSource;
    private float _strideProgress;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        ResetTrail();
    }

    private void Update()
    {
        RecordPlayerPosition();
        float movedDistance = FollowTrail();
        UpdateFootstep(movedDistance);
    }

    /// <summary>
    /// 따라가는 경로를 비우고 오브젝트를 플레이어 발 위치로 옮긴다.
    /// GetFootPosition을 사용하며 _trail, _lastSample, _pathLength, _strideProgress를 초기화한다.
    /// </summary>
    private void ResetTrail()
    {
        Vector3 footPosition = GetFootPosition();
        _trail.Clear();
        transform.position = footPosition;
        _lastSample = footPosition;
        _pathLength = 0f;
        _strideProgress = 0f;
    }

    /// <summary>
    /// 플레이어 발 위치가 _sampleSpacing 이상 움직였으면 현재 발 위치를 경로에 추가한다.
    /// GetFootPosition을 사용하며 _trail, _lastSample, _pathLength를 갱신한다.
    /// </summary>
    private void RecordPlayerPosition()
    {
        Vector3 footPosition = GetFootPosition();
        float distance = Vector3.Distance(_lastSample, footPosition);
        if (distance < _sampleSpacing)
            return;

        _trail.Enqueue(footPosition);
        _lastSample = footPosition;
        _pathLength += distance;
    }

    /// <summary>
    /// 플레이어까지의 경로 길이가 _followDistance를 넘은 만큼 경로를 따라 이동한다.
    /// _trail의 지점을 순서대로 소비하고 _pathLength를 줄이며, 이번 프레임에 이동한 거리를 반환한다.
    /// </summary>
    private float FollowTrail()
    {
        float excess = _pathLength - _followDistance;
        float movedDistance = 0f;

        while (excess > 0f && _trail.Count > 0)
        {
            Vector3 next = _trail.Peek();
            float step = Mathf.Min(excess, Vector3.Distance(transform.position, next));
            transform.position = Vector3.MoveTowards(transform.position, next, step);
            if (transform.position == next)
                _trail.Dequeue();

            excess -= step;
            movedDistance += step;
        }

        _pathLength -= movedDistance;
        return movedDistance;
    }

    /// <summary>
    /// 이동 거리를 누적해 보폭에 도달할 때마다 발소리를 한 번 재생한다.
    /// movedDistance를 _strideProgress에 더하고 _strideLength를 넘으면 _audioSource로 재생한 뒤 차감한다.
    /// </summary>
    private void UpdateFootstep(float movedDistance)
    {
        _strideProgress += movedDistance;
        if (_strideProgress < _strideLength)
            return;

        _strideProgress -= _strideLength;
        //소리 재생;
    }

    /// <summary>
    /// 플레이어 위치에서 _footHeightOffset만큼 내린 발 위치를 계산한다.
    /// _player 위치를 사용하며 계산한 월드 좌표를 반환한다.
    /// </summary>
    private Vector3 GetFootPosition()
    {
        return _player.position + Vector3.down * _footHeightOffset;
    }
}
