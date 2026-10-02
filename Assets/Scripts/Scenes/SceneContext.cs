using UnityEngine;
using UnityEngine.UI;

public class SceneContext : MonoBehaviour
{
    [SerializeField]
    private Canvas _dateCanvas;
    [SerializeField]
    private Text _currentDay;
    [SerializeField]
    private Text _currentTime;

    void Awake()
    {
        Managers.Input.SetInputMode(InputMode.UI);
        _dateCanvas.gameObject.SetActive(true);
        Managers.Game.ResetMentality();

        Managers.Sound.AmbientSound();
    }

    void Update()
    {
        Managers.Date.ElapsedTime += Time.deltaTime;
        SetDateUI();
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
