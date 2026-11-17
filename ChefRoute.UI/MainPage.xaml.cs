using ChefRoute.JSON;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Storage;
using System.Text;

namespace ChefRoute.UI;

public partial class MainPage : ContentPage
{
    private readonly CargadorReceta _cargador = new CargadorReceta();
    private readonly AnalizadorReceta _analizador = new AnalizadorReceta();
    private readonly CalculadorTiempos _tiempos = new CalculadorTiempos();
    private readonly GrafoDrawable _dibujo = new GrafoDrawable();

    private RecetaCargada _receta;
    private Lista_Enlazada_Simple<Tarea> _orden;
    private Lista_Enlazada_Circular<Tarea> _ronda;
    private int _vuelta;
    private IDispatcherTimer _temporizador;

    public MainPage()
    {
        InitializeComponent();
        GraficoGrafo.Drawable = _dibujo;
    }

    // ---------- cargar la receta ----------

    private async void AlAbrirJson(object sender, EventArgs e)
    {
        try
        {
            FilePickerFileType tipos = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json" } }
            });
            FileResult archivo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elige la receta (.json)",
                FileTypes = tipos
            });
            if (archivo == null)
            {
                return; // el usuario canceló
            }

            using Stream flujo = await archivo.OpenReadAsync();
            using StreamReader lector = new StreamReader(flujo);
            string texto = await lector.ReadToEndAsync();
            CargarReceta(texto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo cargar la receta", ex.Message, "Entendido");
        }
    }

    private async void AlCargarEjemploValido(object sender, EventArgs e)
    {
        await CargarEjemploAsync("receta_ejemplo.json");
    }

    private async void AlCargarEjemploCiclo(object sender, EventArgs e)
    {
        await CargarEjemploAsync("receta_con_ciclo.json");
    }

    private async Task CargarEjemploAsync(string nombreArchivo)
    {
        try
        {
            using Stream flujo = await FileSystem.OpenAppPackageFileAsync(nombreArchivo);
            using StreamReader lector = new StreamReader(flujo);
            string texto = await lector.ReadToEndAsync();
            CargarReceta(texto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo cargar el ejemplo", ex.Message, "Entendido");
        }
    }

    // Si el JSON es inválido lanza excepción ANTES de tocar el estado actual
    private void CargarReceta(string texto)
    {
        RecetaCargada nueva = _cargador.CargarDesdeTexto(texto);

        DetenerAutomatico();
        _receta = nueva;
        _orden = null;
        _ronda = null;
        _vuelta = 0;

        _dibujo.Grafo = nueva.Grafo;
        _dibujo.Ciclo = null;
        _dibujo.Orden = null;
        _dibujo.Ronda = null;
        _dibujo.Actual = null;

        LabelPlato.Text = string.IsNullOrWhiteSpace(nueva.Plato) ? "Receta sin nombre" : nueva.Plato;
        ListaTareas.ItemsSource = ConstruirLineasTareas(nueva.Grafo);
        ListaDependencias.ItemsSource = ConstruirLineasDependencias(nueva.Grafo);
        ListaOrden.ItemsSource = null;
        TextoTiempo.Text = "";
        TextoRonda.Text = "Calcula la secuencia para preparar la ronda.";

        BotonValidar.IsEnabled = true;
        BotonCalcular.IsEnabled = true;
        ActualizarBotonesRonda();

        // Se valida de una vez para que el usuario vea enseguida si la receta es posible
        Validar(out _);
    }

    // ---------- módulo 1: ciclos ----------

    private async void AlValidar(object sender, EventArgs e)
    {
        if (_receta == null)
        {
            return;
        }
        if (Validar(out Lista_Enlazada_Simple<Tarea> ciclo))
        {
            await DisplayAlert("Receta imposible", "Esta receta no se puede preparar.\n\nCiclo encontrado:\n" + TextoDelCiclo(ciclo), "Entendido");
        }
    }

    // Devuelve true si hay ciclo. Actualiza el aviso y el dibujo.
    private bool Validar(out Lista_Enlazada_Simple<Tarea> ciclo)
    {
        bool hayCiclo = _analizador.TieneCiclo(_receta.Grafo, out ciclo);
        _dibujo.Ciclo = ciclo;

        if (hayCiclo)
        {
            PintarAviso(Paleta.ChileClaro, Paleta.Chile, "Receta imposible. Ciclo: " + TextoDelCiclo(ciclo));
        }
        else
        {
            PintarAviso(Paleta.AlbahacaClaro, Paleta.Albahaca, "Receta válida: no hay dependencias circulares.");
        }
        GraficoGrafo.Invalidate();
        return hayCiclo;
    }

    private void PintarAviso(Color fondo, Color acento, string texto)
    {
        BannerValidacion.Background = new SolidColorBrush(fondo);
        BannerValidacion.Stroke = new SolidColorBrush(acento);
        TextoValidacion.TextColor = acento;
        TextoValidacion.Text = texto;
    }

    // ---------- módulo 2: orden topológico ----------

    private async void AlCalcularSecuencia(object sender, EventArgs e)
    {
        if (_receta == null)
        {
            return;
        }

        DetenerAutomatico();
        try
        {
            _orden = _analizador.OrdenTopologico(_receta.Grafo);
        }
        catch (InvalidOperationException ex)
        {
            _orden = null;
            _ronda = null;
            _dibujo.Orden = null;
            _dibujo.Ronda = null;
            _dibujo.Actual = null;
            ListaOrden.ItemsSource = null;
            TextoTiempo.Text = "";
            Validar(out Lista_Enlazada_Simple<Tarea> ciclo);
            ActualizarBotonesRonda();
            await DisplayAlert("Receta imposible", ex.Message + "\n\nCiclo encontrado:\n" + TextoDelCiclo(ciclo), "Entendido");
            return;
        }

        Validar(out _);
        ListaOrden.ItemsSource = ConstruirLineasOrden(_orden);
        double total = _tiempos.TiempoMinimoTotal(_receta.Grafo, _orden);
        TextoTiempo.Text = "Tiempo mínimo del plato: " + total.ToString("0.##") + " min (camino más largo)";

        _dibujo.Orden = _orden;
        PrepararRonda();
    }

    // ---------- módulo 3: ronda circular ----------

    private void PrepararRonda()
    {
        DetenerAutomatico();
        _ronda = _analizador.CrearRonda(_orden);
        _vuelta = 0;
        _dibujo.Ronda = _ronda;
        _dibujo.Actual = null;
        TextoRonda.Text = "Ronda lista con " + _ronda.Cantidad + " tareas. Pulsa Siguiente para que el chef empiece.";
        ActualizarBotonesRonda();
        GraficoGrafo.Invalidate();
    }

    private void AlSiguiente(object sender, EventArgs e)
    {
        AvanzarRonda();
    }

    private void AvanzarRonda()
    {
        if (_ronda == null || _ronda.Cantidad == 0)
        {
            return;
        }

        Tarea tarea = _ronda.Avanzar();
        if (_ronda.Actual == _ronda.Cabeza)
        {
            _vuelta++; // volvió al principio: empieza una vuelta nueva
        }

        _dibujo.Actual = tarea;
        TextoRonda.Text = "El chef está en: " + tarea.Nombre + "\nVuelta " + _vuelta + " · " + _ronda.Cantidad + " tareas activas en la ronda";
        GraficoGrafo.Invalidate();
    }

    private void AlTerminarTarea(object sender, EventArgs e)
    {
        if (_ronda == null || _ronda.Cantidad == 0)
        {
            return;
        }
        if (_ronda.Actual == null)
        {
            TextoRonda.Text = "Primero pulsa Siguiente para que el chef llegue a una tarea.";
            return;
        }

        Tarea terminada = _ronda.Actual.Valor;
        _ronda.Eliminar(terminada); // sale de la ronda sin romper el círculo
        _dibujo.Actual = null;

        if (_ronda.Cantidad == 0)
        {
            DetenerAutomatico();
            TextoRonda.Text = "Terminada: " + terminada.Nombre + "\n¡Todas las tareas están listas! El plato está terminado.";
        }
        else
        {
            TextoRonda.Text = "Terminada: " + terminada.Nombre + "\nQuedan " + _ronda.Cantidad + " tareas activas. Pulsa Siguiente.";
        }
        ActualizarBotonesRonda();
        GraficoGrafo.Invalidate();
    }

    private void AlReiniciarRonda(object sender, EventArgs e)
    {
        if (_orden == null)
        {
            return;
        }
        PrepararRonda();
    }

    private void AlCambiarAutomatico(object sender, EventArgs e)
    {
        if (_temporizador != null && _temporizador.IsRunning)
        {
            DetenerAutomatico();
            return;
        }
        if (_ronda == null || _ronda.Cantidad == 0)
        {
            return;
        }

        if (_temporizador == null)
        {
            _temporizador = Dispatcher.CreateTimer();
            _temporizador.Interval = TimeSpan.FromSeconds(1.2);
            _temporizador.Tick += (remitente, argumentos) => AvanzarRonda();
        }
        _temporizador.Start();
        BotonAuto.Text = "Pausar";
    }

    private void DetenerAutomatico()
    {
        if (_temporizador != null && _temporizador.IsRunning)
        {
            _temporizador.Stop();
        }
        BotonAuto.Text = "Automático";
    }

    private void ActualizarBotonesRonda()
    {
        bool activa = _ronda != null && _ronda.Cantidad > 0;
        BotonSiguiente.IsEnabled = activa;
        BotonAuto.IsEnabled = activa;
        BotonTerminar.IsEnabled = activa;
        BotonReiniciar.IsEnabled = _orden != null;
    }

    // ---------- textos para las listas de la pantalla ----------

    private static string TextoDelCiclo(Lista_Enlazada_Simple<Tarea> ciclo)
    {
        if (ciclo == null)
        {
            return "";
        }
        StringBuilder texto = new StringBuilder();
        foreach (Tarea tarea in ciclo)
        {
            if (texto.Length > 0)
            {
                texto.Append(" → ");
            }
            texto.Append(tarea.Nombre);
        }
        return texto.ToString();
    }

    private static string[] ConstruirLineasTareas(Grafo grafo)
    {
        string[] lineas = new string[grafo.Tareas.Cantidad];
        int i = 0;
        foreach (Tarea tarea in grafo.Tareas)
        {
            lineas[i] = tarea.ID + ". " + tarea.Nombre;
            i++;
        }
        return lineas;
    }

    private static string[] ConstruirLineasDependencias(Grafo grafo)
    {
        int total = 0;
        foreach (Tarea tarea in grafo.Tareas)
        {
            total += tarea.Aristas.Cantidad;
        }

        string[] lineas = new string[total];
        int i = 0;
        foreach (Tarea tarea in grafo.Tareas)
        {
            foreach (Arista arista in tarea.Aristas)
            {
                lineas[i] = tarea.Nombre + " → " + arista.NodoDestino.Nombre + "  (" + arista.Peso.ToString("0.##") + " min)";
                i++;
            }
        }
        return lineas;
    }

    private static string[] ConstruirLineasOrden(Lista_Enlazada_Simple<Tarea> orden)
    {
        string[] lineas = new string[orden.Cantidad];
        int i = 0;
        foreach (Tarea tarea in orden)
        {
            lineas[i] = (i + 1) + ". " + tarea.Nombre;
            i++;
        }
        return lineas;
    }
}
