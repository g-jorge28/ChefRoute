using System;
using System.Collections;
using System.Collections.Generic;

namespace ChefRoute
{
    public class Lista_Enlazada_Circular<T> 
    {
        public Nodo<T> Cabeza { get; set; }

        // Posición actual de la ronda donde está el chef ahora mismo
        public Nodo<T> Actual { get; set; }
        public int Cantidad { get; set; }

        public void Agregar(T valor)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(valor, null);
            if (Cabeza == null)
            {
                Cabeza = nuevoNodo;
                nuevoNodo.Siguiente = Cabeza;
            }
            else
            {
                Nodo<T> actual = Cabeza;
                while (actual.Siguiente != Cabeza)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
                nuevoNodo.Siguiente = Cabeza;
            }
            Cantidad++;
        }

        public void Eliminar(T valor)
        {
            if (Cabeza == null)
            {
                return;
            }

            Nodo<T> anterior = Cabeza;
            do
            {
                Nodo<T> candidato = anterior.Siguiente;
                if (Equals(candidato.Valor, valor))
                {
                    if (candidato == anterior)
                    {
                        Cabeza = null;
                        Actual = null;
                    }
                    else
                    {
                        anterior.Siguiente = candidato.Siguiente;
                        if (candidato == Cabeza)
                        {
                            Cabeza = candidato.Siguiente;
                        }
                        if (candidato == Actual)
                        {
                            Actual = anterior;
                        }
                    }
                    Cantidad--;
                    return;
                }
                anterior = anterior.Siguiente;
            } while (anterior != Cabeza);
        }

        // Mueve al chef al siguiente paso de la ronda y devuelve ese valor.
        // Cuando llega al final vuelve solo a la cabeza .
        public T Avanzar()
        {
            if (Cabeza == null)
            {
                throw new InvalidOperationException("La ronda está vacía.");
            }
            Actual = (Actual == null) ? Cabeza : Actual.Siguiente;
            return Actual.Valor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Cabeza == null)
            {
                yield break;
            }
            Nodo<T> actual = Cabeza;
            do
            {
                yield return actual.Valor;
                actual = actual.Siguiente;
            } while (actual != Cabeza);
        }
    }
}
