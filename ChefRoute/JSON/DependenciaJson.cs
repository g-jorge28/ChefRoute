using System;
using System.Collections.Generic;
using System.Text;

namespace ChefRoute.JSON
{
    public class DependenciaJson
    {
        public string Desde { get; set; } = "";
        public string Hacia { get; set; } = "";
        public double TiempoMinutos { get; set; }
    }
}
