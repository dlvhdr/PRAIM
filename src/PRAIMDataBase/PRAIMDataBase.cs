using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

using PRAIM;
using Common;

namespace PRAIMDB
{
    public class PRAIMDataBase
    {
        //constructor
        public PRAIMDataBase(int currentID)
        {
            this.currentID = currentID;
        }

        //static string app_location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string connectionString = "Data Source=(LocalDB)\\v11.0;" +
                //@"AttachDbFilename=|DataDirectory|PRAIMTable.mdf;" +
                //@"AttachDbFilename=C:\Users\dlv\Google Drive\Studies\6th Semester\Industrial Project\github_project\src\PRAIMDataBase\PRAIMTable.mdf;" +
                @"AttachDbFilename=C:\Users\Adi&Dvir\PRAIM\src\PRAIMDataBase\PRAIMTable.mdf;" +
                "Integrated Security=True; Trusted_Connection=True;";

        public bool InsertActionItem(ActionItem actionItem)
        {
            actionItem.id = currentID;
            int priority = (int)actionItem.metaData.Priority;
            string ProjectName = actionItem.metaData.ProjectName;
            string version = actionItem.metaData.Version;
            Nullable<DateTime> dateTime = actionItem.metaData.DateTime;
            string comments = actionItem.metaData.Comments;
            byte[] snapShot = actionItem.snapShot;




            //System.Console.WriteLine("actionItemId: {0}", actionItemId);
            //System.Console.WriteLine("priority: {0}", priority);
            //System.Console.WriteLine("ProjectName: {0}", ProjectName);
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
            Nullable<DateTime> dateTime = metaData.DateTime;
            string comments = metaData.Comments;

            string priorityString = null;
            string ProjectNameString = null;
            string versionString = null;
            string dateTimeString = null;
            string commentsString = null;

            if (priority != null)
            {
                priorityString = "Priority = @priority AND ";
            }
            if (ProjectName != null)
            {
                ProjectNameString = "ProjectName LIKE @ProjectName AND ";
            }
            if (version != null)
            {
                versionString = "Version LIKE @version AND ";
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
                                                         ProjectNameString + versionString + dateTimeString + commentsString +
                                                         "2=2", connection);
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

        public int currentID {get; set;}
    }
}
