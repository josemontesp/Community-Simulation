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

namespace T5_Jose_Montes
{
    /// <summary>
    /// Interaction logic for Criatura.xaml
    /// </summary>
    public partial class Criatura : UserControl
    {
        public int id;
        public string tipo;

        public Criatura(int _id, string _tipo, double X, double Y)
        {
            InitializeComponent();
            this.id = _id;
            this.tipo = _tipo;
            
            var converter = new ImageSourceConverter();
            var eruditoSource = (ImageSource)converter.ConvertFromString(@"..\..\Imagenes\EruditoFadic.png");
            var soldadoSource = (ImageSource)converter.ConvertFromString(@"..\..\Imagenes\SoldadoZodto.png");
            var ermitanoSource = (ImageSource)converter.ConvertFromString(@"..\..\Imagenes\ermitanio.png");
            var elegidoSource = (ImageSource)converter.ConvertFromString(@"..\..\Imagenes\Elegido.png");

            if (tipo == "soldado")
            {
                icono.Source = soldadoSource;
            }
            else if (tipo == "erudito")
            {
                icono.Source = eruditoSource;
            }
            else if (tipo == "ermitano")
            {
                icono.Source = ermitanoSource;
            }
            else if (tipo == "elegido")
            {
                icono.Source = elegidoSource;
            }
            Canvas.SetLeft(this, X);
            Canvas.SetTop(this, Y);
        }
    }
}
