
using UnityEngine;

namespace DVRSDK.Plugins
{
    public class OculusVRMirror : Mirror
    {
        public Transform LeftEyeAnchor;
        public Transform RightEyeAnchor;

        protected override void Render(MirrorSetting mirrorSetting, Camera currentCamera)
        {

            // ステレオモード時は左右の目で別のレンダリングが必要
            if (currentCamera.stereoEnabled)
            {
                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, Camera.StereoscopicEye.Left, LeftEyeAnchor);
                }

                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, Camera.StereoscopicEye.Right, RightEyeAnchor);
                }
            }
            else
            {
                base.Render(mirrorSetting, currentCamera);
            }
        }

        private void RenderEyeMirror(RenderTexture targetTexture, Camera currentCamera, Camera.StereoscopicEye targetEye, Transform eyeAnchor)
        {
            Vector3 eyePos = eyeAnchor.position;
            Quaternion eyeRot = eyeAnchor.rotation;
            Matrix4x4 projectionMatrix;
            if (currentCamera.stereoEnabled)
                projectionMatrix = currentCamera.GetStereoProjectionMatrix(targetEye);
            else
                projectionMatrix = currentCamera.projectionMatrix;

            RenderMirror(targetTexture, eyePos, eyeRot, projectionMatrix, targetEye == Camera.StereoscopicEye.Left ? leftEyeRect : rightEyeRect);
        }

    }
}
