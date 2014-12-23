using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;

using PRAIM;
using Common;
using PRAIM.Models;


namespace PRAIMDB
{
    public class PRAIMDataBase
    {
        //constructor
        public PRAIMDataBase(int currentID)
        {
            this.currentID = currentID;
            //CreateDatabase();
        }

        //public static void CreateSqlDatabase(string filename)
        //{

        //    SqlConnection myConn = new SqlConnection(connectionString);

        //    String str = "CREATE DATABASE MyDatabase ON PRIMARY " +
        //        "(NAME = MyDatabase_Data, " +
        //        "FILENAME = 'C:\\Users\\Adi&Dvir\\PRAIM\\src\\PRAIMDataBase\\PRAIMTable.mdf')";
        //    SqlCommand myCommand = new SqlCommand(str, myConn);
        //    try
        //    {
        //        myConn.Open();
        //        myCommand.ExecuteNonQuery();
        //        Console.WriteLine("DB was crated successfully"); //TODO: tobe removed
                
        //        if (myConn.State == ConnectionState.Open)
        //        {
        //            using (SqlCommand command = new SqlCommand(@"CREATE TABLE PRAIMDBTable (ActionItemId INT PRIMARY KEY" +
        //                ",Priority INT,ProjectName NVARCHAR (MAX),Version NVARCHAR (MAX)" +
        //                ",DateTime dateTime,Comments NVARCHAR (MAX), Snapshot IMAGE)", myConn))
        //                try
        //                {
        //                    command.ExecuteNonQuery();
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    Console.WriteLine("DB was failed, {0}", ex); //TODO: tobe removed
        //                }
        //            myConn.Close();
        //        }
                
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Console.WriteLine("DB was failed, {0}", ex); //TODO: tobe removed
        //    }       
        //}

        //create the DB 
        //public void CreateDatabase()
        //{
        //    var filename = System.IO.Path.Combine(appLocation, "PRAIMTable.mdf");
        //    if (!System.IO.File.Exists(filename))
        //    {
        //        CreateSqlDatabase(filename);
        //    }
        //    CreateSqlDatabase(filename); //TODO: tobe removed
        //    Console.WriteLine("DB already exist"); //TODO: tobe removed
        //}


        public bool InsertActionItem(ActionItem actionItem)
        {
            actionItem.id = currentID;
            int priority = (int)actionItem.metaData.Priority;
            string ProjectName = actionItem.metaData.ProjectName;
            string version = actionItem.metaData.Version;
            Nullable<DateTime> dateTime = actionItem.metaData.DateTime;
            string comments = actionItem.metaData.Comments;
            byte[] snapShot = actionItem.snapShot;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand("INSERT INTO PRAIMDBTable " +
                                         "(ActionItemId, Priority, ProjectName, Version, DateTime, Comments, Snapshot) " +
                                         "VALUES (@actionItemId, @priority, @ProjectName, @version, @dateTime, @comments, @snapshot)"
                                         , connection);

                    command.Parameters.AddWithValue("@actionItemId", actionItem.id);
                    command.Parameters.AddWithValue("@priority", priority);
                    command.Parameters.AddWithValue("@ProjectName", ProjectName);
                    command.Parameters.AddWithValue("@version", version);
                    command.Parameters.AddWithValue("@dateTime", dateTime);
                    command.Parameters.AddWithValue("@comments", comments);
                    command.Parameters.Add("@snapshot", SqlDbType.Image, actionItem.snapShot.Length).Value = actionItem.snapShot;

                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    connection.Close();
                    currentID++;
                    Console.WriteLine("InsertActionItem was succeeded");
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return false;
            }
           
            return true;
        }

        public List<ActionItem> GetActionItems(ActionMetaData metaData) {
            int? priority = (int?)metaData.Priority;
            string ProjectName = metaData.ProjectName;
            string version = metaData.Version;
            Nullable<DateTime> fromDate = metaData.FromDate;
            Nullable<DateTime> toDate = metaData.ToDate + new TimeSpan(23,59,59);
            string comments = metaData.Comments;

            string priorityString = null;
            string projectNameString = null;
            string versionString = null;
            string fromDateString = null;
            string toDateString = null;
            string commentsString = null;

            if (priority != null)
            {
                priorityString = "Priority = @priority AND ";
            }
            if (ProjectName != null)
            {
                projectNameString = "ProjectName LIKE @ProjectName AND ";
            }
            if (version != null)
            {
                versionString = "Version LIKE @version AND ";
            }
            if (fromDate != null)
            {
                fromDateString = "DateTime >= @fromDate AND ";
            }
            if (toDate != null)
            {
                toDateString = "DateTime <= @toDate AND ";
            }
            if (comments != null)
            {
                commentsString = "Comments LIKE @comments AND ";
            }
            List<ActionItem> list = new List<ActionItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand("SELECT * FROM PRAIMDBTable WHERE " + priorityString +
                                                         projectNameString + versionString + fromDateString + 
                                                         toDateString + commentsString + "2=2", connection);
                    if (priority != null)
                    {
                        command.Parameters.AddWithValue("@priority", priority);
                    }
                    if (ProjectName != null)
                    {
                        command.Parameters.AddWithValue("@ProjectName", '%' + ProjectName + '%');
                    }
                    if (version != null)
                    {
                        command.Parameters.AddWithValue("@version", '%' + version + '%');
                    }
                    if (fromDate != null)
                    {
                        command.Parameters.AddWithValue("@fromDate", fromDate);
                    }
                    if (toDate != null)
                    {
                        command.Parameters.AddWithValue("@toDate", toDate);
                    }
                    if (comments != null)
                    {
                        command.Parameters.AddWithValue("@comments", '%' + comments + '%');
                    }

                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ActionItem receivedActionItem = new ActionItem(reader.GetInt32(0));
                            ActionMetaData receivedMetaData = new ActionMetaData();

                            if (!reader.IsDBNull(1))
                            {
                                receivedMetaData.Priority = (Priority)reader.GetInt32(1);
                            }
                            if (!reader.IsDBNull(2))
                            {
                                receivedMetaData.ProjectName = reader.GetString(2);
                            }
                            if (!reader.IsDBNull(3))
                            {
                                receivedMetaData.Version = reader.GetString(3);
                            }
                            if (!reader.IsDBNull(4))
                            {
                                receivedMetaData.DateTime = reader.GetDateTime(4);
                            }
                            if (!reader.IsDBNull(5))
                            {
                                receivedMetaData.Comments = reader.GetString(5);
                            }
                            if (!reader.IsDBNull(6))
                            {
                                receivedActionItem.snapShot = (byte[])reader.GetSqlBinary(6);
                            }
                            receivedActionItem.metaData = receivedMetaData;
                            list.Add(receivedActionItem);
                        }
                    }
                    connection.Close();
                    Console.WriteLine("GetActionItems was succeeded");
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return null;
            }

            return list;
        }

        public bool DeleteActionItems(ActionItem actionItem)
        {
            try
            {
                int actionItemId = actionItem.id;
                string version = actionItem.metaData.Version;
                int priority = (int)actionItem.metaData.Priority;
                string ProjectName = actionItem.metaData.ProjectName;
                Nullable<DateTime> dateTime = actionItem.metaData.DateTime;
                string comments = actionItem.metaData.Comments;

                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "DELETE FROM PRAIMDBTable WHERE ActionItemId = @actionItemId";

                    command.Parameters.AddWithValue("@actionItemId", actionItemId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DeleteActionItem was failed: {0}", e.ToString());
                return false;
            }
            Console.WriteLine("DeleteActionItem was succeeded");
            return true;
        }

        public bool InsertProject(string ProjectName, string Description)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand("INSERT INTO Projects " +
                                         "(ProjectName, Description) " +
                                         "VALUES (@projectName, @description)"
                                         , connection);
                    if (Description == null) {
                        Description = "";
                    }

                    command.Parameters.AddWithValue("@projectName", ProjectName);
                    command.Parameters.AddWithValue("@description", Description);

                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    connection.Close();
                    Console.WriteLine("InsertProject was succeeded");
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return false;
            }
            return true;
        }

        public bool DeleteProject(string ProjectName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "DELETE FROM Projects WHERE ProjectName = @projectName";

                    command.Parameters.AddWithValue("@projectName", ProjectName);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("DeleteActionItem was failed: {0}", e.ToString());
                return false;
            }
            //Console.WriteLine("DeleteActionItem was succeeded");
            return true;
        }

        public bool InsertVersion(string ProjectName, string Version)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand("INSERT INTO Versions " +
                                         "(ProjectName, Version) " +
                                         "VALUES (@projectName, @version)"
                                         , connection);

                    command.Parameters.AddWithValue("@projectName", ProjectName);
                    command.Parameters.AddWithValue("@version", Version);

                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    connection.Close();
                    Console.WriteLine("InsertVersion was succeeded");
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return false;
            }
            return true;
        }

        public bool DeleteVersion(string Version)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "DELETE FROM Versions WHERE Version = @version";

                    command.Parameters.AddWithValue("@version", Version);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("DeleteActionItem was failed: {0}", e.ToString());
                return false;
            }
            //Console.WriteLine("DeleteActionItem was succeeded");
            return true;
        }

        public List<Project> GetAllProjectProperties()
        {
            List<Project> list = new List<Project>();
            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    //SqlCommand command = new SqlCommand("SELECT * FROM Projects,Versions WHERE " +
                    //                                    "Projects.ProjectName = Versions.ProjectName", connection);
                    SqlCommand command = new SqlCommand("SELECT * FROM Projects", connection);
                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Project projectProperties = new Project();

                            if (!reader.IsDBNull(0)) {
                                projectProperties.Name = reader.GetString(0);
                            }
                            if (!reader.IsDBNull(1)) {
                                projectProperties.Description = reader.GetString(1);
                            }
                            SqlCommand command2 = new SqlCommand("SELECT Version FROM Versions WHERE " +
                                                                "ProjectName = @projectName", connection);
                            command2.Parameters.AddWithValue("@projectName", projectProperties.Name);
                            using (SqlDataReader reader2 = command2.ExecuteReader()) {
                                while (reader2.Read()) {
                                    if (!reader2.IsDBNull(0)) {
                                        projectProperties.Versions.Add(reader2.GetString(0));
                                    }
                                }
                            }

                            list.Add(projectProperties);
                        }
                    }

                    connection.Close();
                    Console.WriteLine("GetAllProjectProperties was succeeded");
                }
            }
            catch (Exception e) {
                //Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return null;
            }

            return list;
        }

        public int currentID {get; set;}
        static string appLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string connectionString = "Data Source=(LocalDB)\\v11.0;" +
            //@"AttachDbFilename=C:\Users\dlv\Google Drive\Studies\6th Semester\Industrial Project\github_project\src\PRAIMDataBase;" +
            @"AttachDbFilename=" + appLocation + @"\PRAIMTable.mdf;" +
                "Integrated Security=True; Trusted_Connection=True; MultipleActiveResultSets=True;";
    }
}
