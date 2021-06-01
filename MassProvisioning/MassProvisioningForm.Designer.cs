
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
            this.labelWarning = new System.Windows.Forms.Label();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnCreateAll = new System.Windows.Forms.Button();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btnDeleteAll = new System.Windows.Forms.Button();
            this.btnGenerateFakeData = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbNbStudents = new System.Windows.Forms.TextBox();
            this.tbNbTeachers = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNbClassroomsByTeacher = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbNbStudentsByClassrooms = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbCompanyName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelWarning
            // 
            this.labelWarning.AutoSize = true;
            this.labelWarning.ForeColor = System.Drawing.Color.Maroon;
            this.labelWarning.Location = new System.Drawing.Point(605, 30);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(353, 45);
            this.labelWarning.TabIndex = 0;
            this.labelWarning.Text = "It\'s mandatory to specify  in file ApplicationInfo.cs:\r\n - APP_ID, APP_SECRET_KEY" +
    ", HOST_NAME, \r\n - ORGANIZATION_ADMIN_LOGIN, ORGANIZATION_ADMIN_PWD\r\n";
            this.labelWarning.UseWaitCursor = true;
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(56, 139);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(288, 29);
            this.btnLoadData.TabIndex = 1;
            this.btnLoadData.Text = "Load School Data: Company, Teachers, Students";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.UseWaitCursor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // btnCreateAll
            // 
            this.btnCreateAll.Location = new System.Drawing.Point(350, 139);
            this.btnCreateAll.Name = "btnCreateAll";
            this.btnCreateAll.Size = new System.Drawing.Size(272, 29);
            this.btnCreateAll.TabIndex = 2;
            this.btnCreateAll.Text = "Create Teachers, Students, Classrooms";
            this.btnCreateAll.UseVisualStyleBackColor = true;
            this.btnCreateAll.UseWaitCursor = true;
            this.btnCreateAll.Click += new System.EventHandler(this.btnCreateAll_Click);
            // 
            // tbLog
            // 
            this.tbLog.Location = new System.Drawing.Point(12, 174);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLog.Size = new System.Drawing.Size(974, 517);
            this.tbLog.TabIndex = 3;
            this.tbLog.UseWaitCursor = true;
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.Location = new System.Drawing.Point(628, 139);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.Size = new System.Drawing.Size(272, 29);
            this.btnDeleteAll.TabIndex = 4;
            this.btnDeleteAll.Text = "Delete all: Teachers, Students, Classrooms";
            this.btnDeleteAll.UseVisualStyleBackColor = true;
            this.btnDeleteAll.UseWaitCursor = true;
            this.btnDeleteAll.Click += new System.EventHandler(this.btnDeleteAll_Click);
            // 
            // btnGenerateFakeData
            // 
            this.btnGenerateFakeData.Location = new System.Drawing.Point(206, 93);
            this.btnGenerateFakeData.Name = "btnGenerateFakeData";
            this.btnGenerateFakeData.Size = new System.Drawing.Size(288, 29);
            this.btnGenerateFakeData.TabIndex = 5;
            this.btnGenerateFakeData.Text = "Generate fake data: Company, Teachers, Students";
            this.btnGenerateFakeData.UseVisualStyleBackColor = true;
            this.btnGenerateFakeData.UseWaitCursor = true;
            this.btnGenerateFakeData.Click += new System.EventHandler(this.btnGenerateFakeData_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(169, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Nb Students:";
            this.label2.UseWaitCursor = true;
            // 
            // tbNbStudents
            // 
            this.tbNbStudents.Location = new System.Drawing.Point(250, 5);
            this.tbNbStudents.Name = "tbNbStudents";
            this.tbNbStudents.Size = new System.Drawing.Size(35, 23);
            this.tbNbStudents.TabIndex = 7;
            this.tbNbStudents.Text = "912";
            this.tbNbStudents.UseWaitCursor = true;
            // 
            // tbNbTeachers
            // 
            this.tbNbTeachers.Location = new System.Drawing.Point(250, 35);
            this.tbNbTeachers.Name = "tbNbTeachers";
            this.tbNbTeachers.Size = new System.Drawing.Size(35, 23);
            this.tbNbTeachers.TabIndex = 9;
            this.tbNbTeachers.Text = "33";
            this.tbNbTeachers.UseWaitCursor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(161, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Nb teachers:";
            this.label3.UseWaitCursor = true;
            // 
            // tbNbClassroomsByTeacher
            // 
            this.tbNbClassroomsByTeacher.Location = new System.Drawing.Point(459, 5);
            this.tbNbClassroomsByTeacher.Name = "tbNbClassroomsByTeacher";
            this.tbNbClassroomsByTeacher.Size = new System.Drawing.Size(35, 23);
            this.tbNbClassroomsByTeacher.TabIndex = 11;
            this.tbNbClassroomsByTeacher.Text = "6";
            this.tbNbClassroomsByTeacher.UseWaitCursor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(323, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "Students by classrooms:";
            this.label4.UseWaitCursor = true;
            // 
            // tbNbStudentsByClassrooms
            // 
            this.tbNbStudentsByClassrooms.Location = new System.Drawing.Point(459, 38);
            this.tbNbStudentsByClassrooms.Name = "tbNbStudentsByClassrooms";
            this.tbNbStudentsByClassrooms.Size = new System.Drawing.Size(35, 23);
            this.tbNbStudentsByClassrooms.TabIndex = 13;
            this.tbNbStudentsByClassrooms.Text = "25";
            this.tbNbStudentsByClassrooms.UseWaitCursor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(301, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 15);
            this.label5.TabIndex = 12;
            this.label5.Text = "Nb classrooms by teacher:";
            this.label5.UseWaitCursor = true;
            // 
            // tbCompanyName
            // 
            this.tbCompanyName.Location = new System.Drawing.Point(305, 67);
            this.tbCompanyName.Name = "tbCompanyName";
            this.tbCompanyName.Size = new System.Drawing.Size(194, 23);
            this.tbCompanyName.TabIndex = 15;
            this.tbCompanyName.Text = "CSharpSDKCompany_MassPro";
            this.tbCompanyName.UseWaitCursor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(202, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 15);
            this.label6.TabIndex = 14;
            this.label6.Text = "Company Name:";
            this.label6.UseWaitCursor = true;
            // 
            // MassProvisioningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 703);
            this.Controls.Add(this.tbCompanyName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbNbStudentsByClassrooms);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbNbClassroomsByTeacher);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbNbTeachers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbNbStudents);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnGenerateFakeData);
            this.Controls.Add(this.btnDeleteAll);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.btnCreateAll);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.labelWarning);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "MassProvisioningForm";
            this.Text = "Mass Provisioning example";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);
            this.PerformLayout();

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

