using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RoomSwitcherGizmos
{
    static RoomSwitcher linkingSource = null;
    static bool isDragging = false;
    static bool showGizmos = true;

    static RoomSwitcherGizmos()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sv)
    {
        // Draw toggle button
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 150, 30));
        if (GUILayout.Button(showGizmos ? "Hide Room Nodes" : "Show Room Nodes"))
        {
            showGizmos = !showGizmos;
            if (!showGizmos)
            {
                linkingSource = null;
                isDragging = false;
                GUIUtility.hotControl = 0;
            }
            SceneView.RepaintAll();
        }
        GUILayout.EndArea();
        Handles.EndGUI();

        if (!showGizmos) return;

        var all = Object.FindObjectsOfType<RoomSwitcher>();
        if (all == null || all.Length == 0)
        {
            linkingSource = null;
            isDragging = false;
            return;
        }

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        // Find hovered node
        RoomSwitcher hoveredNode = null;
        foreach (var rs in all)
        {
            if (rs == null) continue;
            Vector3 pos = rs.transform.position;
            float handleSize = HandleUtility.GetHandleSize(pos) * 0.12f;
            float dist = HandleUtility.DistanceToCircle(pos, handleSize * 1.6f);
            
            if (dist < 10f) // 10 pixel tolerance
            {
                hoveredNode = rs;
                break;
            }
        }

        // Handle mouse events
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && hoveredNode != null)
                {
                    linkingSource = hoveredNode;
                    isDragging = true;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (isDragging && GUIUtility.hotControl == controlID)
                {
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (e.button == 0 && isDragging && GUIUtility.hotControl == controlID)
                {
                    if (hoveredNode != null && hoveredNode != linkingSource)
                    {
                        // Set bidirectional connection
                        Undo.RecordObjects(new Object[] { linkingSource, hoveredNode }, "Link RoomSwitchers");
                        linkingSource.targetTransform = hoveredNode.transform;
                        hoveredNode.targetTransform = linkingSource.transform;
                        EditorUtility.SetDirty(linkingSource);
                        EditorUtility.SetDirty(hoveredNode);
                        EditorSceneManager.MarkSceneDirty(linkingSource.gameObject.scene);
                    }
                    
                    linkingSource = null;
                    isDragging = false;
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;

            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Escape && isDragging)
                {
                    linkingSource = null;
                    isDragging = false;
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
        }

        // Draw all nodes
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        
        foreach (var rs in all)
        {
            if (rs == null) continue;
            Vector3 pos = rs.transform.position;
            float handleSize = HandleUtility.GetHandleSize(pos) * 0.12f;

            // Highlight hovered node
            Handles.color = (rs == hoveredNode) ? Color.white : Color.cyan;
            Handles.DrawWireDisc(pos, Vector3.forward, handleSize * 1.6f);
            Handles.DotHandleCap(0, pos, Quaternion.identity, handleSize, EventType.Repaint);

            // Name label
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = (rs == hoveredNode) ? Color.white : Color.cyan;
            Handles.Label(pos + Vector3.up * handleSize * 1.6f, rs.gameObject.name, style);

            // Connection line
            if (rs.targetTransform != null)
            {
                Handles.color = Color.yellow;
                Vector3 tgt = rs.targetTransform.position;
                Handles.DrawAAPolyLine(3f, pos, tgt);
                
                // Arrow head
                Vector3 dir = (tgt - pos).normalized;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Vector3 arrowPos = Vector3.Lerp(pos, tgt, 0.8f);
                    Handles.ConeHandleCap(0, arrowPos, Quaternion.LookRotation(Vector3.forward, dir), handleSize * 1.5f, EventType.Repaint);
                }
            }
        }

        // Draw drag line
        if (isDragging && linkingSource != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.forward, linkingSource.transform.position);
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 mouseWorld = ray.GetPoint(enter);
                Handles.color = Color.green;
                Handles.DrawLine(linkingSource.transform.position, mouseWorld);
            }
            SceneView.RepaintAll();
        }
    }
}
