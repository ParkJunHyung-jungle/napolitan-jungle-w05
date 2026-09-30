using System;
using UnityEngine;
using UnityEngine.UI;

public class StartPanelView : MonoBehaviour
{
    [SerializeField] private Button startButton;

    public void Initialize(Action action)
    {
        gameObject.SetActive(true);
        startButton.onClick.AddListener(action.Invoke);
    }
}
