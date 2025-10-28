using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProfileUI : MonoBehaviour
{
    public GameObject profilePannel;
    public GameObject profileEditPannel;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI userId;
    public TextMeshProUGUI userNickName;
    public async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);
        await ProfileManager.Instance.WaitForReadyAsync();
        if (await ProfileManager.Instance.ProfileExistAsync())
        {
            var s = await ProfileManager.Instance.LoadProfileAsync();
            if(s.profile != null)
            {
                nickNameText.text = s.profile.nickname;
                userNickName.text = s.profile.nickname;
                Debug.Log("프로필 불러오기 성공");
            }
            else
            {
                Debug.LogError($"프로필 불러오기 실패: {s.error}");
            }
        }
        userId.text = AuthManager.Instance.UserId;
    }
    public void OnClickEditNickName(TMP_InputField tMP_InputField)
    {
        OnAsyncEditNickName(tMP_InputField).Forget();
    }

    private async UniTaskVoid OnAsyncEditNickName(TMP_InputField tMP_InputField)
    {
        var s = await ProfileManager.Instance.SaveProfileAsync(tMP_InputField.text);
        if(s.success)
        {
            Debug.Log("닉네임 변경 성공");
            nickNameText.text = ProfileManager.Instance.CachedProfile.nickname;
            userNickName.text = ProfileManager.Instance.CachedProfile.nickname;
            //userId.text = AuthManager.Instance.UserId;
        }
        else
        {
            Debug.LogError($"닉네임 변경 실패: {s.error}");
        }
    }
    public void OnclickEdit(GameObject go)
    {
        go.SetActive(true);
    }
    public void OnClickSignOut()
    {
        AuthManager.Instance.SignOut();
        profileEditPannel.SetActive(false);
        profilePannel.SetActive(false);
        var loginUI = FindFirstObjectByType<LoginUI>();
        loginUI.UpdateUI().Forget();
    }
    public void OnClickClose(GameObject go)
    {
        go.SetActive(false);
    }
}