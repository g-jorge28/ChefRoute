using System;
using System.Collections;
using System.Collections.Generic;

namespace ChefRoute
{
    public class Lista_Enlazada_Simple<T> 
    {
        public Nodo<T> Cabeza { get; set; }
        public int Cantidad { get; set; }

        public void Agregar(T valor)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(valor, null);
            if (Cabeza == null)
            {
                Cabeza = nuevoNodo;
            }
            else
            {
                Nodo<T> actual = Cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            Cantidad++;
        }

        public void Eliminar(T valor)
        {
            if (Cabeza == null)
            {
                return;
            }
            if (Equals(Cabeza.Valor, valor))
            {
                Cabeza = Cabeza.Siguiente;
                Cantidad--;
                return;
            }
            Nodo<T> actual = Cabeza;
            while (actual.Siguiente != null && !Equals(actual.Siguiente.Valor, valor))
            {
                actual = actual.Siguiente;
            }
            if (actual.Siguiente != null)
            {
                actual.Siguiente = actual.Siguiente.Siguiente;
                Cantidad--;
            }
        }

        // Permite recorrer la lista con foreach y enlazarla a la interfaz (CollectionView)
        public IEnumerator<T> GetEnumerator()
        {
            Nodo<T> actual = Cabeza;
            while (actual != null)
            {
                yield return actual.Valor;
                actual = actual.Siguiente;
            }
        }
    }
}
