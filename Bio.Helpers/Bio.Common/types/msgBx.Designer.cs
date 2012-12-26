namespace Bio.Helpers.Common.Types {
#if !SILVERLIGHT
  partial class msgBx {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing) {
      if(disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.paText = new System.Windows.Forms.Panel();
      this.rtxtText = new System.Windows.Forms.RichTextBox();
      this.laTxtBox = new System.Windows.Forms.Label();
      this.btnClose = new System.Windows.Forms.Button();
      this.paText.SuspendLayout();
      this.SuspendLayout();
      // 
      // paText
      // 
      this.paText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.paText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.paText.Controls.Add(this.rtxtText);
      this.paText.Location = new System.Drawing.Point(11, 26);
      this.paText.Name = "paText";
      this.paText.Size = new System.Drawing.Size(635, 203);
      this.paText.TabIndex = 0;
      // 
      // rtxtText
      // 
      this.rtxtText.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtxtText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtxtText.Location = new System.Drawing.Point(0, 0);
      this.rtxtText.Name = "rtxtText";
      this.rtxtText.ReadOnly = true;
      this.rtxtText.Size = new System.Drawing.Size(633, 201);
      this.rtxtText.TabIndex = 0;
      this.rtxtText.Text = "";
      this.rtxtText.WordWrap = false;
      // 
      // laTxtBox
      // 
      this.laTxtBox.AutoSize = true;
      this.laTxtBox.Location = new System.Drawing.Point(9, 11);
      this.laTxtBox.Name = "laTxtBox";
      this.laTxtBox.Size = new System.Drawing.Size(68, 13);
      this.laTxtBox.TabIndex = 1;
      this.laTxtBox.Text = "Сообщение:";
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Location = new System.Drawing.Point(554, 235);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(92, 29);
      this.btnClose.TabIndex = 2;
      this.btnClose.Text = "Закрыть";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // msgBx
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnClose;
      this.ClientSize = new System.Drawing.Size(658, 270);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.laTxtBox);
      this.Controls.Add(this.paText);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "msgBx";
      this.Padding = new System.Windows.Forms.Padding(9);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Title";
      this.paText.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel paText;
    private System.Windows.Forms.Label laTxtBox;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.RichTextBox rtxtText;

  }
#endif
}
