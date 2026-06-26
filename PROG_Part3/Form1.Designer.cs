namespace PROG_Part3
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            rtbChat = new System.Windows.Forms.RichTextBox();
            txtInput = new System.Windows.Forms.TextBox();
            btnSend = new System.Windows.Forms.Button();
            btnClear = new System.Windows.Forms.Button();

            // 
            // rtbChat
            // 
            rtbChat.BackColor = System.Drawing.Color.Black;
            rtbChat.ForeColor = System.Drawing.Color.White;
            rtbChat.Location = new System.Drawing.Point(12, 12);
            rtbChat.Name = "rtbChat";
            rtbChat.ReadOnly = true;
            rtbChat.Size = new System.Drawing.Size(760, 350);
            rtbChat.TabIndex = 0;
            rtbChat.Text = "";
            rtbChat.Anchor = System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            // 
            // txtInput
            // 
            txtInput.Location = new System.Drawing.Point(12, 375);
            txtInput.Name = "txtInput";
            txtInput.Size = new System.Drawing.Size(600, 23);
            txtInput.TabIndex = 1;
            txtInput.Anchor = System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;
            txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            txtInput.BackColor = System.Drawing.Color.FromArgb(30, 30, 46);
            txtInput.ForeColor = System.Drawing.Color.White;
            txtInput.Font = new System.Drawing.Font("Consolas", 10F);
            txtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // 
            // btnSend
            // 
            btnSend.Location = new System.Drawing.Point(618, 374);
            btnSend.Name = "btnSend";
            btnSend.Size = new System.Drawing.Size(75, 25);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.Anchor = System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Right;
            btnSend.Click += new System.EventHandler(this.btnSend_Click);

            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(699, 374);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(73, 25);
            btnClear.TabIndex = 3;
            btnClear.Text = "Clear";
            btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Right;
            btnClear.Click += new System.EventHandler(this.btnClear_Click);

            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 411);
            Controls.Add(rtbChat);
            Controls.Add(txtInput);
            Controls.Add(btnSend);
            Controls.Add(btnClear);
            Text = "CyberSecurity Awareness Bot";
        }

                // 
        // btnQuit
        // 
        this.btnQuit = new System.Windows.Forms.Button();
        this.btnQuit.Location = new System.Drawing.Point(718, 373);
        this.btnQuit.Name = "btnQuit";
        this.btnQuit.Size = new System.Drawing.Size(70, 27);
        this.btnQuit.Text = "Quit";
        this.btnQuit.BackColor = System.Drawing.Color.FromArgb(180, 50, 50);
        this.btnQuit.ForeColor = System.Drawing.Color.White;
        this.btnQuit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnQuit.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
        this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
        #endregion

        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnQuit;
    }
}