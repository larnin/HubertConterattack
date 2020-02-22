using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class DraggablePoint2DDrawer : Editor
{
    public void OnSceneGUI()
    {
        var property = serializedObject.GetIterator();
        while (property.Next(true))
        {
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var field = serializedObject.targetObject.GetType().GetField(property.name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (field == null)
                {
                    continue;
                }

                var draggablePoints = field.GetCustomAttributes(typeof(DraggablePoint2DAttribute), false);
                if (draggablePoints.Length > 0)
                {
                    Handles.Label(property.vector2Value, property.name);
                    property.vector2Value = Handles.PositionHandle(property.vector2Value, Quaternion.identity);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}