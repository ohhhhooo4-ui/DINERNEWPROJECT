using UnityEngine;

namespace Snog.DialogueSystem.Demo
{
    [RequireComponent(typeof(Renderer))]
    public class ChangeColorShowcase : MonoBehaviour
    {
        public void ChangeColor(string key)
        {
            if (key != "change_Color")  return;
            
            Color newColor = new(Random.value, Random.value, Random.value);
            GetComponent<Renderer>().material.color = newColor;
        }
    }
}