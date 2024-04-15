# VRTask 1

Dependencies -

1. org.nanohttpd:nanohttpd:2.3.1
2. com.sparkjava:spark-core:2.9.4
3. ch.qos.logback:logback-classic:1.2.3

Initially run the VR server
Run VideoEncoder file 
  Ensure to update the path of video file to the location where its saved on ur device
  
This run, produces mpd file and later on run the VideoServer file which hosts the mpd file on server

Move the client code to Unity and create a game object with DashMetricsCalculator as its compoenent.

Run the configuration and you can see video files getting streamed and metrics being loggoed to a .txt file
