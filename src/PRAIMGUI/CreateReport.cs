using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Collections.ObjectModel;
using Common;
using System.IO;

namespace PRAIM
{
    public class CreateReport
    {
        public static bool CreateCSVReport(ObservableCollection<ActionItem> oc, string filePath)
        {
            var csv = new StringBuilder();
            string newLine;
            try
            {
                //inserting the title:
                newLine = string.Format("{0},{1},{2},{3}{4}",
                    "", "", "", "PRAIM Report. generated at: " + DateTime.Now, Environment.NewLine);
                csv.Append(newLine);
                //inserting the Columns names:
                newLine = string.Format("{0},{1},{2},{3},{4},{5}{6}",
                    "id", "ProjectName", "Version", "Priority", "DateTime", "Comments", Environment.NewLine);
                csv.Append(newLine);
            }
            catch
            {
                return false;
            }
            //Inserting the table itself:
            foreach (ActionItem actionItem in oc)
            {
                int id = actionItem.id;
                string ProjectName = actionItem.metaData.ProjectName;
                string Version = actionItem.metaData.Version;
                Priority? Priority = actionItem.metaData.Priority;
                DateTime? DateTime = actionItem.metaData.DateTime;
                string Comments = actionItem.metaData.Comments;
                
                try
                {
                    newLine = string.Format("{0},{1},{2},{3},{4},{5}{6}",
                        id, ProjectName, Version, Priority, DateTime, Comments, Environment.NewLine);
                    csv.Append(newLine);
                }
                catch
                {
                    return false;
                }
            }
            File.WriteAllText(filePath, csv.ToString());
            return true;
        }

    }
}
