using System;
using System.Collections.Generic;
using System.Text;

namespace ChefRoute.JSON
{
    public class RecetaJson
    {
        public string Plato { get; set; } = "";
        public string[] Tareas { get; set; } = Array.Empty<string>();
        public DependenciaJson[] Dependencias { get; set; } = Array.Empty<DependenciaJson>();
    }
}
