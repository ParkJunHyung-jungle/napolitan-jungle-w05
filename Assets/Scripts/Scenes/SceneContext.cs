using UnityEngine;
using UnityEngine.UI;

public class SceneContext : MonoBehaviour
{
    private const float DOOR_OPEN_DRAIN_RATE = 0.25f;
    private const float LIGHT_OFF_DRAIN_RATE = 0.25f;

    [SerializeField]
    private Canvas _dateCanvas;
    [SerializeField]
    private Text _currentDay;
    [SerializeField]
    private Text _currentTime;

    void Awake()
    {
        Managers.Game.RestoreStartScreen();
        _dateCanvas.gameObject.SetActive(true);
        Managers.Game.ResetMentality();
    }

    void Start()
    {
        Managers.Sound.SubAmbientSound();
    }

    void Update()
    {
        Managers.Date.ElapsedTime += Time.deltaTime;
        SetDateUI();

        // 감소와 회복을 합쳐 한 번에 반영해 같은 프레임에 임계값을 오가며 효과음이나 게임오버가 잘못 실행되지 않게 한다.
        float mentalityRate = 0f;
        if (Managers.Timeline.IsDoorLeftOpen)
            mentalityRate -= DOOR_OPEN_DRAIN_RATE;
        if (Managers.Timeline.IsLightLeftOff)
            mentalityRate -= LIGHT_OFF_DRAIN_RATE;

        if (Managers.Timeline.OverdueAnomalyCount > 0)
            mentalityRate -= Managers.Timeline.OverdueAnomalyCount;
        else if (!Managers.Game.IsDayEnded && !Managers.Timeline.HasActiveAnomalies)
            mentalityRate += 0.5f;

        Managers.Game.ChangeMentality(mentalityRate * Time.deltaTime);
    }

    /// <summary>
    /// GameManager의 날짜값을 화면의 날짜 및 시간 텍스트에 반영한다.
    /// 현재 일자와 시각을 조회해 _currentDay와 _currentTime을 갱신한다.
    /// </summary>
    private void SetDateUI()
    {
        _currentDay.text = $"Day {Managers.Date.CurrentDay}";
        _currentTime.text = $"{Managers.Date.CurrentTime}";
    }
}
