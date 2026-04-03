using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

public class FindURPLitShaders : EditorWindow
{
    [MenuItem("Tools/Encontrar URP/Lit na Cena")]
    public static void ShowWindow()
    {
        GetWindow<FindURPLitShaders>("URP/Lit Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Procurar objetos com URP/Lit"))
        {
            FindObjectsWithURPLit();
        }
    }

    private void FindObjectsWithURPLit()
    {
        string targetShaderName = "Universal Render Pipeline/Lit";
        int count = 0;
        List<string> results = new List<string>();

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat != null && mat.shader != null && mat.shader.name == targetShaderName)
                {
                    results.Add(renderer.gameObject.name);
                    count++;
                    break;
                }
            }
        }

        if (count > 0)
        {
            Debug.Log($"Encontrados {count} objetos usando o shader '{targetShaderName}':");
            foreach (var objName in results)
                Debug.Log(objName);
        }
        else
        {
            Debug.Log($"Nenhum objeto com o shader '{targetShaderName}' foi encontrado.");
        }
    }
}
