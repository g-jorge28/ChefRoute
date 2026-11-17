using Microsoft.Maui.Graphics;

namespace ChefRoute.UI;

// Dibuja el grafo: cada tarea es una caja y cada dependencia una flecha con sus minutos.
// Solo lee las estructuras del backend; no modifica nada.
public class GrafoDrawable : IDrawable
{
    public Grafo Grafo { get; set; }
    public Lista_Enlazada_Simple<Tarea> Ciclo { get; set; }
    public Lista_Enlazada_Simple<Tarea> Orden { get; set; }
    public Lista_Enlazada_Circular<Tarea> Ronda { get; set; }
    public Tarea Actual { get; set; }

    private const float AltoNodo = 40f;
    private float _anchoNodo = 130f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Grafo == null || Grafo.Tareas.Cantidad == 0)
        {
            canvas.FontColor = Paleta.TextoSuave;
            canvas.FontSize = 14;
            canvas.DrawString("Carga una receta para ver su grafo", dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height,
                HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        int n = Grafo.Tareas.Cantidad;
        Tarea[] tareas = new Tarea[n];
        int k = 0;
        foreach (Tarea t in Grafo.Tareas)
        {
            tareas[k] = t;
            k++;
        }

        PointF[] centros = CalcularPosiciones(tareas, dirtyRect);

        // 1) flechas (debajo de las cajas)
        for (int i = 0; i < n; i++)
        {
            foreach (Arista arista in tareas[i].Aristas)
            {
                int j = BuscarIndice(tareas, arista.NodoDestino);
                if (j < 0)
                {
                    continue;
                }
                DibujarFlecha(canvas, centros[i], centros[j], arista.Peso, i == j, AristaEnCiclo(tareas[i], arista.NodoDestino));
            }
        }

        // 2) cajas de las tareas
        for (int i = 0; i < n; i++)
        {
            DibujarNodo(canvas, tareas[i], centros[i]);
        }
    }

    // ---------- posiciones ----------

    private PointF[] CalcularPosiciones(Tarea[] tareas, RectF area)
    {
        int n = tareas.Length;
        PointF[] centros = new PointF[n];

        if (Ciclo != null)
        {
            // Con ciclo no hay "capas": se reparten en círculo para que se vea la vuelta
            _anchoNodo = 118f;
            float cx = area.Width / 2f;
            float cy = area.Height / 2f;
            float rx = MathF.Max(10f, area.Width / 2f - _anchoNodo / 2f - 24f);
            float ry = MathF.Max(10f, area.Height / 2f - AltoNodo / 2f - 34f);
            for (int i = 0; i < n; i++)
            {
                float angulo = -MathF.PI / 2f + 2f * MathF.PI * i / n;
                centros[i] = new PointF(cx + rx * MathF.Cos(angulo), cy + ry * MathF.Sin(angulo));
            }
            return centros;
        }

        // Sin ciclo: cada tarea va en una columna según cuántos pasos previos necesita
        int[] capa = new int[n];
        for (int pasada = 0; pasada < n; pasada++)
        {
            for (int i = 0; i < n; i++)
            {
                foreach (Arista arista in tareas[i].Aristas)
                {
                    int j = BuscarIndice(tareas, arista.NodoDestino);
                    if (j >= 0 && capa[j] < capa[i] + 1)
                    {
                        capa[j] = capa[i] + 1;
                    }
                }
            }
        }

        int numCapas = 1;
        for (int i = 0; i < n; i++)
        {
            if (capa[i] + 1 > numCapas)
            {
                numCapas = capa[i] + 1;
            }
        }

        int[] enCapa = new int[numCapas];
        int[] posicionEnCapa = new int[n];
        for (int i = 0; i < n; i++)
        {
            posicionEnCapa[i] = enCapa[capa[i]];
            enCapa[capa[i]]++;
        }

        float margen = 80f;
        float anchoUtil = MathF.Max(1f, area.Width - 2f * margen);
        float separacion = numCapas > 1 ? anchoUtil / (numCapas - 1) : anchoUtil;
        _anchoNodo = MathF.Max(80f, MathF.Min(130f, separacion - 34f));

        for (int i = 0; i < n; i++)
        {
            float x = numCapas == 1 ? area.Width / 2f : margen + separacion * capa[i];
            float y = area.Height * (posicionEnCapa[i] + 0.5f) / enCapa[capa[i]];
            centros[i] = new PointF(x, y);
        }
        return centros;
    }

    // ---------- dibujo ----------

    private void DibujarNodo(ICanvas canvas, Tarea tarea, PointF centro)
    {
        float x = centro.X - _anchoNodo / 2f;
        float y = centro.Y - AltoNodo / 2f;

        Color relleno = Paleta.Blanco;
        Color borde = Paleta.Tinta;
        float grosor = 1.5f;

        if (EstaTerminada(tarea))
        {
            relleno = Paleta.AlbahacaClaro;
            borde = Paleta.Albahaca;
        }
        if (NodoEnCiclo(tarea))
        {
            relleno = Paleta.ChileClaro;
            borde = Paleta.Chile;
        }
        if (tarea == Actual)
        {
            relleno = Paleta.Azafran;
            borde = Paleta.Tinta;
            grosor = 3f;
        }

        canvas.FillColor = relleno;
        canvas.FillRoundedRectangle(x, y, _anchoNodo, AltoNodo, 8f);
        canvas.StrokeColor = borde;
        canvas.StrokeSize = grosor;
        canvas.DrawRoundedRectangle(x, y, _anchoNodo, AltoNodo, 8f);

        canvas.FontColor = Paleta.Tinta;
        canvas.FontSize = _anchoNodo >= 110f ? 12f : 10f;
        canvas.DrawString(tarea.Nombre, x + 4f, y, _anchoNodo - 8f, AltoNodo,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        // Número del paso en la secuencia (solo cuando ya se calculó el orden)
        int posicion = PosicionEnOrden(tarea);
        if (posicion > 0)
        {
            canvas.FillColor = Paleta.Tinta;
            canvas.FillCircle(x, y, 11f);
            canvas.FontColor = Paleta.Blanco;
            canvas.FontSize = 11f;
            canvas.DrawString(posicion.ToString(), x - 11f, y - 11f, 22f, 22f,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private void DibujarFlecha(ICanvas canvas, PointF desde, PointF hasta, double peso, bool haciaSiMisma, bool enCiclo)
    {
        Color color = enCiclo ? Paleta.Chile : Color.FromArgb("#7C898D");
        float grosor = enCiclo ? 2.5f : 1.6f;

        if (haciaSiMisma)
        {
            canvas.StrokeColor = color;
            canvas.StrokeSize = grosor;
            canvas.DrawEllipse(desde.X + _anchoNodo / 4f, desde.Y - AltoNodo / 2f - 16f, 30f, 22f);
            return;
        }

        float dx = hasta.X - desde.X;
        float dy = hasta.Y - desde.Y;
        PointF inicio = PuntoEnBorde(desde, dx, dy);
        PointF fin = PuntoEnBorde(hasta, -dx, -dy);

        canvas.StrokeColor = color;
        canvas.StrokeSize = grosor;
        canvas.DrawLine(inicio.X, inicio.Y, fin.X, fin.Y);

        // punta de la flecha
        float largo = MathF.Sqrt((fin.X - inicio.X) * (fin.X - inicio.X) + (fin.Y - inicio.Y) * (fin.Y - inicio.Y));
        if (largo > 1f)
        {
            float ux = (fin.X - inicio.X) / largo;
            float uy = (fin.Y - inicio.Y) / largo;
            PointF baseFlecha = new PointF(fin.X - ux * 11f, fin.Y - uy * 11f);
            PathF punta = new PathF();
            punta.MoveTo(fin.X, fin.Y);
            punta.LineTo(baseFlecha.X - uy * 5.5f, baseFlecha.Y + ux * 5.5f);
            punta.LineTo(baseFlecha.X + uy * 5.5f, baseFlecha.Y - ux * 5.5f);
            punta.Close();
            canvas.FillColor = color;
            canvas.FillPath(punta);
        }

        // minutos de la dependencia, sobre la mitad de la flecha
        float mx = (inicio.X + fin.X) / 2f;
        float my = (inicio.Y + fin.Y) / 2f;
        canvas.FillColor = Paleta.Fondo;
        canvas.FillRoundedRectangle(mx - 17f, my - 10f, 34f, 20f, 6f);
        canvas.FontColor = enCiclo ? Paleta.Chile : Paleta.Tinta;
        canvas.FontSize = 11f;
        canvas.DrawString(peso.ToString("0.##"), mx - 17f, my - 10f, 34f, 20f,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    // Punto donde la línea que sale del centro (hacia dx, dy) cruza el borde de la caja
    private PointF PuntoEnBorde(PointF centro, float dx, float dy)
    {
        float tx = dx == 0f ? float.MaxValue : (_anchoNodo / 2f) / MathF.Abs(dx);
        float ty = dy == 0f ? float.MaxValue : (AltoNodo / 2f) / MathF.Abs(dy);
        float t = MathF.Min(tx, ty);
        return new PointF(centro.X + dx * t, centro.Y + dy * t);
    }

    // ---------- consultas sobre el estado ----------

    private int BuscarIndice(Tarea[] tareas, Tarea buscada)
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

    private bool NodoEnCiclo(Tarea tarea)
    {
        if (Ciclo == null)
        {
            return false;
        }
        foreach (Tarea t in Ciclo)
        {
            if (t == tarea)
            {
                return true;
            }
        }
        return false;
    }

    private bool AristaEnCiclo(Tarea origen, Tarea destino)
    {
        if (Ciclo == null)
        {
            return false;
        }
        Tarea anterior = null;
        foreach (Tarea t in Ciclo)
        {
            if (anterior == origen && t == destino && anterior != null)
            {
                return true;
            }
            anterior = t;
        }
        return false;
    }

    private int PosicionEnOrden(Tarea tarea)
    {
        if (Orden == null)
        {
            return 0;
        }
        int posicion = 1;
        foreach (Tarea t in Orden)
        {
            if (t == tarea)
            {
                return posicion;
            }
            posicion++;
        }
        return 0;
    }

    // Terminada = ya salió de la ronda de supervisión
    private bool EstaTerminada(Tarea tarea)
    {
        if (Ronda == null || Orden == null)
        {
            return false;
        }
        foreach (Tarea t in Ronda)
        {
            if (t == tarea)
            {
                return false;
            }
        }
        return true;
    }
}
