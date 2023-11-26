using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation
{
    public class TopDownModeCamera : MonoBehaviour
    {
        // A Message to anybody reading this code:
        // Don't judge, this code has been butchered and is awful, it contains some terrible practices and 
        // all the class members are public, this is because harmony decided to not work in subclasses specifically for this file, idk why
        // So I had to adjust my code, and it is now awful. 

        public static float maxCamHeight = 150f;
        public static float minCamHeight = 20f;
        public static float startCamHeight = 50f;

        public static float camHeightZoomSpeed = 5f;
        public static float camHeight = startCamHeight;

        public static float camHeightSpeedModifierBuffer = 0.8f;

        public static float normalSpeed  { get => Settings.inst.c_CameraControls.s_speed.Value; }

        public static float shiftSpeedBoost { get => Settings.inst.c_CameraControls.s_shiftSpeed.Value; }

        public static float snap {  get => Settings.inst.c_CameraControls.s_snap.Value; }

        public static float velocityAcceleration = 4f;
        public static float velocityDeceleration = 5f;
        public static float maxVelocity = 2f;

        public static float contiguousVelocity = 0f;

        public static bool dragging;
        public static Vector3 dragStartPos;
        public static Vector3 dragCurrentPos;
        


        public static bool active { get; private set; } = false;

        #region Exposed Methods

        public static void ActivateTopDownView()
        {
            active = true;
        }

        public static void DeactivateTopDownView()
        {
            active = false;
        }

        public static void ToggleTopDownView()
        {
            active = !active;
        }

        #endregion
    }

    #region Disabler Patches

    [HarmonyPatch(typeof(Cam), "MoveRight")]
    public static class CamMoveRightPatch
    {
        static bool Prefix()
        {
            return !TopDownModeCamera.active;
        }
    }

    [HarmonyPatch(typeof(Cam), "MoveForward")]
    public static class CamMoveForwardPatch
    {
        static bool Prefix()
        {
            return !TopDownModeCamera.active;
        }
    }


    [HarmonyPatch(typeof(Cam), "Zoom")]
    public static class CamZoomPatch
    {
        static bool Prefix()
        {
            return !TopDownModeCamera.active;
        }
    }

    [HarmonyPatch(typeof(Cam), "Rotate")]
    public static class CamRotatePatch
    {
        static bool Prefix()
        {
            return !TopDownModeCamera.active;
        }
    }

    #endregion

    #region Correction Patches

    [HarmonyPatch(typeof(Cam), "ClampToWorldBounds")]
    public class CameraTrackTargetYCorrectionPatch
    {
        static void Postfix(Vector3 p, ref Vector3 __result)
        {
            Cell cell = World.inst.GetCellDataClamped(__result);
            if (cell == null)
                return;

            CellMeta meta = Grid.Cells.Get(cell);
            if (meta == null)
                return;

            if (__result.y < meta.Elevation)
                __result.y = meta.Elevation;
        }
    }

    #endregion

    [HarmonyPatch(typeof(Cam), "Update")]
    public static class MainCamMovementPatch
    {
        private static Camera cam = Cam.inst.cam;

        #region Normal Movement
        private static void ApplyTrackingPos()
        {
            if (Cam.inst.OverrideTrack != null && !Cam.inst.OverrideTrack.Equals(null))
                Cam.inst.DesiredTrackingPos = Cam.inst.OverrideTrack.GetDesiredTrackingPos();
        }

        private static void ApplyPos()
        {
            ApplyTrackingPos();

            Vector3 pos = new Vector3(Cam.inst.DesiredTrackingPos.x, TopDownModeCamera.camHeight, Cam.inst.DesiredTrackingPos.z);

            if (TopDownModeCamera.snap > 0f && !TopDownModeCamera.dragging)
            {
                pos.x = Utils.Util.RoundToFactor(pos.x, TopDownModeCamera.snap);
                pos.z = Utils.Util.RoundToFactor(pos.z, TopDownModeCamera.snap);
            }

            cam.transform.rotation = Quaternion.AngleAxis(90f, new Vector3(1f, 0f, 0f));
            cam.transform.position = pos;
        }

        private static float CalcSpeed()
        {
            float camHeightSpeedModifier = (TopDownModeCamera.camHeight - TopDownModeCamera.minCamHeight) / (TopDownModeCamera.maxCamHeight - TopDownModeCamera.minCamHeight) + TopDownModeCamera.camHeightSpeedModifierBuffer;
            camHeightSpeedModifier = Mathf.Clamp(camHeightSpeedModifier, 0f, 1f);

            float speed = TopDownModeCamera.normalSpeed * TopDownModeCamera.contiguousVelocity * camHeightSpeedModifier;

            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraMoveFast, true, true, true))
            {
                speed *= TopDownModeCamera.shiftSpeedBoost;
            }

            return speed;
        }

        private static Vector3 CalcMovement()
        {
            Vector3 movement = Vector3.zero;

            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraMoveForward, true, true, true))
                movement += Vector3.forward;
            
            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraMoveBack, true, true, true))
                movement += Vector3.back;
            
            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraMoveRight, true, true, true))
                movement += Vector3.right;
            
            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraMoveLeft, true, true, true))
                movement += Vector3.left;

            return movement;

            
        }

        private static void CalcZoom()
        {
            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraZoomOut, true, true, true))
                TopDownModeCamera.camHeight += TopDownModeCamera.camHeightZoomSpeed;
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                TopDownModeCamera.camHeight += TopDownModeCamera.camHeightZoomSpeed;

            if (ConfigurableControls.inst.GetInputActionKey(InputActions.CameraZoomIn, true, true, true))
                TopDownModeCamera.camHeight -= TopDownModeCamera.camHeightZoomSpeed;
            else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                TopDownModeCamera.camHeight -= TopDownModeCamera.camHeightZoomSpeed;

            TopDownModeCamera.camHeight = Mathf.Clamp(TopDownModeCamera.camHeight, TopDownModeCamera.minCamHeight, TopDownModeCamera.maxCamHeight);
        }

        private static void CalcPos(Vector3 movement, float speed, bool movedThisFrame)
        {
            Vector3 clampedPos = (Cam.inst.DesiredTrackingPos + (movement * speed));

            Cell cell = World.inst.GetCellDataClamped(clampedPos);
            if (cell != null && Grid.Cells.Get(cell))
            {
                clampedPos.x = cell.Center.x;
                clampedPos.y = Grid.Cells.Get(cell).Elevation;
                clampedPos.z = cell.Center.z;
            }


            Cam.inst.SetDesiredTrackingPos(clampedPos);

            TopDownModeCamera.contiguousVelocity += movedThisFrame ? (Time.unscaledDeltaTime * TopDownModeCamera.velocityAcceleration) : (-Time.unscaledDeltaTime * TopDownModeCamera.velocityDeceleration);
            TopDownModeCamera.contiguousVelocity = Mathf.Clamp(TopDownModeCamera.contiguousVelocity, 0, TopDownModeCamera.maxVelocity);

        }

        #endregion

        #region Dragging

        private static void CheckDrag()
        {
            bool input = false;
            if (Input.GetMouseButton(0))
            {
                
                if (Assets.Settings.inst.LegacyMouseControls && !GameUI.AltHeld())
                {
                    input = true;
                }
                if (!Assets.Settings.inst.LegacyMouseControls && GameUI.AltHeld())
                {
                    input = true;
                }
            }
            if (Input.GetMouseButton(1) && !Assets.Settings.inst.LegacyMouseControls && !GameUI.AltHeld())
                input = true;
            

            if (input)
            {
                if (!TopDownModeCamera.dragging)
                {
                    TopDownModeCamera.dragging = true;
                    TopDownModeCamera.dragStartPos = GetHit();
                }
            }
            else
            {
                TopDownModeCamera.dragging = false;
            }
        }

        private static void ApplyDragPosition()
        {
            if (!TopDownModeCamera.dragging)
                return;

            DebugExt.dLog("dragging", true, TopDownModeCamera.dragStartPos);
            DebugExt.dLog("dragging2", true, TopDownModeCamera.dragCurrentPos);
            
            TopDownModeCamera.dragCurrentPos = GetHit();
            Cam.inst.SetDesiredTrackingPos(Cam.inst.DesiredTrackingPos - (TopDownModeCamera.dragCurrentPos - TopDownModeCamera.dragStartPos));
            
            DebugExt.dLog("dragging3", true, Cam.inst.DesiredTrackingPos);
        }


        private static Vector3 GetHit()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 0f));
            plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        #endregion

        static bool Prefix()
        {
            CheckDrag();
            if (TopDownModeCamera.active)
            {
                ApplyPos();
                if (!TopDownModeCamera.dragging)
                {
                    float speed = CalcSpeed();
                    Vector3 movement = CalcMovement();

                    bool movedThisFrame = movement != Vector3.zero;

                    CalcZoom();

                    CalcPos(movement, speed, movedThisFrame);
                }
                else
                {
                    ApplyDragPosition();
                }
            }
            else
            {
                if (Cam.inst.OverrideTrack == null)
                {
                    Vector3 clampedPos = Cam.inst.DesiredTrackingPos;

                    Cell cell = World.inst.GetCellDataClamped(clampedPos);
                    if (cell != null && Grid.Cells.Get(cell))
                        clampedPos.y = Grid.Cells.Get(cell).Elevation;

                    Cam.inst.DesiredTrackingPos = clampedPos;
                }
            }
            return !TopDownModeCamera.active;
        }

        
    }

}
