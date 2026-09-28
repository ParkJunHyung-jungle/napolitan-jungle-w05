using UnityEngine;

public class ButtonOutline : MonoBehaviour
{
    public Material outlineMaterial;
    public Renderer clickableRenderer;
    public Renderer FocusIndicatorRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisableOutline(clickableRenderer);
        DisableOutline(FocusIndicatorRenderer);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseEnter()
    {
        EnableOutline(clickableRenderer);
        EnableOutline(FocusIndicatorRenderer);
    }
    private void OnMouseExit()
    {
        DisableOutline(clickableRenderer);
        DisableOutline(FocusIndicatorRenderer);
    }

    private void EnableOutline(Renderer targetRenderer)
    {
        Material[] materials = targetRenderer.sharedMaterials;
        if (materials.Length >= 2 && materials[1] == outlineMaterial)
            return;

        // 현재 첫 번째 Material을 그대로 유지
        Material currentBaseMaterial = materials[0];

        targetRenderer.sharedMaterials = new Material[]
        {
            currentBaseMaterial,
            outlineMaterial
        };
    }

    private void DisableOutline(Renderer targetRenderer)
    {
        Material[] materials = targetRenderer.sharedMaterials;
        if (materials.Length == 0)
            return;

        // "지금 현재" 첫 번째 Material을 가져옴
        Material currentBaseMaterial = materials[0];

        targetRenderer.sharedMaterials = new Material[]
        {
            currentBaseMaterial
        };
    }
}
