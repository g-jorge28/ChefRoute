using System;

namespace ChefRoute
{
    public class Grafo
    {
        public Lista_Enlazada_Simple<Tarea> Tareas { get;  set; }

        public Grafo()
        {
            Tareas = new Lista_Enlazada_Simple<Tarea>();
        }

        public void AgregarTarea(Tarea tarea)
        {
            if (tarea == null)
            {
                throw new ArgumentNullException(nameof(tarea));
            }
            if (BuscarTarea(tarea.ID) != null)
            {
                throw new ArgumentException($"Ya existe una tarea con ID {tarea.ID}.");
            }
            Tareas.Agregar(tarea);
        }

        public Tarea BuscarTarea(int id)
        {
            Nodo<Tarea> actual = Tareas.Cabeza;
            while (actual != null)
            {
                if (actual.Valor.ID == id)
                {
                    return actual.Valor;
                }
                actual = actual.Siguiente;
            }
            return null;
        }

        public Tarea BuscarTareaPorNombre(string nombre)
        {
            Nodo<Tarea> actual = Tareas.Cabeza;
            while (actual != null)
            {
                if (actual.Valor.Nombre == nombre)
                {
                    return actual.Valor;
                }
                actual = actual.Siguiente;
            }
            return null;
        }

        public void AgregarArista(int idOrigen, int idDestino, double peso)
        {
            Tarea origen = BuscarTarea(idOrigen);
            Tarea destino = BuscarTarea(idDestino);
            if (origen == null || destino == null)
            {
                throw new ArgumentException("No se pudo agregar la arista. Verifique que las tareas de origen y destino existan.");
            }
            if (peso < 0)
            {
                throw new ArgumentException("El tiempo de una dependencia no puede ser negativo.");
            }
            origen.Aristas.Agregar(new Arista(destino, peso));
        }
    }
}
