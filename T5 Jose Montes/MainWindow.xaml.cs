using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BackEnd;

namespace T5_Jose_Montes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<int, Criatura> Criaturas = new Dictionary<int, Criatura>();

        public Random randy = new Random();
        public Simulador sim = new Simulador();
        

        public MainWindow(int segundos)
        {
            InitializeComponent();



            sim.MoverElementoFront += moverElementoFrontEnd;
            sim.CrearElementoFront += crearElementoFront;
            sim.EliminarElementoFront += eliminarElementoFront;

            sim.crearCriaturasIniciales();

            this.Closed += MainWindow_Closed;
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            //this.Deactivated += MainWindow_Closed;
        }

        void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MyCanvas);
            sim.crearErmitano(pos.X, pos.Y);
        }

        //Parar la ejecución cuando se cierra la ventana
        void MainWindow_Closed(object sender, EventArgs e) 
        {
            Environment.Exit(0);
        }

        #region Mover
        public void moverElementoFrontEnd(CriaturaBack criaturaBack)
        {
            Action<CriaturaBack> delegateAlDispatcher = new Action<CriaturaBack>(moverElementoDelegate);
            Dispatcher.BeginInvoke(delegateAlDispatcher, criaturaBack);
        }

        public void moverElementoDelegate(CriaturaBack criaturaBack)
        {
            try
            {
                Criatura criatura = Criaturas[criaturaBack.id];
                Canvas.SetLeft(criatura, criaturaBack.CanvasPosX);
                Canvas.SetTop(criatura, criaturaBack.CanvasPosY);
            }
            catch
            {
                Console.Write("id no encontrado");
            }
        }
        #endregion

        #region Crear
        public void crearElementoFront(CriaturaBack criaturaBack)
        {
            Action<CriaturaBack> delegateAlDispatcher = new Action<CriaturaBack>(crearElementoDelegate);
            Dispatcher.BeginInvoke(delegateAlDispatcher, criaturaBack);
        }

        public void crearElementoDelegate(CriaturaBack criaturaBack)
        {
            Criatura criatura = new Criatura(criaturaBack.id, criaturaBack.tipo, criaturaBack.CanvasPosX, criaturaBack.CanvasPosY);
            Criaturas.Add(criatura.id, criatura);
            MyCanvas.Children.Add(criatura);
        }
        #endregion

        #region Eliminar
        public void eliminarElementoFront(CriaturaBack criaturaBack)
        {
            Action<CriaturaBack> delegateAlDispatcher = new Action<CriaturaBack>(eliminarElementoDelegate);
            Dispatcher.BeginInvoke(delegateAlDispatcher, criaturaBack);
        }

        public void eliminarElementoDelegate(CriaturaBack criaturaBack)
        {
            try
            {
                Criatura porEliminar = Criaturas[criaturaBack.id];
                Criaturas.Remove(criaturaBack.id);
                MyCanvas.Children.Remove(porEliminar);
            }
            catch
            {
                Console.Write("id no encontrado");
            }
        }
        #endregion

        
    }
}
//foreach (KeyValuePair<int,Criatura> kv in Eruditos)
