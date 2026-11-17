using System;
using System.Collections.Generic;
using System.Text;
namespace ChefRoute
{
    public class Nodo<T>
    {
        public T Valor { get; set; }
        public Nodo<T> Siguiente { get; set; }
        public Nodo(T valor, Nodo<T> siguiente)
        {
            Valor = valor;
            Siguiente = siguiente;
        }
    }
}
