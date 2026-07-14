using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerUI : NetworkBehaviour
{
    private Animator animator;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private bool Paused;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void RoundWinScreen(ulong winningClient)
    {
        winnerText.text = "Player " + winningClient + " Wins!";
        animator.SetTrigger("RoundWin");
    }

    public void Unpause()
    {
        FindFirstObjectByType<FPController>().Pause();
    }
}
