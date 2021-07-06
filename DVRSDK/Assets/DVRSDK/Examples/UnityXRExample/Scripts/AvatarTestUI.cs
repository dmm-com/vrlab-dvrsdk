using DVRSDK.Auth;
using DVRSDK.Avatar;
using DVRSDK.Avatar.Tracking;
using DVRSDK.Avatar.Tracking.UnityXR;
using DVRSDK.Serializer;
using DVRSDK.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DVRSDK.Test
{

    public class AvatarTestUI : MonoBehaviour
    {
        [SerializeField]
        private UnityXRTracker unityXRTracker = null;

        private VRMLoader vrmLoader = new VRMLoader();

        private string currentStatus = "Start";

        private FinalIKCalibrator calibrator = null;

        private GameObject CurrentModel = null;

        private Dictionary<ApiRequestErrors, string> apiRequestErrorMessages = new Dictionary<ApiRequestErrors, string> {
            { ApiRequestErrors.Unknown,"Unknown request error" },
            { ApiRequestErrors.Forbidden, "Request forbidden" },
            { ApiRequestErrors.Unregistered, "User unregistered" },
            { ApiRequestErrors.Unverified, "User email unverified" },
         };

        private void OnGUI()
        {
            var buttonWidth = 120f;
            var buttonHeight = 30f;
            var margin = 10f;
            var top = 40f;
#if UNITY_STANDALONE_WIN
            if (GUI.Button(new Rect(margin, top, buttonWidth, buttonHeight), "OpenVRM"))
            {
                var path = WindowsDialogs.OpenFileDialog("Open VRM File", ".vrm");
                if (path != null)
                {
                    CurrentModel = vrmLoader.LoadVrmModelFromFile(path);
                    currentStatus = "VRM Loaded";
                    vrmLoader?.ShowMeshes();
                    vrmLoader?.AddAutoBlinkComponent();
                }
            }
#endif
            top += buttonHeight + margin;
            if (GUI.Button(new Rect(margin, top, buttonWidth, buttonHeight), "Dispose"))
            {
                vrmLoader.Dispose();
                CurrentModel = null;
                currentStatus = "VRM Disposed";
            }
            top += buttonHeight + margin;
            var defaultBG = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(margin, top, buttonWidth, 3), "");
            GUI.backgroundColor = defaultBG;
            top += 3 + margin;
            if (GUI.Button(new Rect(margin, top, buttonWidth, buttonHeight), "Login"))
            {
                DoLogin();
            }
            top += buttonHeight + margin;
            if (GUI.Button(new Rect(margin, top, buttonWidth, buttonHeight), "Download"))
            {
                var t = GetMyVRM();
            }
            top += buttonHeight + margin;
            if (GUI.Button(new Rect(margin, top, buttonWidth, buttonHeight), "Calibration"))
            {
                DoCalibration();
            }
            top += buttonHeight + margin;

            GUI.Box(new Rect(margin, margin, 620, 20), currentStatus);
        }



        private void DoLogin()
        {
            var sdkSettings = Resources.Load<SdkSettings>("SdkSettings");
            var client_id = sdkSettings.client_id;
            var config = new DVRAuthConfiguration(client_id, new UnitySettingStore(), new UniWebRequest(), new NewtonsoftJsonSerializer());
            Authentication.Instance.Init(config);

            Authentication.Instance.Authorize(
                openBrowser: url =>
                {
                    Application.OpenURL(url);
                },
                onAuthSuccess: isSuccess =>
                {
                    if (isSuccess)
                    {
                        currentStatus = "Login Success!";
                    }
                    else
                    {
                        currentStatus = "Login Failed";
                    }
                },
                onAuthError: exception =>
                {
                    currentStatus = exception.Message;
                });
        }

        private async Task GetMyVRM()
        {
            try
            {
                //自身のアバター一覧からカレントを取得する場合
                //var currentAvatars = await Authentication.Instance.Okami.GetAvatarsAsync();
                //if (currentAvatars == null || currentAvatars.Count == 0)
                //{
                //    currentStatus = "No avatars on your account.";
                //    return;
                //}
                //var currentAvatar = currentAvatars.FirstOrDefault(avatar => avatar.is_current);
                //if (currentAvatar == null) currentAvatar = currentAvatars.First();

                //自身のユーザーからカレントを取得する場合
                //var currentUser = await Authentication.Instance.Okami.GetCurrentUserAsync();
                //var currentAvatar = currentUser.current_avatar;

                //自身のユーザーIDからユーザー情報を取得して取得する場合(データ暗号化)
                var currentUser = await Authentication.Instance.Okami.GetCurrentUserAsync();
                var myUser = await Authentication.Instance.Okami.GetUserAsync(currentUser.id);
                var currentAvatar = myUser.current_avatar;

                CurrentModel = await Authentication.Instance.Okami.LoadAvatarVRMAsync(currentAvatar, vrmLoader.LoadVRMModelFromConnect) as GameObject;
                vrmLoader?.ShowMeshes();
                vrmLoader?.AddAutoBlinkComponent();
            }
            catch (ApiRequestException ex)
            {
                Debug.LogError(apiRequestErrorMessages[ex.ErrorType]);
            }

            if (CurrentModel != null)
            {
                currentStatus = "VRM Loaded";
            }
            else
            {
                currentStatus = "Download Error";
            }
        }

        private void SetTrackers()
        {
            unityXRTracker?.AutoAttachTrackerTargets();
        }

        private void DoCalibration()
        {
            SetTrackers();
            if (CurrentModel == null) return;
            if (calibrator == null) calibrator = new FinalIKCalibrator(unityXRTracker);
            calibrator?.LoadModel(CurrentModel);
            calibrator?.DoCalibration();
        }

    }
}
