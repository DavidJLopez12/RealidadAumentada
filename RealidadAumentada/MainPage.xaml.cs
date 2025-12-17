using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace RealidadAumentada;

public partial class MainPage : ContentPage, IDrawable
{
    // ========== VARIABLES GLOBALES ==========
    
    // Lista donde guardamos todos los objetos que dibujamos
    private List<ARObject> arObjects = new List<ARObject>();
    
    // Para generar números aleatorios
    private Random random = new Random();
    
    // Para saber si la animación está activada o pausada
    private bool animacionActiva = true;

    // ========== CONSTRUCTOR ==========
    
    public MainPage()
    {
        InitializeComponent();
        
        // Le decimos al GraphicsView que esta clase es la que dibuja
        graphicsView.Drawable = this;
        
        // Creamos un temporizador que se ejecuta cada 100 milisegundos
        // Esto hace que los objetos se muevan solos
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
        {
            if (animacionActiva)
            {
                // Esto hace que se vuelva a dibujar la pantalla
                graphicsView.Invalidate();
            }
            return true; // true = el timer sigue funcionando
        });
    }

    // ========== MÉTODO PARA DIBUJAR ==========
    
    // Este método se llama automáticamente cada vez que hay que dibujar
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // 1. Pintamos el fondo de negro
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect);
        
        // 2. Dibujamos una cuadrícula verde (como un suelo de AR)
        canvas.StrokeColor = Colors.DarkGreen;
        canvas.StrokeSize = 1;
        
        // Líneas verticales (de arriba a abajo)
        for (float x = 0; x < dirtyRect.Width; x += 80)
        {
            canvas.DrawLine(x, 0, x, dirtyRect.Height);
        }
        
        // Líneas horizontales (de izquierda a derecha)
        for (float y = 0; y < dirtyRect.Height; y += 80)
        {
            canvas.DrawLine(0, y, dirtyRect.Width, y);
        }
        
        // 3. Dibujamos cada objeto de la lista
        foreach (var obj in arObjects)
        {
            // Calculamos un pequeño movimiento para que parezca que "flota"
            float movimientoX = (float)Math.Sin(DateTime.Now.Millisecond / 200.0) * 10;
            float movimientoY = (float)Math.Cos(DateTime.Now.Millisecond / 200.0) * 10;
            
            float x = obj.X + movimientoX;
            float y = obj.Y + movimientoY;
            
            // Dibujamos el objeto según su tipo
            canvas.FillColor = obj.Color;
            
            if (obj.Tipo == "circulo")
            {
                canvas.FillEllipse(x, y, obj.Tamanio, obj.Tamanio);
            }
            else
            {
                // Dibujamos un cuadrado (más fácil que una estrella)
                canvas.FillRectangle(x, y, obj.Tamanio, obj.Tamanio);
            }
        }
    }

    // ========== BOTÓN: AGREGAR OBJETO ==========
    
    private void OnAddObjectClicked(object sender, EventArgs e)
    {
        // Creamos un objeto nuevo con valores aleatorios
        ARObject nuevoObjeto = new ARObject();
        nuevoObjeto.X = random.Next(50, 300);
        nuevoObjeto.Y = random.Next(50, 400);
        nuevoObjeto.Tamanio = random.Next(40, 80);
        nuevoObjeto.Color = GetRandomColor();
        
        // Elegimos tipo aleatorio: 0 = circulo, 1 = cuadrado
        if (random.Next(0, 2) == 0)
        {
            nuevoObjeto.Tipo = "circulo";
        }
        else
        {
            nuevoObjeto.Tipo = "cuadrado";
        }
        
        // Lo añadimos a la lista
        arObjects.Add(nuevoObjeto);
        
        // Actualizamos el contador en pantalla
        CounterLabel.Text = "Objetos AR: " + arObjects.Count;
    }

    // ========== BOTÓN: PAUSAR / REANUDAR ==========
    
    private void OnToggleTrackingClicked(object sender, EventArgs e)
    {
        // Cambiamos el estado (si estaba true pasa a false, y viceversa)
        animacionActiva = !animacionActiva;
        
        // Cambiamos el texto del botón y la etiqueta de estado
        if (animacionActiva)
        {
            ToggleButton.Text = "Pausar Seguimiento";
            StatusLabel.Text = "Sistema AR Activo";
            StatusLabel.TextColor = Colors.Lime;
        }
        else
        {
            ToggleButton.Text = "Reanudar Seguimiento";
            StatusLabel.Text = "Sistema AR Pausado";
            StatusLabel.TextColor = Colors.Orange;
        }
    }

    // ========== BOTÓN: LIMPIAR ==========
    
    private void OnClearClicked(object sender, EventArgs e)
    {
        // Vaciamos la lista de objetos
        arObjects.Clear();
        
        // Reseteamos el contador
        CounterLabel.Text = "Objetos AR: 0";
    }

    // ========== MÉTODO AUXILIAR: COLOR ALEATORIO ==========
    
    private Color GetRandomColor()
    {
        int numero = random.Next(0, 5);
        
        if (numero == 0) return Colors.Red;
        if (numero == 1) return Colors.Blue;
        if (numero == 2) return Colors.Green;
        if (numero == 3) return Colors.Yellow;
        return Colors.Purple;
    }
}

// ========== CLASE PARA REPRESENTAR UN OBJETO AR ==========

public class ARObject
{
    public float X { get; set; }        // Posición horizontal
    public float Y { get; set; }        // Posición vertical
    public float Tamanio { get; set; }  // Tamaño del objeto
    public Color Color { get; set; }    // Color del objeto
    public string Tipo { get; set; }    // "circulo" o "cuadrado"
}