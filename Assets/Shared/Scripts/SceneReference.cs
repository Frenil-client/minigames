using UnityEngine;

[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif
    [SerializeField] private string scenePath;

    public string path    => scenePath;
    public bool   isValid => !string.IsNullOrEmpty(scenePath);
}
