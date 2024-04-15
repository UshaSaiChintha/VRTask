using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Video;

public class DashMetricsCalculator : MonoBehaviour
{
    public string lowQualityMpdUrl = "http://localhost:8000/144p/144p.mpd"; // URL of the low quality MPD file
    public string highQualityMpdUrl = "http://localhost:8000/360p/360p.mpd"; // URL of the high quality MPD file
    public VideoPlayer videoPlayer;

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
            string mimeType = adaptationSet.Attributes["mimeType"].Value;
            string representationId = adaptationSet.FirstChild.Attributes["id"].Value;
            float bitrate = float.Parse(adaptationSet.FirstChild.Attributes["bandwidth"].Value) / 1000f; // Convert to kbps
            videoQualities.Add(representationId, bitrate);
        }
    }

    public void MonitorSegmentDownload()
    {
        // Hook into VideoPlayer's PrepareCompleted event to monitor segment download
        videoPlayer.prepareCompleted += (source) =>
        {
            segmentDownloadStartTime = Time.time;
        };

        // Hook into VideoPlayer's loopPointReached event to log metrics when a segment finishes downloading
        videoPlayer.loopPointReached += (source) =>
        {
            float segmentDownloadTime = Time.time - segmentDownloadStartTime;
            float videoQuality = GetVideoQuality(videoPlayer.source.ToString());
            Debug.Log("Download Time: " + segmentDownloadTime + " seconds, Video Quality: " + videoQuality + " kbps");
        };
    }

    float GetVideoQuality(string representationId)
    {
        // Retrieve video quality (bitrate) based on representationId
        if (lowQualityVideoQualities.ContainsKey(representationId))
        {
            return lowQualityVideoQualities[representationId];
        }
        else if (highQualityVideoQualities.ContainsKey(representationId))
        {
            return highQualityVideoQualities[representationId];
        }
        else
        {
            return 0f;
        }
    }
}