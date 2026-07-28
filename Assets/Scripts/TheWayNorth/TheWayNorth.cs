using System.Collections.Generic;
using System.Linq;
using ColePersistence;
using Newtonsoft.Json;
using OysterUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheWayNorth : MonoBehaviour, ISaveableComponent
{
    public static TheWayNorth Instance { get; private set; }

    [SerializeField] private TheWayNorthConfig _config;
    [SerializeField] private Logger _logger = new();

    private List<SceneNames> _scenePath = new();
    private int _currentSceneIndex = -1;
    private Dictionary<string, string> _exitToEntrance = new();
    private Dictionary<string, string> _entranceToExit = new();
    private bool _isActive;

    private BiomeNode _rootNode;
    private BiomeNode _currentNode;
    private List<BiomeNode> _nodeOrder = new();

    private bool _isRunTransition;
    private string _pendingTargetId;
    private string _pendingSpawnLabel;
    private string _pendingSourceLink;

    private class BiomeNode
    {
        public BiomeConfig Config;
        public List<SceneNames> Path;
        public string LeftExitId;
        public string RightExitId;
        public BiomeNode Left;
        public BiomeNode Right;
        public BiomeNode Parent;
    }

    private class TreeNodeData
    {
        public string ConfigName;
        public List<string> Path;
        public string LeftExitId;
        public string RightExitId;
        public TreeNodeData Left;
        public TreeNodeData Right;
    }

    private class TwnSaveData
    {
        public List<string> ScenePath;
        public int CurrentSceneIndex;
        public Dictionary<string, string> ExitToEntrance;
        public Dictionary<string, string> EntranceToExit;
        public bool IsActive;
        public TreeNodeData RootNode;
        public List<int> CurrentNodePath;
        public List<List<int>> NodeOrder;
    }

    public string ComponentId => "TheWayNorth";

    public string CaptureStateAsJson()
    {
        if (!_isActive) return null;

        var data = new TwnSaveData
        {
            ScenePath = _scenePath.Select(s => s.ToString()).ToList(),
            CurrentSceneIndex = _currentSceneIndex,
            ExitToEntrance = new Dictionary<string, string>(_exitToEntrance),
            EntranceToExit = new Dictionary<string, string>(_entranceToExit),
            IsActive = _isActive,
            RootNode = SerializeNode(_rootNode),
            CurrentNodePath = GetNodePath(_rootNode, _currentNode),
            NodeOrder = _nodeOrder.Select(n => GetNodePath(_rootNode, n)).ToList()
        };

        return JsonConvert.SerializeObject(data);
    }

    public void RestoreStateFromJson(string json)
    {
        if (_isActive) return;

        var data = JsonConvert.DeserializeObject<TwnSaveData>(json);
        if (data == null) return;

        _scenePath = data.ScenePath.Select(ParseSceneName).ToList();
        _currentSceneIndex = data.CurrentSceneIndex;
        _exitToEntrance = data.ExitToEntrance ?? new Dictionary<string, string>();
        _entranceToExit = data.EntranceToExit ?? new Dictionary<string, string>();
        _isActive = data.IsActive;

        _rootNode = DeserializeNode(data.RootNode, null);

        if (data.NodeOrder != null && data.NodeOrder.Count > 0)
        {
            _nodeOrder = new List<BiomeNode>();
            foreach (var path in data.NodeOrder)
            {
                BiomeNode node = WalkNodePath(_rootNode, path);
                if (node != null)
                    _nodeOrder.Add(node);
            }
        }
        else
        {
            _currentNode = WalkNodePath(_rootNode, data.CurrentNodePath);
            RebuildNodeOrderFromPath();
        }

        if (_currentSceneIndex >= 0 && _currentSceneIndex < _scenePath.Count)
        {
            BiomeNode node = FindNodeByPathIndex(_currentSceneIndex);
            if (node != null)
                _currentNode = node;

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (IsForkScene(_currentNode, currentSceneName) &&
                (string.IsNullOrEmpty(_currentNode.LeftExitId) || string.IsNullOrEmpty(_currentNode.RightExitId)))
            {
                FindForkExits(_currentNode);
            }
        }

        _logger.Info("The Way North state restored from save");
    }

    private void RebuildNodeOrderFromPath()
    {
        _nodeOrder.Clear();
        if (_rootNode == null) return;
        int offset = 1;
        RebuildNodeOrderRecursive(_rootNode, ref offset);
    }

    private void RebuildNodeOrderRecursive(BiomeNode node, ref int offset)
    {
        if (node == null) return;
        _nodeOrder.Add(node);

        offset += node.Path.Count;
        if (offset >= _scenePath.Count) return;

        if (node.Left != null && PathMatchesAt(node.Left, offset))
        {
            RebuildNodeOrderRecursive(node.Left, ref offset);
            if (node.Right != null && PathMatchesAt(node.Right, offset))
                RebuildNodeOrderRecursive(node.Right, ref offset);
        }
        else if (node.Right != null && PathMatchesAt(node.Right, offset))
        {
            RebuildNodeOrderRecursive(node.Right, ref offset);
        }
    }

    private bool PathMatchesAt(BiomeNode node, int offset)
    {
        if (offset + node.Path.Count > _scenePath.Count) return false;
        for (int i = 0; i < node.Path.Count; i++)
        {
            if (_scenePath[offset + i] != node.Path[i])
                return false;
        }
        return true;
    }

    private TreeNodeData SerializeNode(BiomeNode node)
    {
        if (node == null) return null;

        return new TreeNodeData
        {
            ConfigName = node.Config.name,
            Path = node.Path.Select(s => s.ToString()).ToList(),
            LeftExitId = node.LeftExitId,
            RightExitId = node.RightExitId,
            Left = SerializeNode(node.Left),
            Right = SerializeNode(node.Right)
        };
    }

    private BiomeNode DeserializeNode(TreeNodeData data, BiomeNode parent)
    {
        if (data == null) return null;

        BiomeNode node = new BiomeNode
        {
            Config = _config.FindBiomeConfig(data.ConfigName),
            Path = data.Path.Select(ParseSceneName).ToList(),
            LeftExitId = data.LeftExitId,
            RightExitId = data.RightExitId,
            Parent = parent
        };

        node.Left = DeserializeNode(data.Left, node);
        node.Right = DeserializeNode(data.Right, node);

        return node;
    }

    private static List<int> GetNodePath(BiomeNode root, BiomeNode target)
    {
        var path = new List<int>();
        if (root == null || target == null) return path;

        var reversePath = new List<int>();
        BiomeNode walker = target;
        while (walker != null && walker != root)
        {
            if (walker.Parent == null) break;

            if (walker.Parent.Left == walker)
                reversePath.Add(0);
            else if (walker.Parent.Right == walker)
                reversePath.Add(1);
            else
                break;

            walker = walker.Parent;
        }

        for (int i = reversePath.Count - 1; i >= 0; i--)
            path.Add(reversePath[i]);

        return path;
    }

    private static BiomeNode WalkNodePath(BiomeNode root, List<int> path)
    {
        if (root == null) return null;

        BiomeNode walker = root;
        foreach (int dir in path)
        {
            if (dir == 0) walker = walker.Left;
            else if (dir == 1) walker = walker.Right;

            if (walker == null) break;
        }
        return walker ?? root;
    }

    private BiomeNode FindNodeByPathIndex(int index)
    {
        if (index <= 0 || index >= _scenePath.Count) return null;
        int offset = 1;
        foreach (var node in _nodeOrder)
        {
            int end = offset + node.Path.Count - 1;
            if (index >= offset && index <= end)
                return node;
            offset += node.Path.Count;
        }
        return null;
    }

    private bool IsForkScene(BiomeNode node, string sceneName)
    {
        if (node == null || node.Path.Count == 0) return false;
        return node.Path[node.Path.Count - 1].ToString() == sceneName;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_isActive && scene.name == _config.StartScene.ToString() && !_isRunTransition)
        {
            if (_config.AlwaysRegenerateNetwork || !JsonPersistence.JsonExists("PlayerComponents.json"))
            {
                StartRun();
                LogNetworkState();
            }
        }
        else if (_isActive)
        {
            RestoreCurrentNode(scene.name);
        }
        _isRunTransition = false;
    }

    public void TakeExit(string exitId, string transitionLabel)
    {
        if (!_isActive)
            StartRun();

        string currentScene = _scenePath[_currentSceneIndex].ToString();
        int sourceIndex = _currentSceneIndex;
        string key = $"{currentScene}|{sourceIndex}|{exitId}";

        if (_exitToEntrance.TryGetValue(key, out string target))
        {
            HandleReturnTransition(target, transitionLabel, sourceIndex, currentScene, exitId);
            return;
        }

        BiomeNode child = TryGetForkChild(exitId);
        if (child != null)
        {
            HandleForkTransition(child, key, transitionLabel, currentScene, exitId, sourceIndex);
            return;
        }

        HandleForwardTransition(key, transitionLabel, sourceIndex, currentScene, exitId);
    }

    private void HandleReturnTransition(string target, string transitionLabel, int sourceIndex, string currentScene, string exitId)
    {
        string[] parts = target.Split('|');
        string targetSceneName = parts[0];
        int targetIndex = int.Parse(parts[1]);
        string entranceId = parts[2];
        _currentSceneIndex = targetIndex;

        PerformTransition(targetSceneName, targetIndex.ToString(), sourceIndex,
            $"↩ Return: {currentScene}|{exitId} → {targetSceneName}|{entranceId} (spawn:{transitionLabel})",
            null, transitionLabel, entranceId);
    }

    private void HandleForkTransition(BiomeNode child, string key, string transitionLabel, string currentScene, string exitId, int sourceIndex)
    {
        _currentNode = child;
        _nodeOrder.Add(child);
        _scenePath.AddRange(child.Path);
        _currentSceneIndex = _scenePath.Count - child.Path.Count;

        PerformTransition(child.Path[0].ToString(), _currentSceneIndex.ToString(), sourceIndex,
            $"↳ Fork: {currentScene}|{exitId} → {child.Config.name}:{child.Path[0]} (spawn:{transitionLabel})",
            key, transitionLabel, null);
    }

    private void HandleForwardTransition(string key, string transitionLabel, int sourceIndex, string currentScene, string exitId)
    {
        int nextIndex = sourceIndex + 1;
        if (nextIndex >= _scenePath.Count)
        {
            _logger.Info($"✗ Dead end: {currentScene}|{exitId} (spawn:{transitionLabel}) — no further scenes");
            return;
        }
        _currentSceneIndex = nextIndex;
        PerformTransition(_scenePath[nextIndex].ToString(), nextIndex.ToString(), sourceIndex,
            null, key, transitionLabel, null);
    }

    private void PerformTransition(string targetScene, string targetSuffix, int saveSuffix, string logMessage, string sourceLink, string spawnLabel, string targetId)
    {
        if (logMessage != null)
            _logger.Info(logMessage);
        LogNetworkState();
        _pendingSourceLink = sourceLink;
        _pendingSpawnLabel = spawnLabel;
        _pendingTargetId = targetId;
        SceneManager.sceneLoaded += ResolvePlayerSpawn;
        _isRunTransition = true;
        SceneSaveLoadManager.SceneSaveSuffix = saveSuffix.ToString();
        SmoothSceneManager.LoadScene(targetScene, targetSuffix);
    }

    public void UseEntrance(string entranceId, string transitionLabel)
    {
        if (!_isActive) return;

        string currentScene = _scenePath[_currentSceneIndex].ToString();
        int sourceIndex = _currentSceneIndex;
        string key = $"{currentScene}|{sourceIndex}|{entranceId}";

        if (!_entranceToExit.TryGetValue(key, out string source))
        {
            Debug.LogError($"Entrance '{entranceId}' in '{currentScene}' has no linked exit");
            return;
        }

        string[] parts = source.Split('|');
        string sourceSceneName = parts[0];
        int targetIndex = int.Parse(parts[1]);
        string exitId = parts[2];

        _currentSceneIndex = targetIndex;

        BiomeNode owningNode = FindNodeByPathIndex(targetIndex);
        if (owningNode != null)
            _currentNode = owningNode;

        _logger.Info($"↩ Return: {currentScene}|{entranceId} ← {sourceSceneName}|{exitId} (spawn:{transitionLabel})");
        LogNetworkState();

        _pendingTargetId = exitId;
        _pendingSpawnLabel = transitionLabel;
        SceneManager.sceneLoaded += ResolvePlayerSpawn;
        _isRunTransition = true;
        SceneSaveLoadManager.SceneSaveSuffix = sourceIndex.ToString();
        SmoothSceneManager.LoadScene(sourceSceneName, targetIndex.ToString());
    }

    private void StartRun()
    {
        _scenePath.Clear();
        _exitToEntrance.Clear();
        _entranceToExit.Clear();

        _scenePath.Add(_config.StartScene);
        _currentSceneIndex = 0;

        BiomeConfig rootConfig = _config.Biomes[0];
        _rootNode = GenerateBiomeNode(rootConfig, 0);
        _currentNode = _rootNode;
        _nodeOrder = new List<BiomeNode> { _rootNode };

        _scenePath.AddRange(_rootNode.Path);

        _isActive = true;

        LogTree();
    }

    private BiomeNode GenerateBiomeNode(BiomeConfig config, int level)
    {
        BiomeNode node = new BiomeNode
        {
            Config = config,
            Path = new List<SceneNames>()
        };

        for (int i = 0; i < config.PoolScenesBeforeFork; i++)
        {
            if (config.ScenePool.Count == 0) break;
            node.Path.Add(_config.PickWeighted(config.ScenePool));
        }

        if (config.Forks.Count > 0)
            node.Path.Add(_config.PickWeighted(config.Forks));

        if (level < _config.NetworkDepth - 1)
        {
            List<TheWayNorthConfig.WeightedBiome> possibleBiomes = _config.GetPossibleBiomesForDepth(level + 1);
            if (possibleBiomes.Count > 0)
            {
                BiomeConfig leftConfig = _config.PickBiome(possibleBiomes);
                BiomeConfig rightConfig = _config.PickBiome(possibleBiomes);

                node.Left = GenerateBiomeNode(leftConfig, level + 1);
                node.Right = GenerateBiomeNode(rightConfig, level + 1);

                node.Left.Parent = node;
                node.Right.Parent = node;
            }
        }

        return node;
    }

    private void RestoreCurrentNode(string sceneName)
    {
        if (_currentNode == null) return;

        BiomeNode node = FindNodeByPathIndex(_currentSceneIndex);
        if (node != null)
            _currentNode = node;

        if (IsForkScene(_currentNode, sceneName) &&
            (string.IsNullOrEmpty(_currentNode.LeftExitId) || string.IsNullOrEmpty(_currentNode.RightExitId)))
        {
            FindForkExits(_currentNode);
        }
    }

    private void FindForkExits(BiomeNode node)
    {
        if (node.Path.Count == 0) return;

        SceneNames forkScene = node.Path[node.Path.Count - 1];
        string forkSceneName = forkScene.ToString();

        foreach (var exit in FindObjectsByType<TheWayNorthExit>(FindObjectsSortMode.None))
        {
            if (exit.gameObject.scene.name != forkSceneName) continue;

            if (exit.ForkDir == TheWayNorthExit.ForkDirection.Left)
                node.LeftExitId = exit.ExitId;
            else if (exit.ForkDir == TheWayNorthExit.ForkDirection.Right)
                node.RightExitId = exit.ExitId;
        }
    }

    private BiomeNode TryGetForkChild(string exitId)
    {
        if (_currentNode == null) return null;

        if (_currentNode.Left != null && _currentNode.LeftExitId == exitId)
            return _currentNode.Left;

        if (_currentNode.Right != null && _currentNode.RightExitId == exitId)
            return _currentNode.Right;

        return null;
    }

    private void RecordLink(string from, string to)
    {
        _exitToEntrance[from] = to;
        _entranceToExit[to] = from;
    }

    private void ResolvePlayerSpawn(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= ResolvePlayerSpawn;
        string targetId = _pendingTargetId;
        string label = _pendingSpawnLabel;
        string sourceLink = _pendingSourceLink;
        _pendingTargetId = null;
        _pendingSpawnLabel = null;
        _pendingSourceLink = null;

        if (string.IsNullOrEmpty(targetId) && string.IsNullOrEmpty(sourceLink)) return;

        if (!string.IsNullOrEmpty(sourceLink))
        {
            foreach (var entrance in FindObjectsByType<TheWayNorthEntrance>(FindObjectsSortMode.None))
            {
                TheWayNorthSpawn spawn = entrance.GetSpawn(label);
                if (spawn == null) continue;

                string sceneKey = $"{scene.name}|{_currentSceneIndex}|{entrance.EntranceId}";
                RecordLink(sourceLink, sceneKey);
                PlacePlayer(spawn.transform.position, scene.name, label);
                return;
            }
            Debug.LogError($"No Entrance with spawn '{label}' found in '{scene.name}'");
            return;
        }

        foreach (var entrance in FindObjectsByType<TheWayNorthEntrance>(FindObjectsSortMode.None))
        {
            if (entrance.EntranceId != targetId) continue;
            TheWayNorthSpawn spawn = entrance.GetSpawn(label);
            Vector3 pos = spawn != null ? spawn.transform.position : entrance.transform.position;
            PlacePlayer(pos, scene.name, label);
            return;
        }

        foreach (var exit in FindObjectsByType<TheWayNorthExit>(FindObjectsSortMode.None))
        {
            if (exit.ExitId != targetId) continue;
            TheWayNorthSpawn spawn = exit.GetSpawn(label);
            Vector3 pos = spawn != null ? spawn.transform.position : exit.transform.position;
            PlacePlayer(pos, scene.name, label);
            return;
        }
    }

    private void PlacePlayer(Vector3 pos, string sceneName, string label)
    {
        PlayerSceneData.PendingSpawnPosition = pos;
        PlayerSceneData.HasPendingSpawn = true;
        FindFirstObjectByType<PlayerSceneData>().SceneSpawnPosition = pos;
    }

    private void LogNetworkState()
    {
        var pathParts = _scenePath.Select<SceneNames, string>((s, i) => i == _currentSceneIndex ? $"<b>{s}</b>" : s.ToString());
        string path = string.Join(" → ", pathParts);
        _logger.Info($"TWN Path: [{path}]");
    }

    private void LogTree()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== The Way North Network (Depth: {_config.NetworkDepth}) ===");
        if (_rootNode != null)
            LogTreeNode(_rootNode, 0, sb);
        _logger.Info(sb.ToString());
    }

    private void LogTreeNode(BiomeNode node, int depth, System.Text.StringBuilder sb)
    {
        if (node == null) return;

        string indent = new string(' ', depth * 2);
        string pathStr = string.Join(" → ", node.Path);

        sb.AppendLine($"{indent}{node.Config.name}: {pathStr}");

        if (node.Left != null)
        {
            sb.AppendLine($"{indent}  ├─");
            LogTreeNode(node.Left, depth + 2, sb);
        }

        if (node.Right != null)
        {
            sb.AppendLine($"{indent}  └─");
            LogTreeNode(node.Right, depth + 2, sb);
        }
    }

    private static SceneNames ParseSceneName(string name)
    {
        if (System.Enum.TryParse<SceneNames>(name, out var result))
            return result;
        return SceneNames.CanyonStart;
    }
}
