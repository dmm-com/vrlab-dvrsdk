using DVRSDK.Avatar;
using DVRSDK.Avatar.Tracking;
using UnityEngine;

namespace DVRSDK.Test
{
    public class SteamVRCalibrationAvatarManager : MonoBehaviour
    {
        [SerializeField]
        private DMMVRConnectUI dmmVRConnectUI;

        [SerializeField]
        private SteamVRTracker steamVRTracker = null;

        [SerializeField]
        private SteamVRHandSkeletalController skeletalController = null;

        [SerializeField]
        private VRFaceBlendShapeController faceBlendShapeController = null;

        [SerializeField]
        private Camera FirstPersonCamera = null;

        private FinalIKCalibrator calibrator = null;

        private GameObject CurrentModel;

        private void Awake()
        {
            dmmVRConnectUI.OnAvatarLoadedAction += OnAvatarLoaded;
        }

        private void OnAvatarLoaded(GameObject model)
        {
            CurrentModel = model;
            dmmVRConnectUI.SetupFirstPerson(FirstPersonCamera);
            DoCalibration();
            dmmVRConnectUI.ShowVRM();
            dmmVRConnectUI.AddAutoBlink();
        }

        private void SetTrackers()
        {
            steamVRTracker?.AutoAttachTrackerTargets();
        }

        public void DoCalibration()
        {
            SetTrackers();
            if (CurrentModel == null) return;
            if (calibrator == null) calibrator = new FinalIKCalibrator(steamVRTracker);
            calibrator?.LoadModel(CurrentModel);
            skeletalController?.LoadModel(CurrentModel);
            faceBlendShapeController?.LoadModel(CurrentModel);
            calibrator?.DoCalibration();
        }
    }
}
