using UnityEngine;
using System;
using MoreMountains.TopDownEngine;
using NotZombiller;

public class CustomHealth : Health
{
    public static event Action<Transform, EnemyType> OnEnemyDied;

    [SerializeField] private EnemyType enemyType;

    // Do the base killing stuff plus add some custom logic to talk with the wave manager
    public override void Kill()
    {
        base.Kill();
        OnEnemyDied?.Invoke(this.transform, this.enemyType);
    }
}
