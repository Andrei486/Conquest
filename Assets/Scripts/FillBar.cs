using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    private float currentValue = 0;
    public float CurrentValue{
        get{
            return currentValue;
        }
        set{
            currentValue = value;
            Redraw();
        }
    }

    private float maxValue = 1;
    public float MaxValue{
        get{
            return maxValue;
        }
        set{
            maxValue = value;
            Redraw();
        }
    }

    Slider slider;
    Image border;
    Image fill;
    // Start is called before the first frame update
    void Start()
    {
        slider = this.GetComponent<Slider>();
        border = transform.Find("Foreground Border").GetComponent<Image>();
        fill = transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
    }
    void Update()
    {
        
    }

    public void Redraw(){
        slider.value = Mathf.Clamp(currentValue / maxValue, 0, 1);
    }
    public void SetColors(Color borderColor, Color bgColor){
        fill.color = bgColor;
        border.color = borderColor;
    }
}
