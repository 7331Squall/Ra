using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AutoIncrementVersion : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Lê a versão atual
        string currentVersion = PlayerSettings.bundleVersion;

        // Garante que tenha formato "x.y.z"
        string[] parts = currentVersion.Split('.');
        int major = 1, minor = 0, patch = 0;

        if (parts.Length > 0) int.TryParse(parts[0], out major);
        if (parts.Length > 1) int.TryParse(parts[1], out minor);
        if (parts.Length > 2) int.TryParse(parts[2], out patch);

        // Incrementa o patch
        patch++;

        // Nova versão no formato "x.y.z"
        string newVersion = $"{major}.{minor}.{patch}";

        // Salva no PlayerSettings (isso grava no arquivo ProjectSettings.asset)
        PlayerSettings.bundleVersion = newVersion;

        // Também atualiza o código de versão (Android e iOS)
        PlayerSettings.Android.bundleVersionCode++;
        if (int.TryParse(PlayerSettings.iOS.buildNumber, out int iosBuildNum))
            PlayerSettings.iOS.buildNumber = (iosBuildNum + 1).ToString();
        else
            PlayerSettings.iOS.buildNumber = "1";

        // Mostra no console
        Debug.Log($"[AutoIncrementVersion] Versão atualizada de {currentVersion} → {newVersion}");
    }
}