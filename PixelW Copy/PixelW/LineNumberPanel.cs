using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelW
{
    internal class LineNumberPanel : Panel
    {
        private RichTextBox _editor;

        public LineNumberPanel(RichTextBox editor = null)
        {
            _editor = editor;
            DoubleBuffered = true;
            Width = 30;
            BackColor = Color.WhiteSmoke;
            BorderStyle = BorderStyle.FixedSingle;

            if (_editor != null)
            {
                ConnectEditor();
            }
        }

        public void AttachToEditor(RichTextBox editor)
        {
            // Desconectar el editor anterior si existe
            if (_editor != null)
            {
                _editor.TextChanged -= Editor_TextChanged;
                _editor.VScroll -= Editor_VScroll;
            }

            // Conectar el nuevo editor
            _editor = editor;

            if (_editor != null)
            {
                ConnectEditor();
            }
        }

        private void ConnectEditor()
        {
            _editor.TextChanged += Editor_TextChanged;
            _editor.VScroll += Editor_VScroll;
            Invalidate(); // Forzar redibujado inicial
        }

        private void Editor_TextChanged(object sender, EventArgs e) => Invalidate();
        private void Editor_VScroll(object sender, EventArgs e) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_editor == null || _editor.IsDisposed)
                return;

            int firstChar = _editor.GetCharIndexFromPosition(Point.Empty);
            int firstLine = _editor.GetLineFromCharIndex(firstChar);
            int currentLine = _editor.GetLineFromCharIndex(_editor.SelectionStart);

            float lineHeight = _editor.Font.GetHeight();
            int visibleLines = (int)(_editor.ClientSize.Height / lineHeight) + 1;

            for (int i = 0; i < visibleLines; i++)
            {
                int lineNum = firstLine + i + 1;
                bool isCurrent = (lineNum == currentLine + 1);

                if (isCurrent)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, 0, i * lineHeight, Width, lineHeight);
                }

                string num = lineNum.ToString();
                SizeF size = e.Graphics.MeasureString(num, _editor.Font);
                float x = Width - size.Width - 2;
                float y = i * lineHeight;

                e.Graphics.DrawString(num, _editor.Font,
                    isCurrent ? Brushes.Red : Brushes.Black, x, y);
            }
        }
    }
}
