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

            SetupAutoComplete();
            // Resto de la inicialización
            variableManager = new VariableManager();
            InitializeCanvas(DefaultSize);
            numCanvasSize.Value = DefaultSize;


        }
        private void txtEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Mostrar autocompletado cuando se escriba '(' o '['
            if (e.KeyChar == '(' || e.KeyChar == '[')
            {
                ShowCompletionMenu();

                // Opcional: prevenir que el caracter se duplique
                e.Handled = true;
            }

            // Puedes añadir más triggers según necesites
            else if (e.KeyChar == 'C') // Ejemplo: al empezar a escribir "Color"
            {
                ShowCompletionMenuForPrefix("C");
                e.Handled = true;
            }
        }

        private void ShowCompletionMenu()
        {
            var menu = new ContextMenuStrip();

            // Añade ítems al menú
            menu.Items.Add("Spawn(x, y)");
            menu.Items.Add("Color(\"Red\")");
            menu.Items.Add("DrawLine(dirX, dirY, distance)");
            menu.Items.Add("GoTo [etiqueta] (condición)");

            // Posiciona el menú cerca del cursor
            int position = txtEditor.SelectionStart;
            Point cursorPos = txtEditor.GetPositionFromCharIndex(position);
            menu.Show(txtEditor, cursorPos);

            // Maneja la selección de un ítem
            menu.ItemClicked += (s, e) =>
            {
                txtEditor.SelectedText = e.ClickedItem.Text;
            };
        }
        private void SetupAutoComplete()
        {
            // RichTextBox no soporta autocompletado nativo, usaremos un TextBox alternativo
            // o implementaremos una solución manual

            // Opción 1: Cambiar a TextBox (si es posible)
            // txtEditor.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            // txtEditor.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            // txtEditor.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Opción 2: Implementación manual para RichTextBox
            var commands = new List<string>
    {
        "Spawn(", "Color(", "DrawLine(", "DrawCircle(", "DrawRectangle(",
        "Fill()", "Size(", "GetActualX()", "GetActualY()", "GetCanvasSize()",
        "IsBrushColor(", "IsBrushSize(", "IsCanvasColor(", "GetColorCount(",
        "GoTo ["
    };

            txtEditor.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Space && Control.ModifierKeys == Keys.Control)
                {
                    ShowCustomCompletionMenu(commands);
                }
            };
        }

        private void ShowCustomCompletionMenu(List<string> commands)
        {
            var menu = new ContextMenuStrip();

            foreach (var cmd in commands)
            {
                menu.Items.Add(cmd);
            }

            menu.Show(txtEditor, new Point(
                txtEditor.GetPositionFromCharIndex(txtEditor.SelectionStart).X,
                txtEditor.GetPositionFromCharIndex(txtEditor.SelectionStart).Y + txtEditor.Font.Height
            ));

            menu.ItemClicked += (sender, e) =>
            {
                txtEditor.SelectedText = e.ClickedItem.Text;
            };
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

        private void ShowCompletionMenuForPrefix(string prefix)
        {
            var menu = new ContextMenuStrip();
            var commands = new List<string>
    {
        "Color(", "Circle(", "CanvasSize"
    };

            foreach (var cmd in commands.Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                menu.Items.Add(cmd);
            }

            if (menu.Items.Count > 0)
            {
                Point cursorPos = txtEditor.GetPositionFromCharIndex(txtEditor.SelectionStart);
                menu.Show(txtEditor, cursorPos);

                menu.ItemClicked += (s, e) =>
                {
                    txtEditor.SelectedText = e.ClickedItem.Text.Substring(prefix.Length);
                };
            }
        }
        private void BtnRun_Click(object sender, EventArgs e)
        {
            try
            {
                // Limpiar estado previo
                variableManager = new VariableManager();
                InitializeCanvas(canvas.Size); // Mantiene el tamaño actual

                parser.Execute(txtEditor.Text);
                UpdateCanvas();
            }
            catch (Exception ex)
            {
                // Mostrar error con línea específica
                var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"línea (\d+)");
                if (match.Success)
                {
                    int errorLine = int.Parse(match.Groups[1].Value);
                    HighlightErrorLine(errorLine);
                }

                MessageBox.Show(ex.Message, "Error de ejecución",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HighlightErrorLine(int lineNumber)
        {
            txtEditor.SelectionStart = txtEditor.GetFirstCharIndexFromLine(lineNumber - 1);
            txtEditor.SelectionLength = txtEditor.Lines[lineNumber - 1].Length;
            txtEditor.SelectionBackColor = Color.LightPink;
        }
        private void BtnResize_Click(object sender, EventArgs e)
        {
            try
            {
                int newSize = (int)numCanvasSize.Value;
                if (newSize < 10 || newSize > 500) // Ajusta los límites según necesites
                {
                    MessageBox.Show("El tamaño debe estar entre 10 y 500", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Liberar recursos de la imagen anterior
                if (picCanvas.Image != null)
                {
                    picCanvas.Image.Dispose();
                }

                // Crear nuevo canvas y robot
                canvas = new Canvas(newSize);
                robot = new WallE(canvas);

                // Actualizar la visualización
                picCanvas.Image = canvas.ToBitmap();

                MessageBox.Show($"Canvas redimensionado a {newSize}x{newSize}", "Éxito",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al redimensionar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pixel Wall-E Files (*.pw)|*.pw";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
            }
        }

        private void btn_Load_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Pixel Wall-E Files (*.pw)|*.pw|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtEditor.Text = File.ReadAllText(ofd.FileName);
                    MessageBox.Show("Archivo cargado correctamente", "Éxito",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnResize_Click_1(object sender, EventArgs e)
        {
            try
            {
                int newSize = (int)numCanvasSize.Value;
                if (newSize < 10 || newSize > 500) // Ajusta los límites según necesites
                {
                    MessageBox.Show("El tamaño debe estar entre 10 y 500", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Liberar recursos de la imagen anterior
                if (picCanvas.Image != null)
                {
                    picCanvas.Image.Dispose();
                }

                // Crear nuevo canvas y robot
                canvas = new Canvas(newSize);
                robot = new WallE(canvas);

                // Actualizar la visualización
                picCanvas.Image = canvas.ToBitmap();

                MessageBox.Show($"Canvas redimensionado a {newSize}x{newSize}", "Éxito",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al redimensionar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
