using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndPanel : MonoBehaviour
    {
        [SerializeField] private Button restartButton;

        public void Initialize(Action action)
        {
            restartButton.onClick.AddListener(action.Invoke);
        }
    }
}
