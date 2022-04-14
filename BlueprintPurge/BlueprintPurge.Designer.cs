namespace BlueprintPurge
{
    partial class BlueprintPurge
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
            this.textBoxBlueprints = new System.Windows.Forms.TextBox();
            this.textBoxSavePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioWhitelist = new System.Windows.Forms.RadioButton();
            this.radioBlacklist = new System.Windows.Forms.RadioButton();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buttonPurge = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.openFileDialogSave = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogBlueprints = new System.Windows.Forms.OpenFileDialog();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxPeek = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxBlueprints
            // 
            this.textBoxBlueprints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBlueprints.Location = new System.Drawing.Point(12, 122);
            this.textBoxBlueprints.Multiline = true;
            this.textBoxBlueprints.Name = "textBoxBlueprints";
            this.textBoxBlueprints.Size = new System.Drawing.Size(814, 138);
            this.textBoxBlueprints.TabIndex = 0;
            // 
            // textBoxSavePath
            // 
            this.textBoxSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSavePath.Location = new System.Drawing.Point(70, 60);
            this.textBoxSavePath.Name = "textBoxSavePath";
            this.textBoxSavePath.Size = new System.Drawing.Size(675, 23);
            this.textBoxSavePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Blueprints";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Save File";
            // 
            // radioWhitelist
            // 
            this.radioWhitelist.AutoSize = true;
            this.radioWhitelist.Location = new System.Drawing.Point(168, 97);
            this.radioWhitelist.Name = "radioWhitelist";
            this.radioWhitelist.Size = new System.Drawing.Size(71, 19);
            this.radioWhitelist.TabIndex = 5;
            this.radioWhitelist.Text = "Whitelist";
            this.radioWhitelist.UseVisualStyleBackColor = true;
            // 
            // radioBlacklist
            // 
            this.radioBlacklist.AutoSize = true;
            this.radioBlacklist.Checked = true;
            this.radioBlacklist.Location = new System.Drawing.Point(83, 97);
            this.radioBlacklist.Name = "radioBlacklist";
            this.radioBlacklist.Size = new System.Drawing.Size(68, 19);
            this.radioBlacklist.TabIndex = 6;
            this.radioBlacklist.TabStop = true;
            this.radioBlacklist.Text = "Blacklist";
            this.radioBlacklist.UseVisualStyleBackColor = true;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(12, 266);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(97, 23);
            this.buttonSearch.TabIndex = 7;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 295);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(814, 182);
            this.dataGridView1.TabIndex = 8;
            this.dataGridView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DataGridView1_MouseClick);
            // 
            // buttonPurge
            // 
            this.buttonPurge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPurge.Enabled = false;
            this.buttonPurge.Location = new System.Drawing.Point(12, 483);
            this.buttonPurge.Name = "buttonPurge";
            this.buttonPurge.Size = new System.Drawing.Size(97, 23);
            this.buttonPurge.TabIndex = 9;
            this.buttonPurge.Text = "Purge Now!";
            this.buttonPurge.UseVisualStyleBackColor = true;
            this.buttonPurge.Click += new System.EventHandler(this.ButtonPurge_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(751, 60);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 10;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // openFileDialogSave
            // 
            this.openFileDialogSave.DefaultExt = "*.zks";
            this.openFileDialogSave.Filter = "Save file|*.zks|All files|*.*";
            this.openFileDialogSave.RestoreDirectory = true;
            this.openFileDialogSave.Title = "Select save file...";
            // 
            // openFileDialogBlueprints
            // 
            this.openFileDialogBlueprints.DefaultExt = "*.txt";
            this.openFileDialogBlueprints.Filter = "txt files|*.txt|All files|*.*";
            this.openFileDialogBlueprints.Title = "Select list of blueprints to filter...";
            // 
            // buttonHelp
            // 
            this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelp.Location = new System.Drawing.Point(751, 89);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(75, 23);
            this.buttonHelp.TabIndex = 11;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(599, 42);
            this.label3.TabIndex = 12;
            this.label3.Text = "☠️ Warning: This app is a last ditch effort to rescue a corrupted save file.    ☠" +
    "️\r\n☠️ It may cause more errors or corrupt certain areas. Use at your own risk! ☠" +
    "️";
            // 
            // checkBoxPeek
            // 
            this.checkBoxPeek.AutoSize = true;
            this.checkBoxPeek.Checked = true;
            this.checkBoxPeek.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPeek.Location = new System.Drawing.Point(256, 97);
            this.checkBoxPeek.Name = "checkBoxPeek";
            this.checkBoxPeek.Size = new System.Drawing.Size(106, 19);
            this.checkBoxPeek.TabIndex = 13;
            this.checkBoxPeek.Text = "Generate Peeks";
            this.checkBoxPeek.UseVisualStyleBackColor = true;
            // 
            // BlueprintPurge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 518);
            this.Controls.Add(this.checkBoxPeek);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.buttonPurge);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.radioBlacklist);
            this.Controls.Add(this.radioWhitelist);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSavePath);
            this.Controls.Add(this.textBoxBlueprints);
            this.Name = "BlueprintPurge";
            this.Text = "Blueprint Purge";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBlueprints;
        private System.Windows.Forms.TextBox textBoxSavePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioWhitelist;
        private System.Windows.Forms.RadioButton radioBlacklist;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button buttonPurge;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialogSave;
        private System.Windows.Forms.OpenFileDialog openFileDialogBlueprints;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxPeek;
    }
}
