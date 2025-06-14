# Pixel Wall-E 🚀

![Captura del editor y canvas](screenshot.png) <!-- Reemplaza con tu propia imagen -->

Un lenguaje de programación visual para crear pixel art mediante comandos simples que controlan un robot (Wall-E) en un canvas.

## ✨ Características principales

- **Lenguaje simple** con comandos intuitivos para dibujar
- **Editor de código** con resaltado de sintaxis y números de línea
- **Canvas interactivo** con zoom y redimensionado
- **Sistema de variables y expresiones** para programación avanzada
- **Saltos condicionales y etiquetas** para crear lógica compleja
- **Múltiples formas de dibujo**: líneas, círculos, rectángulos, relleno
- **Importar/exportar** proyectos en formato .pw

## 🛠 Instalación

1. Clona el repositorio:
   ```bash
   git clone https://github.com/tu-usuario/pixel-walle.git
2. Abre la solución en Visual Studio
3. Compila y Ejecuta el Proyecto

## 🎮 Domina el Lenguaje Pixel Wall-E

### 📜 Estructura Básica
Cada creación comienza colocando a Wall-E en el canvas:

```python
# Configuración inicial
Spawn(10, 10)   # 🏁 Punto de partida en (X,Y)
Color("Navy")   # 🎨 Elige tu color
Size(3)         # 🔍 Grosor del pincel (solo impares)

🛠 Toolbox de Comandos
🖌 Comandos Esenciales
Comando	Ejemplo	Efecto Visual
Spawn(x,y)	Spawn(0,0)	🚀 Teletransporta a Wall-E
Color("Nombre")	Color("Coral")	🌈 Cambia el color actual
Size(k)	Size(5)	⚖️ Ajusta el grosor (1,3,5,...)
✨ Comandos de Dibujo
python
DrawLine(1, 0, 10)       # ➡️ Línea horizontal
DrawCircle(0, 1, 8)      # ⭕ Círculo de radio 8
DrawRectangle(1,1,5,4,2) # ▭ Rectángulo 4x2
Fill()                   # 🌊 Relleno mágico
🧠 Lógica Avanzada
🔢 Variables y Operaciones
python
ancho <- 20
alto <- ancho * 1.618   # 📐 Proporción áurea

# 📊 Condicionales avanzadas
esBonito <- (ancho > 10) && (alto < 30)
🔄 Bucles con GoTo
python
# 🌀 Patrón fractal
Spawn(5, 5)
Color("Teal")
tam <- 30

bucle:
  DrawRectangle(0, 0, 0, tam, tam)
  tam <- tam - 2
  GoTo [bucle] (tam > 5)
🎨 Paleta de Colores Disponibles
<div style="display: flex; flex-wrap: wrap; gap: 10px;"> <div style="background: #FF0000; width: 60px; height: 30px; border-radius: 4px; display: flex; justify-content: center; align-items: center; color: white; font-weight: bold;">Red</div> <div style="background: #00FF00; width: 60px; height: 30px; border-radius: 4px; display: flex; justify-content: center; align-items: center;">Green</div> <!-- Añade más colores --> </div>
🚦 Manejo de Errores (¡Aprende Debugging!)
El editor te ayudará a encontrar:

🔍 Errores de sintaxis: Spawn(5,5 → Falta paréntesis

🧩 Problemas lógicos: GoTo [inexistente] → Etiqueta no definida

🚧 Límites: DrawLine(1,0,1000) → Fuera del canvas

🏗 Ejemplo Completo: Cielo Estrellado
python
# 🌌 Configuración
Spawn(0, 0)
Color("MidnightBlue")
Fill()  # Fondo nocturno

# ✨ Estrellas
Color("Gold")
Size(1)
[10].forEach(i -> {
  x <- Random(0, GetCanvasSize())
  y <- Random(0, GetCanvasSize())
  Spawn(x, y)
  DrawCircle(0, 0, 2)
})
🏆 Retos Creativos
Intenta recrear estos diseños:

🌈 Arcoíris (usa círculos parciales)

🏙 Skyline de ciudad (rectángulos variables)

🎨 Tu firma artística

📚 ¿Quieres Saber Más?
Guía Avanzada - Funciones personalizadas

Galería de Arte - Inspírate con creaciones de la comunidad

Trucos Pro - Optimiza tus diseños

<div align="center"> <img src="assets/demo.gif" width="400" alt="Demo animada"> <p>¡Sube tus creaciones a #PixelWallE en redes!</p> </div> ```