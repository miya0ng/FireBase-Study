using UnityEngine;
using Firebase.Database;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;

    private DatabaseReference scoresRef; // 서버에있는 ref라서 읽고쓰기 할때마다 통신 하는애

    private int cachedBestScore = 0;

    public int CachedBestScore => cachedBestScore;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private async UniTaskVoid Start()
    {
        await FirebaseInitializer.Instance.WaitForInitilazationAsync();
        scoresRef = FirebaseDatabase.DefaultInstance.RootReference.Child("scores");

        Debug.Log("[Score] 초기화 완료");
        await LoadBestScoreAsync();
    }

    private async UniTask<int> LoadBestScoreAsync()
    {
        if(!AuthManager.Instance.IsLoggedIn)
        {
            return 0;
        }

        string uid = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await scoresRef.Child(uid).Child("bestScore").GetValueAsync().AsUniTask();
            if(snapshot.Exists)
            {
                cachedBestScore = int.Parse(snapshot.Value.ToString());
                Debug.Log("[Score] 최고기록 로드: {cachedBestScore}");
            }
            else
            {
                cachedBestScore = 0;
                Debug.Log("[Score] 최고 기록 없음");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Score] 최고기록 로드 실패 {ex.Message}");
        }
        return 0;
    }
    public async UniTask<(bool success, string error)> SaveScoreAsync(int score)
    {
        if (!AuthManager.Instance.IsLoggedIn) return (false, "로그인이 필요합니다");
        string userId = AuthManager.Instance.UserId;

        try
        {
            Debug.Log($"점수 저장시도: {score}");

            DatabaseReference historyRef = scoresRef.Child(userId).Child("history");
            DatabaseReference newHistoryRef = historyRef.Push();

            var scoreData = new Dictionary<string, object>
            {
                { "score", score },
                { "timestamp", ServerValue.Timestamp }
            };
            await newHistoryRef.UpdateChildrenAsync(scoreData).AsUniTask();

            bool shouldUpdateBestScore = false; 

            if(cachedBestScore == 0)
            {
                var bestScoreSnapshot = await scoresRef.Child(userId).Child("bestScore").GetValueAsync().AsUniTask();
                if( ! bestScoreSnapshot.Exists)
                {
                    shouldUpdateBestScore = true;
                }
                else if(score>cachedBestScore)
                {
                    shouldUpdateBestScore = true;
                }
            }
            else if (score > cachedBestScore)
            {
               
            }

            if(shouldUpdateBestScore)
            {
                //
            }

            Debug.Log($"점수 저장 성공: {score}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"점수 저장 실패! {ex.Message}");
            return (false, ex.Message);
        }
    }

    private async UniTask UpdateBestScoreAsync(int newBestScore)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return;
        }
        string userId = AuthManager.Instance.UserId;
        try
        {
            await scoresRef.Child(userId).Child("bestScore").SetValueAsync(newBestScore).AsUniTask();
            cachedBestScore = newBestScore;
            Debug.Log("[Score] 최고 기록 점수 갱신");
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("[Score] 최고 기록 로드 실패: {0}", ex.Message);
        }
    }
        
    public async UniTask<List<ScoreData>> LoadHistroyAsync(int limit = 10)
    {   
        var list = new List<ScoreData>();

        if (!AuthManager.Instance.IsLoggedIn)
        {
            return list;
        }

        string userId = AuthManager.Instance.UserId;
        try
        {
            Debug.Log($"[score] 히스토리 로드 시도");
            DatabaseReference historyRef = scoresRef.Child(userId).Child("history");
            Query query = historyRef.OrderByChild("timestamp").LimitToLast(limit);
            DataSnapshot snapshot = await query.GetValueAsync().AsUniTask();
            if(snapshot.Exists)
            {
                foreach (DataSnapshot child in snapshot.Children)
                {
                    string json = child.GetRawJsonValue();
                    ScoreData data = ScoreData.FromJson(json);
                    list.Add(data);
                }
            }
            Debug.Log($"[score]  히스토리 로드 성공: {list.Count}개");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[score] 히스토리 로드 실패: {ex.Message}");
        }
        return list;
    }
}