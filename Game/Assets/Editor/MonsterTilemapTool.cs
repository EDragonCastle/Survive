#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MonsterTilemapTool : EditorWindow
{
    private List<Tilemap> waveTilemaps = new List<Tilemap>();
    private WaveSpawnData targetData;

    [MenuItem("Tools/Wave Spawn Data Baker")]
    private static void OpenWindow()
    {
        GetWindow<MonsterTilemapTool>("Wave Spawn Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Wave Spawn Data 굽기", EditorStyles.boldLabel);

        targetData = (WaveSpawnData)EditorGUILayout.ObjectField(
            "저장할 SO", targetData, typeof(WaveSpawnData), false);

        EditorGUILayout.Space();
        GUILayout.Label("Wave별 Tilemap (순서대로)");

        int removeIndex = -1;
        for (int i = 0; i < waveTilemaps.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            waveTilemaps[i] = (Tilemap)EditorGUILayout.ObjectField(
                $"Wave {i + 1}", waveTilemaps[i], typeof(Tilemap), true);
            if (GUILayout.Button("-", GUILayout.Width(20)))
                removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0) waveTilemaps.RemoveAt(removeIndex);

        if (GUILayout.Button("Tilemap 추가"))
            waveTilemaps.Add(null);

        EditorGUILayout.Space();

        GUI.enabled = targetData != null && waveTilemaps.Count > 0;
        if (GUILayout.Button("굽기 (Bake)"))
        {
            Bake();
        }
        GUI.enabled = true;
    }

    private void Bake()
    {
        targetData.waves.Clear();

        for (int i = 0; i < waveTilemaps.Count; i++)
        {
            var tilemap = waveTilemaps[i];
            if (tilemap == null) continue;

            var entry = new WaveSpawnData.WaveEntry { wave = i + 1 };
            BoundsInt bounds = tilemap.cellBounds;

            foreach (var cell in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cell))
                    entry.positions.Add(tilemap.GetCellCenterWorld(cell));
            }

            targetData.waves.Add(entry);
        }

        EditorUtility.SetDirty(targetData);
        AssetDatabase.SaveAssets();

        Debug.Log($"Wave Spawn Data 저장 완료. 총 {targetData.waves.Count}개 Wave, " +
                   $"{CountTotalPositions(targetData)}개 위치.");
    }

    private int CountTotalPositions(WaveSpawnData data)
    {
        int total = 0;
        foreach (var entry in data.waves) total += entry.positions.Count;
        return total;
    }
}
#endif