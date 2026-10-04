using System;
using System.Collections.Generic;
using UnityEngine;

public class FaxManager
{
    [Header("Fax")]
    private Transform _fax;
    private Transform _faxInstructionSpawner;
    private List<FaxInstructionController> _instructions = new();
    private InstructionPanelController _instructionPanel;

    [Header("Prefab")]
    private GameObject _faxMessagePrefab;
    public GameObject LoadInstructionPanel => Resources.Load<GameObject>("Prefabs/UIs/InstructionCanvas");
    public GameObject LoadFax => Resources.Load<GameObject>("Prefabs/Fax");
    public GameObject LoadFaxInstruction => Resources.Load<GameObject>("Prefabs/FaxInstruction");
    public Func<string, GameObject> LoadPastMessage => prefabName => Resources.Load<GameObject>($"Prefabs/{prefabName}");

    public void Init()
    {
        _faxMessagePrefab = LoadFaxInstruction;
        InstantiateFax();
        GameObject instructionCanvas = UnityEngine.Object.Instantiate(LoadInstructionPanel);
        _instructionPanel = instructionCanvas.GetComponent<InstructionPanelController>();
        instructionCanvas.transform.SetParent(Managers.Instance.transform, false);
        instructionCanvas.SetActive(false);
        Managers.Date.OnDayEnd += OnDayEnd;
    }

    public void Clear()
    {
        _instructions.Clear();
        _instructionPanel.InitializeInstructionCount(0);
    }

    /// <summary>
    /// 하루 종료 시 가짜 출력물을 제거하고 남은 명령서의 아웃라인을 활성화한다.
    /// _instructions의 각 출력물 IsInstruction 상태를 확인하고 남은 개수를 UI에 반영한다.
    /// </summary>
    public void OnDayEnd()
    {
        _instructionPanel.gameObject.SetActive(true);
        for (int i = _instructions.Count - 1; i >= 0; i--)
        {
            FaxInstructionController instruction = _instructions[i];
            if (!instruction.IsInstruction)
            {
                UnityEngine.Object.Destroy(instruction.gameObject);
                _instructions.RemoveAt(i);
                continue;
            }
            instruction.SetOutline(true);
        }

        _instructionPanel.InitializeInstructionCount(_instructions.Count);
    }

    /// <summary>
    /// 보관 또는 파쇄된 명령서를 목록에서 제거하고 카운터를 갱신한다.
    /// instruction을 _instructions에서 제거하고 남은 개수를 UI에 전달한다.
    /// </summary>
    public void RemoveInstruction(FaxInstructionController instruction)
    {
        if (!_instructions.Remove(instruction))
            return;

        _instructionPanel.OnInstructionDestroyed();
        if (Managers.Game.IsDayEnded && _instructionPanel.AreAllInstructionsCompleted)
            Managers.Game.CompleteMission();
    }

    //리치 텍스트에서 폰트 사용 시 TMP Settings에 지정된 경로(기본 Resources/Fonts & Materials)에 폰트 에셋이 있어야 함
    /// <summary>
    /// 팩스 위치에 명령서를 출력하고 _instructions에 추가한다.
    /// message를 출력물 텍스트로, isReal을 출력물의 진짜 명령서 여부로 설정한다.
    /// </summary>
    public void InstantiateFaxMessage(string message, bool isReal)
    {
        SpawnFaxMessage(message, isReal, true);
        Managers.Sound.FaxSound();
    }

    /// <summary>
    /// 팩스 위치에 에러 팩스를 출력한다.
    /// message를 출력물 텍스트로 설정하며, 명령서가 아니므로 가짜로 표시하고 _instructions에는 추가하지 않는다.
    /// </summary>
    public void PrintErrorFax(string message)
    {
        SpawnFaxMessage(message, false, false);
        Managers.Sound.FaxErrorSound();
    }

    /// <summary>
    /// FaxInstructionSpawner 위치에 출력물을 생성하고 팩스 소리를 재생한다.
    /// message와 isReal을 출력물에 설정하고, 생성한 출력물을 반환한다.
    /// </summary>
    private void SpawnFaxMessage(string message, bool isReal, bool isInstruction)
    {

        GameObject instructionObject = UnityEngine.Object.Instantiate(_faxMessagePrefab, _faxInstructionSpawner.position, _faxInstructionSpawner.rotation);

        FaxInstructionController instruction = instructionObject.GetComponent<FaxInstructionController>();
        instruction.SetMessage(message);
        instruction.SetReal(isReal);
        instruction.SetInstruction(isInstruction);

        _instructions.Add(instruction);
    }

    public void SpawnPastMessage(string prefabName)
    {
        GameObject instructionObject = UnityEngine.Object.Instantiate(LoadPastMessage(prefabName));
        FaxInstructionController instruction = instructionObject.GetComponent<FaxInstructionController>();
        instruction.SetReal(true);
        instruction.SetInstruction(false);

        _instructions.Add(instruction);
    }

    private void InstantiateFax()
    {
        GameObject fax = UnityEngine.Object.Instantiate(LoadFax);
        Managers.Sound.RegisterAudioSource(AudioSourceTypes.FAX, fax.GetComponent<AudioSource>());
        _fax = fax.transform;
        _fax.SetParent(Managers.Instance.transform, true);
        _faxInstructionSpawner = _fax.Find("FaxInstructionSpawner");
    }
}
