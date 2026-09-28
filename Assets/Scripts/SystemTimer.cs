using System.Collections;
using UnityEngine;

public class SystemTimer : MonoBehaviour
{
    private GameManager _gameManager;
    private float FatalTime;
    private void Awake()
    {
        FatalTime = 30;
    }

    public void LimitTimer()
    {
        StartCoroutine(CountDown());
        //if문으로 다른 스크립트에서 고장된걸 고치면 FatalTime을 30으로 초기화 하고 코루틴을 종료
    }

    IEnumerator CountDown()
    {
        while (true)
        {
            FatalTime--;
            yield return new WaitForSeconds(1f);
            if (FatalTime < 0)
            {
                _gameManager._hp--;
                FatalTime = 30;

            }

        }
    }
}
