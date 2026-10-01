using UnityEngine;

public class GameManager
{
    public GameInfo GameInfo { get; private set; }

    public void Init()
    {
        GameInfo = Resources.Load<GameInfo>("Datas/GameInfo");
    }

    public void Clear()
    {

    }
}
