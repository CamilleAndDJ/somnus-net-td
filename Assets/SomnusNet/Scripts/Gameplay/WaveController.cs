using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SomnusNet.Core;
using SomnusNet.Data;
using SomnusNet.UI;
using SomnusNet.Units;
using UnityEngine;

namespace SomnusNet.Gameplay
{
    public class WaveController : MonoBehaviour
    {
        const int AccelerateAfterDefeats = 2;
        const float AcceleratedSpawnMultiplier = 2.5f;
        const float BaseSpawnSpeedMultiplier = 1.35f;
        const float SpawnRateIncreasePerRound = 0.30f;
        const float ExtraSpawnDelayStep = 9f;

        [SerializeField] GameCatalog catalog;
        [SerializeField] GameObject glitchPrefab;

        float _roundStartTime;
        int _nextSpawnIndex;
        int _dualPathOddExtraPath;
        Coroutine _spawnRoutine;

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnRoundStarted += HandleRoundStarted;

            StartCoroutine(BeginRoundSpawnsWhenReady());
        }

        System.Collections.IEnumerator BeginRoundSpawnsWhenReady()
        {
            while (!LevelLayoutSession.IsReady
                   || (GameManager.Instance != null && GameManager.Instance.IsHoldingLevelStart))
                yield return null;

            if (LevelLayoutCatalog.SkipsGlitchSpawns(LevelLayoutSession.Selected))
                yield break;

            BeginRoundSpawns();
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
        }

        void HandleRoundStarted(int round)
        {
            if (LevelLayoutCatalog.SkipsGlitchSpawns(LevelLayoutSession.Selected))
                return;

            if (round <= 1) return;
            BeginRoundSpawns();
        }

        void BeginRoundSpawns()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            _nextSpawnIndex = 0;
            _dualPathOddExtraPath = Random.Range(0, 2);
            _spawnRoutine = StartCoroutine(SpawnAll());
        }

        IEnumerator SpawnAll()
        {
            try
            {
                if (!TryGetOrderedSpawns(out var ordered))
                    yield break;

                if (ShouldWaitForFirstBlobPlacement())
                {
                    while (!HasAnyBlobOnField())
                    {
                        if (ShouldAbortSpawning())
                            yield break;
                        yield return null;
                    }
                }

                _roundStartTime = Time.time;

                if (ShouldUseDualPathSpawning())
                {
                    yield return SpawnAllDualPath(ordered);
                    yield break;
                }

                for (; _nextSpawnIndex < ordered.Count; _nextSpawnIndex++)
                {
                    if (ShouldAbortSpawning())
                        yield break;

                    var entry = ordered[_nextSpawnIndex];
                    var spawnSpeed = GetRoundSpawnSpeedMultiplier();
                    var scaledDelay = entry.delayFromWaveStart / spawnSpeed;
                    var wait = _roundStartTime + scaledDelay - Time.time;
                    if (wait > 0f)
                        yield return WaitForSpawnDelay(wait);

                    if (ShouldAbortSpawning())
                        yield break;

                    SpawnGlitch(entry, _nextSpawnIndex, ordered.Count);
                }
            }
            finally
            {
                _spawnRoutine = null;
            }
        }

        IEnumerator SpawnAllDualPath(List<WaveSpawnEntry> ordered)
        {
            var total = ordered.Count;
            var pairCount = total / 2;
            var spawnCounter = 0;

            for (var pair = 0; pair < pairCount; pair++)
            {
                if (ShouldAbortSpawning())
                    yield break;

                yield return WaitUntilSpawnTime(ordered[pair * 2].delayFromWaveStart);
                if (ShouldAbortSpawning())
                    yield break;

                SpawnGlitch(ordered[pair * 2], spawnCounter++, total, 0);
                SpawnGlitch(ordered[pair * 2 + 1], spawnCounter++, total, 1);
            }

            if (total % 2 != 1)
                yield break;

            if (ShouldAbortSpawning())
                yield break;

            var lastIndex = total - 1;
            yield return WaitUntilSpawnTime(ordered[lastIndex].delayFromWaveStart);
            if (ShouldAbortSpawning())
                yield break;

            SpawnGlitch(ordered[lastIndex], spawnCounter, total, _dualPathOddExtraPath);
        }

        IEnumerator WaitUntilSpawnTime(float delayFromWaveStart)
        {
            var spawnSpeed = GetRoundSpawnSpeedMultiplier();
            var scaledDelay = delayFromWaveStart / spawnSpeed;
            var wait = _roundStartTime + scaledDelay - Time.time;
            if (wait > 0f)
                yield return WaitForSpawnDelay(wait);
        }

        static bool ShouldUseDualPathSpawning()
        {
            var grid = GridManager.Instance;
            return grid != null && grid.GlitchPathCount >= 2;
        }

        IEnumerator WaitForSpawnDelay(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                if (ShouldAbortSpawning())
                    yield break;

                while (GameManager.Instance != null && GameManager.Instance.Phase == GamePhase.Paused)
                {
                    yield return null;
                    if (ShouldAbortSpawning())
                        yield break;
                }

                var speed = GameManager.Instance != null
                            && GameManager.Instance.GlitchesDefeated >= AccelerateAfterDefeats
                    ? AcceleratedSpawnMultiplier
                    : 1f;
                elapsed += Time.deltaTime * speed;
                yield return null;
            }
        }

        bool TryGetOrderedSpawns(out List<WaveSpawnEntry> ordered)
        {
            ordered = null;
            if (catalog == null)
            {
                Debug.LogError("WaveController: catalog is not assigned.");
                return false;
            }

            if (catalog.level01Waves == null || catalog.level01Waves.spawns == null || catalog.level01Waves.spawns.Count == 0)
            {
                Debug.LogError("WaveController: level01Waves has no spawn entries.");
                return false;
            }

            ordered = BuildSpawnListForCurrentRound(catalog.level01Waves.spawns);
            return ordered.Count > 0;
        }

        static List<WaveSpawnEntry> BuildSpawnListForCurrentRound(List<WaveSpawnEntry> baseSpawns)
        {
            var ordered = baseSpawns.OrderBy(s => s.delayFromWaveStart).ToList();
            var extraCount = GetExtraGlitchCountForRound();
            if (extraCount <= 0)
                return ordered;

            var lastDelay = ordered[^1].delayFromWaveStart;
            var templateCount = Mathf.Max(1, ordered.Count - 1);

            for (var i = 0; i < extraCount; i++)
            {
                var template = ordered[i % templateCount];
                ordered.Add(new WaveSpawnEntry
                {
                    glitch = template.glitch,
                    lane = template.lane,
                    delayFromWaveStart = lastDelay + ExtraSpawnDelayStep * (i + 1),
                });
            }

            return ordered.OrderBy(s => s.delayFromWaveStart).ToList();
        }

        static int GetExtraGlitchCountForRound()
        {
            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            return GameManager.GetExtraGlitchCountForRound(round);
        }

        void SpawnGlitch(WaveSpawnEntry entry, int spawnIndex, int totalSpawns, int? pathIndexOverride = null)
        {
            if (glitchPrefab == null)
            {
                Debug.LogError("WaveController: glitchPrefab is not assigned.");
                return;
            }

            var grid = GridManager.Instance;
            if (grid == null)
            {
                Debug.LogError("WaveController: GridManager is missing.");
                return;
            }

            var kind = spawnIndex == totalSpawns - 1
                ? BossSpawnRules.ResolveLastSpawnKind(
                    GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1)
                : entry.glitch;

            if (!catalog.TryGetGlitch(kind, out var def))
            {
                Debug.LogError($"WaveController: missing GlitchDefinition for {kind}.");
                return;
            }

            var pathCount = Mathf.Max(1, grid.GlitchPathCount);
            var pathIndex = pathIndexOverride ?? (pathCount > 1 ? spawnIndex % pathCount : 0);
            pathIndex = Mathf.Clamp(pathIndex, 0, pathCount - 1);
            var pathWaypoints = grid.GetGlitchWaypoints(pathIndex);
            var spawnPos = pathWaypoints.Count > 0
                ? pathWaypoints[0]
                : grid.CellToWorld(GridManager.SpawnColumn, GridManager.PathRow);

            var go = Instantiate(glitchPrefab, spawnPos, Quaternion.identity);
            var glitch = go.GetComponent<Glitch>();
            if (glitch == null)
            {
                Debug.LogError("WaveController: glitchPrefab is missing a Glitch component.");
                Destroy(go);
                return;
            }

            glitch.Initialize(def, pathIndex);

            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            if (StoryModeRules.Active && StoryModeRules.HasDialogs && round == 1 && spawnIndex == 0)
                StoryModeStargazerSpeech.TryShowLeaveItToMe();

            if (LoopedModifierRules.ShouldApplyToSpawn(round, kind))
                glitch.ApplyLoopedModifier();
        }

        static bool ShouldAbortSpawning()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.Phase is GamePhase.Won or GamePhase.Lost or GamePhase.RoundIntermission;
        }

        static bool ShouldWaitForFirstBlobPlacement() =>
            LevelStartRules.ShouldWaitForFirstBlobPlacement();

        static bool HasAnyBlobOnField() =>
            GridManager.Instance != null && GridManager.Instance.HasAnyBlobOnField();

        static float GetRoundSpawnSpeedMultiplier()
        {
            var round = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
            return BaseSpawnSpeedMultiplier * (1f + (round - 1) * SpawnRateIncreasePerRound);
        }
    }
}
