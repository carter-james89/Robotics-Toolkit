using TelloLib;
using UnityEngine;

namespace  QuadcopterUtilities
{
    /// <summary>
    /// Controller which controls the Game UI, and displays the Tello's video feed to a texture
    /// </summary>
    public class TelloVideoFeed : MonoBehaviour
    {
        public enum VideoBitRate
        {
            // VideoBitRateAuto sets the bitrate for streaming video to auto-adjust.
            VideoBitRateAuto = 0,

            // VideoBitRate1M sets the bitrate for streaming video to 1 Mb/s.
            VideoBitRate1M = 1,

            // VideoBitRate15M sets the bitrate for streaming video to 1.5 Mb/s
            VideoBitRate15M = 2,

            // VideoBitRate2M sets the bitrate for streaming video to 2 Mb/s.
            VideoBitRate2M = 3,

            // VideoBitRate3M sets the bitrate for streaming video to 3 Mb/s.
            VideoBitRate3M = 4,

            // VideoBitRate4M sets the bitrate for streaming video to 4 Mb/s.
            VideoBitRate4M = 5,

        };
        /// <summary>
        /// The texture to pass the video data to
        /// </summary>
        [SerializeField]
        private TelloVideoTexture telloVideoTexture;

        /// <summary>
        /// Initialize the vido feed to listen for <see cref="Tello.onVideoData"/>
        /// </summary>
        /// <param name="tello">The Quadcopter to get video from</param>
        public void InitializeFeed()
        {
            Debug.Log("Initialize video feed");
            Tello.onVideoData += Tello_onVideoData;
        }
        /// <summary>
        /// Called from <see cref="Tello.onVideoData"/>
        /// </summary>
        /// <param name="data"></param>
        private void Tello_onVideoData(byte[] data)
        {
            if (telloVideoTexture != null)
            {
                telloVideoTexture.PutVideoData(data);
            }
            else
            {
                Debug.LogWarning("Recieving video, but telloVideoTexture is null, assign in inspector");
            }
        }

        private void OnDestroy()
        {
            Tello.onVideoData -= Tello_onVideoData;
        }
    } 
}
