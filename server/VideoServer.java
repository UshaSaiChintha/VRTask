import spark.Spark;

import java.io.File;

public class VideoServer {

    public static void main(String[] args) {

        // Directory containing the MPEG-DASH content
        String dashContentDirectory = "/Users/ushasaichintha/IdeaProjects/VRTASK1/server/segments";

        // Set up Spark web server
        Spark.port(8000); // Set the port number

        // Serve the MPEG-DASH content
        Spark.staticFiles.externalLocation(dashContentDirectory);

        // Define route for serving the MPEG-DASH manifest file
        Spark.get("/:videoName/:videoName.mpd", (request, response) -> {
            String videoName = request.params(":videoName");
            File mpdFile = new File(dashContentDirectory + "/" + videoName + "/" + videoName + ".mpd");
            if (mpdFile.exists()) {
                // Set the correct MIME type for the response
                response.type("application/dash+xml");

                return mpdFile;
            } else {
                response.status(404);
                return "MPEG-DASH manifest file not found";
            }
        });
    }
}