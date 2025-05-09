using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PixelW
{
    public partial class Form1 : Form
    {
        private VariableManager variableManager;
        private Canvas canvas;
        private WallE robot;
        private CommandParser parser;
        private const int DefaultSize = 200;

        public Form1()
        {
            InitializeComponent();
            if (panelLineNumbers != null && txtEditor != null)
            {
                panelLineNumbers.AttachToEditor(txtEditor);
            }

            // Resto de la inicialización
            variableManager = new VariableManager();
            InitializeCanvas(DefaultSize);
            numCanvasSize.Value = DefaultSize;
        }

        private void InitializeApplication()
        {
            // Configurar el editor y panel de números
            panelLineNumbers.AttachToEditor(txtEditor);

            // Inicializar componentes de la aplicación
            variableManager = new VariableManager();
            InitializeCanvas(DefaultSize);
            numCanvasSize.Value = DefaultSize;

            // Configurar eventos
            btnRun.Click += BtnRun_Click;
            btnResize.Click += BtnResize_Click;
        }

        private void InitializeCanvas(int size)
        {
            canvas = new Canvas(size);
            robot = new WallE(canvas);
            parser = new CommandParser(robot, variableManager);
            UpdateCanvas();
        }

        private void UpdateCanvas()
        {
            var oldImage = picCanvas.Image;
            picCanvas.Image = canvas.ToBitmap();
            oldImage?.Dispose();
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            try
            {
                int newSize = (int)numCanvasSize.Value;
                if (newSize < 10 || newSize > 500)
                {
                    MessageBox.Show("El tamaño debe estar entre 10 y 500");
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (picCanvas.Image != null) {
                    picCanvas.Image.Dispose();
                }
                canvas = new Canvas(newSize);
                robot = new WallE(canvas);

                picCanvas.Image = canvas.ToBitmap();
                MessageBox.Show($"Canvas redimensionado a {newSize}x{newSize}", "Éxito",
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"Error al redimensionar: {ex.Message}", "Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private BtnSave_Click(object sender, EventArgs e)
        {
            try
            {

            }
        }

        private void BtnResize_Click(object sender, EventArgs e)
        {
            try
            {
                int newSize = (int)numCanvasSize.Value;
                if (newSize < 10) return;

                InitializeCanvas(newSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
