using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiplayerUI m_multiplayerUI;
    [SerializeField]
    private GameObject m_playerPrefab;

    private List<ulong> m_joinedClients = new List<ulong>();
    private List<BaseCharacter> m_activePlayers = new List<BaseCharacter>();

    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> availableSpawnPoints;


    [SerializeField] private bool m_inMatch = false;

    public static GameManager Instance { get; private set; }


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

    private void AddClient(ulong clientID)
    {
        if (!m_joinedClients.Contains(clientID))
        {
            m_joinedClients.Add(clientID);
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

            BaseCharacter spawnedPlayer = player.GetComponent<BaseCharacter>();
            m_activePlayers.Add(spawnedPlayer);
            spawnedPlayer.OnCharacterDeath += () => CheckForWin(spawnedPlayer);
        }
    }

    private void CheckForWin(BaseCharacter deadPlayer)
    {
        m_activePlayers.Remove(deadPlayer);
        if (m_activePlayers.Count == 1)
        {
            PlayerVictory(deadPlayer.OwnerClientId);
        }
    }

    private void PlayerVictory(ulong winningClient)
    {
        LoadScene(SceneManager.GetActiveScene().name);
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