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

        Managers.Sound.AmbientSound();
    }

    void Update()
    {
        Managers.Date.ElapsedTime += Time.deltaTime;
        SetDateUI();
    }

    private void SetDateUI()
    {
        _currentDay.text = $"Day {Managers.Date.CurrentDay}";
        _currentTime.text = $"{Managers.Date.CurrentTime}";
    }
}
