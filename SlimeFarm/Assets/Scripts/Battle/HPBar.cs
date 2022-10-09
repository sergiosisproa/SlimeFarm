using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpProcess)
    {
        health.transform.localScale = new Vector3(hpProcess, 1f);
    }
}
