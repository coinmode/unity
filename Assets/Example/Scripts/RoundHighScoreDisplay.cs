using CoinMode;
using UnityEngine;
using UnityEngine.UI;

public class RoundHighScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Text positionText = null;

    [SerializeField]
    private Text nameText = null;

    [SerializeField]
    private Text scoreText = null;

    [SerializeField]
    private Text payoutText = null;

    public void SetInfo(HighScore highScore)
    {
        positionText.text = "#" + highScore.position.ToString();
        nameText.text = highScore.displayName;
        scoreText.text = highScore.formattedScore;
        payoutText.text = highScore.paidOut > 0 ? "Paid out: " + highScore.paidOut.ToString() : "No payout";
    }
}
