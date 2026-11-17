using System;
using System.Collections.Generic;
using System.Text;

namespace ChefRoute
{
    public class Tarea
    {
        public enum EstadoDFS
        {
            NoVisitado = 0,
            EnProceso = 1,
            Visitado = 2
        }
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public double Tiempo { get; set; }
        public EstadoDFS Estado { get; set; }
        public Lista_Enlazada_Simple<Arista> Aristas { get; set; }
        public Tarea(int id, string nombre, string descripcion, double tiempo)
        {
            ID = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Tiempo = tiempo;
            Aristas = new Lista_Enlazada_Simple<Arista>();
            Estado = EstadoDFS.NoVisitado;
        }
        
    }
}
