using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class DarknessBoxVolume : MonoBehaviour
{
    [Header("Darkness Box")]
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private Color _darknessColor = Color.black;
    [SerializeField, Min(0f)] private float _density = 1.5f;

    public static DarknessBoxVolume Active { get; private set; }
    public BoxCollider Box => _boxCollider;
    public Color DarknessColor => _darknessColor;
    public float Density => _density;

    /// <summary>
    /// color를 어둠 영역의 색상으로 설정한다.
    /// 변경된 색상을 _darknessColor에 저장해 다음 렌더 패스에 반영한다.
    /// </summary>
    public void SetColor(Color color)
    {
        _darknessColor = color;
    }

    void OnEnable()
    {
        Active = this;
        Managers.Date.OnDayEnd += HandleDayEnd;
    }

    void OnDisable()
    {
        Managers.Date.OnDayEnd -= HandleDayEnd;
        if (Active == this)
            Active = null;
    }

    /// <summary>
    /// 하루 종료 알림을 받으면 게임의 DayEnd 상태를 확인한다.
    /// 종료 상태일 때 이 볼륨의 색상을 파란색으로 변경한다.
    /// </summary>
    private void HandleDayEnd()
    {
        //if (Managers.Game.IsDayEnded)
        //SetColor(new Color(30 / 255f, 50 / 255f, 81 / 255f));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_boxCollider.center, _boxCollider.size);
        Gizmos.matrix = Matrix4x4.identity;
    }

}
