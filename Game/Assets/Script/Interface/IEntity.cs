
public interface IEntity
{
    public void OnSpawn();
    public void OnDespawn();

    public int GetObjectKey();
    public void SetObjectKey(int _key);
    public void SetTransform(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, float multiplier = 1, UnityEngine.Transform parent = null);
}