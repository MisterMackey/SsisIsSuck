namespace FileLoaderClient
{
    partial class FileLoaderForm
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
            this.ContextViewer = new System.Windows.Forms.TreeView();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.btnStartTransfer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ContextViewer
            // 
            this.ContextViewer.Location = new System.Drawing.Point(12, 12);
            this.ContextViewer.Name = "ContextViewer";
            this.ContextViewer.Size = new System.Drawing.Size(358, 487);
            this.ContextViewer.TabIndex = 0;
            // 
            // btnStartTransfer
            // 
            this.btnStartTransfer.Location = new System.Drawing.Point(404, 475);
            this.btnStartTransfer.Name = "btnStartTransfer";
            this.btnStartTransfer.Size = new System.Drawing.Size(75, 23);
            this.btnStartTransfer.TabIndex = 1;
            this.btnStartTransfer.Text = "Davai";
            this.btnStartTransfer.UseVisualStyleBackColor = true;
            // 
            // FileLoaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 687);
            this.Controls.Add(this.btnStartTransfer);
            this.Controls.Add(this.ContextViewer);
            this.Name = "FileLoaderForm";
            this.Text = "FileLoader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ContextViewer;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button btnStartTransfer;
    }
}

