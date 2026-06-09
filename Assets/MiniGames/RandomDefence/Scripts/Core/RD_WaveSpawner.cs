using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_WaveSpawner : MonoBehaviour
    {
        public const int MAX_ALIVE = 40;

        [SerializeField] private RD_PathManager pathManager;
        [SerializeField] private Transform      monsterParent;

        private readonly List<RD_MonsterBase> _alive = new();
        private float _hpBonus;

        public int aliveCount => _alive.Count;

        public System.Action<int> onAliveCountChanged;
        public System.Action      onAllSpawned;
        public System.Action      onWaveCleared;

        public void StartWave(RD_WaveDataSO wave, int round)
        {
            _hpBonus = (round - 1) * 2f;
            StopAllCoroutines();
            StartCoroutine(SpawnRoutine(wave));
        }

        public void OnMonsterDied(RD_MonsterBase monster)
        {
            _alive.Remove(monster);
            NotifyAliveChanged();
            CheckWaveCleared();
        }

        public void ClearAll()
        {
            foreach (var m in _alive) if (m != null) Destroy(m.gameObject);
            _alive.Clear();
            NotifyAliveChanged();
            StopAllCoroutines();
        }

        private IEnumerator SpawnRoutine(RD_WaveDataSO wave)
        {
            foreach (var entry in wave.spawnEntries)
            {
                if (entry.monster?.prefab == null)
                {
                    Debug.LogWarning("[RD_WaveSpawner] SpawnEntry 에 프리팹 없음");
                    continue;
                }
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnMonster(entry.monster);
                    if (entry.spawnInterval > 0f)
                        yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            onAllSpawned?.Invoke();

            yield return null;
            CheckWaveCleared();
        }

        private void SpawnMonster(RD_MonsterDataSO data)
        {
            Vector3    pos = pathManager != null ? pathManager.spawnPosition : Vector3.zero;
            GameObject go  = Instantiate(data.prefab, pos, Quaternion.identity, monsterParent);

            if (!go.TryGetComponent<RD_MonsterBase>(out var monster))
            {
                Debug.LogError($"[RD_WaveSpawner] '{data.prefab.name}' 에 RD_MonsterBase 없음");
                Destroy(go);
                return;
            }

            monster.Initialize(data, pathManager, _hpBonus);
            _alive.Add(monster);
            NotifyAliveChanged();

            if (_alive.Count > MAX_ALIVE)
                RD_GameManager.instance?.TriggerGameOver();
        }

        private void CheckWaveCleared()
        {
            if (RD_GameManager.instance?.state is GameState.GameOver or GameState.Clear) return;
            if (_alive.Count == 0) onWaveCleared?.Invoke();
        }

        private void NotifyAliveChanged()
        {
            onAliveCountChanged?.Invoke(_alive.Count);
        }
    }
}
