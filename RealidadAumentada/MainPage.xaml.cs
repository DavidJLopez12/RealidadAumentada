using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace RealidadAumentada;

public partial class MainPage : ContentPage, IDrawable
{
    // Lista de objetos AR que se van a dibujar en la pantalla
    private readonly List<ARObject> arObjects = new();

    // Generador de números aleatorios para posición, tamaño, color, etc.
    private readonly Random random = new();

    // Indica si el "seguimiento" (animación) está activo o pausado
    private bool trackingEnabled = true;

    // Márgenes para que los objetos no se dibujen pegados a los bordes
    private const int MarginX = 30;
    private const int MarginY = 30;

    // Desplazamiento máximo usado para el pequeño movimiento sinusoidal de los objetos
    private const float MotionOffset = 15f;

    public MainPage()
    {
        // Inicializa los componentes declarados en XAML
        InitializeComponent();

        System.Diagnostics.Debug.WriteLine("MainPage ctor: InitializeComponent OK");

        // Comprobación defensiva: asegurarse de que el GraphicsView existe
        if (graphicsView is null)
        {
            System.Diagnostics.Debug.WriteLine("MainPage ctor: graphicsView es NULL");
        }
        else
        {
            // Asigna esta página como drawable del GraphicsView
            graphicsView.Drawable = this;
            System.Diagnostics.Debug.WriteLine("MainPage ctor: graphicsView.Drawable asignado");
        }

        // Temporizador periódico para refrescar el dibujo y simular movimiento en tiempo real
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
        {
            try
            {
                // Solo invalidar el dibujo si el tracking está activo y la vista existe
                if (trackingEnabled && graphicsView is not null)
                {
                    graphicsView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                // Log de cualquier excepción que ocurra dentro del timer
                System.Diagnostics.Debug.WriteLine($"Timer EXCEPCIÓN: {ex.GetType().Name} - {ex.Message}");
            }

            // Devolvemos true para que el timer siga ejecutándose
            return true;
        });
    }

    // Método de dibujo llamado por GraphicsView en cada invalidación
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        try
        {
            // Fondo negro que simula la cámara
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            // Dibuja un grid verde translúcido que simula un plano detectado
            canvas.StrokeColor = Colors.DarkGreen.WithAlpha(0.3f);
            canvas.StrokeSize = 1;

            // Líneas verticales del grid
            for (float x = 0; x < dirtyRect.Width; x += 80)
            {
                canvas.DrawLine(x, 0, x, dirtyRect.Height);
            }

            // Líneas horizontales del grid
            for (float y = 0; y < dirtyRect.Height; y += 80)
            {
                canvas.DrawLine(0, y, dirtyRect.Width, y);
            }

            // Recorremos todos los objetos AR para dibujarlos
            foreach (var obj in arObjects)
            {
                // Cálculo de un pequeño desplazamiento sinusoidal en X e Y
                float offsetX = (float)Math.Sin(DateTime.Now.Millisecond / 200.0 + obj.Id) * MotionOffset;
                float offsetY = (float)Math.Cos(DateTime.Now.Millisecond / 200.0 + obj.Id) * MotionOffset;

                // Posición base más desplazamiento
                float x = obj.X + offsetX;
                float y = obj.Y + offsetY;

                // Evitar que se salgan de los límites visibles, aplicando márgenes
                x = Math.Clamp(x, MarginX, dirtyRect.Width - MarginX - obj.Size);
                y = Math.Clamp(y, MarginY, dirtyRect.Height - MarginY - obj.Size);

                // Dibuja una sombra ovalada por debajo del objeto (para dar sensación de volumen)
                canvas.FillColor = Colors.Black.WithAlpha(0.5f);
                canvas.FillEllipse(x - 8, y + 15, obj.Size + 10, (obj.Size + 10) / 3);

                // Dibuja el objeto AR propiamente dicho (círculo o estrella)
                canvas.FillColor = obj.Color;
                if (obj.Type == ARObjectType.Circle)
                {
                    // Círculo simple
                    canvas.FillEllipse(x, y, obj.Size, obj.Size);
                }
                else
                {
                    // Estrella centrada en el objeto
                    DrawStar(canvas, x + obj.Size / 2, y + obj.Size / 2, obj.Size / 2, 5);
                }
            }
        }
        catch (Exception ex)
        {
            // Cualquier problema en el dibujado se loguea, pero no se propaga
            System.Diagnostics.Debug.WriteLine($"Draw EXCEPCIÓN: {ex.GetType().Name} - {ex.Message}");
        }
    }

    // Dibuja una estrella con un número de puntas determinado
    private void DrawStar(ICanvas canvas, float cx, float cy, float radius, int points)
    {
        // Path que describe la forma de la estrella
        var path = new PathF();

        // La estrella tiene el doble de vértices (puntas y entrantes)
        for (int i = 0; i < points * 2; i++)
        {
            // Ángulo actual para el vértice i
            double angle = Math.PI * i / points - Math.PI / 2;

            // Radio alternando entre grande (punta) y pequeño (entrante)
            float r = i % 2 == 0 ? radius : radius * 0.5f;

            // Coordenadas cartesianas a partir del ángulo y el radio
            float x = cx + (float)(Math.Cos(angle) * r);
            float y = cy + (float)(Math.Sin(angle) * r);

            // Primer punto -> MoveTo, el resto -> LineTo
            if (i == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        // Cierra la figura y la rellena
        path.Close();
        canvas.FillPath(path);
    }

    // Manejador del botón "Agregar Objeto AR"
    private void OnAddObjectClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnAddObjectClicked: inicio");

        // Crea un nuevo objeto AR con propiedades aleatorias
        var obj = new ARObject
        {
            Id = arObjects.Count,
            X = random.Next(50, 250),
            Y = random.Next(50, 350),
            Size = random.Next(40, 80),
            Color = GetRandomColor(),
            Type = random.Next(0, 2) == 0 ? ARObjectType.Circle : ARObjectType.Star
        };

        // Lo añade a la lista de objetos
        arObjects.Add(obj);
        System.Diagnostics.Debug.WriteLine($"OnAddObjectClicked: añadido objeto {obj.Id}");

        try
        {
            // Actualiza la etiqueta de contador si está disponible
            if (CounterLabel is null)
            {
                System.Diagnostics.Debug.WriteLine("OnAddObjectClicked: CounterLabel es NULL");
            }
            else
            {
                CounterLabel.Text = $"Objetos AR: {arObjects.Count}";
            }

            // Fuerza el redibujado de la vista gráfica
            if (graphicsView is null)
            {
                System.Diagnostics.Debug.WriteLine("OnAddObjectClicked: graphicsView es NULL");
            }
            else
            {
                graphicsView.Invalidate();
            }
        }
        catch (Exception ex)
        {
            // Log de cualquier excepción que se produzca al actualizar la UI
            System.Diagnostics.Debug.WriteLine($"OnAddObjectClicked: EXCEPCIÓN {ex.GetType().Name} - {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("OnAddObjectClicked: fin");
    }

    // Manejador del botón "Pausar/Reanudar Seguimiento"
    private void OnToggleTrackingClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnToggleTrackingClicked: inicio");

        // Cambia el estado de tracking (true/false)
        trackingEnabled = !trackingEnabled;

        try
        {
            // Actualiza el texto del botón según el estado
            if (ToggleButton is null)
            {
                System.Diagnostics.Debug.WriteLine("OnToggleTrackingClicked: ToggleButton es NULL");
            }
            else
            {
                ToggleButton.Text = trackingEnabled ? "Pausar Seguimiento" : "Reanudar Seguimiento";
            }

            // Actualiza el texto y color de la etiqueta de estado
            if (StatusLabel is null)
            {
                System.Diagnostics.Debug.WriteLine("OnToggleTrackingClicked: StatusLabel es NULL");
            }
            else
            {
                StatusLabel.Text = trackingEnabled
                    ? "Sistema AR Activo - Mueve el dispositivo"
                    : "Sistema AR Pausado";

                StatusLabel.TextColor = trackingEnabled ? Colors.Lime : Colors.Orange;
            }
        }
        catch (Exception ex)
        {
            // Log de cualquier excepción en la actualización de UI
            System.Diagnostics.Debug.WriteLine($"OnToggleTrackingClicked: EXCEPCIÓN {ex.GetType().Name} - {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("OnToggleTrackingClicked: fin");
    }

    // Manejador del botón "Limpiar Objetos"
    private void OnClearClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnClearClicked: inicio");

        // Elimina todos los objetos AR de la lista
        arObjects.Clear();

        try
        {
            // Resetea el contador visual si la etiqueta existe
            if (CounterLabel is null)
            {
                System.Diagnostics.Debug.WriteLine("OnClearClicked: CounterLabel es NULL");
            }
            else
            {
                CounterLabel.Text = "Objetos AR: 0";
            }

            // Fuerza un redibujado de la vista gráfica para que desaparezcan los objetos
            if (graphicsView is null)
            {
                System.Diagnostics.Debug.WriteLine("OnClearClicked: graphicsView es NULL");
            }
            else
            {
                graphicsView.Invalidate();
            }
        }
        catch (Exception ex)
        {
            // Log de errores en la actualización de UI
            System.Diagnostics.Debug.WriteLine($"OnClearClicked: EXCEPCIÓN {ex.GetType().Name} - {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("OnClearClicked: fin");
    }

    // Devuelve un color aleatorio para los objetos AR
    private Color GetRandomColor()
    {
        return random.Next(0, 5) switch
        {
            0 => Colors.Red,
            1 => Colors.Blue,
            2 => Colors.Green,
            3 => Colors.Yellow,
            _ => Colors.Purple
        };
    }
}

// Modelo que representa un objeto AR dibujado en pantalla
public class ARObject
{
    // Identificador del objeto (índice en la lista, principalmente)
    public int Id { get; set; }

    // Posición base en X dentro del área de dibujo
    public float X { get; set; }

    // Posición base en Y dentro del área de dibujo
    public float Y { get; set; }

    // Tamaño (diámetro) del objeto
    public float Size { get; set; }

    // Color del objeto; por defecto blanco
    public Color Color { get; set; } = Colors.White;

    // Tipo de objeto (círculo o estrella)
    public ARObjectType Type { get; set; }
}

// Tipos de objetos AR disponibles
public enum ARObjectType
{
    // Objeto con forma de círculo
    Circle,

    // Objeto con forma de estrella
    Star
}