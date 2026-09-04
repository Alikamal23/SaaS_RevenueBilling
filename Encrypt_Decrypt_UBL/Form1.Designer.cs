namespace Encrypt_Decrypt_UBL
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
            label1 = new Label();
            txt_Input = new TextBox();
            txt_Result = new TextBox();
            label2 = new Label();
            Encrypt = new Button();
            button2 = new Button();
            Decrypt = new Button();
            button1 = new Button();
            button3 = new Button();
            txt_server = new TextBox();
            txt_port = new TextBox();
            txt_username = new TextBox();
            label3 = new Label();
            label4 = new Label();
            txt_password = new TextBox();
            label5 = new Label();
            label6 = new Label();
            chk_ssl = new CheckBox();
            btn_very_hashed_pw = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 67);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 0;
            label1.Text = "INPUT ";
            // 
            // txt_Input
            // 
            txt_Input.Location = new Point(89, 64);
            txt_Input.Name = "txt_Input";
            txt_Input.Size = new Size(897, 23);
            txt_Input.TabIndex = 1;
            // 
            // txt_Result
            // 
            txt_Result.Location = new Point(89, 114);
            txt_Result.Name = "txt_Result";
            txt_Result.Size = new Size(897, 23);
            txt_Result.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 121);
            label2.Name = "label2";
            label2.Size = new Size(45, 15);
            label2.TabIndex = 3;
            label2.Text = "RESULT";
            // 
            // Encrypt
            // 
            Encrypt.Location = new Point(566, 172);
            Encrypt.Name = "Encrypt";
            Encrypt.Size = new Size(217, 40);
            Encrypt.TabIndex = 4;
            Encrypt.Text = "Encrypt";
            Encrypt.UseVisualStyleBackColor = true;
            Encrypt.Click += Encrypt_Click;
            // 
            // button2
            // 
            button2.Location = new Point(880, 204);
            button2.Name = "button2";
            button2.Size = new Size(8, 8);
            button2.TabIndex = 5;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // Decrypt
            // 
            Decrypt.Location = new Point(789, 172);
            Decrypt.Name = "Decrypt";
            Decrypt.Size = new Size(197, 40);
            Decrypt.TabIndex = 6;
            Decrypt.Text = "Decrypt";
            Decrypt.UseVisualStyleBackColor = true;
            Decrypt.Click += Decrypt_Click;
            // 
            // button1
            // 
            button1.Location = new Point(566, 218);
            button1.Name = "button1";
            button1.Size = new Size(217, 40);
            button1.TabIndex = 7;
            button1.Text = "Encrypt Argon";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button3
            // 
            button3.Location = new Point(790, 218);
            button3.Name = "button3";
            button3.Size = new Size(196, 40);
            button3.TabIndex = 8;
            button3.Text = "Email";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // txt_server
            // 
            txt_server.Location = new Point(790, 292);
            txt_server.Name = "txt_server";
            txt_server.Size = new Size(196, 23);
            txt_server.TabIndex = 9;
            txt_server.Text = "mail.bmcsolution.com";
            // 
            // txt_port
            // 
            txt_port.Location = new Point(789, 321);
            txt_port.Name = "txt_port";
            txt_port.Size = new Size(196, 23);
            txt_port.TabIndex = 10;
            txt_port.Text = "26";
            // 
            // txt_username
            // 
            txt_username.Location = new Point(789, 350);
            txt_username.Name = "txt_username";
            txt_username.Size = new Size(196, 23);
            txt_username.TabIndex = 11;
            txt_username.Text = "ali.kamal@bmcsolution.com";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(711, 295);
            label3.Name = "label3";
            label3.Size = new Size(72, 15);
            label3.TabIndex = 12;
            label3.Text = "SMTP Server";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(721, 329);
            label4.Name = "label4";
            label4.Size = new Size(62, 15);
            label4.TabIndex = 13;
            label4.Text = "SMTP Port";
            // 
            // txt_password
            // 
            txt_password.Location = new Point(789, 379);
            txt_password.Name = "txt_password";
            txt_password.Size = new Size(196, 23);
            txt_password.TabIndex = 14;
            txt_password.Text = "K@m@1#0300";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(690, 358);
            label5.Name = "label5";
            label5.Size = new Size(93, 15);
            label5.TabIndex = 15;
            label5.Text = "SMTP Username";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(693, 382);
            label6.Name = "label6";
            label6.Size = new Size(90, 15);
            label6.TabIndex = 16;
            label6.Text = "SMTP Password";
            // 
            // chk_ssl
            // 
            chk_ssl.AutoSize = true;
            chk_ssl.Checked = true;
            chk_ssl.CheckState = CheckState.Checked;
            chk_ssl.Location = new Point(790, 412);
            chk_ssl.Name = "chk_ssl";
            chk_ssl.Size = new Size(63, 19);
            chk_ssl.TabIndex = 17;
            chk_ssl.Text = "Is SSL ?";
            chk_ssl.UseVisualStyleBackColor = true;
            // 
            // btn_very_hashed_pw
            // 
            btn_very_hashed_pw.Location = new Point(89, 218);
            btn_very_hashed_pw.Name = "btn_very_hashed_pw";
            btn_very_hashed_pw.Size = new Size(217, 40);
            btn_very_hashed_pw.TabIndex = 18;
            btn_very_hashed_pw.Text = "Verify Hashed Password";
            btn_very_hashed_pw.UseVisualStyleBackColor = true;
            btn_very_hashed_pw.Click += btn_very_hashed_pw_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(1021, 443);
            Controls.Add(btn_very_hashed_pw);
            Controls.Add(chk_ssl);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(txt_password);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(txt_username);
            Controls.Add(txt_port);
            Controls.Add(txt_server);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(Decrypt);
            Controls.Add(button2);
            Controls.Add(Encrypt);
            Controls.Add(label2);
            Controls.Add(txt_Result);
            Controls.Add(txt_Input);
            Controls.Add(label1);
            Name = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion

        private Label label1;
        private TextBox txt_Input;
        private TextBox txt_Result;
        private Label label2;
        private Button Encrypt;
        private Button button2;
        private Button Decrypt;
        private Button button1;
        private Button button3;
        private TextBox txt_server;
        private TextBox txt_port;
        private TextBox txt_username;
        private Label label3;
        private Label label4;
        private TextBox txt_password;
        private Label label5;
        private Label label6;
        private CheckBox chk_ssl;
        private Button btn_very_hashed_pw;
    }
}