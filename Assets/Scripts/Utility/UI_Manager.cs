using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Manager : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI _scoreText;
    [SerializeField]
    public TextMeshProUGUI _hightScoreText;

    [SerializeField]
    public static int _score = 0;
    public static bool update = false;

    void Start()
    {
        _scoreText.text = "����: " + _score.ToString();
        _hightScoreText.text = "������: " + PlayerPrefs.GetInt("score").ToString();
    }

    public static void AddScore(int score)
    {
        _score += score;
    }

    public static void ScoreUpdate()
    {
        update = true;
        _score = 0;
    }

    private void Update()
    {
        if (update)
        {
            update = false;
            _hightScoreText.text = "������: " + PlayerPrefs.GetInt("score").ToString();
        }
        _scoreText.text = "����: " + _score.ToString();
        if (PlayerPrefs.GetInt("score") < _score)
            PlayerPrefs.SetInt("score", _score);
    }
}
