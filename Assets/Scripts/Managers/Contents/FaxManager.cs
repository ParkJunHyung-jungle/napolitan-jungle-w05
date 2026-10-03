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

    }

    //리치 텍스트에서 폰트 사용 시 TMP Settings에 지정된 경로(기본 Resources/Fonts & Materials)에 폰트 에셋이 있어야 함
    /// <summary>
    /// 팩스 위치에 출력물을 생성하고 팩스 소리를 재생한다.
    /// message를 출력물 텍스트로, isReal을 출력물의 진짜 명령서 여부로 설정한다.
    /// </summary>
    public void InstantiateFaxMessage(string message, bool isReal)
    {
        GameObject faxMessage = Object.Instantiate(_faxMessagePrefab, _fax.position, _fax.rotation);

        Managers.Sound.FaxSound();
        FaxInstructionController fax = faxMessage.GetComponent<FaxInstructionController>();
        fax.SetMessage(message, isReal);
        _instructions.Add(faxMessage);
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
