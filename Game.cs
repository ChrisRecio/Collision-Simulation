﻿using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Collision_Simulation
{
    internal class Game : GameWindow
    {
        private int shaderProgramHandle;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;

        private int vertexCount, indexCount;

        public Game(int width = 1280, int height = 768, String title = "Collision Simmulation") : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                Size = new Vector2i(width, height),
                WindowBorder = WindowBorder.Fixed,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)

            })
        {
            this.CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {

            this.IsVisible = true;

            // Background Color
            GL.ClearColor(new Color4(0.5f, 0.5f, 0.5f, 1.0f));

            //pixelColor = vec4(1.0f, 0.5f, 0.31f, 1.0f); ORANGE

            Random rand = new Random();
            int boxCount = 10;
            int windowWidth = this.ClientSize.X;
            int windowHeight = this.ClientSize.Y;

            VertexPositionColor[] vertices = new VertexPositionColor[boxCount * 4];
            this.vertexCount = 0;

            for(int i = 0; i < boxCount; i++)
            {
                int w = rand.Next(32, 128);
                int h = rand.Next(32, 128);
                int x = rand.Next(0, windowWidth - w);
                int y = rand.Next(0, windowHeight - h);

                float r = (float)rand.NextDouble();
                float g = (float)rand.NextDouble();
                float b = (float)rand.NextDouble();


                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x, y + h), new Color4(r, g, b, 1.0f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + w, y + h), new Color4(r, g, b, 1.0f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + w, y), new Color4(r, g, b, 1.0f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x, y), new Color4(r, g, b, 1.0f));
            }


            int[] indices = new int[boxCount * 6];
            this.indexCount = 0;
            this.vertexCount = 0;

            for (int i = 0; i < boxCount; i++)
            {
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 1 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 3 + this.vertexCount;

                this.vertexCount += 4;
            }

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);

            //------------------------------------------------------------------------------------

            // Compile Shaders
            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, File.ReadAllText("../../../assets/vertexShader.glsl"));
            GL.CompileShader(vertexShaderHandle);

            // VertexShader Error Log
            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if(vertexShaderInfo != String.Empty)
            {
                Console.WriteLine("vertexShaderHandle Info");
                Console.WriteLine(vertexShaderInfo);
            }


            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, File.ReadAllText("../../../assets/fragmentShader.glsl"));
            GL.CompileShader(fragmentShaderHandle);

            // FragmentShader Error Log
            string fragmentShaderInfo = GL.GetShaderInfoLog(fragmentShaderHandle);
            if (fragmentShaderInfo != String.Empty)
            {
                Console.WriteLine("fragmentShaderHandle Info");
                Console.WriteLine(fragmentShaderInfo);
            }

            this.shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(this.shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(this.shaderProgramHandle, fragmentShaderHandle);

            GL.LinkProgram(this.shaderProgramHandle);

            GL.DetachShader(this.shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(this.shaderProgramHandle, fragmentShaderHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            int[] viewport = new int[4]; // x, y, Width, Height
            GL.GetInteger(GetPName.Viewport, viewport);

            GL.UseProgram(this.shaderProgramHandle);
            int viewportSizeUniformLocation = GL.GetUniformLocation(this.shaderProgramHandle, "viewportSize");
            GL.Uniform2(viewportSizeUniformLocation, (float)viewport[2], (float)viewport[3]);
            GL.UseProgram(0);

            // ShaderProgram Error Log
            string shaderProgramInfo = GL.GetShaderInfoLog(shaderProgramHandle);
            if (shaderProgramInfo != String.Empty)
            {
                Console.WriteLine("shaderProgramHandle Info");
                Console.WriteLine(shaderProgramInfo);
            }


            base.OnLoad();
        }

        protected override void OnUnload()
        {
            this.vertexBuffer?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexArray?.Dispose();

            GL.UseProgram(0);
            GL.DeleteProgram(this.shaderProgramHandle);

            base.OnUnload();
        }

        // Called per frame update
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        // Initial frame render
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit); // Clear screen

            GL.UseProgram(this.shaderProgramHandle);
            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle);
            GL.DrawElements(PrimitiveType.Triangles, this.indexCount, DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

    }
}
