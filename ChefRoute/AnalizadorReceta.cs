using System;

namespace ChefRoute
{
    public class AnalizadorReceta
    {
        // ---------- MÓDULO 1: detección de ciclos (DFS con 3 estados) ----------

        // Devuelve true si la receta es imposible. En "ciclo" queda el recorrido
        // que se cierra sobre sí mismo (ej: A -> B -> C -> A) para mostrarlo al usuario.
        public bool TieneCiclo(Grafo grafo, out Lista_Enlazada_Simple<Tarea> ciclo)
        {
            ciclo = null;
            ReiniciarEstados(grafo);
            Lista_Enlazada_Simple<Tarea> camino = new Lista_Enlazada_Simple<Tarea>();

            Nodo<Tarea> actual = grafo.Tareas.Cabeza;
            while (actual != null)
            {
                if (actual.Valor.Estado == Tarea.EstadoDFS.NoVisitado)
                {
                    if (BuscarCiclo(actual.Valor, camino, out ciclo))
                    {
                        return true;
                    }
                }
                actual = actual.Siguiente;
            }
            return false;
        }

        public bool BuscarCiclo(Tarea tareaActual, Lista_Enlazada_Simple<Tarea> camino, out Lista_Enlazada_Simple<Tarea> ciclo)
        {
            ciclo = null;
            tareaActual.Estado = Tarea.EstadoDFS.EnProceso;
            camino.Agregar(tareaActual);

            Nodo<Arista> aristaActual = tareaActual.Aristas.Cabeza;
            while (aristaActual != null)
            {
                Tarea destino = aristaActual.Valor.NodoDestino;
                if (destino.Estado == Tarea.EstadoDFS.NoVisitado)
                {
                    if (BuscarCiclo(destino, camino, out ciclo))
                    {
                        return true;
                    }
                }
                else if (destino.Estado == Tarea.EstadoDFS.EnProceso)
                {
                    // Volvimos a un nodo que seguimos explorando: hay ciclo
                    ciclo = ConstruirCiclo(camino, destino);
                    return true;
                }
                aristaActual = aristaActual.Siguiente;
            }

            camino.Eliminar(tareaActual);
            tareaActual.Estado = Tarea.EstadoDFS.Visitado;
            return false;
        }

        public Lista_Enlazada_Simple<Tarea> ConstruirCiclo(Lista_Enlazada_Simple<Tarea> camino, Tarea inicio)
        {
            Lista_Enlazada_Simple<Tarea> ciclo = new Lista_Enlazada_Simple<Tarea>();
            bool dentro = false;
            Nodo<Tarea> actual = camino.Cabeza;
            while (actual != null)
            {
                if (actual.Valor == inicio)
                {
                    dentro = true;
                }
                if (dentro)
                {
                    ciclo.Agregar(actual.Valor);
                }
                actual = actual.Siguiente;
            }
            ciclo.Agregar(inicio); // cierra el ciclo: A -> B -> C -> A
            return ciclo;
        }

        // ---------- MÓDULO 2: orden topológico (DFS + Pila) ----------

        // Lanza InvalidOperationException si la receta tiene un ciclo.
        public Lista_Enlazada_Simple<Tarea> OrdenTopologico(Grafo grafo)
        {
            if (TieneCiclo(grafo, out Lista_Enlazada_Simple<Tarea> ciclo))
            {
                throw new InvalidOperationException("La receta es inválida: tiene dependencias circulares.");
            }

            ReiniciarEstados(grafo);
            Pila<Tarea> pila = new Pila<Tarea>();

            Nodo<Tarea> actual = grafo.Tareas.Cabeza;
            while (actual != null)
            {
                if (actual.Valor.Estado == Tarea.EstadoDFS.NoVisitado)
                {
                    VisitarParaOrden(actual.Valor, pila);
                }
                actual = actual.Siguiente;
            }

            Lista_Enlazada_Simple<Tarea> orden = new Lista_Enlazada_Simple<Tarea>();
            while (!pila.EstaVacia())
            {
                orden.Agregar(pila.Pop());
            }
            return orden;
        }

        // Aquí ya sabemos que no hay ciclos, así que solo apilamos al terminar cada tarea
        private void VisitarParaOrden(Tarea tarea, Pila<Tarea> pila)
        {
            tarea.Estado = Tarea.EstadoDFS.EnProceso;
            Nodo<Arista> aristaActual = tarea.Aristas.Cabeza;
            while (aristaActual != null)
            {
                Tarea destino = aristaActual.Valor.NodoDestino;
                if (destino.Estado == Tarea.EstadoDFS.NoVisitado)
                {
                    VisitarParaOrden(destino, pila);
                }
                aristaActual = aristaActual.Siguiente;
            }
            tarea.Estado = Tarea.EstadoDFS.Visitado;
            pila.Push(tarea);
        }

        // ---------- MÓDULO 3: ronda de supervisión (lista circular) ----------

        public Lista_Enlazada_Circular<Tarea> CrearRonda(Lista_Enlazada_Simple<Tarea> orden)
        {
            Lista_Enlazada_Circular<Tarea> ronda = new Lista_Enlazada_Circular<Tarea>();
            Nodo<Tarea> actual = orden.Cabeza;
            while (actual != null)
            {
                ronda.Agregar(actual.Valor);
                actual = actual.Siguiente;
            }
            return ronda;
        }

        public void ReiniciarEstados(Grafo grafo)
        {
            Nodo<Tarea> actual = grafo.Tareas.Cabeza;
            while (actual != null)
            {
                actual.Valor.Estado = Tarea.EstadoDFS.NoVisitado;
                actual = actual.Siguiente;
            }
        }
    }
}
