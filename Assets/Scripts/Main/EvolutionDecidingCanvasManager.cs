using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionDecidingCanvasManager : MonoBehaviour
{
    private enum RpsChoice { None = -1, Scissors = 0, Rock = 1, Paper = 2 }

    [System.Serializable]
    private class ParticipantSlot
    {
        public StateTypes stateType;
        public Transform root;

        [System.NonSerialized] public Image rcpImage;
    }

    [SerializeField] private List<ParticipantSlot> participantSlots = new();
    [SerializeField] private List<Sprite> rcpSprites = new(); // Scissors, Rock, Paper
    [SerializeField] private float rcpCycleInterval = 0.12f;
    [SerializeField] private Vector2 nightmareDecideDelayRange = new(1f, 1.5f);
    [SerializeField] private float roundResultDelay = 0.6f;

    public StateTypes DecidedStateType { get; private set; }

    private readonly Dictionary<StateTypes, ParticipantSlot> _slotByStateType = new();

    private void Awake()
    {
        CacheSlots();
        HideAllSlots();
        gameObject.SetActive(false);
    }

    public IEnumerator DecideEvolutionStateType(List<StateTypes> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            yield break;

        gameObject.SetActive(true);
        transform.localScale = Vector3.one;

        List<StateTypes> participants = candidates.Distinct().ToList();
        SetActiveParticipants(participants);

        while (participants.Count > 1)
        {
            yield return StartCoroutine(RunRpsRound(participants, result => participants = result));

            SetActiveParticipants(participants);
            if (participants.Count > 1 && roundResultDelay > 0f)
                yield return new WaitForSeconds(roundResultDelay);
        }

        DecidedStateType = participants[0];

        HideAllSlots();
        gameObject.SetActive(false);
    }

    private void CacheSlots()
    {
        _slotByStateType.Clear();

        foreach (var slot in participantSlots)
        {
            if (slot == null || slot.root == null) continue;

            slot.rcpImage = slot.root
                .GetComponentsInChildren<Image>(true)
                .FirstOrDefault(image => image.gameObject.name == "RCP Sprite");

            _slotByStateType[slot.stateType] = slot;
        }
    }

    private void HideAllSlots()
    {
        foreach (var slot in participantSlots)
            if (slot?.root != null)
                slot.root.gameObject.SetActive(false);
    }

    private void SetActiveParticipants(List<StateTypes> participants)
    {
        foreach (var slot in participantSlots)
        {
            if (slot?.root == null) continue;
            slot.root.gameObject.SetActive(participants.Contains(slot.stateType));
        }
    }

    private IEnumerator RunRpsRound(List<StateTypes> participants, System.Action<List<StateTypes>> onRoundEnd)
    {
        var inputs = new IPlayerInputReader[5]; // index 1~4 사용
        for (int i = 1; i <= 4; i++)
            inputs[i] = GameManager.Instance.GetPlayerInputReader(i);

        var choices = new Dictionary<StateTypes, RpsChoice>();
        foreach (StateTypes participant in participants)
            choices[participant] = RpsChoice.None;

        int currentSpriteIndex = 0;
        float cycleTimer = 0f;
        float elapsed = 0f;
        float nightmareDecideTime = UnityEngine.Random.Range(
            nightmareDecideDelayRange.x,
            nightmareDecideDelayRange.y);

        UpdateCyclingSprites(participants, choices, currentSpriteIndex);

        // 모든 참가자가 선택할 때까지 대기
        while (choices.Values.Any(c => c == RpsChoice.None))
        {
            elapsed += Time.deltaTime;
            cycleTimer += Time.deltaTime;

            if (cycleTimer >= rcpCycleInterval)
            {
                cycleTimer = 0f;
                currentSpriteIndex = (currentSpriteIndex + 1) % 3;
                UpdateCyclingSprites(participants, choices, currentSpriteIndex);
            }

            foreach (StateTypes participant in participants)
            {
                if (choices[participant] != RpsChoice.None) continue;

                if (participant == StateTypes.Nightmare)
                {
                    if (elapsed >= nightmareDecideTime)
                        SetChoice(choices, participant, currentSpriteIndex);
                    continue;
                }

                int playerId = (int)participant;
                var input = inputs[playerId];
                if (input.Right)
                    SetChoice(choices, participant, currentSpriteIndex);
            }

            yield return null;
        }

        foreach (var (participant, choice) in choices)
            Debug.Log($"[RPS] {participant} -> {choice}");

        if (roundResultDelay > 0f)
            yield return new WaitForSeconds(roundResultDelay);

        onRoundEnd(ResolveRps(choices));
    }

    private void UpdateCyclingSprites(
        List<StateTypes> participants,
        Dictionary<StateTypes, RpsChoice> choices,
        int spriteIndex)
    {
        foreach (StateTypes participant in participants)
        {
            if (choices[participant] != RpsChoice.None) continue;
            SetSlotSprite(participant, spriteIndex);
        }
    }

    private void SetChoice(Dictionary<StateTypes, RpsChoice> choices, StateTypes participant, int spriteIndex)
    {
        choices[participant] = (RpsChoice)spriteIndex;
        SetSlotSprite(participant, spriteIndex);
    }

    private void SetSlotSprite(StateTypes participant, int spriteIndex)
    {
        if (!_slotByStateType.TryGetValue(participant, out var slot) || slot.rcpImage == null)
            return;

        if (rcpSprites == null || spriteIndex < 0 || spriteIndex >= rcpSprites.Count)
            return;

        slot.rcpImage.sprite = rcpSprites[spriteIndex];
    }

    // 가위바위보 판정. 단독 승자면 그 참가자만, 무승부면 무승부 참가자들 반환
    private static List<StateTypes> ResolveRps(Dictionary<StateTypes, RpsChoice> choices)
    {
        var usedChoices = choices.Values.Distinct().ToList();

        // 전원 같은 선택 → 전원 무승부
        if (usedChoices.Count == 1)
            return choices.Keys.ToList();

        // 세 가지 선택이 모두 나왔으면 → 전원 무승부 (삼파전)
        if (usedChoices.Count == 3)
            return choices.Keys.ToList();

        // 두 종류만 나온 경우: 이기는 쪽 판별
        RpsChoice a = usedChoices[0], b = usedChoices[1];
        RpsChoice winner = Beats(a, b) ? a : b;

        var winners = choices.Where(kv => kv.Value == winner).Select(kv => kv.Key).ToList();
        return winners;
    }

    // a가 b를 이기면 true
    private static bool Beats(RpsChoice a, RpsChoice b)
    {
        return (a == RpsChoice.Rock     && b == RpsChoice.Scissors) ||
               (a == RpsChoice.Scissors && b == RpsChoice.Paper)    ||
               (a == RpsChoice.Paper    && b == RpsChoice.Rock);
    }
}
