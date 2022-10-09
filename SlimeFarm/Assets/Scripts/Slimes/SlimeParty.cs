using System.Collections;
using System.Linq; // https://docs.microsoft.com/es-es/dotnet/api/system.linq?view=net-6.0 para el .Where - revisar más adelante
using System.Collections.Generic;
using UnityEngine;

public class SlimeParty : MonoBehaviour
{
    [SerializeField] List<Slime> slimes;

    public List<Slime> Slimes
    {
        get
        {
            return slimes;
        }
    }

    private void Start()
    {
        foreach (var slime in slimes)
        {
            slime.Init();
        }
    }

    public Slime GetAlivesSlime()
    {
        //return slimes.Where(x => x.HP > 0).FirstOrDefault();  //Entiendo que es un loop que revisa todo lo que tenga una variable HP (Slimes en este caso) y devuelve los que cumplen la condición - Magia Negra, revisar!!!
        return slimes.FirstOrDefault(x => x.HP > 0);
        // Si no tiene nada que hacer return es un bug!
    }
}
