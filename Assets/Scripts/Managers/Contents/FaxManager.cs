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
    public void InstantiateFaxMessage(int currentMinute)
    {
        GameObject faxMessage = Object.Instantiate(_faxMessagePrefab, _fax.position, _fax.rotation);

        FaxInstructionController fax = faxMessage.GetComponent<FaxInstructionController>();
        fax.SetMessage(Managers.Game.GameInfo.GetInstruction(currentMinute));
    }

    private void InstantiateFax()
    {
        GameObject fax = Object.Instantiate(LoadFax);
        fax.transform.SetParent(Managers.Instance.transform, true);
        _fax = fax.transform;
    }
}
