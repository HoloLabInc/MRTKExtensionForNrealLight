using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrackingState = Microsoft.MixedReality.Toolkit.TrackingState;

namespace HoloLab.MixedReality.Toolkit.NrealLight.Input
{
    [MixedRealityController(SupportedControllerType.ArticulatedHand, new[] { Handedness.Left,Handedness.Right })]
    public class NrealLightController : BaseController
    {
        public NrealLightController(
            TrackingState trackingState,
            Handedness controllerHandedness,
            IMixedRealityInputSource inputSource = null,
            MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions) { }

        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
       {
           new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger, MixedRealityInputAction.None)
        };

        public override bool IsInPointingPose => true;

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        public override void SetupDefaultInteractions(Handedness controllerHandedness) => AssignControllerMappings(DefaultInteractions);

        public void UpdateController()
        {
            if (!Enabled) { return; }

            var controllerAnchor = NRInput.DomainHand == ControllerHandEnum.Left ? ControllerAnchorEnum.LeftModelAnchor : ControllerAnchorEnum.RightModelAnchor;
            var pointerAnchor = NRInput.RaycastMode == RaycastModeEnum.Gaze ? ControllerAnchorEnum.GazePoseTrackerAnchor : controllerAnchor;
            var controller = NRInput.AnchorsHelper.GetAnchor(pointerAnchor);

            // hand pose
            var lastState = TrackingState;
            TrackingState = NRInput.CheckControllerAvailable(NRInput.DomainHand) ? TrackingState.Tracked : TrackingState.NotTracked;
            if (lastState != TrackingState)
            {
                CoreServices.InputSystem?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }
            if (TrackingState == TrackingState.Tracked)
            {
                CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, new MixedRealityPose(controller.position, controller.rotation));
            }

            // hand interaction
            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for Nreal Light Controller Source");
                Enabled = false;
            }
            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.None:
                        break;
                    case DeviceInputType.SpatialPointer:
                        var pointer = new MixedRealityPose(controller.position, controller.rotation);
                        Interactions[i].PoseData = pointer;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, pointer);
                        }
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.TriggerPress:
                        Interactions[i].BoolData = NRInput.GetButton(ControllerButton.TRIGGER);

                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                }
            }
        }
    }
}
