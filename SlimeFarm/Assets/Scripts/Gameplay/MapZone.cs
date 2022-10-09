using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    [SerializeField] List<Slime> wildSlimes;

    public Slime GetRandomSlime()
    {
        int ran = Random.Range(0, 101);
        int slimeGenerate = 0;
        //var wildSlime = wildSlimes[Random.Range(0, wildSlimes.Count)]; // Cambiar más adelante para añadir "spawn ratio"

        slimeGenerate = (ran < 50) ? 0 : ((ran > 50) ? 1 : 2);

        var wildSlime = wildSlimes[slimeGenerate];

        wildSlime.Init();
        return wildSlime;
    }
}
