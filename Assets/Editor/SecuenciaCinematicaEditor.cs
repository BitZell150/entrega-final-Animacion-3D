using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

[CustomEditor(typeof(SecuenciaCinematica))]
public class SecuenciaCinematicaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SecuenciaCinematica script = (SecuenciaCinematica)target;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Cargar Sprites desde Carpeta", GUILayout.Height(30)))
        {
            CargarSprites(script);
        }
        
        GUI.backgroundColor = Color.white;
    }

    private void CargarSprites(SecuenciaCinematica script)
    {
        string path = AssetDatabase.GetAssetPath(script);
        string directory = Path.GetDirectoryName(path);

        // Buscamos todos los archivos de imagen en la misma carpeta que el Asset
        string[] GUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { directory });
        
        var sprites = GUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(s => s.name) // Los ordena por nombre (01, 02, 03...)
            .ToArray();

        if (sprites.Length > 0)
        {
            script.frames = sprites;
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SecuenciaEditor] Se cargaron {sprites.Length} sprites automáticamente.");
        }
        else
        {
            Debug.LogWarning("No se encontraron Sprites en la misma carpeta que este Asset. Asegúrate de que las imágenes estén configuradas como 'Sprite (2D and UI)'.");
        }
    }
}