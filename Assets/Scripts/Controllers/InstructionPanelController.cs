using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class InstructionPanelController : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField]
    private GameObject _instructionCirclePrefab;

    private readonly List<GameObject> _instructionCircles = new();
    private RectTransform _panel;
    private int _instructionCount;
    private int _completedInstructionCount;

    private void Awake()
    {
        _panel = transform.Find("Panel").GetComponent<RectTransform>();
        _panel.GetComponent<HorizontalLayoutGroup>().enabled = false;

        for (int i = _panel.childCount - 1; i >= 0; i--)
        {
            Transform child = _panel.GetChild(i);
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 새 instruction 카운터를 초기화하고 빈 원을 생성한다.
    /// instructionCount를 전체 개수로 사용해 완료 수를 0으로 설정하고 Panel 레이아웃을 갱신한다.
    /// </summary>
    public void InitializeInstructionCount(int instructionCount)
    {
        _instructionCount = Mathf.Max(0, instructionCount);
        _completedInstructionCount = 0;
        ResizeInstructionCircles();
        SetInstructionCircleLayout();
        UpdateFilledCircles();
    }

    /// <summary>
    /// 파기 완료된 instruction 하나를 카운터에 반영한다.
    /// 입력 없이 완료 수를 증가시키고 왼쪽부터 채움 표시를 갱신한다.
    /// </summary>
    public void OnInstructionDestroyed()
    {
        _completedInstructionCount++;
        UpdateFilledCircles();
    }

    /// <summary>
    /// 초기화된 instruction 수에 맞게 Panel 자식 원형 prefab을 추가하거나 제거한다.
    /// _instructionCount를 사용해 _instructionCircles 수를 일치시킨다.
    /// </summary>
    private void ResizeInstructionCircles()
    {
        while (_instructionCircles.Count < _instructionCount)
        {
            GameObject circle = Instantiate(_instructionCirclePrefab, _panel, false);
            _instructionCircles.Add(circle);
        }

        while (_instructionCircles.Count > _instructionCount)
        {
            GameObject circle = _instructionCircles[^1];
            circle.SetActive(false);
            Destroy(circle);
            _instructionCircles.RemoveAt(_instructionCircles.Count - 1);
        }
    }

    /// <summary>
    /// 초기화된 instruction 수에 맞춰 Panel 폭과 각 원의 anchor를 설정한다.
    /// _instructionCount를 사용해 원들이 정사각형 비율로 균등 배치되도록 한다.
    /// </summary>
    private void SetInstructionCircleLayout()
    {
        RectTransform canvas = (RectTransform)transform;
        float widthAnchor = _instructionCount * 0.1f * canvas.rect.height / canvas.rect.width;
        _panel.anchorMin = new Vector2(0.5f - widthAnchor * 0.5f, _panel.anchorMin.y);
        _panel.anchorMax = new Vector2(0.5f + widthAnchor * 0.5f, _panel.anchorMax.y);

        for (int i = 0; i < _instructionCircles.Count; i++)
        {
            RectTransform circleTransform = _instructionCircles[i].GetComponent<RectTransform>();
            circleTransform.anchorMin = new Vector2((float)i / _instructionCount, 0f);
            circleTransform.anchorMax = new Vector2((float)(i + 1) / _instructionCount, 1f);
            circleTransform.offsetMin = Vector2.zero;
            circleTransform.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 파기 완료 수에 맞춰 왼쪽부터 채움 원의 활성 상태만 갱신한다.
    /// _completedInstructionCount를 사용하며 원 개수와 Panel anchor는 변경하지 않는다.
    /// </summary>
    private void UpdateFilledCircles()
    {
        for (int i = 0; i < _instructionCircles.Count; i++)
            _instructionCircles[i].transform.Find("FilledCircle").gameObject.SetActive(i < _completedInstructionCount);
    }
}
