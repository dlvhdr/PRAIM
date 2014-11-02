enum Priority { High =1, Medium, Low }

class ActionMetaData {
	int projectID;
	Priority priority;
	double version;
	DateTime dateTime;
	string comments;
}

class ActionItem {
	int id;
	string imagePath;
	ImageSettings settings;
}

//The app that use this DLL need to create one PRAIM object and use open() to open PRAIM application
public class PRAIM {
	//PRAIM constructor. Provide default values for the project under development.
	public PRAIM(int projectID, double version, Priority defaultPriority);
	
	//open the PRAIM dialog
	public bool open();
	
	//Take snapshot handler
	bool takeSnapshot();
	
	// return true if succeed & false if fail
	bool insertActionItem(ActionItem actionItem);
	
	// return the list of images 
	List<ActionItem> getActionItem(ActionMetaData metaData);
}

