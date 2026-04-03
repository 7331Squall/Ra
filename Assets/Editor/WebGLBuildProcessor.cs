using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class WebGLBuildProcessor : IPostprocessBuildWithReport {
    public int callbackOrder => 0;
    private const string ProjectName = "{{PROJNAME}}";
    private const string ProjectVersion = "{{PROJVER}}";
    private const string Output = "{{OUTPUT}}";
    private const string OutputExt = "{{EXTENSION}}";

    public void OnPreprocessBuild(BuildReport report) {
        // Lê versão atual
        string version = PlayerSettings.bundleVersion; // Mesma coisa que Application.version
        string[] parts = version.Split('.');
        int major = parts.Length > 0 ? int.Parse(parts[0]) : 1;
        int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
        // Incrementa o patch
        patch++;
        string newVersion = $"{major}.{minor}.{patch}";
        PlayerSettings.bundleVersion = newVersion;
        Debug.Log($"Versão atualizada para: {newVersion}");
    }

    public void OnPostprocessBuild(BuildReport report) {
        if (report.summary.platform != BuildTarget.WebGL) return;

        // Pega o nome da pasta final da build
        string projName = Application.productName;
        string projVersion = $"v{PlayerSettings.bundleVersion}";
        string outputFolder = Path.GetFileName(report.summary.outputPath.TrimEnd(Path.DirectorySeparatorChar));
        string extension = GetCompressionExtension();

        // Caminho do index.html gerado pela Unity
        string indexPath = Path.Combine(report.summary.outputPath, "index.html");
        if (File.Exists(indexPath)) {
            string html = File.ReadAllText(indexPath);
            html = html.Replace(Output, outputFolder)
                       .Replace(OutputExt, extension)
                       .Replace(ProjectName, projName)
                       .Replace(ProjectVersion, projVersion);
            File.WriteAllText(indexPath, html);
            Debug.Log($"[BuildProcessor] Substituído '{Output}' por '{outputFolder}' no index.html");
            Debug.Log($"[BuildProcessor] Substituído '{OutputExt}' por '{extension}' no index.html");
            Debug.Log($"[BuildProcessor] Substituído '{ProjectName}' por '{projName}' no index.html");
            Debug.Log($"[BuildProcessor] Substituído '{ProjectVersion}' por '{projVersion}' no index.html");
        } else { Debug.LogWarning("[BuildProcessor] index.html não encontrado na pasta de build."); }
    }

    string GetCompressionExtension() {
        switch (PlayerSettings.WebGL.compressionFormat) {
            case WebGLCompressionFormat.Gzip:   return ".gz";
            case WebGLCompressionFormat.Brotli: return ".br";
            case WebGLCompressionFormat.Disabled:
            default: return "";
        }
    }
}