using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRAIM;

namespace PRAIMDataBase
{
    class Program
    {
        static void Main(string[] args)
        {
            ActionItem actionItem = new ActionItem(6);
            PRAIMDataBase db = new PRAIMDataBase();

            ActionMetaData metaData = new ActionMetaData();
            metaData.Priority = Priority.High;
            metaData.ProjectID = 1;
            metaData.Version = 1;
            metaData.DateTime = new DateTime(2010,10,10);
            metaData.Comments = "comment";
            actionItem.metaData = metaData;

            bool result = db.InsertActionItem(actionItem);
            System.Console.WriteLine("InsertActionItem result: {0}", result);
            result = db.InsertActionItem(actionItem);
            System.Console.WriteLine("InsertActionItem result: {0}", result);

            //GetActionItems test
            ActionMetaData metaData2 = new ActionMetaData();
            metaData2.Priority = (Priority)(-1);//Priority.High;
            metaData2.ProjectID = -1;
            metaData2.Version = -1;
            metaData2.DateTime = null;
            metaData2.Comments = null;//"comment";

            List<ActionItem> list = db.GetActionItems(metaData2);
            if (list != null)
            {
                foreach (ActionItem item in list)
                {
                    Console.WriteLine("{0}, {1} , {2}, {3} ,{4}, {5}",
                                       item.id, item.metaData.Priority, item.metaData.ProjectID,
                                       item.metaData.Version, item.metaData.DateTime, item.metaData.Comments);
                }
            }

            //DeleteActionItems test

            ActionItem actionItemToDelete = list[0];//new ActionItem(1);
            //metaData.Priority = Priority.High;
            //metaData.ProjectID = 1;
            //metaData.Version = 1;
            //metaData.DateTime = null;
            //metaData.Comments = "aaa";
            //actionItemToDelete.metaData = metaData;

            result = db.DeleteActionItems(actionItemToDelete);
            System.Console.WriteLine("DeleteActionItems result: {0}", result);

        }
    }
}
