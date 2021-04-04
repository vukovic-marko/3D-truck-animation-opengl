// Marko Vukovic
// RA200-2015

using System;
using System.Collections;
using Assimp;
using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Core;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace Transformations
{
    ///<summary> Klasa koja enkapsulira OpenGL programski kod </summary>
    class World
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;
        private AssimpScene m_scene2;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 1000.0f;
        
        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 10.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        private float visinaBandere = 250.0f;

        private int animationSpeed = 50;

        private float skaliranjeBandere = 1.0f;

        private uint[] m_textures = null;

        private string[] m_textureFiles = { "..//..//Textures//asphalt3.bmp", "..//..//Textures//metal1.bmp" };
        private readonly int m_textureCount = 2;

        private bool animationActive = false;

        private float truckPosition = -500.0f;
        private float dumpsterAngle = 0.0f;
        private bool dumpsterRotating = false;
        private bool frontRotation = true;
        public float dumpsterHeight = 0.0f;

        private DispatcherTimer timer1;
        private DispatcherTimer timer2;


        #endregion

        #region Properties

        public bool AnimationActive
        {
            get { return animationActive; }
            set { animationActive = value; }
        }

        public float VisinaBandere
        {
            get { return visinaBandere; }
            set { visinaBandere = value; }
        }

        public int AnimationSpeed
        {
            get { return animationSpeed; }
            set
            {
                animationSpeed = value;
                if (timer1 != null && timer2 != null)
                {
                    timer1.Interval = TimeSpan.FromMilliseconds(animationSpeed);
                    timer2.Interval = TimeSpan.FromMilliseconds(animationSpeed);
                }
            }
        }

        public float SkaliranjeBandere
        {
            get { return skaliranjeBandere; }
            set { skaliranjeBandere = value; }
        }

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        public AssimpScene Scene2
        {
            get { return m_scene2; }
            set { m_scene2 = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///		Konstruktor opengl sveta.
        /// </summary>
        public World(String scenePath, String sceneFileName1, String sceneFileName2, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName1, gl);
            this.m_scene2 = new AssimpScene(scenePath, sceneFileName2, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion

        #region Metode

        /// <summary>
        /// Korisnicka inicijalizacija i podesavanje OpenGL parametara
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            SetupLighting(gl);  
            m_scene.LoadScene();
            m_scene.Initialize();
            m_scene2.LoadScene();
            m_scene2.Initialize();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            //gl.TexParameter(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; i++)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT); 
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }

            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(AnimationSpeed);
            timer1.Tick += new EventHandler(UpdateAnimation1);

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(AnimationSpeed);
            timer2.Tick += new EventHandler(UpdateAnimation2);
        }

        public void StartAnimation()
        {
            timer1.Start();
            timer2.Start();

            animationActive = true;
        }

        public void UpdateAnimation1(object sender, EventArgs e)
        {
            if (truckPosition < 0.0f)
                truckPosition += 5.0f;
            else if (truckPosition == 0)
            {
                dumpsterRotating = true;
            }
            else if (truckPosition >= 5.0f && truckPosition < 500.0f && !dumpsterRotating)
                truckPosition += 5.0f;
            else if (truckPosition <= 500.0f)
                truckPosition = 500.0f;
            else if (truckPosition > 500.0f)
            {
                timer1.Stop();
                timer2.Stop();
                animationActive = false;

                truckPosition = -500.0f;
                dumpsterAngle = 0.0f;
                frontRotation = true;
                dumpsterRotating = false;
            }
        }

        public void UpdateAnimation2(object sender, EventArgs e)
        {
            if (dumpsterAngle < 90.0f && dumpsterRotating && frontRotation)
            {
                dumpsterHeight = 100.0f;
                dumpsterAngle += 1.0f;
            }
            else if (dumpsterAngle > 0.0f && dumpsterRotating && !frontRotation)
            {
                dumpsterHeight = 100.0f;
                dumpsterAngle -= 1.0f;
            }

            if (dumpsterAngle == 0.0f)
            {
                dumpsterRotating = false;
                dumpsterHeight = 0.0f;
                truckPosition += 5.0f;
            }

            if (dumpsterAngle == 90.0f)
            {
                frontRotation = false;
            }
        }

        /// <summary>
        /// Podesavanje osvetljenja. Više informacija u nastavku kursa, za sada samo koristiti
        /// </summary>
        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GLU_SMOOTH);

            gl.Enable(OpenGL.GL_LIGHTING);

            float[] ambijentalnaKomponenta = { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] difuznaKomponenta = { 0.9f, 0.9f, 0.9f, 1.0f };
            float[] spekularnaKomponenta = { 1.0f, 1.0f, 1.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, spekularnaKomponenta);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            float[] smer = { 0.0f, -1.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_DIRECTION, smer);

            gl.Enable(OpenGL.GL_LIGHT0);
            
            

            gl.Enable(OpenGL.GL_NORMALIZE);
            
            float[] ambijentalnaKomponenta1 = { 0.1f, 0.1f, 0.0f, 1.0f };
            float[] difuznaKomponenta1 = { 1.0f, 1.0f, 0.0f, 1.0f };
            float[] spekularnaKomponenta1 = { 1.0f, 1.0f, 0.0f, 1.0f };
            //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuznaKomponenta1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, spekularnaKomponenta1);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);

            //float[] smer1 = { 0.0f, -1.0f, 0.0f };
            //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer1);

            gl.Enable(OpenGL.GL_LIGHT1);
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LookAt(0.0f, 20.0f, -100.0f, 0.0f, 0.0f, -1500.0f, 0.0f, 1.0f, 0.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.LoadIdentity();

            gl.Viewport(0, 0, m_width, m_height);


            //  Move the geometry into a fairly central position.
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);

            if (m_xRotation >= 0.0f && m_xRotation <= 90.0f)
            {
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            }
            else if (m_xRotation < 0.0f)
            {
                m_xRotation = 0.0f;
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            }
            else if (m_xRotation > 90.0f)
            {
                m_xRotation = 90.0f;
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            }
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.PushMatrix();

            // ----------------------------------------------
            // PODLOGA, KAMION i KONTEJNER
            // ----------------------------------------------


            gl.TexParameter(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[0]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            gl.Color(0.7f, 0.7f, 0.7f);

            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-1000f, 0f, -1000f);
            gl.TexCoord(0.0f, 8.0f);
            gl.Vertex(-1000f, 0f, 1000f);
            gl.TexCoord(8.0f, 8.0f);
            gl.Vertex(1000f, 0f, 1000f);
            gl.TexCoord(8.0f, 0.0f);
            gl.Vertex(1000f, 0f, -1000f);

            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            gl.PopMatrix();
            gl.PushMatrix();

            gl.Translate(0.0f, 0.0f, truckPosition);
            m_scene.Draw();
            gl.Translate(0.0f, 0.0f, -truckPosition);

            gl.Translate(300.0f, 0.0f, 0.0f);

            float[] pozicija1 = { 0.0f, 350.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija1);

            float[] smer1 = { 0.0f, -1.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer1);

            gl.Rotate(270.0f, 0.0f, 1.0f, 0.0f);


            gl.Translate(0.0f, dumpsterHeight, 0.0f);
            gl.Rotate(dumpsterAngle, 0.0f, 0.0f);
            m_scene2.Draw();
            gl.Rotate(-dumpsterAngle, 0.0f, 0.0f);
            gl.Translate(0.0f, -dumpsterHeight, 0.0f);

            

            // ----------------------------------------------------------
            // ZIDOVI OKO KONTEJNERA
            // ----------------------------------------------------------

            gl.PopMatrix();
            gl.PushMatrix();

            gl.Translate(425.0f, 100.0f, 0.0f);

            gl.Scale(10.0f, 100.0f, 125.0f);
            Cube zadnjiZid = new Cube();
            zadnjiZid.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(325.0f, 100.0f, 125.0f);
            gl.Scale(100.0f, 100.0f, 10.0f);
            Cube leviZid = new Cube();
            leviZid.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(325.0f, 100.0f, -125.0f);
            gl.Scale(100.0f, 100.0f, 10.0f);
            Cube desniZid = new Cube();
            desniZid.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);


            // ---------------------------------------------------------
            // SVETILJKA
            // ---------------------------------------------------------

            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(250.0f, VisinaBandere*SkaliranjeBandere, 300.0f);
            gl.Rotate(90.0f, 1.0f, 0.0f, 0.0f);



            gl.Color(0.7f, 0.7f, 0.7f);
            gl.TexParameter(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[1]);
            Cylinder cil = new Cylinder();
            cil.TextureCoords = true;
            cil.Height = VisinaBandere;
            cil.BaseRadius = 5.0f;
            cil.TopRadius = cil.BaseRadius;

            gl.Scale(skaliranjeBandere, skaliranjeBandere, skaliranjeBandere);

            cil.CreateInContext(gl);

            cil.NormalGeneration = Normals.Smooth;
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            

            gl.Translate(-15.0f, 0.0f, 0.0f);

            float[] pozicija = { 0.0f, 0.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicija);

            float[] smer = { 0.0f, -1.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_DIRECTION, smer);

            gl.Scale(15.0f, 15.0f, 5.0f);
            gl.TexParameter(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_REPLACE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[1]);
            Cube cube = new Cube();
            cube.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            gl.PopMatrix();

            // ---------------------------------------------------------
            // TEKST
            // ---------------------------------------------------------

            float aspect = (float)m_width / (float)m_height;

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);
            gl.Ortho2D(0.0, 9.0 * aspect, 0.0, 9.0);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.PushMatrix();
            gl.Color(1.0f, 1.0f, 0.0f);
            gl.Translate(0.0f, 5.0f, 0.0f);
            gl.DrawText3D("Helvetica Bold", 14.0f, 1.0f, 0.1f, "Predmet: Racunarska grafika");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 4.0f, 0.0f);
            gl.DrawText3D("Helvetica Bold", 14.0f, 1.0f, 0.1f, "Sk.god: 2018/19.");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 3.0f, 0.0f);
            gl.DrawText3D("Helvetica Bold", 14.0f, 1.0f, 0.1f, "Ime: Marko");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 2.0f, 0.0f);
            gl.DrawText3D("Helvetica Bold", 14.0f, 1.0f, 0.1f, "Prezime: Vukovic");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0.0f, 1.0f, 0.0f);
            gl.DrawText3D("Helvetica Bold", 14.0f, 1.0f, 0.1f, "Sifra zad: 1.1");
            gl.PopMatrix();

            gl.FrontFace(OpenGL.GL_CCW);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Viewport(0, 0, m_width, m_height);
            gl.Perspective(45f, (double)m_width / m_height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            // Oznaci kraj iscrtavanja
            gl.Flush();


        }

        #endregion

        #region IDisposable Metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion
    }
}
