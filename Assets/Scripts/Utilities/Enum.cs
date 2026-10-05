public static class FaxMessage
{
    public const string RULE_FOLLOW_COMMANDER = "<color=red>명령자</color>의 말을 따라야 한다.";
    public const string RULE_OPEN_DOOR_ON_KNOCK = "노크 소리가 들리면 문을 열어야 한다.";
    public const string RULE_ANSWER_PHONE = "전화가 울리면 전화를 받아야 한다.";
    public const string RULE_IGNORE_PHONE_WITH_KNOCK = "노크 소리와 같이 울리는 전화는 받지 말아야 한다.";
    public const string RULE_CLOSE_DOOR_THREE_TO_FOUR = "3시에서 4시 사이에는 문을 닫아야 한다.";
}

public enum AudioSourceTypes
{
    UNKNOWN,
    DOOR,
    LOCKEDDOOR,
    FAX,
    TELEPHONE,
    LIGHTSWITCH,
    CRYING,
    LAMP,
    FOOTSTEP,
};
