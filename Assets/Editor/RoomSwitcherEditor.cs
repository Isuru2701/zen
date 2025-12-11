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

    void OnSceneGUI()
    {
        RoomSwitcher rs = (RoomSwitcher)target;
        Transform t = rs.transform;
        Vector3 pos = t.position;

        // Draw node marker
        Handles.color = Color.cyan;
        float handleSize = HandleUtility.GetHandleSize(pos) * 0.15f;
        Handles.DrawWireDisc(pos, Vector3.forward, handleSize * 1.5f);

        // clickable handle to start linking
        if (Handles.Button(pos, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
        {
            linkingSource = rs;
            isDragging = true;
            // consume event so scene view doesn't also select
            Event.current.Use();
        }

        // draw existing target connection
        if (rs.targetTransform != null)
        {
            Handles.color = Color.yellow;
            Vector3 tgt = rs.targetTransform.position;
            Handles.DrawLine(pos, tgt);
            // arrow head
            Vector3 dir = (tgt - pos).normalized;
            Vector3 arrowPos = Vector3.Lerp(pos, tgt, 0.8f);
            Handles.ConeHandleCap(0, arrowPos, Quaternion.LookRotation(Vector3.forward, dir), handleSize * 2f, EventType.Repaint);
        }

        // handle dragging to connect
        if (isDragging && linkingSource != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            float enter;
            Vector3 mouseWorld = Vector3.zero;
            if (plane.Raycast(ray, out enter)) mouseWorld = ray.GetPoint(enter);

            Handles.color = Color.green;
            Handles.DrawLine(linkingSource.transform.position, mouseWorld);
            SceneView.RepaintAll();

            if (Event.current.type == EventType.MouseUp)
            {
                GameObject picked = HandleUtility.PickGameObject(Event.current.mousePosition, false);
                if (picked != null)
                {
                    RoomSwitcher pickedRs = picked.GetComponent<RoomSwitcher>();
                    if (pickedRs != null && pickedRs != linkingSource)
                    {
                        Undo.RecordObject(linkingSource, "Set RoomSwitcher Target");
                        linkingSource.targetTransform = pickedRs.transform;
                        EditorUtility.SetDirty(linkingSource);
                        EditorSceneManager.MarkSceneDirty(linkingSource.gameObject.scene);
                    }
                }
                linkingSource = null;
                isDragging = false;
                Event.current.Use();
            }

            if (Event.current.type == EventType.ContextClick)
            {
                linkingSource = null;
                isDragging = false;
                Event.current.Use();
            }
        }
    }

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
