using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;
using UIKit;

namespace MauiPlayer;

public class VirtualVideoPlayer : UIView
{
    VideoPlayer video_player;
    AVSampleBufferDisplayLayer display_layer;
    CMVideoFormatDescription? format_descriptor;

    public VirtualVideoPlayer(VideoPlayer video_player)
    {
        this.video_player = video_player;
        // Create the display player
        display_layer = new AVSampleBufferDisplayLayer();
        display_layer.VideoGravity = "AVLayerVideoGravityResize";
        display_layer.Frame = new CGRect(new CGPoint(0, 0), new CGSize(320, 240));
        Layer.AddSublayer(display_layer);
        // Create the decoder
        List<byte[]> format_parameters = new List<byte[]>();
        format_parameters.Add(video_player.sps);
        format_parameters.Add(video_player.pps);
        CMFormatDescriptionError format_description_error;
        format_descriptor = CMVideoFormatDescription.FromH264ParameterSets(format_parameters, sizeof(UInt32), out format_description_error);
    }

    public void SendPacket(byte[] data)
    {
        // Add the size prefix
        MemoryStream? mem = new MemoryStream();
        BinaryWriter? buffer = new BinaryWriter(mem);
        buffer.Write(BitConverter.GetBytes((UInt32)data.Length).Reverse().ToArray(), 0, sizeof(UInt32));
        buffer.Write(data, 0, data.Length);
        buffer.Flush();
        byte[] array = mem.ToArray();
        // Build the block for the decoder
        CMBlockBuffer block_buffer;
        unsafe
        {
            fixed (byte* p = array)
            {
                IntPtr ptr = (IntPtr)p;
                CMBlockBufferError block_buffer_error;
                block_buffer = CMBlockBuffer.FromMemoryBlock(ptr, (nuint)array.Length, null, 0, (nuint)array.Length, CMBlockBufferFlags.AlwaysCopyData, out block_buffer_error)!;
            }
        }
        // Send it
        CMSampleBufferError sample_buffer_error;
        CMSampleBuffer sample_buffer = CMSampleBuffer.CreateReady(block_buffer, format_descriptor, 1, null, new nuint[] { (nuint)array.Length }, out sample_buffer_error)!;
        CMSampleBufferAttachmentSettings?[] settings = sample_buffer.GetSampleAttachments(true);
        if ((settings != null) && (settings.Length > 0) && (settings[0] != null))
        {
            settings[0]!.DisplayImmediately = true;
        }
        if (display_layer.ReadyForMoreMediaData == true)
        {
            display_layer.Enqueue(sample_buffer);
        }
    }
}
