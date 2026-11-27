using UnityEngine;
using TMPro;
using System.Collections;

public class TextActionDisapear : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private GameObject text;
    [SerializeField] private bool isSpawningAtStart;

    private void Start() 
    {
        text = gameObject;  
      if (isSpawningAtStart)
        {
            StartCoroutine(WaitForDisapear());
        }
    }

    public void ShowText(bool isShowing)
    {
        text.SetActive(isShowing);
    }

    private IEnumerator WaitForDisapear()
    {
        text.SetActive(true);
        yield return new WaitForSeconds(duration);
        text.SetActive(false);
    }
}
