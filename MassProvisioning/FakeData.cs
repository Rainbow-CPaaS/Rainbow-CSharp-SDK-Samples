using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rainbow;

using NLog;

namespace MassProvisioning
{
    static class FakeData
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(FakeData));
        
        private static Random random;

        static FakeData()
        {
            random = new Random();
        }

        static public Model.CompanyMassProvisioning CreateCompany(String companyName)
        {
            Model.CompanyMassProvisioning company = new Model.CompanyMassProvisioning();
            company.Name = companyName;
            company.Country = "FRA";
            company.State = "";

            return company;
        }

        static public Model.UserMassProvisioning CreateCompanyAdmin(String companyName)
        {
            Model.UserMassProvisioning result = new Model.UserMassProvisioning();
            String _companyName = companyName.Replace(" ", "").Replace("_", ""); // prevent spaces and underscore

            result.FirstName = $"CompanyAdmin_fn";
            result.LastName = $"CompanyAdmin_ln";

            result.Email = $"CompanyAdmin@{_companyName}.com".ToLower();  // EMAIL ADDRESS ARE ALWAYS STORED IN LOWER CASE => IT'S IMPORTANT SINCE THEY ARE USED AS DICTIONARY KEY
            result.Password = $"CompanyAdmin_0AbC!+";

            result.Id = result.Email;
            
            return result;
        }

        static public Dictionary<String , Model.UserMassProvisioning> CreateTeachers(String companyName, int nb)
        {
            Dictionary<String, Model.UserMassProvisioning> result = new Dictionary<String, Model.UserMassProvisioning>();
            String _companyName = companyName.Replace(" ", "").Replace("_", ""); // prevent spaces and underscore
            String id;

            for (int index=0; index< nb; index++)
            {
                Model.UserMassProvisioning user = new Model.UserMassProvisioning();

                id = index.ToString("D8");
                                
                user.FirstName = $"CSharpTeacher_fn_{id}";
                user.LastName = $"CSharpTeacher_ln_{id}";
                
                user.Email = $"CSharpTeacher_{id}@{_companyName}.com".ToLower(); // EMAIL ADDRESS ARE ALWAYS STORED IN LOWER CASE => IT'S IMPORTANT SINCE THEY ARE USED AS DICTIONARY KEY
                user.Password = $"CSharpTeacher_{id}AbC!+";

                user.Id = user.Email;

                result.Add(user.Id, user);

            }
            return result;
        }

        static public Dictionary<String, Model.UserMassProvisioning> CreateStudents(String companyName, int nb)
        {
            Dictionary<String, Model.UserMassProvisioning> result = new Dictionary<String, Model.UserMassProvisioning>();
            String _companyName = companyName.Replace(" ", "").Replace("_", ""); // prevent spaces and underscore
            String id;

            for (int index = 0; index < nb; index++)
            {
                Model.UserMassProvisioning user = new Model.UserMassProvisioning();

                id = index.ToString("D8");

                user.FirstName = $"CSharpStudent_fn_{id}";
                user.LastName = $"CSharpStudent_ln_{id}";
                
                user.Email = $"CSharpStudent_{id}@{_companyName}.com".ToLower();  // EMAIL ADDRESS ARE ALWAYS STORED IN LOWER CASE => IT'S IMPORTANT SINCE THEY ARE USED AS DICTIONARY KEY
                user.Password = $"CSharpStudent_{id}AbC!+";

                user.Id = user.Email;

                result.Add(user.Id, user);

            }
            return result;
        }

        static public Dictionary<String, Dictionary<String, List<String>>> CreateClassrooms(Dictionary<String, Model.UserMassProvisioning> students, Dictionary<String, Model.UserMassProvisioning> teachers, int nbClassroomsByTeacher, int nbStudentsByClassrooms)
        {
            Dictionary<String, Dictionary<String, List<String>>> result = new Dictionary<String, Dictionary<String, List<String>>>();

            // Ensure to have more students that the numbers of students by classroom
            if (nbStudentsByClassrooms > students.Count)
                return null;


            List<Model.UserMassProvisioning> studentsList = students.Values.ToList<Model.UserMassProvisioning>();
            List<Model.UserMassProvisioning> teachersList = teachers.Values.ToList<Model.UserMassProvisioning>();

            int i;
            int max = students.Count - 1;

            String name;
            String emailTeacher;
            String email;
            

            int nbClassrooms = 0;

            // Loop on all teachers
            for (int indexTeacher = 0; indexTeacher < teachers.Count; indexTeacher++)
            {
                emailTeacher = teachersList[indexTeacher].Email;

                Dictionary<String, List<String>> classroomsForTeacher = new Dictionary<string, List<string>>();

                // Loop to create x classroom by teacher
                for (int indexClassroom = 0; indexClassroom < nbClassroomsByTeacher; indexClassroom++)
                {
                    // Create the name of the classroom
                    name = $"CSharpClassroom_{nbClassrooms.ToString("D8")}";
                    nbClassrooms++;

                    List<String> studentsEmail = new List<string>();

                    // Create list of students emails
                    for (int indexStudent = 0; indexStudent < nbStudentsByClassrooms; indexStudent++)
                    {
                        i = random.Next(max);
                        email = studentsList[i].Email;
                        while (studentsEmail.Contains(email))
                        {
                            i++;
                            if (i >= students.Count)
                                i = 0;
                            email = studentsList[i].Email;
                        }
                        studentsEmail.Add(email);
                    }

                    classroomsForTeacher.Add(name, studentsEmail);

                }
                // Add all classrooms for this teacher
                result.Add(emailTeacher, classroomsForTeacher);
            }

            return result;
        }
    }
}
