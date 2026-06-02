using UnityEngine;

public class Note : MonoBehaviour
{
    public GameManager manager;
    public float targetTime;
    public int lane;

    void Update()
    {
        if (manager == null || manager.isGameOver) return;

        // Y座標の計算
        float y = manager.judgeLineY + ((targetTime - manager.songPosition) * manager.noteSpeed);


        float x = -3f + (lane * 2f);

        // 計算したXとYで位置を更新
        transform.position = new Vector3(x, y, 0);

        // 見逃し判定
        if (manager.songPosition > targetTime + 0.3f)
        {
            manager.MissHitFromNote();
            manager.activeNotes.Remove(this);
            Destroy(gameObject);
        }
    }
}