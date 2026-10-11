using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class SearchSkill : MonoBehaviour
{
    [SerializeField] private SearchSettings settings;
    [SerializeField] private Transform fieldRoot;
    [SerializeField] private SearchWaveView waveView;

    private PlayerInputReader input;
    private FieldTile[] tiles;

    private readonly HashSet<Stone> revealedStones = new();

    private Vector3 searchCenter;
    private float currentRadius;
    private float remainingCooldown;
    private bool isSearching;

    public float RemainingCooldown => remainingCooldown;

    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        TurnManager turn = TurnManager.Instance;

        if (turn == null || !turn.IsBattleStarted())
        {
            isSearching = false;
            return;
        }

        if (Time.timeScale <= 0f || turn.IsBattlePaused())
        {
            return;
        }

        remainingCooldown = Mathf.Max(
            0f, remainingCooldown - Time.deltaTime);

        if (turn.GetCurrentStateType() == TurnStateType.Reposition ||
            turn.GetCurrentStateType() == TurnStateType.Result)
        {
            isSearching = false;
            return;
        }

        if (input != null && input.isActiveAndEnabled &&
            input.Skill.Pressed)
        {
            TryUse();
        }

        if (!isSearching)
        {
            return;
        }

        currentRadius = Mathf.Min(
            settings.Range,
            currentRadius + settings.ExpansionSpeed * Time.deltaTime);

        RevealStonesInRange();

        if (currentRadius >= settings.Range)
        {
            isSearching = false;
        }
    }

    private void LateUpdate()
    {
        if (waveView == null)
        {
            return;
        }

        if (isSearching)
        {
            waveView.Show(searchCenter, currentRadius);
        }
        else
        {
            waveView.Hide();
        }
    }

    public bool TryUse()
    {
        if (!isActiveAndEnabled || settings == null || fieldRoot == null ||
            isSearching || remainingCooldown > 0f ||
            settings.Range <= 0f || settings.ExpansionSpeed <= 0f)
        {
            return false;
        }

        TurnManager turn = TurnManager.Instance;

        if (turn == null || !turn.IsBattleStarted() ||
            turn.IsBattlePaused() || Time.timeScale <= 0f ||
            turn.GetCurrentStateType() != TurnStateType.Playing)
        {
            return false;
        }

        tiles = fieldRoot.GetComponentsInChildren<FieldTile>();

        searchCenter = transform.position;
        currentRadius = 0f;
        remainingCooldown = Mathf.Max(0f, settings.CooldownTime);

        revealedStones.Clear();
        isSearching = true;

        RevealStonesInRange();
        return true;
    }

    private void RevealStonesInRange()
    {
        foreach (FieldTile tile in tiles)
        {
            if (tile == null || !tile.isActiveAndEnabled)
            {
                continue;
            }

            Stone stone = tile.PutedStone;

            if (stone == null || !stone.isActiveAndEnabled ||
                stone.Owner == null || revealedStones.Contains(stone))
            {
                continue;
            }

            Vector3 offset = stone.transform.position - searchCenter;
            offset.y = 0f;

            if (offset.sqrMagnitude > currentRadius * currentRadius)
            {
                continue;
            }

            stone.RevealColor(
                stone.Owner.TeamColor,
                settings.RevealDuration,
                settings.FadeDuration);

            revealedStones.Add(stone);
        }
    }

    private void OnDisable()
    {
        isSearching = false;
        revealedStones.Clear();

        if (waveView != null)
        {
            waveView.Hide();
        }
    }
}