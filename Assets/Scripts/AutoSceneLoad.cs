using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneLoad : NetworkBehaviour
{
    [SerializeField] private string sceneName;
    private void Start()
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
