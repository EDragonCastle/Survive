using UnityEngine;

[DefaultExecutionOrder(-99)]
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        var eventManager = new EventManager();
        Locator<EventManager>.Provide(eventManager);

        var resourceManager = new ResourceManager();
        resourceManager.Initalize();
        Locator<ResourceManager>.Provide(resourceManager);

        var factory = new Factory();
        Locator<Factory>.Provide(factory);

        var enemyManager = new EnemyManager();
        Locator<EnemyManager>.Provide(enemyManager);

        var battleManager = new BattleManager();
        Locator<BattleManager>.Provide(battleManager);

        var soundManager = new SoundManager();
        Locator<SoundManager>.Provide(soundManager);
    }
}
