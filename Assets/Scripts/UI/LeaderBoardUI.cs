using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class LeaderBoardUI : MonoBehaviour
{
    public GameObject scoreListRoot;
    public GameObject scorePrefab;
    //private TextMeshProUGUI[] users;

    private List<ScoreData> scoreDatas = new List<ScoreData>();

    public void OnShow(GameObject go)
    {
        OnShowAsync(go).Forget();   // fire-and-forget(오류 로그 확인하려면 await 권장)
    }

    private async UniTaskVoid OnShowAsync(GameObject go)
    {
        go.SetActive(true);
        await OnAsyncLoadScore();
    }

    private async UniTask OnAsyncLoadScore()
    {
        var loaded = await ScoreManager.Instance.LoadHistroyAsync(10);
        scoreDatas = loaded;
        for (int i = 0; i < scoreDatas.Count; i++)
        {
            var data = scoreDatas[i];
            var go = Instantiate(scorePrefab, scoreListRoot.transform, false);
            var txtChild = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txtChild != null)
                txtChild.text = $"{data.score}";
        }
    }
}