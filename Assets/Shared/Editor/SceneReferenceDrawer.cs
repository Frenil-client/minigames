using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var assetProp = property.FindPropertyRelative("sceneAsset");
        var pathProp  = property.FindPropertyRelative("scenePath");

        EditorGUI.BeginChangeCheck();
        EditorGUI.ObjectField(position, assetProp, typeof(SceneAsset), label);

        if (EditorGUI.EndChangeCheck())
        {
            var asset = assetProp.objectReferenceValue as SceneAsset;
            pathProp.stringValue = asset != null
                ? AssetDatabase.GetAssetPath(asset)
                : string.Empty;
        }

        EditorGUI.EndProperty();
    }
}
