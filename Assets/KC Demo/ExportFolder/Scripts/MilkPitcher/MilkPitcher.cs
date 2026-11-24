using UnityEngine;
using System.Collections;

public class MilkPitcher : MonoBehaviour
{
    public float milkAmount = 0f;

    public float maxMilkAmount = 300f;

    public float milkTemperature = 5f;

    public AudioSource milkPourAudio;

    public float pourAngle = 120f;

    public bool milkBurnt = false;

    private bool pitcherReset;

    public GameObject readyBorder;

    public Transform milkLiquidPivot;

    private Vector3 originalScale;

    private bool readySignalPlayed;

    private void Start()
    {
        originalScale = milkLiquidPivot.localScale;
    }

    private void Update()
    {
        float fillAmount =
            Mathf.Clamp01(
                milkAmount /
                maxMilkAmount);

        milkLiquidPivot.localScale =
            new Vector3(
                originalScale.x,
                originalScale.y * fillAmount,
                originalScale.z
            );

        if (!readySignalPlayed &&
            milkTemperature >= 60f && milkTemperature <= 70f)
        {
            readySignalPlayed = true;

            StartCoroutine(FlashReadyBorder());

            Debug.Log("Milk Ready");
        }

        if (IsPouring() && milkAmount > 0f)
        {
            if (milkPourAudio != null &&
                !milkPourAudio.isPlaying)
            {
                milkPourAudio.Play();
            }
        }
        else
        {
            if (milkPourAudio != null &&
                milkPourAudio.isPlaying)
            {
                milkPourAudio.Stop();
            }
        }

        if (milkAmount <= 0f && !pitcherReset)
        {
            milkTemperature = 5f;

            milkBurnt = false;

            readySignalPlayed = false;

            if (readyBorder != null)
            {
                readyBorder.SetActive(false);
            }

            pitcherReset = true;

            Debug.Log("Milk Pitcher Reset");
        }

        if (milkAmount > 0f)
        {
            pitcherReset = false;
        }
    }

    public bool IsPouring()
    {
        return transform.eulerAngles.z > pourAngle;
    }

    private IEnumerator FlashReadyBorder()
    {
        for (int i = 0; i < 6; i++)
        {
            readyBorder.SetActive(true);

            yield return new WaitForSeconds(0.15f);

            readyBorder.SetActive(false);

            yield return new WaitForSeconds(0.15f);
        }

        readyBorder.SetActive(true);
    }
}