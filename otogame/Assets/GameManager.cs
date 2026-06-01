using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NoteData
{
    public float time;
    [Range(0, 3)] public int lane;
}

public class GameManager : MonoBehaviour
{
    [Header("設定項目")]
    public AudioSource bgmSource;
    public AudioSource seSource;
    public GameObject notePrefab;
    public float noteSpeed = 5f;
    public float judgeLineY = -3f;

    [Header("★ランダム譜面設定")]
    public int totalNotes = 100;
    public float noteInterval = 0.6f;

    [HideInInspector]
    public NoteData[] scoreData;

    [Header("ゲーム状態")]
    public int score = 0;
    public int combo = 0;
    public int life = 5;
    public bool isGameOver = false;

    // 演出用
    string judgeText = "";
    float judgeTimer = 0f;
    Color judgeColor = Color.white;

    // システム用
    double dspStartTime;
    public float songPosition;
    int nextNoteIndex = 0;

    public List<Note> activeNotes = new List<Note>();

    void Start()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        life = 5; score = 0; combo = 0; nextNoteIndex = 0;
        activeNotes.Clear();
        judgeText = "";

        scoreData = new NoteData[totalNotes];
        for (int i = 0; i < totalNotes; i++)
        {
            scoreData[i] = new NoteData();
            scoreData[i].time = 2f + (i * noteInterval);
            scoreData[i].lane = Random.Range(0, 4);
        }

        dspStartTime = AudioSettings.dspTime;
        if (bgmSource != null) bgmSource.Play();
    }

    void Update()
    {
        if (isGameOver) return;

        songPosition = (float)(AudioSettings.dspTime - dspStartTime);

        // BGMが最後まで再生されたら自動でゲームクリア！
        if (bgmSource != null && bgmSource.clip != null)
        {
            if (songPosition >= bgmSource.clip.length)
            {
                isGameOver = true;
                Time.timeScale = 0f;
                if (bgmSource != null) bgmSource.Stop();
            }
        }

        if (judgeTimer > 0f)
        {
            judgeTimer -= Time.deltaTime;
            if (judgeTimer <= 0f) judgeText = "";
        }

        if (nextNoteIndex < scoreData.Length && scoreData[nextNoteIndex].time < songPosition + 2f)
        {
            GameObject obj = Instantiate(notePrefab);
            Note note = obj.GetComponent<Note>();
            if (note != null)
            {
                note.manager = this;
                note.targetTime = scoreData[nextNoteIndex].time;
                note.lane = scoreData[nextNoteIndex].lane;
                activeNotes.Add(note);
            }
            nextNoteIndex++;
        }

        int pressedLane = -1;
        if (Input.GetKeyDown(KeyCode.D)) pressedLane = 0;
        if (Input.GetKeyDown(KeyCode.F)) pressedLane = 1;
        if (Input.GetKeyDown(KeyCode.J)) pressedLane = 2;
        if (Input.GetKeyDown(KeyCode.K)) pressedLane = 3;

        if (pressedLane != -1)
        {
            activeNotes.RemoveAll(item => item == null);

            Note target = null;
            foreach (var note in activeNotes)
            {
                if (note.lane == pressedLane)
                {
                    target = note;
                    break;
                }
            }

            if (target != null)
            {
                float diff = Mathf.Abs(target.targetTime - songPosition);

                if (diff < 0.1f)
                {
                    SetJudgeAction("PERFECT!!", Color.yellow);
                    score += 100; combo++;
                    if (seSource != null) seSource.Play();
                    activeNotes.Remove(target);
                    Destroy(target.gameObject);
                }
                else if (diff < 0.2f)
                {
                    SetJudgeAction("GREAT!", Color.cyan);
                    score += 50; combo++;
                    if (seSource != null) seSource.Play();
                    activeNotes.Remove(target);
                    Destroy(target.gameObject);
                }
                else
                {
                    SetJudgeAction("BAD...", Color.red);
                    MissHit();
                    activeNotes.Remove(target);
                    Destroy(target.gameObject);
                }
            }
        }
    }

    void SetJudgeAction(string text, Color color)
    {
        judgeText = text; judgeColor = color; judgeTimer = 0.4f;
    }

    public void MissHit()
    {
        if (isGameOver) return;
        combo = 0; life--;
        if (life <= 0)
        {
            life = 0;
            isGameOver = true;
            Time.timeScale = 0f;
            if (bgmSource != null) bgmSource.Stop();
        }
    }


    public void MissHitFromNote()
    {
        SetJudgeAction("MISS...", Color.gray);
        MissHit();
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 50;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.normal.textColor = new Color(1f, 1f, 1f, 0.5f);

        GUI.Label(new Rect(Screen.width / 2 - 324 - 40, Screen.height - 150, 80, 80), "D");
        GUI.Label(new Rect(Screen.width / 2 - 108 - 40, Screen.height - 150, 80, 80), "F");
        GUI.Label(new Rect(Screen.width / 2 + 108 - 40, Screen.height - 150, 80, 80), "J");
        GUI.Label(new Rect(Screen.width / 2 + 324 - 40, Screen.height - 150, 80, 80), "K");

        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.skin.label.normal.textColor = Color.white;
        GUI.skin.label.fontSize = 25;

        if (!isGameOver)
        {
            GUI.Label(new Rect(20, 20, 400, 50), "SCORE: " + score);
            GUI.Label(new Rect(20, 60, 400, 50), "COMBO: " + combo);
            GUI.Label(new Rect(20, 100, 400, 50), "LIFE: " + life);

            if (judgeText != "")
            {
                GUI.skin.label.fontSize = 55;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.skin.label.normal.textColor = judgeColor;
                GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 400, 100), judgeText);
            }

        }

    }

}