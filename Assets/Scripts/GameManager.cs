using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiplayerUI m_multiplayerUI;
    [SerializeField]
    private PlayerUI m_playerUI;
    [SerializeField]
    private GameObject m_playerPrefab;

    private List<ulong> m_joinedClients = new List<ulong>();
    private List<FPController> m_activePlayers = new List<FPController>();

    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> availableSpawnPoints;


    [SerializeField] private bool m_inMatch = false;

    public static GameManager Instance { get; private set; }

    [SerializeField] private List<string> availableMaps;

    public class PlayerStats
    {
        public RoundWinIndicator roundWinIndicator;
        public int roundWins;
        public int matchWins;

        public PlayerStats()
        {
            roundWinIndicator = null;
            roundWins = 0;
            matchWins = 0;
        }
    }

    private Dictionary<ulong, PlayerStats> clientStats = new Dictionary<ulong, PlayerStats>();

    
    [SerializeField] private RoundWinIndicator roundWinIndicator;
    [SerializeField] private Transform uiContainer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Hook up UI events exactly once, here at startup. (Previously this also
        // ran inside Initialize(), which LoadScene() called every time a match
        // started/restarted, re-subscribing the same handlers each time.)
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDisconnectClient += DisconnectClient;
            m_multiplayerUI.OnStartGame += StartGame;
        }

        RefreshSpawnPoints();
    }

    private void Update()
    {
        Debug.Log(m_activePlayers.Count);
    }

    private void RefreshSpawnPoints()
    {
        GameObject[] taggedSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = new List<Transform>(taggedSpawnPoints.Length);
        foreach (var spawnPoint in taggedSpawnPoints)
        {
            spawnPoints.Add(spawnPoint.transform);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer == false)
            return;
        NetworkManager.OnClientConnectedCallback += AddClient;
        NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= AddClient;
            NetworkManager.SceneManager.OnLoadEventCompleted -= HandleSceneLoadCompleted;
        }
        base.OnNetworkDespawn();
    }

    private void HandleSceneLoadCompleted(string sceneName,
        LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientID in clientsCompleted)
        {
            AddClient(clientID);
        }

        // The new scene has now actually finished loading on the server, so it's
        // safe to look for spawn points and spawn players here. Previously this
        // happened synchronously right after calling LoadScene(), before the new
        // scene existed, so spawnPoints was empty and SpawnPlayers() crashed.
        RefreshSpawnPoints();

        if (m_inMatch)
        {
            SpawnPlayers();
        }
    }



    private void AddClient(ulong clientId)
    {
        if (!m_joinedClients.Contains(clientId))
        {
            m_joinedClients.Add(clientId);
            clientStats[clientId] = new PlayerStats();
        }
    }

    private void StartGame()
    {
        m_inMatch = true;
        LoadScene("SampleScene");
    }

    private void SpawnPlayers()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("SpawnPlayers called but no SpawnPoint-tagged objects were found in the active scene.");
            return;
        }

        // Copy the list instead of aliasing it. Previously this was
        // "availableSpawnPoints = spawnPoints;", which meant Remove() below
        // permanently deleted entries from the master spawnPoints list, so
        // each subsequent match had fewer spawn points available.
        availableSpawnPoints = new List<Transform>(spawnPoints);

        foreach (var clientID in m_joinedClients)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientID))
                continue;

            // Was "return" - which skipped spawning every remaining client
            // the moment it found one client that already had a PlayerObject.
            if (NetworkManager.ConnectedClients[clientID].PlayerObject != null)
                continue;

            if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning($"Ran out of spawn points while trying to spawn client {clientID}.");
                break;
            }

            int index = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[index];
            availableSpawnPoints.Remove(spawnPoint);

            GameObject player = Instantiate(m_playerPrefab, spawnPoint.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);

            FPController spawnedPlayer = player.GetComponent<FPController>();
            m_activePlayers.Add(spawnedPlayer);
            spawnedPlayer.OnCharacterDeath += () => CheckForWin(spawnedPlayer);

            CreateRoundUIClientRpc(m_activePlayers.Select(p => p.OwnerClientId).ToArray());
        }
    }

    [ClientRpc]
    private void CreateRoundUIClientRpc(ulong[] clientIds)
    {
        uiContainer = GameObject.Find("UIContainer").transform;
        m_playerUI = FindFirstObjectByType<PlayerUI>();

        foreach (var id in clientIds)
        {
            if (!clientStats.TryGetValue(id, out var stats))
            {
                // Shouldn't normally happen, but guard against it rather than throwing.
                stats = new PlayerStats();
                clientStats[id] = stats;
            }

            if (stats.roundWinIndicator != null)
                continue;

            stats.roundWinIndicator = Instantiate(roundWinIndicator, uiContainer);
        }
    }

    private void CheckForWin(FPController deadPlayer)
    {
        m_activePlayers.Remove(deadPlayer);
        if (m_activePlayers.Count == 1)
        {
            PlayerVictory(m_activePlayers[0].OwnerClientId);
        }
    }

    private void PlayerVictory(ulong winningClient)
    {
        foreach (var kvp in clientStats)
        {
            if (kvp.Key == winningClient)
            {
                clientStats[kvp.Key].roundWins++;
                if (clientStats[kvp.Key].roundWins >= 2)
                {
                    clientStats[kvp.Key].matchWins++;
                    StartCoroutine(ShowVictory(winningClient, true));
                }
            }
        }

        StartCoroutine(ShowVictory(winningClient, false));


    }

    private IEnumerator ShowVictory(ulong winningClient, bool newMap)
    {
        ShowVictoryClientRpc(winningClient);
        yield return new WaitForSeconds(3);
        if (newMap)
        {
            NewMap();
        }
        else
        {
            RestartMap();
        }
    }

    public void RestartMap()
    {
        Cleanup();
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NewMap()
    {
        Cleanup();
        int randomIndex = Random.Range(0, availableMaps.Count);
        LoadScene(availableMaps[randomIndex]);
    }

    private void Cleanup()
    {
        m_activePlayers.Clear();
        foreach (var kvp in clientStats)
        {
            kvp.Value.roundWinIndicator = null;
        }
    }

    [ClientRpc]
    private void ShowVictoryClientRpc(ulong winningClientId)
    {
        
        foreach (var kvp in clientStats)
        {
            clientStats[kvp.Key].roundWinIndicator.ShowWinner(clientStats[kvp.Key].roundWins);
        }
        m_playerUI.RoundWinScreen(winningClientId);

    }


    private void LoadScene(string sceneName)
    {
        // This only kicks off the (async) scene load. Spawn-point discovery
        // and SpawnPlayers() happen later, in HandleSceneLoadCompleted, once
        // the load has actually finished.
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void DisconnectClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.Shutdown();
    }

    private void StartClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartClient();
    }

    private void StartHost()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartHost();
    }
}