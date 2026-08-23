
public class BattleManager
{
    private Player player;
    private Spawner spawner;
    private int level = 1;
    private Stat addStat;

    public void ProvidePlayer(Player input) => player = input;
    public Player GetPlayer() => player;

    public void SetSpawner(Spawner input) => spawner = input;
    public Spawner GetSpawner() => spawner;

    public void SetLevel(int level) => this.level = level;
    public int GetLevel() => level;

    public void SetStat(Stat input) => addStat = input;
    public Stat GetStat() => addStat;
}
