using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MentalityUI : MonoBehaviour
{
    [Header("정신력 UI")]
    [SerializeField]
    private Slider _mentalitySlider;
    [SerializeField]
    private TMP_Text _mentalityValue;

    private GameManager _gameManager;

    void OnDestroy()
    {
        _gameManager.OnMentalityChanged -= UpdateMentality;
    }

    /// <summary>
    /// 정신력 상태와 화면 요소를 연결하고 최초 표시값을 설정한다.
    /// gameManager를 사용해 상태 변경 이벤트에 게이지 갱신 함수를 등록한다.
    /// </summary>
    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        _gameManager.OnMentalityChanged += UpdateMentality;
        UpdateMentality(_gameManager.CurrentMentality, _gameManager.MaxMentality);
    }

    /// <summary>
    /// currentMentality와 maxMentality를 슬라이더와 수치 텍스트에 표시한다.
    /// 게이지 범위와 현재값을 갱신하고 화면에 현재 정신력과 최대 정신력을 출력한다.
    /// </summary>
    private void UpdateMentality(float currentMentality, float maxMentality)
    {
        _mentalitySlider.maxValue = maxMentality;
        _mentalitySlider.SetValueWithoutNotify(currentMentality);
        _mentalityValue.text = $"{Mathf.CeilToInt(currentMentality)} / {Mathf.CeilToInt(maxMentality)}";
    }
}
