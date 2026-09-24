using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
#endif

public class GitVersionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text versionText;

    private void Start()
    {
#if UNITY_EDITOR
        string commitCount = RunGit("rev-list --count HEAD");
        string commitHash = RunGit("rev-parse --short HEAD");
        string branch = RunGit("branch --show-current");

        versionText.text = $"Build {commitCount} | {commitHash} | {branch}";
#else
        versionText.gameObject.SetActive(false);
#endif
    }

#if UNITY_EDITOR
    private string RunGit(string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo);

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output.Trim();
    }
#endif
}