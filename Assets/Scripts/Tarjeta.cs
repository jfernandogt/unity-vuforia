using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
 
public class Tarjeta : MonoBehaviour 
{ 
    [SerializeField] private string nombre; 
 
    public string GetNombre() 
    { 
        return nombre; 
    } 
} 