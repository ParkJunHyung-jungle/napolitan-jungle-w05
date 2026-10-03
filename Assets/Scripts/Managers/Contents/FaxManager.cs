using System.Collections.Generic;
using UnityEngine;

public class FaxManager
{
    [Header("Fax")]
    private Transform _fax;
    [Header("Prefab")]
    private GameObject _faxMessagePrefab;

    public GameObject LoadFax => Resources.Load<GameObject>("Prefabs/Fax");
    public GameObject LoadFaxInstruction => Resources.Load<GameObject>("Prefabs/FaxInstruction");

    public void Init()
    {
        _faxMessagePrefab = LoadFaxInstruction;
        InstantiateFax();
    }

    public void Clear()
    {

    }

    //리치 텍스트에서 폰트 사용 시 TMP Settings에 지정된 경로(기본 Resources/Fonts & Materials)에 폰트 에셋이 있어야 함
    /// <summary>
    /// currentMinute의 팩스 안내를 IJH 씬의 FaxInstructionSpawner 위치에 생성한다.
    /// 생성된 팩스의 문구를 설정하고 팩스 소리를 재생한다.
    /// </summary>
    public void InstantiateFaxMessage(int currentMinute)
    {
        Transform spawner = GameObject.Find("FaxInstructionSpawner").transform;
        GameObject faxMessage = Object.Instantiate(_faxMessagePrefab, spawner.position, spawner.rotation);

        Managers.Sound.FaxSound();
        FaxInstructionController fax = faxMessage.GetComponent<FaxInstructionController>();
        fax.SetMessage(Managers.Game.GameInfo.GetInstruction(currentMinute));
    }

    private void InstantiateFax()
    {
        GameObject fax = Object.Instantiate(LoadFax);
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.FAX, fax.GetComponent<AudioSource>());
        _fax = fax.transform;
        _fax.SetParent(Managers.Instance.transform, true);
        _fax = fax.transform;
    }
}
