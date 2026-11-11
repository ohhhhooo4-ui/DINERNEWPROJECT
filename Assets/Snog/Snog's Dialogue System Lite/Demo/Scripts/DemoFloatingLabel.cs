using UnityEngine;
using TMPro;

namespace Snog.InteractionSystem.Demo
{
    /// <summary>
    /// Renders a floating world-space label above a demo object that always faces the camera.
    /// Use this to label each interactable in the demo scene so buyers understand what is being
    /// demonstrated without reading documentation first.
    ///
    /// Setup:
    ///   1. Create an empty child GameObject above your interactable.
    ///   2. Add a TextMeshPro - Text (3D) component to it.
    ///   3. Add this component alongside the TMP_Text.
    ///   4. Set Label and Sublabel in the inspector.
    ///
    /// The label fades in when the player is within Fade Distance and fades out beyond it.
    /// </summary>
    [AddComponentMenu("Snog/Demo/Demo Floating Label")]
    [RequireComponent(typeof(TMP_Text))]
    public class DemoFloatingLabel : MonoBehaviour
    {
        [Header("Content")]
        [Tooltip("The short label shown in large text, e.g. 'Door'")]
        [SerializeField] private string label = "Interactable";

        [Tooltip("The explanation shown in smaller text below, e.g. 'Demonstrates: animated toggle, interaction lock'")]
        [TextArea(2, 4)]
        [SerializeField] private string sublabel = "Demonstrates: BaseInteractable";

        [Header("Appearance")]
        [Tooltip("Font size of the main label.")]
        [SerializeField] private float labelSize    = 0.18f;

        [Tooltip("Font size of the sublabel.")]
        [SerializeField] private float sublabelSize = 0.11f;

        [Tooltip("Colour of the main label.")]
        [SerializeField] private Color labelColor    = Color.white;

        [Tooltip("Colour of the sublabel.")]
        [SerializeField] private Color sublabelColor = new Color(0.75f, 0.85f, 1f, 1f);

        [Header("Fade")]
        [Tooltip("Distance in metres at which the label starts to become visible.")]
        [SerializeField] private float fadeDistance = 8f;

        [Tooltip("Distance in metres at which the label is fully opaque.")]
        [SerializeField] private float fullAlphaDistance = 4f;

        // ─────────────────────────────────────────────────────────────────────

        private TMP_Text   tmp;
        private Transform  cam;

        private void Awake()
        {
            tmp = GetComponent<TMP_Text>();
            tmp.richText      = true;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;

            BuildText(1f);
        }

        private void Start()
        {
            if (Camera.main != null)
                cam = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (cam == null)
            {
                if (Camera.main != null) cam = Camera.main.transform;
                return;
            }

            Vector3 direction = cam.position - transform.position;
            direction.y = 0f; 

            if (direction != Vector3.zero)
            {
                Vector3 flatForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
                transform.rotation = Quaternion.LookRotation(flatForward);
            }

            // Fade based on distance
            float dist  = Vector3.Distance(transform.position, cam.position);
            float alpha = Mathf.InverseLerp(fadeDistance, fullAlphaDistance, dist);
            alpha       = Mathf.Clamp01(alpha);

            BuildText(alpha);
        }

        private void BuildText(float alpha)
        {
            if (tmp == null) return;

            Color lc = labelColor;    lc.a    *= alpha;
            Color sc = sublabelColor; sc.a    *= alpha;

            string lHex  = ColorUtility.ToHtmlStringRGBA(lc);
            string sHex  = ColorUtility.ToHtmlStringRGBA(sc);

            tmp.text = $"<color=#{lHex}><size={labelSize}><b>{label}</b></size></color>\n" +
                       $"<color=#{sHex}><size={sublabelSize}>{sublabel}</size></color>";
        }

        // Call this at runtime to update label text dynamically.
        public void SetLabel(string newLabel, string newSublabel = "")
        {
            label    = newLabel;
            sublabel = newSublabel;
            BuildText(1f);
        }
    }
}
