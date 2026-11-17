using System;
using System.IO;
using System.Text.Json;

namespace ChefRoute.JSON
{
    public class CargadorReceta
    {
        // Para la interfaz: FilePicker entrega un stream/ruta, se puede leer el texto y pasarlo aquí
        public RecetaCargada CargarDesdeArchivo(string ruta)
        {
            try
            {
                string texto = File.ReadAllText(ruta);
                return CargarDesdeTexto(texto);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidDataException($"No se encontró el archivo: {ruta}");
            }
            catch (DirectoryNotFoundException)
            {
                throw new InvalidDataException($"La ruta no existe: {ruta}");
            }
            catch (IOException ex)
            {
                throw new InvalidDataException($"No se pudo leer el archivo: {ex.Message}");
            }
        }

        public RecetaCargada CargarDesdeTexto(string json)
        {
            RecetaJson datos;
            try
            {
                JsonSerializerOptions opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                datos = JsonSerializer.Deserialize<RecetaJson>(json, opciones);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"El JSON no tiene un formato válido: {ex.Message}");
            }

            if (datos == null || datos.Tareas == null || datos.Tareas.Length == 0)
            {
                throw new InvalidDataException("El JSON no contiene tareas.");
            }

            Grafo grafo = new Grafo();

            // Las tareas vienen solo con nombre: les asignamos ID 1, 2, 3...
            int id = 1;
            for (int i = 0; i < datos.Tareas.Length; i++)
            {
                string nombre = datos.Tareas[i];
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new InvalidDataException("Hay una tarea sin nombre.");
                }
                if (grafo.BuscarTareaPorNombre(nombre) != null)
                {
                    throw new InvalidDataException($"La tarea \"{nombre}\" está repetida.");
                }
                grafo.AgregarTarea(new Tarea(id, nombre, "", 0));
                id++;
            }

            if (datos.Dependencias != null)
            {
                for (int i = 0; i < datos.Dependencias.Length; i++)
                {
                    DependenciaJson dep = datos.Dependencias[i];
                    Tarea origen = grafo.BuscarTareaPorNombre(dep.Desde);
                    Tarea destino = grafo.BuscarTareaPorNombre(dep.Hacia);
                    if (origen == null || destino == null)
                    {
                        throw new InvalidDataException($"La dependencia \"{dep.Desde}\" -> \"{dep.Hacia}\" usa una tarea que no está en la lista de tareas.");
                    }
                    grafo.AgregarArista(origen.ID, destino.ID, dep.TiempoMinutos);
                }
            }

            return new RecetaCargada { Plato = datos.Plato, Grafo = grafo };
        }
    }
}
