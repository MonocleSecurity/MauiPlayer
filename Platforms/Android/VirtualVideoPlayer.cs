using Android.Graphics;
using Android.Media;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using AndroidX.ConstraintLayout.Helper.Widget;
using Java.Nio;
using Java.Text;
using System.Runtime.InteropServices;
using static Android.Telephony.CarrierConfigManager;
using static AndroidX.Core.Content.PM.ShortcutInfoCompat;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace MauiPlayer;

public class VirtualVideoPlayer : TextureView, TextureView.ISurfaceTextureListener
{
    private byte[] H26X_START_SEQUENCE = new byte[] { 0x00, 0x00, 0x00, 0x01 };
    private VideoPlayer video_player;
    public MediaCodec? media_codec;

    public VirtualVideoPlayer(Android.Content.Context context, VideoPlayer video_player) : base(context)
    {
        this.video_player = video_player;

        SurfaceTextureListener = this;
        if (IsAvailable && (SurfaceTexture != null))
        {
            OnSurfaceTextureAvailable(SurfaceTexture, Width, Height);
        }
    }

    public void OnSurfaceTextureAvailable(SurfaceTexture surface_texture, int width, int height)
    {
        Surface surface = new Surface(surface_texture);
        media_codec = MediaCodec.CreateDecoderByType("video/avc");
        MediaFormat format = MediaFormat.CreateVideoFormat("video/avc", 2560, 1440);
        format.SetByteBuffer("csd-0", ByteBuffer.Wrap(CombineArrays(H26X_START_SEQUENCE, video_player.sps)));
        format.SetByteBuffer("csd-1", ByteBuffer.Wrap(CombineArrays(H26X_START_SEQUENCE, video_player.pps)));
        media_codec.Configure(format, surface, null, MediaCodecConfigFlags.None);
        media_codec.Start();
    }

    public bool OnSurfaceTextureDestroyed(SurfaceTexture surface_texture)
    {
        return true;
    }

    public void OnSurfaceTextureSizeChanged(SurfaceTexture surface_texture, int width, int height)
    {
        // Ignore
    }

    public void OnSurfaceTextureUpdated(SurfaceTexture surface_texture)
    {
        // Ignore
    }

    public bool IsReady()
    {
        return (media_codec != null);
    }

    public void SendPacket(byte[] data)
    {
        if (media_codec == null)
        {
            return;
        }
        // Pass input buffers
        int input_index = media_codec.DequeueInputBuffer(0);
        if (input_index >= 0)
        {
            ByteBuffer? buffer = media_codec.GetInputBuffer(input_index);
            if (buffer != null)
            {
                buffer.Clear();
                byte[] d = CombineArrays(H26X_START_SEQUENCE, data);
                buffer.Put(d);
                media_codec.QueueInputBuffer(input_index, 0, d.Length, 0, MediaCodecBufferFlags.None);
            }
        }
        // Release output buffers
        while (true)
        {
            MediaCodec.BufferInfo buffer_info = new MediaCodec.BufferInfo();
            int output_index = media_codec.DequeueOutputBuffer(buffer_info, 0);
            if (output_index >= 0)
            {
                media_codec.ReleaseOutputBuffer(output_index, true);
                if ((buffer_info.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
    public static T[] CombineArrays<T>(T[] a1, T[] a2)
    {
        T[] array_combined = new T[a1.Length + a2.Length];
        Array.Copy(a1, 0, array_combined, 0, a1.Length);
        Array.Copy(a2, 0, array_combined, a1.Length, a2.Length);
        return array_combined;
    }

}
