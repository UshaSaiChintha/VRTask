using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class DashMetricsCalculator : MonoBehaviour
{
    public string lowQualityMpdUrl = "http://localhost:8000/144p/144p.mpd"; // URL of the low quality MPD file
    public string highQualityMpdUrl = "http://localhost:8000/360p/360p.mpd"; // URL of the high quality MPD file

    private Dictionary<string, float> lowQualityVideoQualities = new Dictionary<string, float>(); // Dictionary to store video qualities (bitrates) for low quality
    private Dictionary<string, float> highQualityVideoQualities = new Dictionary<string, float>(); // Dictionary to store video qualities (bitrates) for high quality
    private float segmentDownloadStartTime;

    void Start()
    {
        // Parse MPD files and extract video qualities
        ParseMpdFile(lowQualityMpdUrl, lowQualityVideoQualities);
        ParseMpdFile(highQualityMpdUrl, highQualityVideoQualities);
    }

    void ParseMpdFile(string mpdUrl, Dictionary<string, float> videoQualities)
    {
        // Download MPD file synchronously
        UnityWebRequest mpdRequest = UnityWebRequest.Get(mpdUrl);
        mpdRequest.SendWebRequest();

        while (!mpdRequest.isDone) { }

        if (mpdRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download MPD file: " + mpdRequest.error);
            return;
        }

        // Parse MPD XML
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(mpdRequest.downloadHandler.text);

        XmlNodeList adaptationSets = xmlDocument.GetElementsByTagName("AdaptationSet");
        foreach (XmlNode adaptationSet in adaptationSets)
        {
            // Extract video qualities (bitrates)
            string representationId = adaptationSet.FirstChild.Attributes["id"].Value;
            float bitrate = float.Parse(adaptationSet.FirstChild.Attributes["bandwidth"].Value) / 1000f; // Convert to kbps
            videoQualities.Add(representationId, bitrate);
        }
    }

    public void MonitorSegmentDownload(float segmentDownloadTime, float videoQuality)
    {
        LogMetrics(segmentDownloadTime, videoQuality);
    }

    void LogMetrics(float segmentDownloadTime, float videoQuality)
    {
        // Log metrics to a text file
        string filePath = Application.dataPath + "/Metrics.txt";
        string logMessage = "Download Time: " + segmentDownloadTime + " seconds, Video Quality: " + videoQuality + " kbps";

        // Append the log message to the text file
        using (StreamWriter writer = File.AppendText(filePath))
        {
            writer.WriteLine(logMessage);
        }

        Debug.Log("Metrics logged: " + logMessage);
    }
}
