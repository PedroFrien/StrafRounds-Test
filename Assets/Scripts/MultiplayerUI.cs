using System;
using UnityEngine;
using UnityEngine.UI;



public class MultiplayerUI : MonoBehaviour
{
    //[SerializeField] 
    //private UIDocument m_uiDocument;
    [SerializeField]
    private Button m_hostButton, m_clientButton, m_disconnectButton;
    

    public event Action OnStartHost, OnStartClient, OnDisconnectClient;

    private void Awake()
    {
        //m_hostButton = m_uiDocument.rootVisualElement.Q<Button>("ButtonHost");
        //m_clientButton = m_uiDocument.rootVisualElement.Q<Button>("ButtonClient");
        //m_disconnectButton = m_uiDocument.rootVisualElement.Q<Button>("ButtonDisconnect");
    }

    private void Start()
    {
        m_hostButton.onClick.AddListener(() => OnStartHost?.Invoke());
        m_clientButton.onClick.AddListener(() => OnStartClient?.Invoke());
        m_disconnectButton.onClick.AddListener(() => OnDisconnectClient?.Invoke());
    }

    public void DisableButtons()
    {
        m_hostButton.interactable = false;
        m_clientButton.interactable = false;
        m_disconnectButton.interactable = true;
    }

    public void EnableButtons()
    {
        m_hostButton.interactable = true;
        m_clientButton.interactable = true;
        m_disconnectButton.interactable = false;
    }



}
