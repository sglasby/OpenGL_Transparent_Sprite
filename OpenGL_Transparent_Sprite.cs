using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class AppWindow : GameWindow {
    int texture;
    float x, y;

    public AppWindow() {
        this.Title = "OpenTK App 2";
        this.WindowBorder = WindowBorder.Fixed;
        this.ClientSize = new Size(800, 600);
    }

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);

        GL.ClearColor(Color.CornflowerBlue);
        GL.Ortho(0, 800, 600, 0, -1, 1);
        GL.Viewport(0, 0, 800, 600);

        GL.Enable(EnableCap.Texture2D);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

        GL.GenTextures(1, out this.texture);
        GL.BindTexture(TextureTarget.Texture2D, this.texture);

        //Bitmap bitmap = new Bitmap("bright_marquee.frame_1.png");  // Transparency works on this image -- my transparency problem is NOT with this image file
        Bitmap bitmap = new Bitmap("frame1.png");  // Transparency works on this image
        bitmap.MakeTransparent(Color.Magenta);

        Rectangle  rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData data = bitmap.LockBits(rect,
                                          ImageLockMode.ReadOnly, 
                                          System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        GL.TexImage2D(
            OpenTK.Graphics.OpenGL.TextureTarget.Texture2D, 0,         // texture_target, level
            OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba, data.Width, data.Height, 0,  // internal_format, width, height, border
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,                   // pixel_format
            OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, data.Scan0  // pixel_type, pixels
            );

        bitmap.UnlockBits(data);
        bitmap.Dispose();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

        this.x = 40f;
        this.y = 41.5f;
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        float dx = 0.0f;
        float dy = 0.0f;

        if (this.Keyboard[Key.Left])
            dx = -1.0f;
        else if (this.Keyboard[Key.Right])
            dx = 1.0f;

        if (this.Keyboard[Key.Up])
            dy = -1.0f;
        else if (this.Keyboard[Key.Down])
            dy = 1.0f;

        this.x += 200.0f * dx * (float) e.Time;
        this.y += 200.0f * dy * (float) e.Time;
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();

        GL.Begin(BeginMode.Quads);

        GL.TexCoord2(0, 0);  GL.Vertex2(200 - 64.0f, 200 - 64.0f);
        GL.TexCoord2(1, 0);  GL.Vertex2(200 + 64.0f, 200 - 64.0f);
        GL.TexCoord2(1, 1);  GL.Vertex2(200 + 64.0f, 200 + 64.0f);
        GL.TexCoord2(0, 1);  GL.Vertex2(200 - 64.0f, 200 + 64.0f);

        GL.TexCoord2(0, 0);  GL.Vertex2(this.x - 64.0f, this.y - 64.0f);
        GL.TexCoord2(1, 0);  GL.Vertex2(this.x + 64.0f, this.y - 64.0f);
        GL.TexCoord2(1, 1);  GL.Vertex2(this.x + 64.0f, this.y + 64.0f);
        GL.TexCoord2(0, 1);  GL.Vertex2(this.x - 64.0f, this.y + 64.0f);

        GL.End();

        //GL.Flush();  // Not useful, according to one Stack Overflow OpenGL person
        this.SwapBuffers();
    }

    [STAThread]
    public static void Main() {
        AppWindow window = new AppWindow();
        window.Run(60);
    }
} // class
