using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Fox.Localization;
using Fox.UI;
using UnityEngine.UI;
using System.Net;

namespace Elevation
{
    public class LoadingDialog : MonoBehaviour
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _description;
        private Image _progress;

        public static string titleTermDefault { get; } = "generating_title";
        public static string descriptionTermDefault { get; } = "generating_description";

        public static float smoothing { get; } = 3f;

        public static float progressClamp = 0.0001f;


        /// <summary>
        /// Localization term for the title, set to null or empty string to use default
        /// </summary>
        public string title = "";

        /// <summary>
        /// Localization term for the description, set to null or empty string to use default
        /// </summary>
        public string description = "";

        public float desiredProgress;
        public float progress { get; private set; }


        void Awake()
        {
            gameObject.SetActive(false);

            _title = transform.Find("window/Title").GetComponent<TextMeshProUGUI>();
            _description = transform.Find("window/Description").GetComponent<TextMeshProUGUI>();
            _progress = transform.Find("window/Image").GetComponent<Image>();

            UpdateText();
        }

        void Update()
        {
            //Mod.dLog(desiredProgress);
            if (Math.Abs(_progress.fillAmount - desiredProgress) > progressClamp)
                _progress.fillAmount = Mathf.Lerp(_progress.fillAmount, desiredProgress, Time.unscaledDeltaTime * smoothing);
            else
                _progress.fillAmount = desiredProgress;

            Cam.inst.Rotate(Time.unscaledDeltaTime * 0.5f, 0f);
        }

        /// <summary>
        /// Activates the loading screen immediately in a frame
        /// </summary>
        public void Activate()
        {
            SpeedControlUI.inst.SetSpeed(0);
            ConfigurableControls.inst.SetControlsActive(false);
            GameUI.inst.miniMapUI.gameObject.SetActive(false);
            KingdomLog.inst.Container.SetActive(false);
            GameState.inst.AlphaNumericHotkeysEnabled = false;

            gameObject.SetActive(true);
            UpdateText();
            // TODO: Find out how Canvas went away in new game verison ?????
            // Normally UI is rendered at the end of a frame, however in this case, we want the ui rendered immediately, this method will force render the loading dialog and any other ui. 
            //Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Deactivates the loading screen
        /// </summary>
        public void Deactivate()
        {
            SpeedControlUI.inst.SetSpeed(1);
            ConfigurableControls.inst.SetControlsActive(true);
            GameUI.inst.miniMapUI.gameObject.SetActive(true);
            KingdomLog.inst.Container.SetActive(true);
            GameState.inst.AlphaNumericHotkeysEnabled = true;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the text on the loading screen to match the game language
        /// </summary>
        public void UpdateText()
        {
            _title.alignment = TextAlignmentOptions.Center;
            _description.alignment = TextAlignmentOptions.Center;

            _title.text = !string.IsNullOrEmpty(title) ? Localization.Get(title) : Localization.Get(titleTermDefault);
            _description.text = !string.IsNullOrEmpty(description) ? Localization.Get(description) : Localization.Get(descriptionTermDefault);
        }
    }
}
