using UnityEngine;
using HockeyGame.Input;
using HockeyGame.Gameplay.Player;
using HockeyGame.Gameplay.Puck;
using HockeyGame.Gameplay.AI;
using HockeyGame.Data;

namespace HockeyGame.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameplaySettings _gameplaySettings;
        [SerializeField] private TeamConfig _teamAConfig;
        [SerializeField] private TeamConfig _teamBConfig;
        [SerializeField] private PlayerConfig _defaultPlayerConfig;
        [SerializeField] private InputReader _inputReader;

        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _puckPrefab;

        [Header("Parents")]
        [SerializeField] private Transform _playersParent;
        [SerializeField] private Transform _teamAParent;
        [SerializeField] private Transform _teamBParent;

        [Header("Camera")]
        [SerializeField] private GameCamera _gameCamera;

        [Header("Spawn Positions (Team A - Left Side)")]
        [SerializeField] private Vector3 _teamAGoaliePos = new Vector3(0f, 0f, -27f);
        [SerializeField] private Vector3 _teamALeftDefensePos = new Vector3(-5f, 0f, -20f);
        [SerializeField] private Vector3 _teamARightDefensePos = new Vector3(5f, 0f, -20f);
        [SerializeField] private Vector3 _teamALeftWingPos = new Vector3(-8f, 0f, -5f);
        [SerializeField] private Vector3 _teamACenterPos = new Vector3(0f, 0f, -2f);
        [SerializeField] private Vector3 _teamARightWingPos = new Vector3(8f, 0f, -5f);

        [Header("Human Player")]
        [SerializeField] private int _humanPlayerTeam = 0; // 0 = Team A, 1 = Team B
        [SerializeField] private int _humanPlayerIndex = 4; // 4 = Center (default)

        private PlayerController[] _teamAPlayers;
        private PlayerController[] _teamBPlayers;
        private PuckController _puck;
        private PlayerController _humanPlayer;

        private void Start()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            Debug.Log("[GameInitializer] Initializing game...");

            SpawnPuck();
            SpawnTeamA();
            SpawnTeamB();
            SetupHumanPlayer();
            SetupCamera();
            SetupAI();

            Debug.Log("[GameInitializer] Game initialized successfully!");
        }

        private void SpawnPuck()
        {
            if (_puckPrefab != null)
            {
                var puckObj = Instantiate(_puckPrefab, Vector3.zero, Quaternion.identity);
                puckObj.name = "Puck";
                _puck = puckObj.GetComponent<PuckController>();
            }
            else
            {
                // Create placeholder puck
                var puckObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puckObj.name = "Puck";
                puckObj.transform.position = new Vector3(0f, 0.05f, 0f);
                puckObj.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);

                // Add Rigidbody
                var rb = puckObj.AddComponent<Rigidbody>();
                rb.mass = 0.17f;
                rb.constraints = RigidbodyConstraints.FreezePositionY;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                _puck = puckObj.AddComponent<PuckController>();

                // Black material for puck
                var renderer = puckObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.black;
                }
            }

            Debug.Log("[GameInitializer] Puck spawned");
        }

        private void SpawnTeamA()
        {
            _teamAPlayers = new PlayerController[6];
            Color teamColor = _teamAConfig != null ? _teamAConfig.primaryColor : Color.red;

            Vector3[] positions = new Vector3[]
            {
                _teamAGoaliePos,
                _teamALeftDefensePos,
                _teamARightDefensePos,
                _teamALeftWingPos,
                _teamACenterPos,
                _teamARightWingPos
            };

            string[] names = { "A_Goalie", "A_LeftDef", "A_RightDef", "A_LeftWing", "A_Center", "A_RightWing" };

            for (int i = 0; i < 6; i++)
            {
                var player = SpawnPlayer(positions[i], names[i], _teamAParent ?? _playersParent);
                player.SetTeamColor(teamColor);
                _teamAPlayers[i] = player;
            }

            Debug.Log("[GameInitializer] Team A spawned (6 players)");
        }

        private void SpawnTeamB()
        {
            _teamBPlayers = new PlayerController[6];
            Color teamColor = _teamBConfig != null ? _teamBConfig.primaryColor : Color.blue;

            // Mirror positions for Team B (opposite side)
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0f, 0f, 27f),      // Goalie
                new Vector3(5f, 0f, 20f),      // Left Defense (mirrored)
                new Vector3(-5f, 0f, 20f),     // Right Defense (mirrored)
                new Vector3(8f, 0f, 5f),       // Left Wing (mirrored)
                new Vector3(0f, 0f, 2f),       // Center
                new Vector3(-8f, 0f, 5f)       // Right Wing (mirrored)
            };

            string[] names = { "B_Goalie", "B_LeftDef", "B_RightDef", "B_LeftWing", "B_Center", "B_RightWing" };

            for (int i = 0; i < 6; i++)
            {
                var player = SpawnPlayer(positions[i], names[i], _teamBParent ?? _playersParent);
                player.SetTeamColor(teamColor);
                _teamBPlayers[i] = player;
            }

            Debug.Log("[GameInitializer] Team B spawned (6 players)");
        }

        private PlayerController SpawnPlayer(Vector3 position, string playerName, Transform parent)
        {
            GameObject playerObj;

            if (_playerPrefab != null)
            {
                playerObj = Instantiate(_playerPrefab, position, Quaternion.identity, parent);
            }
            else
            {
                // Create placeholder cube player
                playerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                playerObj.transform.position = new Vector3(position.x, 0.5f, position.z);
                playerObj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

                if (parent != null)
                {
                    playerObj.transform.SetParent(parent);
                }

                // Add required components
                var rb = playerObj.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezePositionY |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                // Add PlayerController
                playerObj.AddComponent<PlayerController>();
            }

            playerObj.name = playerName;
            // Note: "Player" tag must be created in Unity Tag Manager if needed

            return playerObj.GetComponent<PlayerController>();
        }

        private void SetupHumanPlayer()
        {
            PlayerController[] team = _humanPlayerTeam == 0 ? _teamAPlayers : _teamBPlayers;

            if (team != null && _humanPlayerIndex >= 0 && _humanPlayerIndex < team.Length)
            {
                _humanPlayer = team[_humanPlayerIndex];

                // Initialize with human input
                if (_inputReader != null && _defaultPlayerConfig != null)
                {
                    // Ensure InputReader is initialized (critical for ScriptableObjects)
                    _inputReader.Initialize();

                    if (!_inputReader.IsInitialized)
                    {
                        Debug.LogError("[GameInitializer] InputReader failed to initialize!");
                    }

                    _humanPlayer.Initialize(_inputReader, _defaultPlayerConfig, true);
                    Debug.Log($"[GameInitializer] Human player set to {_humanPlayer.name} (InputReader initialized: {_inputReader.IsInitialized})");
                }
                else
                {
                    Debug.LogError($"[GameInitializer] InputReader or PlayerConfig not assigned! InputReader={_inputReader}, Config={_defaultPlayerConfig}");
                }
            }
            else
            {
                Debug.LogError($"[GameInitializer] Could not find human player! Team={team}, Index={_humanPlayerIndex}");
            }
        }

        private void SetupCamera()
        {
            if (_gameCamera != null && _humanPlayer != null)
            {
                _gameCamera.SetTarget(_humanPlayer.transform);

                // Set rink bounds based on gameplay settings
                if (_gameplaySettings != null)
                {
                    float halfWidth = _gameplaySettings.rinkWidth / 2f;
                    float halfLength = _gameplaySettings.rinkLength / 2f;
                    _gameCamera.SetBounds(
                        new Vector2(-halfWidth, -halfLength),
                        new Vector2(halfWidth, halfLength)
                    );
                }

                Debug.Log("[GameInitializer] Camera configured");
            }
        }

        private void SetupAI()
        {
            // Setup AI for all non-human players
            foreach (var player in _teamAPlayers)
            {
                if (player != null && player != _humanPlayer)
                {
                    SetupAIPlayer(player);
                }
            }

            foreach (var player in _teamBPlayers)
            {
                if (player != null && player != _humanPlayer)
                {
                    SetupAIPlayer(player);
                }
            }

            Debug.Log("[GameInitializer] AI configured for all non-human players");
        }

        private void SetupAIPlayer(PlayerController player)
        {
            var agentBrain = player.gameObject.GetComponent<AgentBrain>();
            if (agentBrain == null)
            {
                agentBrain = player.gameObject.AddComponent<AgentBrain>();
            }

            // Initialize AI with its home position (current spawn position)
            agentBrain.Initialize(player, player.transform.position);

            // Set puck reference
            if (_puck != null)
            {
                agentBrain.SetPuckReference(_puck.transform);
            }
        }

        public PlayerController GetHumanPlayer() => _humanPlayer;
        public PuckController GetPuck() => _puck;
        public PlayerController[] GetTeamA() => _teamAPlayers;
        public PlayerController[] GetTeamB() => _teamBPlayers;
    }
}
