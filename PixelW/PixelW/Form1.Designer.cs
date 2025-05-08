namespace PixelW
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtEditor = new System.Windows.Forms.RichTextBox();
            this.numCanvasSize = new System.Windows.Forms.NumericUpDown();
            this.picCanvas = new System.Windows.Forms.PictureBox();
            this.btnResize = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numCanvasSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCanvas)).BeginInit();
            this.SuspendLayout();
            // 
            // txtEditor
            // 
            this.txtEditor.Location = new System.Drawing.Point(32, 12);
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.Size = new System.Drawing.Size(229, 577);
            this.txtEditor.TabIndex = 0;
            this.txtEditor.Text = "";
            // 
            // numCanvasSize
            // 
            this.numCanvasSize.Location = new System.Drawing.Point(988, 213);
            this.numCanvasSize.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numCanvasSize.Name = "numCanvasSize";
            this.numCanvasSize.Size = new System.Drawing.Size(110, 26);
            this.numCanvasSize.TabIndex = 1;
            // 
            // picCanvas
            // 
            this.picCanvas.Location = new System.Drawing.Point(289, 12);
            this.picCanvas.Name = "picCanvas";
            this.picCanvas.Size = new System.Drawing.Size(623, 561);
            this.picCanvas.TabIndex = 2;
            this.picCanvas.TabStop = false;
            // 
            // btnResize
            // 
            this.btnResize.Location = new System.Drawing.Point(985, 12);
            this.btnResize.Name = "btnResize";
            this.btnResize.Size = new System.Drawing.Size(92, 40);
            this.btnResize.TabIndex = 3;
            this.btnResize.Text = "Resize";
            this.btnResize.UseVisualStyleBackColor = true;
            this.btnResize.Click += new System.EventHandler(this.btnResize_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(988, 58);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(89, 38);
            this.btnRun.TabIndex = 4;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(985, 102);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(91, 42);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(988, 150);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(89, 38);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1271, 615);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnResize);
            this.Controls.Add(this.picCanvas);
            this.Controls.Add(this.numCanvasSize);
            this.Controls.Add(this.txtEditor);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numCanvasSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCanvas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtEditor;
        private System.Windows.Forms.NumericUpDown numCanvasSize;
        private System.Windows.Forms.PictureBox picCanvas;
        private System.Windows.Forms.Button btnResize;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTest;
    }
}

