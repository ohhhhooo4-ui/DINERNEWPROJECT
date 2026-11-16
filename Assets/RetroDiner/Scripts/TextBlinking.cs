using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum alphaValue
{
    SHRINKING,
    GROWING,
}
public class FlashingText : MonoBehaviour
{
    public alphaValue currentAlphaValue;
    public float CommentminAlpha;
    public float CommentmaxAlpha;
    public float CommentCurrentAlpha;

    public TextMeshProUGUI MyText;

    void Start()
    {
        CommentminAlpha = 0.1f;
        CommentmaxAlpha = 1f;
        CommentCurrentAlpha = 1.0f;
        currentAlphaValue = alphaValue.SHRINKING;
    }

    void Update()
    {
        alphaComments();
    }

    public void alphaComments()
    {
        if (currentAlphaValue == alphaValue.SHRINKING)
        {
            CommentCurrentAlpha = CommentCurrentAlpha - 0.004f;
            MyText.color = new Color(Color.white.r, Color.white.g, Color.white.b, CommentCurrentAlpha);
            if (CommentCurrentAlpha <= CommentminAlpha)
            {
                currentAlphaValue = alphaValue.GROWING;
            }
        }
        else if (currentAlphaValue == alphaValue.GROWING)
        {
            CommentCurrentAlpha = CommentCurrentAlpha + 0.1f;
            MyText.color = new Color(Color.white.r, Color.white.g, Color.white.b, CommentCurrentAlpha);
            if (CommentCurrentAlpha >= CommentmaxAlpha)
            {
                currentAlphaValue = alphaValue.SHRINKING;
            }
        }
    }
}