using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using PathologicalGames;
using UnityEngine.UI;

namespace NotZombiller
{

    public enum EnemyType { Zombie, Orc, Golem };

    /// <summary>
    /// This class is responsible for managing the wavecount and spawning enemies in the endless prototype
    /// </summary>
    [RequireComponent(typeof(SpawnPool))]
    public class WaveSpawner : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        #region Serializable Fields

        [SerializeField] private Text waveText;

        [Header("Wave Stats")]
        [Tooltip("The starting difficulty points for the first wave")]
        [SerializeField] private float difficultyPoints;
        [Tooltip("The starting spawn time for the first wave")]
        [SerializeField] private float timeBetweenSpawns;
        [Tooltip("Multiplies the difficulty points at the end of each wave")]
        [SerializeField] private float difficultyMultiplier;
        [Tooltip("Multiplies the time between spawns at the end of each wave")]
        [SerializeField] private float timeMultiplier;
        [Tooltip("The minimum difficulty that should always be present")]
        [SerializeField] private float minDifficulty;
        [Tooltip("How fast spawning will go should the minimal difficulty not be present")]
        [SerializeField] private float quickTimeBetweenSpawns;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private int zombieDifficulty;
        [SerializeField] private GameObject orcPrefab;
        [SerializeField] private int orcDifficulty;
        [SerializeField] private GameObject golemPrefab;
        [SerializeField] private int golemDifficulty;

        [Header("Spawn Weights")]
        [SerializeField] private byte zombieSpawnWeight;
        [SerializeField] private byte orcSpawnWeight;
        [SerializeField] private byte golemSpawnWeight;

        [Header("Spawn Points")]
        [SerializeField] private SpriteRenderer[] spawnPoints;

        #endregion

        #region Internal Fields

        private int currentWave;

        // How many points are left in this wave to spawn
        private int diffPointsRemaining;

        // How many points are currently in the field
        private int currentDiffPoints;

        // The total of all the weights
        private int weightTotal;

        // The next enemy to spawn
        private EnemyType enemyToSpawn;

        // Has this spawner already started spawning or not
        private bool hasStarted;

        // List of enemies that is currently spawned and active in the scene
        private List<Transform> enemiesInScene = new List<Transform>();

        #endregion

        #region Cached References

        // Reference to the player character
        private Transform player;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            SetWeights();
        }

        private void Update()
        {
            waveText.text = currentWave.ToString();
        }

        private void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
            CustomHealth.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
            CustomHealth.OnEnemyDied -= HandleEnemyDied;
        }

        #endregion

        #region Event Callbacks

        // Gets called with type levelstart once the scene has been setup and player reference is available
        // Also gets called on unpausing weirdly enough, so with a bool check if the game is already running
        void MMEventListener<TopDownEngineEvent>.OnMMEvent(TopDownEngineEvent eventType)
        {
            if (hasStarted) { return; }
            else hasStarted = true;

            if (eventType.EventType == TopDownEngineEventTypes.LevelStart)
            {
                player = LevelManager.Instance.Players[0].transform;
            }
            StartWave(1);
        }

        #endregion

        #region Private Methods

        private void StartWave(int wave)
        {
            currentWave = wave;

            // Sets the difficulty target for this round
            diffPointsRemaining = (int)difficultyPoints;

            StartCoroutine(SpawnEnemies());
        }

        private void EndWave()
        {
            currentWave++;

            difficultyPoints *= difficultyMultiplier;
            timeBetweenSpawns *= difficultyMultiplier;
            minDifficulty *= difficultyMultiplier;

            StartWave(currentWave);
        }

        private void SpawnEnemy(EnemyType enemyType, Transform spawnTransform)
        {
            Transform newEnemy = null;

            switch (enemyType)
            {
                case EnemyType.Zombie:
                    newEnemy = PoolManager.Pools["EnemyPool"].Spawn(zombiePrefab, spawnTransform.position, spawnTransform.rotation);
                    break;
                case EnemyType.Orc:
                    newEnemy = PoolManager.Pools["EnemyPool"].Spawn(orcPrefab, spawnTransform.position, spawnTransform.rotation);
                    break;
                case EnemyType.Golem:
                    newEnemy = PoolManager.Pools["EnemyPool"].Spawn(golemPrefab, spawnTransform.position, spawnTransform.rotation);
                    break;
            }

            if (newEnemy != null)
            {
                newEnemy.GetComponent<Health>().Revive();
                newEnemy.GetComponent<AIBrain>().Target = player;
                enemiesInScene.Add(newEnemy);
            }
        }

        // Called on start to properly set the total spawn weight
        private void SetWeights()
        {
            weightTotal = zombieSpawnWeight + orcSpawnWeight + golemSpawnWeight;
        }

        // Adds/removes difficulty points when we spawn an enemy
        private void HandleEnemySpawnedPoints(EnemyType enemyType)
        {
            switch (enemyType)
            {
                case EnemyType.Zombie:

                    diffPointsRemaining -= zombieDifficulty;
                    currentDiffPoints += zombieDifficulty;

                    break;
                case EnemyType.Orc:

                    diffPointsRemaining -= orcDifficulty;
                    currentDiffPoints += orcDifficulty;

                    break;
                case EnemyType.Golem:

                    diffPointsRemaining -= golemDifficulty;
                    currentDiffPoints += golemDifficulty;

                    break;
            }
        }

        // Handles the despawning, reducing points and checking if a new wave should start
        private void HandleEnemyDied(Transform enemyTransform, EnemyType enemyType)
        {
            // Despawn the object
            PoolManager.Pools["EnemyPool"].Despawn(enemyTransform);


            // Remove the points
            switch (enemyType)
            {
                case EnemyType.Zombie:
                    currentDiffPoints -= zombieDifficulty;
                    break;
                case EnemyType.Orc:
                    currentDiffPoints -= orcDifficulty;
                    break;
                case EnemyType.Golem:
                    currentDiffPoints -= golemDifficulty;
                    break;
            }


            // Remove from list
            enemiesInScene.Remove(enemyTransform);

            // If a new wave should start, start it
            if (GetHasPointTargetBeenReached() && enemiesInScene.Count == 0)
            {
                EndWave();
            }
        }

        // Returns a random enemytype to spawn based on weights
        private EnemyType GetNextEnemyToSpawn()
        {
            int roll = UnityEngine.Random.Range(1, weightTotal + 1);

            if (roll <= zombieSpawnWeight) { return EnemyType.Zombie; }
            else if (roll <= (zombieSpawnWeight + orcSpawnWeight)) { return EnemyType.Orc; }
            else return EnemyType.Golem;
        }

        // Returns a random spawnpoint
        private Transform GetNextSpawnPoint()
        {
            Transform newSpawnPoint = null;
            while (newSpawnPoint == null)
            {
                int roll = UnityEngine.Random.Range(0, spawnPoints.Length);
                if (!spawnPoints[roll].isVisible)
                {
                    newSpawnPoint = spawnPoints[roll].transform;
                    break;
                }
            }
            return newSpawnPoint;
        }

        // Returns false if there aren't enough enemies in the field
        private bool GetIsMinimumDifficultyAchieved()
        {
            if (currentDiffPoints < minDifficulty) { return false; }
            else return true;
        }

        // returns true if points target for this wave has been reached
        private bool GetHasPointTargetBeenReached()
        {
            if (diffPointsRemaining <= 0) { return true; }
            else return false;
        }

        #endregion

        #region Coroutines

        private IEnumerator SpawnEnemies()
        {
            while (!GetHasPointTargetBeenReached())
            {
                EnemyType enemyToSpawn = GetNextEnemyToSpawn();
                SpawnEnemy(enemyToSpawn, GetNextSpawnPoint());
                HandleEnemySpawnedPoints(enemyToSpawn);
                if (GetIsMinimumDifficultyAchieved())
                {
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }
                else
                {
                    yield return new WaitForSeconds(quickTimeBetweenSpawns);
                }
            }
        }

        #endregion
    }
}