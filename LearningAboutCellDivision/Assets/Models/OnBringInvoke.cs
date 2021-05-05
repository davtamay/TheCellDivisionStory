using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnBringInvoke : MonoBehaviour
{
    public UnityEvent onBringAllInvoke;
    public List<GameObject> itemsToObtain;


    public int currentItemBrought;

    public void OnTriggerEnter(Collider collision)
    {
        if(itemsToObtain.Contains(collision.gameObject))
        {

            if (currentItemBrought == itemsToObtain.Count -1)
                onBringAllInvoke.Invoke();

            collision.gameObject.SetActive(false);
            // Destroy();
            currentItemBrought++;
        }

    }
    
}
