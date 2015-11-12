using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BackEnd
{
    public delegate double newDirDelegate(CriaturaBack criatura);

    public class Simulador
    {
        public event Action<CriaturaBack> MoverElementoFront;
        public event Action<CriaturaBack> CrearElementoFront;
        public event Action<CriaturaBack> EliminarElementoFront;
        public newDirDelegate ndDelegate;

        public Dictionary<int, CriaturaBack> soldados = new Dictionary<int, CriaturaBack>();
        public Dictionary<int, CriaturaBack> eruditos = new Dictionary<int,CriaturaBack>();
        public Dictionary<int, CriaturaBack> ermitanos = new Dictionary<int, CriaturaBack>();
        public CriaturaBack elegido = null;
        public static Random randy = new Random();

        public double radio = 50;
        public int idMaximo = -1;

        protected Thread CreacionesConTiempo;

        public object lockObject = new object();

        public void crearCriaturasIniciales()
        {
            CreacionesConTiempo = new Thread(crearPorTiempo);
            CreacionesConTiempo.Start();
            ndDelegate = new newDirDelegate(Direccion);
            int cantidadSoldados = randy.Next(4, 8);
            int cantidadEruditos = randy.Next(4, 8);
            cantidadEruditos = 10;
            cantidadSoldados = 10;
            int id;
            for (int i = 0; i < cantidadEruditos; i++)
            {
                id = nuevoId();
                var posx = randy.Next(0, 1000);
                var posy = randy.Next(0, 680);
                while ((posx > 720 && posy > 550) || (posx > 700 && posy < 187))
                {
                    posx = randy.Next(0, 1000);
                    posy = randy.Next(0, 680);
                }
                var criatura = new CriaturaBack(id, posx, posy, "erudito", randy);
                eruditos.Add(id, criatura);
                crearElementoFrontEnd(criatura);
                criatura.MoverElemento += moverElementoFrontEnd;
                criatura.ndDelegate = ndDelegate;
            }

            for (int i = 0; i < cantidadSoldados; i++)
            {
                id = nuevoId();
                var posx = randy.Next(0, 1000);
                var posy = randy.Next(0, 680);
                while ((posx < 346 && posy < 200) || (posx > 700 && posy < 187))
                {
                    posx = randy.Next(0, 1000);
                    posy = randy.Next(0, 680);
                }
                var criatura = new CriaturaBack(id, posx, posy, "soldado", randy);
                soldados.Add(id, criatura);
                crearElementoFrontEnd(criatura);
                criatura.MoverElemento += moverElementoFrontEnd;
                criatura.ndDelegate = ndDelegate;
            }

            foreach (KeyValuePair<int, CriaturaBack> kv in soldados)
            {
                kv.Value.IniciarMovimiento();
            }
            foreach (KeyValuePair<int, CriaturaBack> kv in eruditos)
            {
                kv.Value.IniciarMovimiento();
            }
        }

        public void crearPorTiempo()
        {
            while (true)
            {
                Thread.Sleep(60 * 1000);
                //SincronizarThreads();
                randy = new Random();
                lock (lockObject)
                {
                    #region Erudito
                    var id = nuevoId();
                    var posx = randy.Next(0, 8) * 32;
                    var posy = randy.Next(0, 4) + 32;
                    var criatura = new CriaturaBack(id, posx, posy, "erudito", randy);
                    eruditos.Add(id, criatura);
                    crearElementoFrontEnd(criatura);
                    criatura.MoverElemento += moverElementoFrontEnd;
                    criatura.ndDelegate = ndDelegate;
                    criatura.IniciarMovimiento();
                    #endregion

                    #region Soldado
                    id = nuevoId();
                    posx = randy.Next(22, 32) * 32;
                    posy = randy.Next(18, 22) * 32;
                    criatura = new CriaturaBack(id, posx, posy, "soldado", randy);
                    soldados.Add(id, criatura);
                    crearElementoFrontEnd(criatura);
                    criatura.MoverElemento += moverElementoFrontEnd;
                    criatura.ndDelegate = ndDelegate;
                    criatura.IniciarMovimiento();
                    #endregion
                    //ReiniciarMovimiento();
                }
            }
        }

        public void crearErmitano()
        {
            CriaturaBack ermitano = new CriaturaBack(nuevoId(), randy.Next(0, 1000), randy.Next(0, 680), "ermitano", randy);
            ermitanos.Add(ermitano.id, ermitano);
            ermitano.ndDelegate = ndDelegate;
            ermitano.MoverElemento += moverElementoFrontEnd;
            ermitano.IniciarMovimiento();
            crearElementoFrontEnd(ermitano);
        }

        public void crearErmitano(double X, double Y)
        {
            bool crear = true;
            if (X > 670 && Y < 180) //agua
                crear = false;
            else if (X > 675 && Y < 183)
                crear = false;
            if (X < 65 && Y > 219 && Y < 417) //arboles
                crear = false;
            if (crear)
            {
                CriaturaBack ermitano = new CriaturaBack(nuevoId(), X, Y, "ermitano", randy);
                ermitanos.Add(ermitano.id, ermitano);
                ermitano.ndDelegate = ndDelegate;
                ermitano.MoverElemento += moverElementoFrontEnd;
                ermitano.IniciarMovimiento();
                crearElementoFrontEnd(ermitano);
            }
        }

        public void moverElementoFrontEnd(CriaturaBack criatura)
        {
            if (MoverElementoFront != null)
            {
                MoverElementoFront(criatura);
            }
        }

        public void crearElementoFrontEnd(CriaturaBack criatura)
        {
            if (CrearElementoFront != null)
            {
                CrearElementoFront(criatura);
            }
        }

        public int nuevoId()
        {
            idMaximo++;
            return idMaximo;
        }

        public double Direccion(CriaturaBack criatura)
        {
            lock (lockObject)
            {
                #region Control de aliados cercanos
                Dictionary<int, CriaturaBack> aliados = null;
                Dictionary<int, CriaturaBack> enemigos = null;
                if (criatura.tipo == "erudito")
                {
                    aliados = eruditos;
                    enemigos = soldados;
                }
                else if (criatura.tipo == "soldado")
                {
                    aliados = soldados;
                    enemigos = eruditos;
                }
                else if (criatura.tipo == "ermitano")
                {
                    return DireccionErmitano(criatura);
                }

                var centroDeMasaGrupo = CentroDeMasa(criatura.jefe.grupo);

                var cercanos = new List<CriaturaBack>();
                foreach (KeyValuePair<int, CriaturaBack> kv in aliados)
                {
                    var distX = centroDeMasaGrupo.Item1 - kv.Value.CanvasPosX;
                    var distY = centroDeMasaGrupo.Item2 - kv.Value.CanvasPosY;
                    if (Math.Sqrt(distX * distX + distY * distY) < RadioDeGrupo(criatura.jefe.grupo.Count))
                    {
                        cercanos.Add(kv.Value);
                    }
                    else
                    {
                        criatura.jefe.grupo.Remove(kv.Value);
                        kv.Value.jefe = kv.Value;
                        kv.Value.grupo = new List<CriaturaBack>();
                        kv.Value.grupo.Add(kv.Value);
                        kv.Value.PonderadorDeVelocidad = kv.Value.PonderadorOriginal;
                    }
                    cercanos.Sort((emp1, emp2) => emp1.PonderadorOriginal.CompareTo(emp2.PonderadorOriginal));
                    var cantidad = 0;
                    foreach (CriaturaBack c in cercanos)
                    {
                        if (cantidad > 5)
                        {
                            c.jefe.grupo.Remove(c);
                            c.jefe = c;
                            c.grupo = new List<CriaturaBack>();
                            c.grupo.Add(c);
                            c.PonderadorDeVelocidad = c.PonderadorOriginal;
                        }
                        else
                        {
                            c.jefe = criatura.jefe;
                            if (!criatura.jefe.grupo.Contains(c))
                            {
                                criatura.jefe.grupo.Add(c);
                            }
                        }
                        cantidad++;
                    }

                }


                #endregion

                #region Calculo de direccion individual segun grupo

                double direccionGrupal = 0;
                foreach (CriaturaBack c in criatura.jefe.grupo)
                {
                    direccionGrupal += c.direccion;
                }
                direccionGrupal = direccionGrupal / criatura.jefe.grupo.Count;

                #endregion

                #region Ajuste de velocidad dentro del grupo
                foreach (CriaturaBack c in criatura.jefe.grupo)
                {
                    var distX = centroDeMasaGrupo.Item1 - c.CanvasPosX;
                    var distY = centroDeMasaGrupo.Item2 - c.CanvasPosY;
                    var dist = Math.Sqrt(distX * distX + distY * distY);
                    var ponderador = ReductorDeVelocidad(dist, RadioDeGrupo(criatura.jefe.grupo.Count));
                    c.PonderadorDeVelocidad = c.PonderadorOriginal * ponderador;
                }
                #endregion

                #region Redireccion Por repulsion enemiga
                if (enemigos != null)
                {
                    List<List<CriaturaBack>> ListaDeGrupos = new List<List<CriaturaBack>>();
                    var sumaDirecciones = 0.0;
                    var cantidad = 0;
                    foreach (KeyValuePair<int, CriaturaBack> kv in enemigos)
                    {
                        if (!ListaDeGrupos.Contains(kv.Value.jefe.grupo))
                            ListaDeGrupos.Add(kv.Value.jefe.grupo);
                    }
                    foreach (List<CriaturaBack> grupo in ListaDeGrupos)
                    {
                        if (Math.Sqrt(Math.Pow(CentroDeMasa(grupo).Item1 - criatura.CanvasPosX, 2) + Math.Pow(CentroDeMasa(grupo).Item2 - criatura.CanvasPosY, 2)) > RadioDeGrupo(grupo.Count) * 2)
                            continue;
                        var distanciaInicial = Math.Sqrt(Math.Pow(criatura.CanvasPosX - CentroDeMasa(grupo).Item1, 2) + Math.Pow(criatura.CanvasPosY - CentroDeMasa(grupo).Item2, 2));
                        var distanciaFinal = Math.Sqrt(Math.Pow(criatura.CanvasPosX + Math.Cos(criatura.direccion) * criatura.PonderadorOriginal - CentroDeMasa(grupo).Item1, 2) + Math.Pow(criatura.CanvasPosY + Math.Sin(criatura.direccion) * criatura.PonderadorOriginal - CentroDeMasa(grupo).Item2, 2));
                        if (distanciaFinal < distanciaInicial)
                        {
                            sumaDirecciones += grupo[0].direccion * -1;
                            cantidad++;
                        }
                    }
                    if (cantidad > 0)
                    {
                        criatura.redirigirGrupo(sumaDirecciones / cantidad);
                    }
                }
                #endregion

                #region Conversion de ermitaños
                List<CriaturaBack> todelete = new List<CriaturaBack>();
                foreach (KeyValuePair<int, CriaturaBack> kv in ermitanos)
                {
                    var ermitano = kv.Value;
                    if (Distancia(new Tuple<double, double>(ermitano.CanvasPosX, ermitano.CanvasPosY), centroDeMasaGrupo) < RadioDeGrupo(criatura.jefe.grupo.Count) && criatura.jefe.grupo.Count > 1)
                    {
                        var convertido = new CriaturaBack(nuevoId(), ermitano.CanvasPosX, ermitano.CanvasPosY, criatura.tipo, randy);
                        aliados.Add(convertido.id, convertido);
                        crearElementoFrontEnd(convertido);
                        convertido.MoverElemento += moverElementoFrontEnd;
                        convertido.ndDelegate = ndDelegate;
                        convertido.IniciarMovimiento();
                        convertido.jefe = criatura.jefe;
                        convertido.jefe.grupo.Add(convertido);
                        todelete.Add(ermitano);

                    }
                }
                foreach (CriaturaBack c in todelete)
                {
                    c.movimiento = false;
                    c.thread_movimiento.Join(1);
                    if (EliminarElementoFront != null)
                        EliminarElementoFront(c);
                    ermitanos.Remove(c.id);
                }
                #endregion

                return (direccionGrupal + criatura.direccion) / 2;
            }
        }

        public double DireccionErmitano(CriaturaBack criatura)
        {
            var listaGrupos = new List<List<CriaturaBack>>();
            foreach (KeyValuePair<int, CriaturaBack> kv in soldados)
            {
                if (!listaGrupos.Contains(kv.Value.jefe.grupo) && kv.Value.jefe.grupo.Count > 1)
                    listaGrupos.Add(kv.Value.jefe.grupo);
            }
            foreach (KeyValuePair<int, CriaturaBack> kv in eruditos)
            {
                if (!listaGrupos.Contains(kv.Value.jefe.grupo) && kv.Value.jefe.grupo.Count > 1)
                    listaGrupos.Add(kv.Value.jefe.grupo);
            }
            List<CriaturaBack> masCercano = new List<CriaturaBack>();
            Tuple<double,double> posCriatura = new Tuple<double,double>(criatura.CanvasPosX, criatura.CanvasPosY);
            foreach (List<CriaturaBack> grupo in listaGrupos)
            {
                if (masCercano.Count == 0)
                    masCercano = grupo;
                else if (Distancia(CentroDeMasa(grupo), posCriatura) < Distancia(CentroDeMasa(masCercano), posCriatura))
                    masCercano = grupo;
            }
            if (masCercano.Count < 2)
                return criatura.direccion;
            if (Distancia(CentroDeMasa(masCercano), posCriatura) > RadioDeGrupo(masCercano.Count) * 2)
                return criatura.direccion;
            var centroDeMasaMasCercano = CentroDeMasa(masCercano);
            var direccion = (Math.Atan((criatura.CanvasPosY - centroDeMasaMasCercano.Item2) / (criatura.CanvasPosX - centroDeMasaMasCercano.Item1)));
            
            return direccion;

        }

        public Tuple<double, double> CentroDeMasa(List<CriaturaBack> grupo)
        {
            double X = 0;
            double Y = 0;

            foreach (CriaturaBack c in grupo)
            {
                X += c.CanvasPosX;
                Y += c.CanvasPosY;
            }
            X = X / grupo.Count;
            Y = Y / grupo.Count;
            return new Tuple<double, double>(X, Y);

        }

        public double Distancia(Tuple<double, double> A, Tuple<double, double> B)
        {
            return Math.Sqrt(Math.Pow(A.Item1 - B.Item1, 2) + Math.Pow(A.Item2 - B.Item2, 2));
        }

        public double RadioDeGrupo(int cantidad)
        {
            return Math.Sqrt(cantidad * radio * radio);
        }

        //Entrega un ponderador para hacer más lentos a los que están lejos del grupo
        public double ReductorDeVelocidad(double distancia, double radio)
        {
            // solo un 40% de su velocidad se ve afectada
            return 0.6 + 0.4 * (1.0 - radio / distancia);
        }

        ////Hace que todos los threads terminen de trabajar para poder modificar colecciones
        //public void SincronizarThreads()
        //{
        //    foreach (KeyValuePair<int, CriaturaBack> c in soldados)
        //    {
        //        c.Value.movimiento = false;
        //    }
        //    foreach (KeyValuePair<int, CriaturaBack> c in ermitanos)
        //    {
        //        c.Value.movimiento = false;
        //    }
        //    foreach (KeyValuePair<int, CriaturaBack> c in eruditos)
        //    {
        //        c.Value.movimiento = false;
        //    }
        //    Thread.Sleep(1);
        //}

        //public void ReiniciarMovimiento()
        //{
        //    foreach (KeyValuePair<int, CriaturaBack> c in soldados)
        //    {
        //        c.Value.movimiento = true;
        //        c.Value.IniciarMovimiento();
        //    }
        //    foreach (KeyValuePair<int, CriaturaBack> c in ermitanos)
        //    {
        //        c.Value.movimiento = true;
        //        c.Value.IniciarMovimiento();
        //    }
        //    foreach (KeyValuePair<int, CriaturaBack> c in eruditos)
        //    {
        //        c.Value.movimiento = true;
        //        c.Value.IniciarMovimiento();
        //    }
        //}
    }
}
