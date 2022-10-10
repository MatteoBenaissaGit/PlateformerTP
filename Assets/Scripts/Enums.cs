using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right
    }

    public enum EnemyState
    {
        IdlePassive,
        RunToPlayer,
        IdleAttack,
        Flee,
    }
}
