using UnityEngine;

/// <summary>
/// 포스터 굽기 설정. 굽기용 Canvas(Screen Space - Camera) 루트에 붙인다.
/// 실제 굽기는 에디터 전용 PosterBaker가 수행한다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
[AddComponentMenu("Poster/Poster Bake Source")]
public class PosterBakeSource : MonoBehaviour
{
    [Header("Capture")]
    [Tooltip("Orthographic 카메라. Target Texture에 sRGB RenderTexture가 연결되어 있어야 한다.")]
    public Camera bakeCamera;

    [Header("Output")]
    [Tooltip("Assets/ 로 시작하고 .png 로 끝나는 경로")]
    public string outputPath = "Assets/Art/Posters/Poster.png";

    [Tooltip("UI 알파 블렌딩 때문에 글자 가장자리 알파가 1 미만으로 저장되는 것을 막는다. 불투명 포스터면 켜둘 것.")]
    public bool forceOpaque = true;

    [Header("Import Settings")]
    [Tooltip("굽고 나서 텍스처 임포트 설정(Kaiser, Aniso 16, Trilinear, Clamp, BC7)을 자동 적용")]
    public bool applyImportSettings = true;

    [Range(-1f, 0f)]
    public float mipMapBias = -0.5f;

    [Header("Optional")]
    [Tooltip("지정하면 굽고 나서 이 머티리얼의 메인 텍스처(URP Lit은 _BaseMap)에 자동 연결")]
    public Material targetMaterial;
}
