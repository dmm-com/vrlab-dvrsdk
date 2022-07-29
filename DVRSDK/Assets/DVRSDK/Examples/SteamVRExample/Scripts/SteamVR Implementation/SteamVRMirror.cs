using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace DVRSDK.Plugins
{
    public class SteamVRMirror : Mirror
    {
#if !UNITY_ANDROID
        protected override void Render(MirrorSetting mirrorSetting, Camera currentCamera)
        {
            // ステレオモード時は左右の目で別のレンダリングが必要
            if (currentCamera.stereoEnabled)
            {
                mirrorSetting.propertyBlock.SetInt("_StereoMode", 2);
                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, EVREye.Eye_Left);
                }

                if (currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderEyeMirror(mirrorSetting.texture, currentCamera, EVREye.Eye_Right);
                }
            }
            else
            {
                mirrorSetting.propertyBlock.SetInt("_StereoMode", 0);
                base.Render(mirrorSetting, currentCamera);
            }
        }

        private void RenderEyeMirror(RenderTexture targetTexture, Camera currentCamera, EVREye targetEye)
        {
            Vector3 eyePos = currentCamera.transform.TransformPoint(SteamVR.instance.eyes[(int)targetEye].pos);
            Quaternion eyeRot = currentCamera.transform.rotation * SteamVR.instance.eyes[(int)targetEye].rot;
            HmdMatrix44_t hmdProjectionMatrix = SteamVR.instance.hmd.GetProjectionMatrix(targetEye, currentCamera.nearClipPlane, currentCamera.farClipPlane);
            Matrix4x4 projectionMatrix = HMDMatrix4x4ToMatrix4x4(hmdProjectionMatrix);

            RenderMirror(targetTexture, eyePos, eyeRot, projectionMatrix, targetEye == EVREye.Eye_Left ? leftEyeRect : rightEyeRect);
        }

        private static Matrix4x4 HMDMatrix4x4ToMatrix4x4(HmdMatrix44_t input)
        {
            var m = Matrix4x4.identity;

            m[0, 0] = input.m0;
            m[0, 1] = input.m1;
            m[0, 2] = input.m2;
            m[0, 3] = input.m3;

            m[1, 0] = input.m4;
            m[1, 1] = input.m5;
            m[1, 2] = input.m6;
            m[1, 3] = input.m7;

            m[2, 0] = input.m8;
            m[2, 1] = input.m9;
            m[2, 2] = input.m10;
            m[2, 3] = input.m11;

            m[3, 0] = input.m12;
            m[3, 1] = input.m13;
            m[3, 2] = input.m14;
            m[3, 3] = input.m15;

            return m;
        }
#endif
    }
}
