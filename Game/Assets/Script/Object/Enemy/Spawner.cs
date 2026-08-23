using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using System.Collections.Generic;

public class Spawner : MonoBehaviour, IChannel
{
    private JobHandle jobHandle;
    private NativeArray<Vector3> currentPositions;
    private NativeArray<Vector3> resultPositions;

    private GameObject Player;
    private Enemy EnemyOrigin;
    private EnemyManager enemyManager;
    private List<Enemy> enemyList = new List<Enemy>();
    private List<Enemy> removeEnemy = new List<Enemy>();

    private Factory factory;
    private int initHP = 4;
    private int level = 1;
    private WaveSpawnData waveData;

    private int waveIndex = 0;
    private bool isSpawnStart = false;

    private async void Awake()
    {
        await EnemyResourceSetting();
    }

    private void Start()
    {
        factory = Locator<Factory>.Get();
        enemyManager = Locator<EnemyManager>.Get();
        var battleManager = Locator<BattleManager>.Get();
        var player = battleManager.GetPlayer();
        Player = player.gameObject;
        battleManager.SetSpawner(this);
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.GameOver, HandleEvent);
        eventManager.Subscription(ChannelInfo.GameReset, HandleEvent);
        eventManager.Subscription(ChannelInfo.SpawnMonster, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.GameOver, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.GameReset, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.SpawnMonster, HandleEvent);
    }

    private void FixedUpdate()
    {
        if(removeEnemy.Count > 0)
        {
            foreach(var enemy in removeEnemy) {
                enemyList.Remove(enemy);
            }
            removeEnemy.Clear();
        }

        int enemyCount = enemyList.Count;
        if (enemyCount == 0) return;

        // NativeArray 크기가 변했으면 재할당
        if (!currentPositions.IsCreated || currentPositions.Length != enemyCount)
        {
            if (currentPositions.IsCreated) currentPositions.Dispose();
            if (resultPositions.IsCreated) resultPositions.Dispose();
            currentPositions = new NativeArray<Vector3>(enemyCount, Allocator.Persistent);
            resultPositions = new NativeArray<Vector3>(enemyCount, Allocator.Persistent);
        }

        // 1. 현재 Rigidbody 위치를 NativeArray에 채움
        for (int i = 0; i < enemyCount; i++)
            currentPositions[i] = enemyList[i].rigidBody.position;

        // 2. Job 실행 (병렬 계산)
        var job = new EnemyMoveJob
        {
            currentPositions = currentPositions,
            targetPosition = Player.transform.position,
            moveSpeed = EnemyOrigin.moveSpeed,
            deltaTime = Time.fixedDeltaTime,
            resultPositions = resultPositions
        };

        jobHandle = job.Schedule(enemyCount, 64);
        jobHandle.Complete();

        enemyManager = Locator<EnemyManager>.Get();

        for (int i = 0; i < enemyCount; i++)
        {
            enemyList[i].rigidBody.MovePosition(resultPositions[i]);
            enemyManager.UpdatePosition(enemyList[i].transform);
        }
    }

    private void Update()
    {
        if(isSpawnStart) {
            if(enemyList.Count == 0)
            {
                if (waveData.IsFinalIndex(waveIndex)) {
                    var eventManager = Locator<EventManager>.Get();
                    eventManager.Notify(ChannelInfo.GameClear, true);
                    GameClear();
                    return;
                }

                AddRandomEnemy(); 
            }
        }
    }


    public void AddRandomEnemy()
    {
        var wavePositions =  waveData.GetPositions(waveIndex);

        for(int i = 0; i < wavePositions.Count; i++)
        {
            Vector3 position = wavePositions[i];

            var enemy = factory.Create<Enemy>(EnemyOrigin, position, Quaternion.identity);
            enemy.SetUp(Player);

            int random = Random.Range((int)EnemyType.Slime, (int)EnemyType.Large + 1);
            float randomMoveSpeed = Random.Range(0.5f, 2f);
            enemy.EnemySetting((EnemyType)random);
            enemy.SetHP(initHP * (waveIndex + 1));
            enemy.moveSpeed = randomMoveSpeed;
            enemyList.Add(enemy);
        }
        waveIndex++;
    }

    public void AddEnemy(EnemyType type)
    {
        var enemy = factory.Create<Enemy>(EnemyOrigin, Vector3.zero, Quaternion.identity);
        enemy.SetUp(Player);
        enemy.EnemySetting(type);
        enemy.SetHP(initHP);
        enemyList.Add(enemy);
    }

    public void RemoveEnmey(Enemy enemy)
    {
        // List에서 빼야한다.
        if (!removeEnemy.Contains(enemy))
            removeEnemy.Add(enemy);
    }

    private void OnDestroy()
    {
        if (currentPositions.IsCreated) currentPositions.Dispose();
        if (resultPositions.IsCreated) resultPositions.Dispose();
    }

    private async UniTask EnemyResourceSetting()
    {
        var resourceManager = Locator<ResourceManager>.Get();
        var enemyObjectOrigin = await resourceManager.Get<GameObject>("Enemy");

        waveData = await resourceManager.Get<WaveSpawnData>("waveSpawnData");
        EnemyOrigin = enemyObjectOrigin.GetComponent<Enemy>();
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.SpawnMonster:
                isSpawnStart = true;
                break;
            case ChannelInfo.GameOver:
                waveIndex = 0;
                isSpawnStart = false;
                break;
            case ChannelInfo.GameReset:
                waveIndex = 0;
                isSpawnStart = false;
                break;
        }
    }

    private void GameClear()
    {
        waveIndex = 0;
        isSpawnStart = false;
    }
}
