using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

public class ExitController : MonoBehaviour
{
    private const float TOAST_DURATION_SECONDS = 5f;

    [Header("UI")]
    [SerializeField] private GameObject _noCompleteCanvasPrefab;
    [SerializeField] private string _incompleteMessage = "임무를 완수한 후 나가세요.";

    [Header("Contact")]
    private GameObject _toastCanvas;
    private Coroutine _hideToastCoroutine;
    private bool _isDayEnded;
    private bool _isMissionComplete;

    void OnEnable()
    {
        Managers.Date.OnDayEnd += HandleDayEnd;
        Managers.Game.OnMissionCompleted += HandleMissionCompleted;
        _isDayEnded = Managers.Game.IsDayEnded;
    }

    void OnDisable()
    {
        Managers.Date.OnDayEnd -= HandleDayEnd;
        Managers.Game.OnMissionCompleted -= HandleMissionCompleted;
        HideToast();
        if (_toastCanvas != null)
            Destroy(_toastCanvas);
    }

    void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponentInParent<FirstPersonController>();
        if (player == null)
            return;

        if (!_isDayEnded)
            return;

        if (_isMissionComplete)
        {
            Managers.Game.ShowEndingCanvas();
        }
        else
        {
            ShowToast();
        }
    }

    /// <summary>
    /// 하루 종료 알림을 받아 출구의 종료 후 접촉 처리를 활성화한다.
    /// _isDayEnded를 true로 변경하며 UI는 접촉할 때까지 표시하지 않는다.
    /// </summary>
    private void HandleDayEnd()
    {
        _isDayEnded = true;
    }

    /// <summary>
    /// 임무 완료 알림을 받아 완료 상태를 저장하고 남은 미완료 안내를 닫는다.
    /// _isMissionComplete를 true로 바꾸며 엔딩 UI는 이후 접촉 시에만 표시한다.
    /// </summary>
    private void HandleMissionCompleted()
    {
        _isMissionComplete = true;
        HideToast();
    }

    /// <summary>
    /// 연결된 NoCompleteCanvas를 재사용해 미완료 안내를 표시한다.
    /// _incompleteMessage를 출력하고 기존 숨김 예약을 새 5초 예약으로 교체한다.
    /// </summary>
    private void ShowToast()
    {
        if (_toastCanvas == null)
        {
            _toastCanvas = Instantiate(_noCompleteCanvasPrefab, Managers.Instance.transform);
            _toastCanvas.GetComponentInChildren<TMP_Text>().text = _incompleteMessage;
        }
        else
        {
            _toastCanvas.SetActive(true);
        }

        if (_hideToastCoroutine != null)
            StopCoroutine(_hideToastCoroutine);
        _hideToastCoroutine = StartCoroutine(HideToastAfterDelay());
    }

    /// <summary>
    /// 진행 중인 토스트 숨김 예약을 취소하고 표시된 안내를 즉시 닫는다.
    /// _hideToastCoroutine을 비우고 _toastCanvas를 비활성화한다.
    /// </summary>
    private void HideToast()
    {
        if (_hideToastCoroutine != null)
        {
            StopCoroutine(_hideToastCoroutine);
            _hideToastCoroutine = null;
        }

        if (_toastCanvas != null)
            _toastCanvas.SetActive(false);
    }

    /// <summary>
    /// 게임 시간과 무관하게 실제 시간 5초를 기다린 뒤 토스트를 닫는다.
    /// _toastCanvas를 비활성화하고 _hideToastCoroutine을 비운다.
    /// </summary>
    private IEnumerator HideToastAfterDelay()
    {
        yield return new WaitForSecondsRealtime(TOAST_DURATION_SECONDS);
        _toastCanvas.SetActive(false);
        _hideToastCoroutine = null;
    }
}
