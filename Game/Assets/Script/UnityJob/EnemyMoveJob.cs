using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;

public struct EnemyMoveJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> currentPositions;
    [ReadOnly] public Vector3 targetPosition;
    [ReadOnly] public float moveSpeed;
    [ReadOnly] public float deltaTime;

    public NativeArray<Vector3> resultPositions;

    public void Execute(int index)
    {
        Vector3 dir = (targetPosition - currentPositions[index]).normalized;
        resultPositions[index] = currentPositions[index] + dir * moveSpeed * deltaTime;
    }

}
