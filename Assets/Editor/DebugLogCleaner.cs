#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class DebugLogCleaner : EditorWindow
{
    private string m_targetFolder = "Assets/02.Scripts";
    private bool m_includeSubfolders = true;
    private bool m_previewOnly = true;
    private Vector2 m_scrollPosition;
    private string m_previewText = "";
    
    // 백업 관련 상수
    private const string BACKUP_PREFIX = "DebugLogCleaner_Backup_";
    private const string BACKUP_COUNT_KEY = "DebugLogCleaner_BackupCount";
    
    private Dictionary<string, string> m_lastBackup = new Dictionary<string, string>();
    private bool m_hasBackup = false;

    [MenuItem("Tools/Debug Log Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<DebugLogCleaner>("Debug Log Cleaner");
    }

    private void OnEnable()
    {
        LoadBackup();
    }

    private void LoadBackup()
    {
        m_lastBackup.Clear();
        int backupCount = EditorPrefs.GetInt(BACKUP_COUNT_KEY, 0);
        
        if (backupCount > 0)
        {
            for (int i = 0; i < backupCount; i++)
            {
                string fileKey = $"{BACKUP_PREFIX}File_{i}";
                string contentKey = $"{BACKUP_PREFIX}Content_{i}";
                
                if (EditorPrefs.HasKey(fileKey) && EditorPrefs.HasKey(contentKey))
                {
                    string file = EditorPrefs.GetString(fileKey);
                    string content = EditorPrefs.GetString(contentKey);
                    m_lastBackup[file] = content;
                }
            }
            
            m_hasBackup = m_lastBackup.Count > 0;
        }
    }

    private void SaveBackup()
    {
        // 기존 백업 키 모두 삭제
        var keys = new string[] { BACKUP_COUNT_KEY };
        foreach (var key in keys) EditorPrefs.DeleteKey(key);
        
        // 새로운 백업 저장
        int index = 0;
        foreach (var backup in m_lastBackup)
        {
            string fileKey = $"{BACKUP_PREFIX}File_{index}";
            string contentKey = $"{BACKUP_PREFIX}Content_{index}";
            
            EditorPrefs.SetString(fileKey, backup.Key);
            EditorPrefs.SetString(contentKey, backup.Value);
            index++;
        }
        
        EditorPrefs.SetInt(BACKUP_COUNT_KEY, m_lastBackup.Count);
    }

    private void OnGUI()
    {
        GUILayout.Label("Debug Log Cleaner", EditorStyles.boldLabel);

        m_targetFolder = EditorGUILayout.TextField("Target Folder", m_targetFolder);
        m_includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", m_includeSubfolders);
        m_previewOnly = EditorGUILayout.Toggle("Preview Only", m_previewOnly);

        EditorGUILayout.Space();
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clean Debug Logs"))
            {
                CleanDebugLogs();
            }

            GUI.enabled = m_hasBackup;
            if (GUILayout.Button("Undo Last Clean"))
            {
                UndoLastClean();
            }
            GUI.enabled = true;
        }

        if (!string.IsNullOrEmpty(m_previewText))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview of changes:", EditorStyles.boldLabel);
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(m_previewText);
            EditorGUILayout.EndScrollView();
        }
    }

    private void UndoLastClean()
    {
        if (!m_hasBackup || m_lastBackup.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No backup available to restore!", "OK");
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Confirm Restore",
            "This will restore all files to their state before the last clean operation. Continue?",
            "Yes", "No");

        if (!confirmed) return;

        foreach (var backup in m_lastBackup)
        {
            if (File.Exists(backup.Key))
            {
                File.WriteAllText(backup.Key, backup.Value);
            }
        }

        // 백업 데이터 초기화
        m_lastBackup.Clear();
        m_hasBackup = false;
        
        // EditorPrefs에서도 백업 데이터 삭제
        var allKeys = new string[] { BACKUP_COUNT_KEY };
        foreach (var key in allKeys) EditorPrefs.DeleteKey(key);
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Files have been restored to their previous state!", "OK");
    }

    private void CleanDebugLogs()
    {
        if (!Directory.Exists(m_targetFolder))
        {
            EditorUtility.DisplayDialog("Error", "Selected folder does not exist!", "OK");
            return;
        }

        m_previewText = "";
        SearchOption searchOption = m_includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.GetFiles(m_targetFolder, "*.cs", searchOption);

        // 백업 초기화
        if (!m_previewOnly)
        {
            m_lastBackup.Clear();
            m_hasBackup = false;
        }

        foreach (string file in files)
        {
            // SystemManager는 제외
            if (file.Contains("SystemManager"))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            string originalContent = content;

            // Debug.Log, Debug.LogError, Debug.LogWarning 패턴 매칭
            string[] patterns = new string[]
            {
                @"Debug\.Log\s*\([^;]*\);",
                @"Debug\.LogError\s*\([^;]*\);",
                @"Debug\.LogWarning\s*\([^;]*\);"
            };

            int totalMatches = 0;
            if (m_previewOnly)
            {
                m_previewText += $"\nFile: {file}\n";
            }

            foreach (string pattern in patterns)
            {
                MatchCollection matches = Regex.Matches(content, pattern);
                totalMatches += matches.Count;

                if (matches.Count > 0)
                {
                    if (m_previewOnly)
                    {
                        foreach (Match match in matches)
                        {
                            m_previewText += $"- {match.Value}\n";
                        }
                    }
                    else
                    {
                        content = Regex.Replace(content, pattern, "");
                    }
                }
            }

            if (totalMatches > 0)
            {
                if (m_previewOnly)
                {
                    m_previewText += $"Found {totalMatches} debug statements\n";
                }
                else if (content != originalContent)
                {
                    // 변경 전 내용 백업
                    m_lastBackup[file] = originalContent;
                    m_hasBackup = true;
                    
                    File.WriteAllText(file, content);
                }
            }
        }

        if (m_previewOnly)
        {
            if (string.IsNullOrEmpty(m_previewText))
            {
                m_previewText = "No debug statements found.";
            }
        }
        else
        {
            // 백업 데이터 저장
            if (m_hasBackup)
            {
                SaveBackup();
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Debug statements have been cleaned!", "OK");
        }
    }
}
#endif 