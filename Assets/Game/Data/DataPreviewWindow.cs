#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;

/// <summary>
/// Fenêtre d'édition multi-onglets pour visualiser les données de jeu.
/// </summary>
public class DataPreviewWindow : EditorWindow
{
    private Vector2 _scroll;
    private int _currentTab;
    private readonly string[] _tabs = { "Joueurs", "Hot Potato", "Netcode", "Upgrades", "Tick"};

    private GUIStyle boxStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;

    [MenuItem("Window/Game Data Overview")]
    public static void ShowWindow()
    {
        GetWindow<DataPreviewWindow>("Game Data");
    }
    
    private double _nextUpdateTime;

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        _nextUpdateTime = EditorApplication.timeSinceStartup + 1.0;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup >= _nextUpdateTime)
        {
            Repaint(); // Met à jour l’inspecteur
            _nextUpdateTime = EditorApplication.timeSinceStartup + 1.0;
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(6);
        EditorGUILayout.LabelField(
            "🎮 Overview des Données du Jeu",
            new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            }
        );
        GUILayout.Space(4);

        UpdateStyles();
        _currentTab = GUILayout.Toolbar(_currentTab, _tabs);
        GUILayout.Space(6);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        switch (_currentTab)
        {
            case 0: DrawPlayersTab(); break;
            case 1: DrawHotPotatoTab(); break;
            case 2: DrawNetcodeInfo() ; break;
            case 3: DrawUpgradesTab(); break;
            case 4: DrawTickTab(); break;
        }
        EditorGUILayout.EndScrollView();
    }

    private void UpdateStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 6, 6),
            margin  = new RectOffset(0, 0, 4, 4)
        };
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            normal   = { textColor = Color.white }
        };
        labelStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            fontSize = 11,
            wordWrap = true
        };
    }

    private void DrawPlayersTab()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Start play mode to see runtime info.", MessageType.Info);
            return;
        }
        var manager = FindFirstObjectByType<PlayerDataManager>();
        if (manager == null)
        {
            EditorGUILayout.HelpBox("Aucun PlayerDataManager trouvé dans la scène.", MessageType.Warning);
            return;
        }
        var data = manager.GetData();
        if (data == null || data.Count == 0)
        {
            EditorGUILayout.HelpBox("Aucune donnée de joueur disponible.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Joueurs", headerStyle);
        foreach (var kvp in data)
        {
            DrawBoxed(() => DisplayPlayerEntry(kvp.Key, kvp.Value));
        }
    }

    private void DisplayPlayerEntry(ulong clientId, PlayerData data)
    {
        Color bg = Color.gray;
        switch (data.OuterData.playingState)
        {
            case PlayerOuterData.PlayingState.Playing:
                bg = new Color(0.2f, 0.6f, 0.2f); break;
            case PlayerOuterData.PlayingState.SpectatingGame:
                bg = new Color(0.2f, 0.4f, 0.8f); break;
            case PlayerOuterData.PlayingState.Disconnected:
                bg = new Color(0.6f, 0.2f, 0.2f); break;
        }
        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = bg;
        EditorGUILayout.BeginVertical(boxStyle);
        GUI.backgroundColor = prevBg;

        EditorGUILayout.LabelField($"Client ID: {clientId}", headerStyle);
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField($"<b>État :</b> {data.OuterData.playingState}", labelStyle);
        EditorGUILayout.LabelField($"<b>Vie :</b> {data.InGameData.health}", labelStyle);
        EditorGUILayout.LabelField($"<b>Score :</b> {data.InGameData.score}", labelStyle);
        var ups = data.InGameData.GetUpgrades();
        var names = ups.Length > 0 ? string.Join(", ", ups.Select(u => u.UpgradeName)) : "<i>Aucun</i>";
        EditorGUILayout.LabelField($"<b>Upgrades :</b> {names}", labelStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawHotPotatoTab()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Start play mode to see runtime info.", MessageType.Info);
            return;
        }
        var hotpot = FindFirstObjectByType<HotPotatoManager>();
        if (hotpot == null)
        {
            EditorGUILayout.HelpBox("Aucun HotPotatoManager trouvé.", MessageType.Warning);
            return;
        }
        EditorGUILayout.LabelField("Hot Potato", headerStyle);
        DrawBoxed(() =>
        {
            EditorGUILayout.LabelField($"<b>Actif :</b> {hotpot.HotPotatoActiveServer}", labelStyle);
            var tgt = hotpot.target.Value;
            var txt = tgt == ulong.MaxValue ? "Aucun" : tgt.ToString();
            EditorGUILayout.LabelField($"<b>Cible :</b> {txt}", labelStyle);
        });
    }

    private void DrawUpgradesTab()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Start play mode to see runtime info.", MessageType.Info);
            return;
        }
        if (!NetworkManager.Singleton.IsServer)
        {
            EditorGUILayout.HelpBox("Only available in server mode.", MessageType.Warning);
            return;
        }
        var mgr = FindFirstObjectByType<UpgradesManager>();
        if (mgr == null)
        {
            EditorGUILayout.HelpBox("Aucun UpgradesManager trouvé.", MessageType.Warning);
            return;
        }
        EditorGUILayout.LabelField("Choix d'Upgrades", headerStyle);
        if (!mgr.PlayerAvailableUpgrades.Any())
        {
            EditorGUILayout.HelpBox("Pas de choix d'upgrades disponibles.", MessageType.Info);
            return;
        }
        foreach (var kvp in mgr.PlayerAvailableUpgrades)
        {
            DrawBoxed(() =>
            {
                EditorGUILayout.LabelField($"Client ID: {kvp.Key}", headerStyle);
                var choiceNames = string.Join(", ", kvp.Value.Select(i => mgr.GetUpgrade(i)?.UpgradeName ?? "–"));
                EditorGUILayout.LabelField($"<b>Choix dispos :</b> {choiceNames}", labelStyle);
                if (mgr.PlayerUpgradeChoice.TryGetValue(kvp.Key, out var sel))
                {
                    var selName = mgr.GetUpgrade(kvp.Value[sel])?.UpgradeName ?? "–";
                    EditorGUILayout.LabelField($"<b>Choix actuel :</b> {selName}", labelStyle);
                }
            });
        }
    }

    private void DrawTickTab()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Start play mode to see runtime info.", MessageType.Info);
            return;
        }
        if (!NetworkManager.Singleton.IsServer)
        {
            EditorGUILayout.HelpBox("Only available in server mode.", MessageType.Warning);
            return;
        }
        var tick = FindFirstObjectByType<GameTickManager>();
        if (tick == null)
        {
            EditorGUILayout.HelpBox("Aucun GameTickManager trouvé.", MessageType.Warning);
            return;
        }
        EditorGUILayout.LabelField("Tick", headerStyle);
        DrawBoxed(() =>
        {
            EditorGUILayout.LabelField($"<b>Fréquence :</b> {GameTickManager.TICKRATE} Hz", labelStyle);
            EditorGUILayout.LabelField($"<b>Tick actuel :</b> {GameTickManager.CurrentTick}", labelStyle);
        });
    }

    private void DrawBoxed(System.Action content)
    {
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        EditorGUILayout.BeginVertical(boxStyle);
        GUI.backgroundColor = prev;
        content();
        EditorGUILayout.EndVertical();
    }
    
    private void DrawNetcodeInfo()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Start play mode to see runtime info.", MessageType.Info);
            return;
        }

        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            EditorGUILayout.HelpBox("NetworkManager not found.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("🧠 Netcode Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Transport", nm.NetworkConfig.NetworkTransport.GetType().Name);
        EditorGUILayout.LabelField("Mode", nm.IsHost ? "Host" : nm.IsServer ? "Server" : nm.IsClient ? "Client" : "None");
        EditorGUILayout.LabelField("Local Client ID", nm.LocalClientId.ToString());
        EditorGUILayout.LabelField("Connected Clients", nm.ConnectedClients.Count.ToString());
        EditorGUILayout.LabelField("Connection Approval", nm.NetworkConfig.ConnectionApproval.ToString());

        GUILayout.Space(10);
        EditorGUILayout.LabelField("🧍 Connected Clients:", EditorStyles.boldLabel);
        foreach (var kvp in nm.ConnectedClients)
        {
            var clientId = kvp.Key;
            var client = kvp.Value;
            EditorGUILayout.LabelField($"- ClientId: {clientId} | IsLocal: {clientId == nm.LocalClientId} | PlayerObject: {client.PlayerObject != null}");
        }
    }
}
#endif