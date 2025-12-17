# RealidadAumentada

Aplicación de ejemplo en **.NET MAUI** que simula una experiencia de **realidad aumentada 2D** usando `GraphicsView`.

## ¿Qué hace la app?

La pantalla principal muestra:

- **Zona superior**: un lienzo negro que simula la cámara, con un grid verde que simula un plano detectado y objetos AR (círculos y cuadrados) que flotan con un ligero movimiento.
- **Zona inferior**: panel de controles con botones para añadir objetos, pausar/reanudar la animación y limpiar la escena.

## Estructura del proyecto
RealidadAumentada/ ├── RealidadAumentada.csproj        # Proyecto .NET MAUI multiplataforma ├── MauiProgram.cs                  # Configuración de la app MAUI ├── App.xaml / App.xaml.cs          # Punto de entrada de la aplicación ├── AppShell.xaml / AppShell.xaml.cs# Shell de navegación ├── MainPage.xaml                   # Interfaz de usuario principal ├── MainPage.xaml.cs                # Lógica y motor de dibujo AR └── Platforms/ ├── Android/ │   ├── MainActivity.cs         # Actividad principal Android │   └── MainApplication.cs      # Clase Application Android ├── iOS/ │   └── AppDelegate.cs          # Delegado de la app iOS ├── MacCatalyst/ │   └── AppDelegate.cs          # Delegado de la app Mac └── Windows/ └── App.xaml.cs             # Integración WinUI

## Plataformas soportadas

- net10.0-android
- net10.0-ios
- net10.0-maccatalyst
- net10.0-windows10.0.19041.0 (solo en Windows)

## Funcionamiento de MainPage.xaml

MainPage.xaml define la interfaz visual de la pantalla principal.

### Estructura general

La página usa un `Grid` con dos filas:

- **Fila 0 (`*`)**: ocupa todo el espacio disponible, contiene el `GraphicsView`.
- **Fila 1 (`Auto`)**: altura según contenido, contiene el panel de botones y labels.

### GraphicsView (zona AR)

El `GraphicsView` tiene `x:Name="graphicsView"` para poder acceder desde C#. Su `BackgroundColor` es `Black` para simular la cámara. En el code-behind se hace `graphicsView.Drawable = this` para que `MainPage` pinte en esta vista.

### Panel de controles

El `VerticalStackLayout` en la fila 1 tiene fondo negro semitransparente (`#80000000`) y contiene:

- **Label de título**: texto fijo "Vista de Cámara Simulada + Realidad Aumentada"
- **StatusLabel**: muestra el estado del sistema AR (activo/pausado), color verde cuando activo
- **Botón "Agregar Objeto AR"**: ejecuta `OnAddObjectClicked`
- **ToggleButton**: texto "Pausar Seguimiento" / "Reanudar Seguimiento", ejecuta `OnToggleTrackingClicked`
- **Botón "Limpiar Objetos"**: ejecuta `OnClearClicked`
- **CounterLabel**: muestra "Objetos AR: N"

## Funcionamiento de MainPage.xaml.cs

Este archivo contiene toda la lógica de la pantalla: estado, dibujo y handlers de botones.

### Clase principal

La clase `MainPage` hereda de `ContentPage` e implementa `IDrawable`:
- `ContentPage` es la página MAUI enlazada al XAML.
- `IDrawable` permite implementar el método `Draw` para pintar en el `GraphicsView`.

### Campos de estado

- **arObjects**: lista de todos los objetos AR que se deben dibujar en la escena (`List<ARObject>`)
- **random**: generador de aleatorios para posición, tamaño, color y tipo
- **animacionActiva**: `true` significa que los objetos se mueven (animación activa), `false` significa objetos congelados

### Constructor

El constructor hace lo siguiente:

1. Llama a `InitializeComponent()` para cargar el XAML y construir la UI
2. Asigna `graphicsView.Drawable = this` para enlazar el método `Draw` de esta clase con el `GraphicsView`
3. Inicia un `Dispatcher.StartTimer` que se ejecuta cada 100ms y, si `animacionActiva` es `true`, llama a `graphicsView.Invalidate()` para forzar el redibujado

### Método Draw: motor de renderizado

El método `Draw` recibe un `ICanvas` y un `RectF` (área a dibujar) y hace lo siguiente:

1. Pinta todo el área de negro con `FillRectangle` (simula cámara)
2. Dibuja líneas verdes cada 80 píxeles en horizontal y vertical (simula plano detectado)
3. Para cada `ARObject` en la lista:
   - Calcula un offset sinusoidal usando `Math.Sin` y `Math.Cos` con `DateTime.Now.Millisecond`, esto genera movimiento suave
   - Suma el offset a la posición base (`obj.X`, `obj.Y`)
   - Dibuja el objeto: si `Tipo` es `"circulo"` usa `FillEllipse`, si es `"cuadrado"` usa `FillRectangle`

### Handler OnAddObjectClicked

Se ejecuta al pulsar "Agregar Objeto AR". Crea un nuevo `ARObject` con:

- **X**: aleatorio entre 50 y 300
- **Y**: aleatorio entre 50 y 400
- **Tamanio**: aleatorio entre 40 y 80
- **Color**: aleatorio de una paleta de 5 colores (Red, Blue, Green, Yellow, Purple)
- **Tipo**: aleatorio entre "circulo" y "cuadrado"

Lo añade a `arObjects` y actualiza `CounterLabel` con el número total.

### Handler OnToggleTrackingClicked

Se ejecuta al pulsar "Pausar Seguimiento" o "Reanudar Seguimiento":

1. Invierte el valor de `animacionActiva`
2. Si pasa a `false`, el timer ya no invalidará el `GraphicsView` y los objetos dejan de moverse
3. Actualiza el texto de `ToggleButton` ("Pausar Seguimiento" / "Reanudar Seguimiento")
4. Actualiza el texto y color de `StatusLabel` (verde/Lime cuando activo, naranja/Orange cuando pausado)

### Handler OnClearClicked

Se ejecuta al pulsar "Limpiar Objetos":

1. Vacía la lista `arObjects` con `Clear()`
2. Resetea `CounterLabel` a "Objetos AR: 0"

### Método GetRandomColor

Devuelve un color aleatorio de una paleta de 5: `Red`, `Blue`, `Green`, `Yellow` o `Purple`.

## Modelo de datos

### Clase ARObject

Representa un objeto AR con las siguientes propiedades:

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| X | float | Posición horizontal |
| Y | float | Posición vertical |
| Tamanio | float | Tamaño (ancho/alto) del objeto |
| Color | Color | Color del objeto |
| Tipo | string | "circulo" o "cuadrado" |

## Flujo de ejecución resumido

1. La app inicia y `MauiProgram.CreateMauiApp()` configura la aplicación
2. `App.xaml.cs` establece `MainPage` a través de `AppShell`
3. `MainPage` se construye: `InitializeComponent()` crea la UI, se asigna `graphicsView.Drawable = this`, se inicia el timer de 100ms
4. Cada 100ms, si `animacionActiva` es `true`, se llama a `graphicsView.Invalidate()` que provoca que se ejecute `Draw()`
5. `Draw()` pinta fondo negro, grid verde y cada objeto AR con movimiento sinusoidal
6. Cuando el usuario pulsa **"Agregar Objeto AR"**, `OnAddObjectClicked` crea un `ARObject` aleatorio, lo añade a la lista y actualiza el contador
7. Cuando el usuario pulsa **"Pausar Seguimiento"**, `OnToggleTrackingClicked` pone `animacionActiva` a `false`, el timer deja de invalidar y los objetos quedan congelados
8. Cuando el usuario pulsa **"Limpiar Objetos"**, `OnClearClicked` vacía la lista, quedando solo fondo y grid

## Cómo ejecutar

1. Clona el repositorio: `git clone https://github.com/DavidJLopez12/RealidadAumentada/`
2. Abre en Visual Studio 2022/2025/2026 con MAUI instalado
3. Selecciona la plataforma de destino: Windows Machine, emulador Android, simulador iOS o Mac Catalyst
4. Pulsa F5 para ejecutar
