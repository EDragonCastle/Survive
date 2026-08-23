using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [SerializeField]
    private GameObject hp;

    
    public void SetHP(int currentHP, int maxHP)
    {
        if (currentHP <= 0)
            currentHP = 0;

        float ratio = (float)currentHP/maxHP;

        Vector3 scale = Vector3.one;
        scale.x = ratio;

        hp.transform.localScale = scale;
    }
}
