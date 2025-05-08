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
            SetupForm();
            InitializeCanvas(DefaultSize);
        }
        private void SetupForm()
        {
            numCanvasSize.Value = DefaultSize;
            picCanvas.BackColor=Color.White;
            picCanvas.BorderStyle=BorderStyle.FixedSingle;
        }

        private void InitializeCanvas(int size)
        {
            canvas = new Canvas(size);
            robot = new WallE(canvas);
            parser = new CommandParser(robot);
            picCanvas.Image = canvas.ToBitmap();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEditor.Text))
                {
                    MessageBox.Show("Escribe comandos en el editor primero", "Advertencia",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                parser.Execute(txtEditor.Text);
                picCanvas.Image = canvas.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                robot.Spawn(10, 10);
                canvas.DrawPixel(robot.X, robot.Y, Color.Red);
                picCanvas.Image = canvas.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en prueba: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            try
            {
                int newSize = 200; 
                if (newSize < 10) return; 

                if (picCanvas.Image != null)
                {
                    picCanvas.Image.Dispose();
                }

                canvas = new Canvas(newSize);
                robot = new WallE(canvas);

                picCanvas.Size = new Size(newSize, newSize);
                picCanvas.Image = canvas.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivos Pixel Wall-E (*.pw)|*.pw",
                    DefaultExt = "pw",
                    Title = "Guardar comandos"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, txtEditor.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
