namespace ChefRoute.JSON
{
    // Resultado de cargar una receta: el nombre del plato y su grafo de tareas.
    public class RecetaCargada
    {
        public string Plato { get; set; } = "";
        public Grafo Grafo { get; set; }
    }
}
