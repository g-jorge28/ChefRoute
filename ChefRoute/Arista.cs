using System;
using System.Collections.Generic;
using System.Text;

namespace ChefRoute
{
    public class Arista
    {
        public Tarea NodoDestino { get; set; }
        public double Peso { get; set; }
        public Arista(Tarea nodoDestino, double peso)
        {
            NodoDestino = nodoDestino;
            Peso = peso;
        }
    }
}
