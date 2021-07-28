using DVRSDK.Avatar.Tracking;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SteamVRTrackerIndicator : MonoBehaviour
{
    [SerializeField]
    private SteamVRTracker steamvrTracker;
    [SerializeField]
    private TrackerPositions trackerPosition;
    [SerializeField]
    private Text positionText;
    [SerializeField]
    private Text deviceTypeText;
    [SerializeField]
    private Text serialNumberText;
    [SerializeField]
    private Image indicatorImage;

    private TrackerTarget target;

    private TrackingDeviceType oldDeviceType = TrackingDeviceType.Invalid;
    private string oldSerialNumber = null;
    private ulong oldInputSourceHandle = 0;
    private Vector3 oldPosition = Vector3.zero;

    void Start()
    {
        if (steamvrTracker == null)
        {
            steamvrTracker = FindObjectOfType<SteamVRTracker>();
        }

        target = steamvrTracker.TrackerTargets.FirstOrDefault(d => d.TrackerPosition == trackerPosition);

        positionText.text = "";
        deviceTypeText.text = "";
        serialNumberText.text = "";
        SetAlpha(0f);

        if (target != null)
        {
            positionText.text = trackerPosition.ToString();
        }
    }

    void Update()
    {
        if (target == null) return;

        if (target.PoseIsValid)
        {
            if (oldDeviceType != target.UseDeviceType)
            {
                oldDeviceType = target.UseDeviceType;
                deviceTypeText.text = oldDeviceType.ToString().Replace("Generic", "");
            }

            if (oldSerialNumber != target.SerialNumber)
            {
                oldSerialNumber = target.SerialNumber;
                serialNumberText.text = $"[{target.DeviceIndex}] Bat[{(target.BatteryPercentage == -1 ? "N/A" : $"{target.BatteryPercentage*100:0.00}%")}] {oldSerialNumber}";
            }

            if (oldInputSourceHandle != target.InputSourceHandle)
            {
                oldInputSourceHandle = target.InputSourceHandle;
                serialNumberText.text = $"Role [{target.TrackerPosition}] ({oldInputSourceHandle})";
            }

            SetAlpha(Mathf.Abs(((target.TargetTransform.position - oldPosition) * (1.0f / Time.deltaTime)).sqrMagnitude) * 1f);
            oldPosition = target.TargetTransform.position;
        }
    }

    private void SetAlpha(float alpha)
    {
        var c = indicatorImage.color;
        indicatorImage.color = new Color(c.r, c.g, c.b, alpha);
    }
}
