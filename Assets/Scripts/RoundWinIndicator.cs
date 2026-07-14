using UnityEngine;

public class RoundWinIndicator : MonoBehaviour
{


    [SerializeField] private GameObject highlight1;
    [SerializeField] private GameObject highlight2;

    private void Start()
    {
        highlight1.SetActive(false);
        highlight2.SetActive(false);
    }

    public void ShowWinner(int roundsWon)
    {
        if (roundsWon == 1) highlight1.SetActive(true);
        if (roundsWon >= 2)
        {
            highlight1.SetActive(true);
            highlight2.SetActive(true);
        }
    }
}
