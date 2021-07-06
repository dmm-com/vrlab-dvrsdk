using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace DVRSDK.Plugins.Input
{
    public class SteamVRButtonInput : MonoBehaviour, ButtonInputInterface
    {
        public event EventHandler<KeyEventArgs> KeyDownEvent;
        public event EventHandler<KeyEventArgs> KeyUpEvent;
        public event EventHandler<AxisEventArgs> AxisChangedEvent;

        private Vector2 lastLeftStickAxis = Vector2.zero;
        private Vector2 lastRightStickAxis = Vector2.zero;

        public void CheckUpdate()
        {
#if !UNITY_ANDROID
            if (SteamVR_Actions.default_SelectButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Select, true));
            if (SteamVR_Actions.default_CancelButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Cancel, true));
            if (SteamVR_Actions.default_StickButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Stick, true));
            if (SteamVR_Actions.default_MenuButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Menu, true));
            if (SteamVR_Actions.default_TriggerButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Trigger, true));
            if (SteamVR_Actions.default_GripButton.GetStateDown(SteamVR_Input_Sources.LeftHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Grip, true));

            if (SteamVR_Actions.default_SelectButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Select, false));
            if (SteamVR_Actions.default_CancelButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Cancel, false));
            if (SteamVR_Actions.default_StickButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Stick, false));
            if (SteamVR_Actions.default_MenuButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Menu, false));
            if (SteamVR_Actions.default_TriggerButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Trigger, false));
            if (SteamVR_Actions.default_GripButton.GetStateDown(SteamVR_Input_Sources.RightHand)) KeyDownEvent?.Invoke(this, new KeyEventArgs(KeyNames.Grip, false));


            if (SteamVR_Actions.default_SelectButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Select, true));
            if (SteamVR_Actions.default_CancelButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Cancel, true));
            if (SteamVR_Actions.default_StickButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Stick, true));
            if (SteamVR_Actions.default_MenuButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Menu, true));
            if (SteamVR_Actions.default_TriggerButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Trigger, true));
            if (SteamVR_Actions.default_GripButton.GetStateUp(SteamVR_Input_Sources.LeftHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Grip, true));

            if (SteamVR_Actions.default_SelectButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Select, false));
            if (SteamVR_Actions.default_CancelButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Cancel, false));
            if (SteamVR_Actions.default_StickButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Stick, false));
            if (SteamVR_Actions.default_MenuButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Menu, false));
            if (SteamVR_Actions.default_TriggerButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Trigger, false));
            if (SteamVR_Actions.default_GripButton.GetStateUp(SteamVR_Input_Sources.RightHand)) KeyUpEvent?.Invoke(this, new KeyEventArgs(KeyNames.Grip, false));

            var leftStickAxis = SteamVR_Actions.default_StickAxis.GetAxis(SteamVR_Input_Sources.LeftHand); //左スティック
            var rightStickAxis = SteamVR_Actions.default_StickAxis.GetAxis(SteamVR_Input_Sources.RightHand); //右スティック
            if (leftStickAxis != lastLeftStickAxis)
            {
                lastLeftStickAxis = leftStickAxis;
                AxisChangedEvent?.Invoke(this, new AxisEventArgs(KeyNames.Stick, true, leftStickAxis));
            }
            if (rightStickAxis != lastRightStickAxis)
            {
                lastRightStickAxis = rightStickAxis;
                AxisChangedEvent?.Invoke(this, new AxisEventArgs(KeyNames.Stick, false, rightStickAxis));
            }
#endif
        }
    }
}
