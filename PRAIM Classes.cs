enum Priority {High =1, medium, low}

class ImageSettings {
	int projectID;
	Priority priority;
	double version;
	Date date;
	Time time;
	char* comments;
}

class Image {
	int id;
	char* imagePath;
	ImageSettings settings;
}

//The app that use this DLL need to create one PRAIM object and use open() to open PRAIM application
public class PRAIM {
	public PRAIM(int projectID, double version, Priority defaultPriority);
	//open the PRAIM dialog
	public bool open();
	bool takeSnapshot();
	// return true if succeed & false if fail
	bool insertImage(const Image& image);
	list<Image> getImage (const ImageSettings&  settings);
}

