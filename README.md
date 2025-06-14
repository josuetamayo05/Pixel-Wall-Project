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

# Pixel Wall-E 🎨🤖

![Interfaz de Pixel Wall-E](screenshot.png)

Un lenguaje de programación visual para crear *pixel art* mediante comandos que controlan al robot Wall-E en un canvas, siguiendo estrictas reglas sintácticas.

## 🔍 Reglas Esenciales (Del PDF)

```python
# ✅ CORRECTO
Spawn(10, 10)  # Siempre PRIMERA línea
Color("Red")
DrawLine(1, 0, 5)

# ❌ INCORRECTO
Color("Blue")  # Error: Spawn debe ir primero
Spawn(0,0)    # Error: Spawn repetido

🛠 Comandos Básicos (Reglas Estrictas)
1. Spawn(x, y) - Obligatorio y Único
Siempre debe ser el primer comando

Solo se permite una vez por programa

Ejemplo válido:

python
Spawn(5, 5)  # Posición inicial (x,y)
2. Color(colorName) - Paleta Limitada
Colores permitidos (exactos):

python
"Red"    │ "Green"   │ "Blue"
"Yellow" │ "Orange"  │ "Purple"
"Black"  │ "White"   │ "Transparent"
Ejemplo:

python
Color("Purple")  # Cambia a púrpura
Color("White")   # Actúa como "borrador"
3. Size(k) - Sólo Impares
python
Size(3)   # ✅ Válido (3 píxeles)
Size(4)   # ❌ Convertido a 3 automáticamente
✏️ Comandos de Dibujo (Validación Estricta)
DrawLine(dirX, dirY, pasos)
Direcciones permitidas:

text
(-1,-1)  (0,-1)  (1,-1)
(-1, 0)   (0,0)  (1, 0)
(-1, 1)  (0, 1)  (1, 1)
Ejemplo válido:

python
DrawLine(1, 0, 10)  # Horizontal derecha (10px)
DrawCircle(dirX, dirY, radio)
python
# Dibuja círculo y mueve Wall-E al centro
DrawCircle(1, 1, 5)  # Radio 5 en diagonal
DrawRectangle(dirX, dirY, dist, ancho, alto)
python
# Rectángulo 8x4 a 5px de distancia
DrawRectangle(0, 1, 5, 8, 4)
🧠 Estructuras Avanzadas (Reglas Estrictas)
Variables
Nombres válidos: a-Z, 0-9, _ (no empezar con número)

python
ancho <- 20
nombre_valido <- ancho / 2
1nombre <- 5  # ❌ Error sintáctico
GoTo [label] (condición)
python
inicio:
  DrawLine(1, 0, 1)
  contador <- contador + 1
  GoTo [inicio] (contador < 10)  # ✅ Etiqueta existe
  GoTo [fin] (1 == 2)            # ✅ Condición válida
  GoTo [no_existe] (True)        # ❌ Error semántico
🚫 Errores Comunes (Evítalos!)
Spawn múltiple:

python
Spawn(0,0)
Spawn(5,5)  # ❌ Error crítico
Colores no definidos:

python
Color("Azul")  # ❌ Debe ser "Blue"
Direcciones inválidas:

python
DrawLine(2, -3, 5)  # ❌ Solo -1, 0, 1
📜 Ejemplo Completo (100% Válido)
python
# PROGRAMA VÁLIDO (cumple todas las reglas)
Spawn(15, 15)          # ✅ Único Spawn
Color("DarkBlue")      # ✅ Color permitido
Size(3)                # ✅ Tamaño impar

# Dibuja espiral
lados <- 0
max_lados <- 20

dibujar:
  DrawLine(1, 0, 5 + lados)
  DrawLine(0, 1, 5 + lados)
  lados <- lados + 1
  GoTo [dibujar] (lados < max_lados)

# Relleno final
Color("Gold")
Fill()                # ✅ Rellena área actual
📚 Recursos
Guía Completa de Sintaxis

Ejemplos Validados

Validador Online (opcional)

<div align="center"> <img src="assets/validation.gif" width="400" alt="Demo de validación"> <p>El editor marca errores en tiempo real según las reglas del PDF</p> </div> ```