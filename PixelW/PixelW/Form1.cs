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
        private const int InitialCanvasSize = 32; // Renombramos la constante
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnResetZoom;

        public Form1()
        {
            InitializeComponent();
            variableManager = new VariableManager();
            canvas = new Canvas(InitialCanvasSize);
            canvas.ZoomLevel = 8;
            UpdateCanvas();
            robot = new WallE(canvas);
            parser = new CommandParser(robot, variableManager);

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // Habilita eventos de teclado y mouse
            this.KeyPreview = true;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.MouseWheel += PicCanvas_MouseWheel;


            if (panelLineNumbers != null && txtEditor != null)
            {
                panelLineNumbers.AttachToEditor(txtEditor);
            }

            SetupAutoComplete();
            // Resto de la inicialización
            variableManager = new VariableManager();
            InitializeCanvas(InitialCanvasSize);
            picCanvas.SizeMode = PictureBoxSizeMode.Zoom;
            numCanvasSize.Value = InitialCanvasSize;


        }
        private void txtEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Mostrar autocompletado cuando se escriba '(' o '['
            if (e.KeyChar == '|')
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
            txtEditor.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Space && Control.ModifierKeys == Keys.Control)
                {
                    ShowCustomCompletionMenu(GetAutoCompleteItems());
                }
            };

            // También puedes agregar el trigger para paréntesis
            txtEditor.KeyPress += (sender, e) =>
            {
                if (e.KeyChar == '|')
                {
                    ShowCustomCompletionMenu(GetAutoCompleteItems());
                    e.Handled = true;
                }
            };
        }

        private void UpdateStatus()
        {
            statusLabel.Text = $"Wall-E: [{robot.GetActualX()}, {robot.GetActualY()}] | Zoom: {canvas.ZoomLevel}x";
        }

        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas != null)
            {
                int pixelX = e.X / canvas.ZoomLevel;
                int pixelY = e.Y / canvas.ZoomLevel;
                statusLabel.Text = $"Mouse: [{pixelX}, {pixelY}] | Wall-E: [{robot.GetActualX()}, {robot.GetActualY()}]";
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    btnSave.PerformClick();
                    return true;

                case Keys.Control | Keys.O:
                    btn_Load.PerformClick();
                    return true;

                case Keys.F5:
                    btnRun.PerformClick();
                    return true;

                case Keys.Control | Keys.Add:
                    canvas.ZoomLevel++;
                    UpdateCanvas();
                    return true;

                case Keys.Control | Keys.Subtract:
                    canvas.ZoomLevel--;
                    UpdateCanvas();
                    return true;

                case Keys.Control | Keys.D0:
                    canvas.ZoomLevel = 1;
                    UpdateCanvas();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PicCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // Cambiar el incremento del zoom para mayor granularidad
                int zoomChange = e.Delta > 0 ? 2 : -2;
                canvas.ZoomLevel = Math.Max(Canvas.MinZoom, Math.Min(Canvas.MaxZoom, canvas.ZoomLevel + zoomChange));
                UpdateCanvas();
                UpdateStatus();
            }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            canvas.ZoomLevel = Math.Min(20, canvas.ZoomLevel + 2);
            UpdateCanvas();
            UpdateStatus();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            canvas.ZoomLevel = Math.Max(1, canvas.ZoomLevel - 2);
            UpdateCanvas();
            UpdateStatus();
        }

        private void btnResetZoom_Click(object sender, EventArgs e)
        {
            canvas.ZoomLevel = 1;
            UpdateCanvas();
            UpdateStatus();
        }
        private List<string> GetAutoCompleteItems()
        {
            return new List<string>
    {
        "Spawn(", "Color(", "DrawLine(", "DrawCircle(", "DrawRectangle(",
        "Fill()", "Size(", "GetActualX()", "GetActualY()", "GetCanvasSize()",
        "GetColorCount(", "IsBrushColor(", "IsBrushSize(", "IsCanvasColor(",
        "GoTo ["
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
            InitializeCanvas(InitialCanvasSize);
            numCanvasSize.Value = InitialCanvasSize;

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
            // Reiniciar el parser con cada ejecución
            variableManager = new VariableManager();
            parser = new CommandParser(robot, variableManager);

            var result = parser.Execute(txtEditor.Text);

            if (!result.Success)
            {
                MessageBox.Show($"Error en línea {result.ErrorLine}: {result.ErrorMessage}",
                              "Error de ejecución",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);

                HighlightErrorLine(result.ErrorLine);
            }

            UpdateCanvas(); // Actualizar siempre, incluso con errores
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
                if (newSize < 10 || newSize > 640) // Ajusta los límites según necesites
                {
                    MessageBox.Show("El tamaño debe estar entre 10 y 640", "Error",
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
                picCanvas.Refresh();

                this.InvalidateLayout();

                MessageBox.Show($"Canvas redimensionado a {newSize}x{newSize}", "Éxito",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al redimensionar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InvalidateLayout()
        {
            // Ajusta otros controles si es necesario
            panelLineNumbers.Invalidate();
            txtEditor.Invalidate();
        }
        private void UpdateCanvas()
        {
            var oldImage = picCanvas.Image;
            picCanvas.Image = canvas.ToBitmap();

            // Asegúrate que el PictureBox tenga el tamaño adecuado para el zoom
            picCanvas.Size = new Size(
                canvas.Size * canvas.ZoomLevel,
                canvas.Size * canvas.ZoomLevel);

            oldImage?.Dispose();
        }

    }
}
