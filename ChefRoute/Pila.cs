using System;
using System.Collections.Generic;
using System.Text;

namespace ChefRoute
{
    public class Pila<T>
    {
        public Nodo<T> Tope;
        public Pila()
        {
            Tope = null;
        }
        public void Push(T valor)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(valor, Tope);
            Tope = nuevoNodo;
        }
        public T Pop()
        {
            if (Tope == null)
            {
                throw new InvalidOperationException("La pila está vacía.");
            }
            T valor = Tope.Valor;
            Tope = Tope.Siguiente;
            return valor;
        }
        public bool EstaVacia()
        {
            return Tope == null;
        }
    }
}
