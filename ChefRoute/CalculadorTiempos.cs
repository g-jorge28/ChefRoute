using System;

namespace ChefRoute
{
    public class CalculadorTiempos
    {
        // Tiempo mínimo para terminar el plato = camino más largo del grafo (en minutos).
        // Recibe el orden topológico, así cada tarea ya tiene su tiempo final cuando se procesa.
        public double TiempoMinimoTotal(Grafo grafo, Lista_Enlazada_Simple<Tarea> orden)
        {
            int n = orden.Cantidad;
            Tarea[] tareas = new Tarea[n];
            int k = 0;
            foreach (Tarea t in orden)
            {
                tareas[k] = t;
                k++;
            }

            double[] acumulado = new double[n];
            double maximo = 0;
            for (int i = 0; i < n; i++)
            {
                foreach (Arista arista in tareas[i].Aristas)
                {
                    int j = BuscarIndice(tareas, arista.NodoDestino);
                    double candidato = acumulado[i] + arista.Peso;
                    if (j >= 0 && candidato > acumulado[j])
                    {
                        acumulado[j] = candidato;
                    }
                }
                if (acumulado[i] > maximo)
                {
                    maximo = acumulado[i];
                }
            }
            return maximo;
        }

        public int BuscarIndice(Tarea[] tareas, Tarea buscada)
        {
            for (int i = 0; i < tareas.Length; i++)
            {
                if (tareas[i] == buscada)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
