using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
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

    private List<Transform> spawnPoints;
    private List<Transform> availableSpawnPoints;


    [SerializeField] private bool m_inMatch = false;



    private void Start()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDisconnectClient += DisconnectClient;
            m_multiplayerUI.OnStartGame += SpawnPlayers;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer == false)
            return;
        NetworkManager.OnClientConnectedCallback += AddClient;
        NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;

        GameObject[] taggedSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoints = new List<Transform>(taggedSpawnPoints.Length);
        foreach (var spawnPoint in taggedSpawnPoints)
        {
            spawnPoints.Add(spawnPoint.transform);
        }


        

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
    }

    private void AddClient(ulong clientID)
    {
        m_joinedClients.Add(clientID);
    }

    

    private void SpawnPlayers()
    {
        

        availableSpawnPoints = spawnPoints;

        foreach (var clientID in m_joinedClients)
        {
            if (NetworkManager.ConnectedClients[clientID].PlayerObject != null) return;

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
        NetworkManager.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
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
