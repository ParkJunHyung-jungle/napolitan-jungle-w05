using System.Collections.Generic;
using UnityEngine;

public class FaxManager
{
    [Header("Fax")]
    private Transform _fax;
    private List<GameObject> _instructions = new();

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
        _instructions.Clear();
    }

    //리치 텍스트에서 폰트 사용 시 TMP Settings에 지정된 경로(기본 Resources/Fonts & Materials)에 폰트 에셋이 있어야 함
    /// <summary>
    /// 팩스 위치에 명령서를 출력하고 _instructions에 추가한다.
    /// message를 출력물 텍스트로, isReal을 출력물의 진짜 명령서 여부로 설정한다.
    /// </summary>
    public void InstantiateFaxMessage(string message, bool isReal)
    {
        GameObject faxMessage = SpawnFaxMessage(message, isReal);
        _instructions.Add(faxMessage);
    }

    /// <summary>
    /// 팩스 위치에 에러 팩스를 출력한다.
    /// message를 출력물 텍스트로 설정하며, 명령서가 아니므로 가짜로 표시하고 _instructions에는 추가하지 않는다.
    /// </summary>
    public void PrintErrorFax(string message)
    {
        SpawnFaxMessage(message, false);
    }

    /// <summary>
    /// FaxInstructionSpawner 위치에 출력물을 생성하고 팩스 소리를 재생한다.
    /// message와 isReal을 출력물에 설정하고, 생성한 출력물을 반환한다.
    /// </summary>
    private GameObject SpawnFaxMessage(string message, bool isReal)
    {
        Transform spawner = GameObject.Find("FaxInstructionSpawner").transform;
        GameObject faxMessage = Object.Instantiate(_faxMessagePrefab, spawner.position, spawner.rotation);

        Managers.Sound.FaxSound();
        FaxInstructionController fax = faxMessage.GetComponent<FaxInstructionController>();
        fax.SetMessage(message, isReal);

        return faxMessage;
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
