
namespace MassProvisioning
{
    partial class MassProvisioningForm
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
            labelWarning = new System.Windows.Forms.Label();
            btnLoadData = new System.Windows.Forms.Button();
            btnCreateAll = new System.Windows.Forms.Button();
            tbLog = new System.Windows.Forms.TextBox();
            btnDeleteAll = new System.Windows.Forms.Button();
            btnGenerateFakeData = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            tbNbStudents = new System.Windows.Forms.TextBox();
            tbNbTeachers = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbNbClassroomsByTeacher = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            tbNbStudentsByClassrooms = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            tbCompanyName = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // labelWarning
            // 
            labelWarning.AutoSize = true;
            labelWarning.ForeColor = System.Drawing.Color.Maroon;
            labelWarning.Location = new System.Drawing.Point(605, 30);
            labelWarning.Name = "labelWarning";
            labelWarning.Size = new System.Drawing.Size(353, 45);
            labelWarning.TabIndex = 0;
            labelWarning.Text = "It's mandatory to specify  in file ApplicationInfo.cs:\r\n - APP_ID, APP_SECRET_KEY, HOST_NAME, \r\n - ORGANIZATION_ADMIN_LOGIN, ORGANIZATION_ADMIN_PWD\r\n";
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new System.Drawing.Point(56, 139);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new System.Drawing.Size(288, 29);
            btnLoadData.TabIndex = 1;
            btnLoadData.Text = "Load School Data: Company, Teachers, Students";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += btnLoadData_Click;
            // 
            // btnCreateAll
            // 
            btnCreateAll.Location = new System.Drawing.Point(350, 139);
            btnCreateAll.Name = "btnCreateAll";
            btnCreateAll.Size = new System.Drawing.Size(272, 29);
            btnCreateAll.TabIndex = 2;
            btnCreateAll.Text = "Create Teachers, Students, Classrooms";
            btnCreateAll.UseVisualStyleBackColor = true;
            btnCreateAll.Click += btnCreateAll_Click;
            // 
            // tbLog
            // 
            tbLog.Location = new System.Drawing.Point(12, 174);
            tbLog.Multiline = true;
            tbLog.Name = "tbLog";
            tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbLog.Size = new System.Drawing.Size(974, 517);
            tbLog.TabIndex = 3;
            // 
            // btnDeleteAll
            // 
            btnDeleteAll.Location = new System.Drawing.Point(628, 139);
            btnDeleteAll.Name = "btnDeleteAll";
            btnDeleteAll.Size = new System.Drawing.Size(272, 29);
            btnDeleteAll.TabIndex = 4;
            btnDeleteAll.Text = "Delete all: Teachers, Students, Classrooms";
            btnDeleteAll.UseVisualStyleBackColor = true;
            btnDeleteAll.Click += btnDeleteAll_Click;
            // 
            // btnGenerateFakeData
            // 
            btnGenerateFakeData.Location = new System.Drawing.Point(206, 93);
            btnGenerateFakeData.Name = "btnGenerateFakeData";
            btnGenerateFakeData.Size = new System.Drawing.Size(288, 29);
            btnGenerateFakeData.TabIndex = 5;
            btnGenerateFakeData.Text = "Generate fake data: Company, Teachers, Students";
            btnGenerateFakeData.UseVisualStyleBackColor = true;
            btnGenerateFakeData.Click += btnGenerateFakeData_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(169, 8);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(75, 15);
            label2.TabIndex = 6;
            label2.Text = "Nb Students:";
            // 
            // tbNbStudents
            // 
            tbNbStudents.Location = new System.Drawing.Point(250, 5);
            tbNbStudents.Name = "tbNbStudents";
            tbNbStudents.Size = new System.Drawing.Size(35, 23);
            tbNbStudents.TabIndex = 7;
            tbNbStudents.Text = "10";
            // 
            // tbNbTeachers
            // 
            tbNbTeachers.Location = new System.Drawing.Point(250, 35);
            tbNbTeachers.Name = "tbNbTeachers";
            tbNbTeachers.Size = new System.Drawing.Size(35, 23);
            tbNbTeachers.TabIndex = 9;
            tbNbTeachers.Text = "2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(161, 38);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(73, 15);
            label3.TabIndex = 8;
            label3.Text = "Nb teachers:";
            // 
            // tbNbClassroomsByTeacher
            // 
            tbNbClassroomsByTeacher.Location = new System.Drawing.Point(459, 5);
            tbNbClassroomsByTeacher.Name = "tbNbClassroomsByTeacher";
            tbNbClassroomsByTeacher.Size = new System.Drawing.Size(35, 23);
            tbNbClassroomsByTeacher.TabIndex = 11;
            tbNbClassroomsByTeacher.Text = "1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(323, 41);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(134, 15);
            label4.TabIndex = 10;
            label4.Text = "Students by classrooms:";
            // 
            // tbNbStudentsByClassrooms
            // 
            tbNbStudentsByClassrooms.Location = new System.Drawing.Point(459, 38);
            tbNbStudentsByClassrooms.Name = "tbNbStudentsByClassrooms";
            tbNbStudentsByClassrooms.Size = new System.Drawing.Size(35, 23);
            tbNbStudentsByClassrooms.TabIndex = 13;
            tbNbStudentsByClassrooms.Text = "5";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(301, 8);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(146, 15);
            label5.TabIndex = 12;
            label5.Text = "Nb classrooms by teacher:";
            // 
            // tbCompanyName
            // 
            tbCompanyName.Location = new System.Drawing.Point(305, 67);
            tbCompanyName.Name = "tbCompanyName";
            tbCompanyName.Size = new System.Drawing.Size(194, 23);
            tbCompanyName.TabIndex = 15;
            tbCompanyName.Text = "CSharpSDKCompany_MassPro";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(202, 70);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(97, 15);
            label6.TabIndex = 14;
            label6.Text = "Company Name:";
            // 
            // MassProvisioningForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(998, 703);
            Controls.Add(tbCompanyName);
            Controls.Add(label6);
            Controls.Add(tbNbStudentsByClassrooms);
            Controls.Add(label5);
            Controls.Add(tbNbClassroomsByTeacher);
            Controls.Add(label4);
            Controls.Add(tbNbTeachers);
            Controls.Add(label3);
            Controls.Add(tbNbStudents);
            Controls.Add(label2);
            Controls.Add(btnGenerateFakeData);
            Controls.Add(btnDeleteAll);
            Controls.Add(tbLog);
            Controls.Add(btnCreateAll);
            Controls.Add(btnLoadData);
            Controls.Add(labelWarning);
            ForeColor = System.Drawing.SystemColors.ControlText;
            Name = "MassProvisioningForm";
            Text = "Mass Provisioning example";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnCreateAll;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btnDeleteAll;
        private System.Windows.Forms.Button btnGenerateFakeData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbNbStudents;
        private System.Windows.Forms.TextBox tbNbTeachers;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbNbClassroomsByTeacher;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbNbStudentsByClassrooms;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbCompanyName;
        private System.Windows.Forms.Label label6;
    }
}

