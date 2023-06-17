using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemySFM> enemies;

    public void NotifyOtherEnemies(EnemySFM enemy)
    {
        foreach (var e in enemies)
        {
            if (!e.Equals(enemy))
            {
                e.playerFounded = true;
            }
        }
    }
}
