using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using TMPro; 
 
public class PresentadorTexto : MonoBehaviour 
{ 
    [SerializeField] private TMP_Text TMPEtiqueta; 
    private Tarjeta tarjeta; 
 
    void Awake() 
    { 
        tarjeta = GetComponent<Tarjeta>(); 
    } 
 
    public void Saludar() 
    { 
        Debug.Log("Hola " + tarjeta.GetNombre()); 
        TMPEtiqueta.text = tarjeta.GetNombre(); 
    } 
 
    public void LimpiarTexto() 
    { 
        TMPEtiqueta.text = "..."; 
    } 
} 