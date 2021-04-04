using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using System.Reflection;

namespace Transformations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///  Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            
            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Models"), "truck.3DS", "dumpster.3DS", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }

            visinaBandereSlider.Value = m_world.VisinaBandere;
            brzinaAnimacijeSlider.Value = m_world.AnimationSpeed;
            skaliranjeBandereSlider.Value = m_world.SkaliranjeBandere;
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //Iscrtaj svet
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_world.AnimationActive)
            {
                switch (e.Key)
                {
                    case Key.W: m_world.RotationX -= 5.0f; break;
                    case Key.S: m_world.RotationX += 5.0f; break;
                    case Key.A: m_world.RotationY -= 5.0f; break;
                    case Key.D: m_world.RotationY += 5.0f; break;
                    case Key.Add: m_world.SceneDistance -= 100.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 100.0f; break;
                    case Key.C: m_world.StartAnimation(); break;
                }

                switch (e.SystemKey)
                {
                    case Key.F10: this.Close(); break;
                }
            }
        }

        private void VisinaBandereSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                if (!m_world.AnimationActive)
                    m_world.VisinaBandere = (float)e.NewValue;
                else
                    visinaBandereSlider.Value = m_world.VisinaBandere;
                openGLControl.Focus();
                
            }
        }

        private void BrzinaAnimacijeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                if (!m_world.AnimationActive)
                    m_world.AnimationSpeed = (int)e.NewValue;
                else
                    brzinaAnimacijeSlider.Value = m_world.AnimationSpeed;
                openGLControl.Focus();
            }
        }

        private void SkaliranjeBandereSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                if (!m_world.AnimationActive)
                    m_world.SkaliranjeBandere = (int)e.NewValue;
                else
                    skaliranjeBandereSlider.Value = m_world.SkaliranjeBandere;
                openGLControl.Focus();
            }
        }
    }
}
