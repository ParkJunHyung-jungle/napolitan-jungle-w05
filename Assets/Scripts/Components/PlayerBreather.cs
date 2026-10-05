using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerBreather : MonoBehaviour
{
    [Header("Breath")]
    private bool _isBreathing;

    private void Awake()
    {
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.GHOSTBREATH, GetComponent<AudioSource>());
        Managers.Timeline.OnBreatherActiveChanged += SetBreathing;
    }

    /// <summary>
    /// 타임라인의 브레서 활성 여부를 받아 숨소리 반복 재생을 켜거나 끈다.
    /// isActive가 _isBreathing과 다를 때만 GhostBreathSound 또는 StopBreathSound를 호출하고 _isBreathing을 갱신한다.
    /// </summary>
    private void SetBreathing(bool isActive)
    {
        // 같은 값이 반복해서 와도 숨소리가 처음부터 다시 시작되지 않도록 상태가 바뀔 때만 처리한다.
        if (_isBreathing == isActive)
            return;

        _isBreathing = isActive;
        if (isActive)
            Managers.Sound.GhostBreathSound();
        else
            Managers.Sound.StopBreathSound();
    }
}
