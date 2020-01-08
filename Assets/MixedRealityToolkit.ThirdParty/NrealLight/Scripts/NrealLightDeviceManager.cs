using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.MixedReality.Toolkit.NrealLight.Input
{
    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem),
        SupportedPlatforms.Android|SupportedPlatforms.WindowsEditor|SupportedPlatforms.MacEditor|SupportedPlatforms.LinuxEditor,
        "Nreal Light Device Manager")]
    public class NrealLightDeviceManager : BaseInputDeviceManager, IMixedRealityCapabilityCheck
    {
        private NrealLightController controller = null;

        public NrealLightDeviceManager(
            IMixedRealityInputSystem inputSystem,
            string name,
            uint priority,
            BaseMixedRealityProfile profile) : base(inputSystem, name, priority, profile) { }

        public NrealLightDeviceManager(
            IMixedRealityServiceRegistrar registrar,
            IMixedRealityInputSystem inputSystem,
            string name,
            uint priority,
            BaseMixedRealityProfile profile) : base(registrar, inputSystem, name, priority, profile) { }

        public bool CheckCapability(MixedRealityCapability capability) => capability == MixedRealityCapability.MotionController;

        public override IMixedRealityController[] GetActiveControllers() => new[] { controller };

        public override void Update()
        {
            base.Update();

            if (controller==null)
            {
                NRInput.LaserVisualActive = false;
                NRInput.ReticleVisualActive = false;
                var handedness = NRInput.DomainHand == ControllerHandEnum.Left ? Handedness.Left : Handedness.Right;
                var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
                var inputSource = InputSystem?.RequestNewGenericInputSource($"Nreal Light Controller", pointers, InputSourceType.Controller);
                controller = new NrealLightController(Microsoft.MixedReality.Toolkit.TrackingState.NotTracked, handedness, inputSource);
                controller.SetupConfiguration(typeof(NrealLightController), InputSourceType.Controller);
                for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
                {
                    controller.InputSource.Pointers[i].Controller = controller;
                }
                InputSystem.RaiseSourceDetected(controller.InputSource, controller);
            }
            controller.UpdateController();

            // Change RaycastMode
            if (NRInput.GetButtonUp(ControllerButton.HOME))
            {
                InputSystem.RaiseSourceLost(controller.InputSource, controller);
                controller = null;
                NRInput.RaycastMode = NRInput.RaycastMode == RaycastModeEnum.Laser ? RaycastModeEnum.Gaze : RaycastModeEnum.Laser;
            }
        }
    }
}
