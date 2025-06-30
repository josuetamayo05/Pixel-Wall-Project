using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PixelW
{
    public partial class Form1 : Form
    {
        private VariableManager variableManager;
        private Canvas canvas;
        private WallE robot;
        private CommandParser parser;
        private const int InitialCanvasSize = 86; 
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
            AdjustZoomToFit();
            robot = new WallE(canvas);
            parser = new CommandParser(robot, variableManager);

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            this.KeyPreview = true;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.MouseWheel += PicCanvas_MouseWheel;


            if (panelLineNumbers != null && txtEditor != null)
            {
                panelLineNumbers.AttachToEditor(txtEditor);
            }

            SetupAutoComplete();
            variableManager = new VariableManager();
            InitializeCanvas(InitialCanvasSize);
            picCanvas.SizeMode = PictureBoxSizeMode.Zoom;
            numCanvasSize.Value = InitialCanvasSize;


        }
        
        private void txtEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '|')
            {
                ShowCompletionMenu();

                e.Handled = true;
            }

            else if (e.KeyChar == 'C') 
            {
                ShowCompletionMenuForPrefix("C");
                e.Handled = true;
            }
        }

        private void ShowCompletionMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Spawn(x, y)");
            menu.Items.Add("Color(\"Red\")");
            menu.Items.Add("DrawLine(dirX, dirY, distance)");
            menu.Items.Add("GoTo [etiqueta] (condición)");

            int position = txtEditor.SelectionStart;
            Point cursorPos = txtEditor.GetPositionFromCharIndex(position);
            menu.Show(txtEditor, cursorPos);

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
            try
            {
                if (statusLabel == null || statusLabel.IsDisposed ||
                    robot == null || canvas == null)
                {
                    return; 
                }

                int x = 0, y = 0;
                try
                {
                    x = robot.GetActualX();
                    y = robot.GetActualY();
                }
                catch
                {
                    x = -1;
                    y = -1;
                }

                int zoomLevel = 1;
                try
                {
                    zoomLevel = canvas.ZoomLevel;
                }
                catch
                {
                    zoomLevel = -1;
                }

                statusLabel.Text = $"Wall-E: [{x}, {y}] | Zoom: {zoomLevel}x";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateStatus: {ex.Message}");

                if (statusLabel != null && !statusLabel.IsDisposed)
                {
                    statusLabel.Text = "Status: Error";
                }
            }
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
            variableManager = new VariableManager();
            parser = new CommandParser(robot, variableManager);

            var result = parser.Execute(txtEditor.Text);

            UpdateCanvas();

            if (!result.Success)
            {
                ShowErrors(result.Errors);
            }
        }

        private void ShowErrors(List<CommandParser.ErrorInfo> errors)
        {
            if (errors.Count == 0) return;
            txtEditor.SelectAll();
            txtEditor.SelectionBackColor = Color.White;
            var errorMessage = new StringBuilder("Se encontraron múltiples errores:\n\n"); 
            foreach(var error in errors)
            {
                HighlightErrorLine(error.LineNumber);
                errorMessage.AppendLine($"Línea {error.LineNumber}: {error.Message}");
                errorMessage.AppendLine($"Código: {error.CodeSnippet}");
                errorMessage.AppendLine();
            }
            MessageBox.Show(errorMessage.ToString(), "Errores", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (errors.Count > 0)
            {
                txtEditor.SelectionStart = txtEditor.GetFirstCharIndexFromLine(errors[0].LineNumber - 1);
                txtEditor.ScrollToCaret();
            }
        }

        private void HighlightErrorLine(int lineNumber)
        {
            if (lineNumber <= 0 || lineNumber > txtEditor.Lines.Length) return;

            int start = txtEditor.GetFirstCharIndexFromLine(lineNumber - 1);
            int length = txtEditor.Lines[lineNumber - 1].Length;

            txtEditor.Select(start, length);
            txtEditor.SelectionBackColor = Color.LightPink;
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
                if (newSize < 10 || newSize > 500) 
                {
                    MessageBox.Show("El tamaño debe estar entre 10 y 500", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (picCanvas.Image != null)
                {
                    picCanvas.Image.Dispose();
                }

                // Crear nuevo canvas y robot
                canvas = new Canvas(newSize);
                robot = new WallE(canvas);

                // actualizar la visualización
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
        private void AdjustZoomToFit()
        {
            if (canvas == null) return;

            int availableWidth = this.ClientSize.Width - txtEditor.Width - numCanvasSize.Width - 100;
            int availableHeight = this.ClientSize.Height - 50;

            int maxZoomX = availableWidth / canvas.Size;
            int maxZoomY = availableHeight / canvas.Size;

            canvas.ZoomLevel = Math.Max(Canvas.MinZoom, Math.Min(Math.Min(maxZoomX, maxZoomY), Canvas.MaxZoom));
            UpdateCanvas();
            UpdateStatus();
        }

        private void btnFitToWindow_Click(object sender, EventArgs e)
        {
            AdjustZoomToFit();
        }
        private void InvalidateLayout()
        {
            panelLineNumbers.Invalidate();
            txtEditor.Invalidate();
        }
        private void UpdateCanvas()
        {
            try
            {
                if (canvas == null || picCanvas == null) return;

                var oldImage = picCanvas.Image;

                picCanvas.Image = canvas.ToBitmap();

                int maxAvailableWidth = this.ClientSize.Width - txtEditor.Width - numCanvasSize.Width - 120;
                int maxAvailableHeight = this.ClientSize.Height - 80;
                int canvasDisplayWidth = canvas.Size * canvas.ZoomLevel;
                int canvasDisplayHeight = canvas.Size * canvas.ZoomLevel;
                // Determinar si necesitamos escalar
                bool needsScaling = canvasDisplayWidth > maxAvailableWidth ||
                                  canvasDisplayHeight > maxAvailableHeight;

                if (needsScaling)
                {
                    // Calcular factor de escala
                    double scaleFactor = Math.Min(
                        (double)maxAvailableWidth / canvasDisplayWidth,
                        (double)maxAvailableHeight / canvasDisplayHeight
                    );

                    canvasDisplayWidth = (int)(canvasDisplayWidth * scaleFactor);
                    canvasDisplayHeight = (int)(canvasDisplayHeight * scaleFactor);
                    // Usar modo Stretch para imágenes escaladas
                    picCanvas.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    picCanvas.SizeMode = PictureBoxSizeMode.Normal;
                }

                picCanvas.Size = new Size(canvasDisplayWidth, canvasDisplayHeight);

                // Reposicionar el PictureBox para centrarlo
                picCanvas.Location = new Point(
                    txtEditor.Right + 40,
                    Math.Max(20, (this.ClientSize.Height - canvasDisplayHeight) / 2)
                );

                // Liberar recursos de la imagen anterior
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en UpdateCanvas: {ex.Message}");
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateCanvas(); 

            RepositionRightControls();
        }
        private void RepositionRightControls()
        {
            try
            {
                if (this.IsDisposed || numCanvasSize == null || btnResize == null ||
                    btnRun == null || btnSave == null || btnTest == null ||
                    btn_Load == null || btnZoomIn == null || btnZoomOut == null ||
                    btnResetZoom == null || statusStrip == null)
                {
                    return;
                }

                int rightMargin = 20;
                int controlWidth = 120;
                int startY = 20;
                int spacing = 10;

                // Posición X común para todos los controles de la derecha
                int rightX = this.ClientSize.Width - controlWidth - rightMargin;

                // Reposicionar cada control con verificación de nulidad
                SafeRepositionControl(numCanvasSize, rightX, startY);
                SafeRepositionControl(btnResize, rightX, numCanvasSize.Bottom + spacing);
                SafeRepositionControl(btnRun, rightX, btnResize.Bottom + spacing);
                SafeRepositionControl(btnSave, rightX, btnRun.Bottom + spacing);
                SafeRepositionControl(btnTest, rightX, btnSave.Bottom + spacing);
                SafeRepositionControl(btn_Load, rightX, btnTest.Bottom + spacing);
                SafeRepositionControl(btnZoomIn, rightX, btn_Load.Bottom + spacing);
                SafeRepositionControl(btnZoomOut, rightX, btnZoomIn.Bottom + spacing);
                SafeRepositionControl(btnResetZoom, rightX, btnZoomOut.Bottom + spacing);

                // Actualizar statusStrip si existe
                if (statusStrip != null && !statusStrip.IsDisposed)
                {
                    statusStrip.Width = this.ClientSize.Width;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en RepositionRightControls: {ex.Message}");
            }
        }

        private void SafeRepositionControl(Control control, int x, int y)
        {
            if (control != null && !control.IsDisposed)
            {
                control.Location = new Point(x, y);
            }
        }

    }
}
