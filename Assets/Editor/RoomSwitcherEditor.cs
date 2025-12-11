using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(RoomSwitcher))]
public class RoomSwitcherEditor : Editor
{
    static RoomSwitcher linkingSource = null;
    static bool isDragging = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomSwitcher rs = (RoomSwitcher)target;

        GUILayout.Space(6);

        if (GUILayout.Button("Clear Target"))
        {
            Undo.RecordObject(rs, "Clear RoomSwitcher Target");
            rs.targetTransform = null;
            EditorUtility.SetDirty(rs);
            EditorSceneManager.MarkSceneDirty(rs.gameObject.scene);
        }

        if (GUILayout.Button("Select Target"))
        {
            if (rs.targetTransform != null)
                Selection.activeGameObject = rs.targetTransform.gameObject;
        }
    }

    // OnSceneGUI removed to use global RoomSwitcherGizmos


    [MenuItem("GameObject/2D/Room Node", false, 10)]
    static void CreateRoomNode(MenuCommand menuCommand)
    {
        Vector3 spawnPos = Vector3.zero;
        if (SceneView.lastActiveSceneView != null)
            spawnPos = SceneView.lastActiveSceneView.pivot;

        GameObject go = new GameObject("RoomNode");
        Undo.RegisterCreatedObjectUndo(go, "Create RoomNode");
        go.transform.position = spawnPos;

        go.AddComponent<RoomSwitcher>();
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Selection.activeObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
}
