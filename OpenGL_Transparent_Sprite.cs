using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

public class AppWindow : GameWindow {
    int texture_reticle;
    int texture_ship;
    float x, y;

    const int TILE_W = 32;
    const int TILE_H = 32;

    public AppWindow() {
        this.Title        = "OpenTK App 2";
        this.WindowBorder = WindowBorder.Fixed;
        this.ClientSize   = new Size(800, 600);
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
        Check_for_GL_error("After calling GL.Hint()");

        this.texture_reticle = Load_Texture(@"bright_marquee.frame_1.png", 3, 3);
        this.texture_ship    = Load_Texture(@"U4.B_enhanced-32x32.png",    2, 1);

        this.x = 1 + TILE_W/2;
        this.y = 1 + TILE_H/2;
    } // OnLoad()

    protected int Load_Texture(string filename, int xx, int yy) {
        int texture_id;

        Bitmap bitmap = new Bitmap(filename);
        bitmap.MakeTransparent(Color.Magenta);

        int px_x = (xx * TILE_W);
        int px_y = (yy * TILE_H);

        GL.GenTextures(1, out texture_id);
        GL.BindTexture(TextureTarget.Texture2D, texture_id);
        Check_for_GL_error("After calling GL.BindTexture()");

        Rectangle  rect = new Rectangle(px_x, px_y, TILE_W, TILE_H);
        BitmapData data = bitmap.LockBits(rect,
                                          ImageLockMode.ReadOnly, 
                                          //System.Drawing.Imaging.PixelFormat.Format32bppPArgb);  // With this line uncommented, it works on XP and Ubuntu
                                          //System.Drawing.Imaging.PixelFormat.Format32bppRgb);  // This value worked on Windows 7, but on XP I see black pixels rather than transparent
                                          System.Drawing.Imaging.PixelFormat.Format32bppArgb);  // With this line uncommented, I get "scrambled" results on XP
        // Format32bppArgb was indicated by OpenTK.com user "Inertia", citing http://msdn.microsoft.com/en-us/library/8517ckds.aspx
        // However, rather than transparency, I observe a strange display (sending a screen shot)

        GL.TexImage2D(
            OpenTK.Graphics.OpenGL.TextureTarget.Texture2D,   // texture_target,
            0,                                                // level,
            OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba,  // internal_format
            data.Width, data.Height,                          // width, height, 
            0,                                                // border,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,          // pixel_format
            OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,    // pixel_type
            data.Scan0                                        // pixels
            );
        Check_for_GL_error("After calling GL.TexImage2D()");

        bitmap.UnlockBits(data);
        bitmap.Dispose();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

        return texture_id;
    } // Load_Texture()

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

        int origin_x = 1 + TILE_W;
        int origin_y = 1 + TILE_H;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();

        GL.BindTexture(TextureTarget.Texture2D, this.texture_ship);
        Check_for_GL_error("In OnRenderFrame(), after 1st call to GL.BindTexture()");

        GL.Begin(BeginMode.Quads);
        {
            GL.TexCoord2(0, 0);  GL.Vertex2(origin_x - TILE_W/2, origin_y - TILE_H/2);
            GL.TexCoord2(1, 0);  GL.Vertex2(origin_x + TILE_W/2, origin_y - TILE_H/2);
            GL.TexCoord2(1, 1);  GL.Vertex2(origin_x + TILE_W/2, origin_y + TILE_H/2);
            GL.TexCoord2(0, 1);  GL.Vertex2(origin_x - TILE_W/2, origin_y + TILE_H/2);
        }
        GL.End();

        GL.BindTexture(TextureTarget.Texture2D, this.texture_reticle);
        Check_for_GL_error("In OnRenderFrame(), after 2nd call to GL.BindTexture()");
        GL.Begin(BeginMode.Quads);
        {
            GL.TexCoord2(0, 0);  GL.Vertex2(this.x - TILE_W/2, this.y - TILE_H/2);
            GL.TexCoord2(1, 0);  GL.Vertex2(this.x + TILE_W/2, this.y - TILE_H/2);
            GL.TexCoord2(1, 1);  GL.Vertex2(this.x + TILE_W/2, this.y + TILE_H/2);
            GL.TexCoord2(0, 1);  GL.Vertex2(this.x - TILE_W/2, this.y + TILE_H/2);
        }
        GL.End();

        this.SwapBuffers();
    }

    public static void Check_for_GL_error(string msg) {
        ErrorCode err = GL.GetError();
        if (err != ErrorCode.NoError) {
            string str = String.Format("{0} GL.GetError() returns: {1}", msg, err);
            throw new Exception(str);
        }
    } // Check_for_GL_error()

    [STAThread]
    public static void Main() {
        AppWindow window = new AppWindow();
        window.Run(60);
    }

} // class
