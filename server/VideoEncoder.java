import java.io.*;

public class VideoEncoder {

    public static void main(String[] args) {
        // Directory containing the input video files
        String inputDirectory = "/Users/ushasaichintha/IdeaProjects/VRTASK1/server/video";
        // Parent output directory for segmented videos
        String outputParentDirectory = "/Users/ushasaichintha/IdeaProjects/VRTASK1/server/segments";
        // Segment duration in seconds
        int segmentDurationInSeconds = 10;

        // Create the parent output directory if it doesn't exist
        File outputParentDir = new File(outputParentDirectory);
        if (!outputParentDir.exists()) {
            outputParentDir.mkdirs();
        }

        // Get a list of input video files in the directory
        File[] inputFiles = new File(inputDirectory).listFiles();
        if (inputFiles == null) {
            System.err.println("No video files found in the input directory.");
            return;
        }

        // Iterate over each input video file
        for (File inputFile : inputFiles) {
            if (inputFile.isFile() && inputFile.getName().toLowerCase().endsWith(".mp4")) {
                String inputVideoFile = inputFile.getAbsolutePath();

                // Extract the base name of the input video file (without extension)
                String baseName = inputFile.getName().replace(".mp4", "");

                String outputDirectory = outputParentDirectory + "/" + baseName;

                // Create the output directory for this input video if it doesn't exist
                File outputDir = new File(outputDirectory);
                if (!outputDir.exists()) {
                    outputDir.mkdirs();
                }

                // Construct output file name with .mpd extension
                String outputMpdFile = outputDirectory + "/" + baseName + ".mpd";

                // Construct FFmpeg command to generate MPEG-DASH output
                String ffmpegCmd = "ffmpeg -i " + inputVideoFile +
                        " -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 128k" +
                        " -f dash" +
                        " -seg_duration " + segmentDurationInSeconds +
                        " -use_template 1 -use_timeline 1" +
                        " -init_seg_name init" + baseName + ".m4s -media_seg_name chunk" + baseName + "-%05d.m4s" +
                        " " + outputMpdFile;

                try {
                    // Execute FFmpeg command
                    Process process = Runtime.getRuntime().exec(ffmpegCmd);
                    process.waitFor();

                    // Check if FFmpeg command was successful
                    int exitCode = process.exitValue();
                    if (exitCode == 0) {
                        System.out.println("MPEG-DASH output generated for: " + inputVideoFile);
                    } else {
                        System.err.println("Error: FFmpeg command failed with exit code " + exitCode +
                                " for input file: " + inputVideoFile);
                    }
                } catch (IOException | InterruptedException e) {
                    e.printStackTrace();
                }
            }
        }
    }
}