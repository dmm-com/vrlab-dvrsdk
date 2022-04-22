using UnityEngine;
using UnityEngine.XR;

namespace DVRSDK.Plugins
{
    public class UnityXRMirror : Mirror
    {
        protected override void Render(MirrorSetting mirrorSetting, Camera currentCamera)
        {
            // ステレオモード時は左右の目で別のレンダリングが必要
            if (currentCamera.stereoEnabled)
            {
                mirrorSetting.propertyBlock.SetInt("_StereoMode", 1);
                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, Camera.StereoscopicEye.Left, InputTracking.GetLocalPosition(XRNode.LeftEye), InputTracking.GetLocalRotation(XRNode.LeftEye));
                }

                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, Camera.StereoscopicEye.Right, InputTracking.GetLocalPosition(XRNode.RightEye), InputTracking.GetLocalRotation(XRNode.RightEye));
                }
            }
            else
            {
                mirrorSetting.propertyBlock.SetInt("_StereoMode", 0);
                base.Render(mirrorSetting, currentCamera);
            }
        }

        private void RenderEyeMirror(RenderTexture targetTexture, Camera currentCamera, Camera.StereoscopicEye targetEye, Vector3 eyePos, Quaternion eyeRot)
        {
            Matrix4x4 projectionMatrix;
            if (currentCamera.stereoEnabled)
                projectionMatrix = currentCamera.GetStereoProjectionMatrix(targetEye);
            else
                projectionMatrix = currentCamera.projectionMatrix;

            RenderMirror(targetTexture, eyePos, eyeRot, projectionMatrix, targetEye == Camera.StereoscopicEye.Left ? leftEyeRect : rightEyeRect);
        }
    }
}
