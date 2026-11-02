using UnityEngine;

public class LightToggle : MonoBehaviour
{
    public Light controlledLight;   // 2. reference to Light
    public bool lightIsOn;          // 2. tracking our light state
    public float brightness;        // 2. max brightness. You could give it default value ( =10f; )

    public void Toggle() // 3. Started from button press. It will toggle the light on --> off --> on --> off ....
    {
        if (lightIsOn == false)                 // 3. if the lights aren't on
        {
            controlledLight.intensity = brightness; // 3. turn the light on
            lightIsOn = true;                       // 4. change light state to sync with light's intensity --> light is now on
        }
        else                                    // 3. if the lights are on
        {
            controlledLight.intensity = 0;          // 3. turn the light off
            lightIsOn = false;                      // 4. change light state to sync with light's intensity --> light is now off
        }
    }

}
