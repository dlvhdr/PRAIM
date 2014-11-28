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

namespace PRAIMDB
{
    public class PRAIMDataBase
    {
        //static string app_location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string connectionString = "Data Source=(LocalDB)\\v11.0;" +
                //@"AttachDbFilename=|DataDirectory|PRAIMTable.mdf;" +
                @"AttachDbFilename=C:\Users\dlv\Google Drive\Studies\6th Semester\Industrial Project\github_project\src\PRAIMDataBase\PRAIMTable.mdf;" +
                //@"AttachDbFilename=C:\Users\Adi&Dvir\PRAIM\src\PRAIMDataBase\PRAIMTable.mdf;" +
                "Integrated Security=True; Trusted_Connection=True;";
//        static string connectionString = @"Data Source=.\SQLEXPRESS;
//                          AttachDbFilename=|DataDirectory|PRAIMTable.mdf;
//                          Integrated Security=True;
//                          Connect Timeout=5;
//                          User Instance=True";
        public bool InsertActionItem(ActionItem actionItem)
        {
            actionItem.id = 100;
            int priority = (int)actionItem.metaData.Priority;
            int projectID = (int)actionItem.metaData.ProjectID;
            double version = actionItem.metaData.Version;
            Nullable<DateTime> dateTime = actionItem.metaData.DateTime;
            string comments = actionItem.metaData.Comments;
            byte[] snapShot = actionItem.snapShot;

            //Image img = Image.FromFile(@"C:\Users\Adi&Dvir\PRAIM\src\PRAIMDataBase\test.PNG");
            //byte[] arr;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //    arr = ms.ToArray();
            //}
            //string filepath = @"C:\Users\Adi&Dvir\PRAIM\src\PRAIMDataBase\test.PNG";
            //FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read); //create a file stream object associate to user selected file 
            //byte[] image = new byte[fs.Length]; //create a byte array with size of user select file stream length
            //fs.Read(image, 0, Convert.ToInt32(fs.Length));//read user selected file stream in to byte array
            


            //System.Console.WriteLine("actionItemId: {0}", actionItemId);
            //System.Console.WriteLine("priority: {0}", priority);
            //System.Console.WriteLine("projectID: {0}", projectID);
            //System.Console.WriteLine("version: {0}", version);
            //System.Console.WriteLine("dateTime: {0}", dateTime);
            //System.Console.WriteLine("comments: {0}", comments);
            //System.Console.WriteLine("snapShot: {0}", image);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter cmd = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand("INSERT INTO PRAIMDBTable " +
                                         "(ActionItemId, Priority, ProjectID, Version, DateTime, Comments, Snapshot) " +
                                         "VALUES (@actionItemId, @priority, @projectID, @version, @dateTime, @comments, @snapshot)"
                                         , connection);

                    command.Parameters.AddWithValue("@actionItemId", actionItem.id);
                    command.Parameters.AddWithValue("@priority", priority);
                    command.Parameters.AddWithValue("@projectID", projectID);
                    command.Parameters.AddWithValue("@version", version);
                    command.Parameters.AddWithValue("@dateTime", dateTime);
                    command.Parameters.AddWithValue("@comments", comments);
                    //command.Parameters.AddWithValue("@snapshot", image);

                    command.Parameters.Add("@snapshot", SqlDbType.Image, actionItem.snapShot.Length).Value = actionItem.snapShot;

                    command.Connection = connection;
                    cmd.InsertCommand = command;

                    connection.Open();
                    command.ExecuteNonQuery();

                    connection.Close();
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
            int priority = (int)metaData.Priority;
            int projectID = metaData.ProjectID;
            double version = metaData.Version;
            Nullable<DateTime> dateTime = metaData.DateTime;
            string comments = metaData.Comments;
            
            string priorityString = null;
            string projectIDString = null;
            string versionString = null;
            string dateTimeString = null;
            string commentsString = null;

            if (priority != -1)
            {
                priorityString = "Priority = @priority AND ";
            }
            if (projectID != -1)
            {
                projectIDString = "ProjectID = @projectID AND ";
            }
            if (version != -1)
            {
                versionString = "Version = @version AND ";
            }
            if (dateTime != null)
            {
                dateTimeString = "DateTime = @dateTime AND ";
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
                                                         projectIDString + versionString + dateTimeString + commentsString +
                                                         "2=2", connection);
                    command.Parameters.AddWithValue("@priority", priority);
                    command.Parameters.AddWithValue("@projectID", projectID);
                    command.Parameters.AddWithValue("@version", version);
                    if (dateTime != null)
                    {
                        command.Parameters.AddWithValue("@dateTime", dateTime);
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
                                receivedMetaData.ProjectID = reader.GetInt32(2);
                            }
                            if (!reader.IsDBNull(3))
                            {
                                receivedMetaData.Version = reader.GetInt32(3);
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
                Console.WriteLine("connection to PRAIMDB was failed: {0}", e.ToString());
                return null;
            }

            return list;
        }




        public bool DeleteActionItems(ActionItem actionItem)
        {
            try
            {
                int actionItemId = actionItem.id;
                double version = actionItem.metaData.Version;
                int priority = (int)actionItem.metaData.Priority;
                int projectID = actionItem.metaData.ProjectID;
                Nullable<DateTime> dateTime = actionItem.metaData.DateTime;
                string comments = actionItem.metaData.Comments;

                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    //command.CommandText = "DELETE FROM PRAIMDBTable WHERE ActionItemId = @actionItemId AND " +
                    //                   "Priority = @priority AND ProjectID = @projectID AND " +
                    //                   "Version = @version AND DateTime = @dateTime AND Comments = @comments";
                    command.CommandText = "DELETE FROM PRAIMDBTable WHERE ActionItemId = @actionItemId";

                    command.Parameters.AddWithValue("@actionItemId", actionItemId);
                    //command.Parameters.AddWithValue("@priority", priority);
                    //command.Parameters.AddWithValue("@projectID", projectID);
                    //command.Parameters.AddWithValue("@version", version);
                    //command.Parameters.AddWithValue("@dateTime", dateTime);
                    //command.Parameters.AddWithValue("@comments", comments);
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

        private static int _NextID = 0;
    }
}
