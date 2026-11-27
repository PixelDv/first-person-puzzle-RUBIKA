using UnityEngine;
using TMPro;
using System.Collections;

public class TextActionDisapear : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private TextMeshProUGUI text;

    private void Start() 
    {
        StartCoroutine(WaitForDisapear());
    }

    public void ShowText()
    {
        StartCoroutine(WaitForDisapear());
    }

    private IEnumerator WaitForDisapear()
    {
        text.enabled = true;
        yield return new WaitForSeconds(duration);
        text.enabled = false;
    }
}
