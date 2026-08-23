using UnityEngine;
using System.Collections.Generic;

public class EnemyManager
{
    private readonly float cellSize;
    private readonly Dictionary<Vector2Int, HashSet<Transform>> grid = new Dictionary<Vector2Int, HashSet<Transform>>();
    private readonly Dictionary<Transform, Vector2Int> enemyCellMap = new Dictionary<Transform, Vector2Int>();

    public EnemyManager(float cellSize = 5f)
    {
        this.cellSize = cellSize;
    }

    private Vector2Int ToCell(Vector3 pos) =>
        new Vector2Int(Mathf.FloorToInt(pos.x / cellSize), Mathf.FloorToInt(pos.y / cellSize));

    public void Register(Transform enemy)
    {
        var cell = ToCell(enemy.position);
        if (!grid.TryGetValue(cell, out var set))
        {
            set = new HashSet<Transform>();
            grid[cell] = set;
        }
        set.Add(enemy);
        enemyCellMap[enemy] = cell;
    }

    public void Unregister(Transform enemy)
    {
        if (enemyCellMap.TryGetValue(enemy, out var cell))
        {
            if (grid.TryGetValue(cell, out var set))
                set.Remove(enemy);
            enemyCellMap.Remove(enemy);
        }
    }

    // 적이 이동할 때 호출 (몬스터 Update 또는 이동 로직에서 호출)
    public void UpdatePosition(Transform enemy)
    {
        var newCell = ToCell(enemy.position);
        if (enemyCellMap.TryGetValue(enemy, out var oldCell))
        {
            if (oldCell == newCell) return; // 같은 셀이면 아무 것도 안 함 (대부분의 프레임이 여기 해당)
            grid[oldCell].Remove(enemy);
        }
        if (!grid.TryGetValue(newCell, out var set))
        {
            set = new HashSet<Transform>();
            grid[newCell] = set;
        }
        set.Add(enemy);
        enemyCellMap[enemy] = newCell;
    }

    public Transform FindNearest(Vector3 origin, float maxRadius)
    {
        Vector2Int originCell = ToCell(origin);
        int maxRing = Mathf.CeilToInt(maxRadius / cellSize);

        Transform nearest = null;
        float minSqrDist = float.MaxValue;

        for (int ring = 0; ring <= maxRing; ring++)
        {
            foreach (var cell in GetRingCells(originCell, ring))
            {
                if (!grid.TryGetValue(cell, out var enemies)) continue;

                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    float sqrDist = ((Vector2)enemy.position - (Vector2)origin).sqrMagnitude;
                    if (sqrDist <= maxRadius * maxRadius && sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        nearest = enemy;
                    }
                }
            }

            if (nearest != null)
            {
                float nextRingMinDist = ring * cellSize;
                if (nextRingMinDist * nextRingMinDist > minSqrDist)
                    break;
            }
        }

        return nearest;
    }

    private IEnumerable<Vector2Int> GetRingCells(Vector2Int center, int ring)
    {
        if (ring == 0)
        {
            yield return center;
            yield break;
        }
        for (int x = -ring; x <= ring; x++)
        {
            yield return new Vector2Int(center.x + x, center.y - ring);
            yield return new Vector2Int(center.x + x, center.y + ring);
        }
        for (int y = -ring + 1; y <= ring - 1; y++)
        {
            yield return new Vector2Int(center.x - ring, center.y + y);
            yield return new Vector2Int(center.x + ring, center.y + y);
        }
    }
}
