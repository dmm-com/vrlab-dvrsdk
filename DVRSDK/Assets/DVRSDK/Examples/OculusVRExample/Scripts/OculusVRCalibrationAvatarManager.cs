using DVRSDK.Avatar;
using DVRSDK.Avatar.Tracking;
using DVRSDK.Avatar.Tracking.Oculus;
using UnityEngine;

namespace DVRSDK.Test
{
    public class OculusVRCalibrationAvatarManager : MonoBehaviour
    {
        [SerializeField]
        private DMMVRConnectUI dmmVRConnectUI;

        [SerializeField]
        private OculusVRTracker oculusVRTracker = null;

        [SerializeField]
        private HandTracking_Button handTracking = null;

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
            DoCalibration();
            dmmVRConnectUI.SetupFirstPerson(FirstPersonCamera);
            dmmVRConnectUI.ShowVRM();
            dmmVRConnectUI.AddAutoBlink();
        }

        private void SetTrackers()
        {
            oculusVRTracker?.AutoAttachTrackerTargets();
        }

        public void DoCalibration()
        {
            SetTrackers();
            if (CurrentModel == null) return;
            if (calibrator == null) calibrator = new FinalIKCalibrator(oculusVRTracker);
            calibrator?.LoadModel(CurrentModel);
            handTracking?.LoadModel(CurrentModel);
            faceBlendShapeController?.LoadModel(CurrentModel);
            calibrator?.DoCalibration();
        }
    }
}
