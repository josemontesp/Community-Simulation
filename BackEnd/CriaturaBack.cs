using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BackEnd
{
    public class CriaturaBack
    {
        public bool movimiento = false;
        public int id;
        public double CanvasPosX;
        public double CanvasPosY;
        public double PonderadorDeVelocidad;
        public double PonderadorOriginal;
        public double direccion; // 0-2Pi
        public string tipo;
        public object lockObject = new object();
        public Thread thread_movimiento;
        public Random randy;
        public event Action<CriaturaBack> MoverElemento;
        public newDirDelegate ndDelegate;
        public double vision = 300;

        public List<CriaturaBack> grupo = new List<CriaturaBack>();
        public CriaturaBack jefe;
       

        public CriaturaBack(int _id, double _canvasPosX, double _canvasPosY, string _tipo, Random _randy)
        {
            id = _id;
            CanvasPosX = _canvasPosX;
            CanvasPosY = _canvasPosY;
            tipo = _tipo;
            randy = _randy;
            PonderadorDeVelocidad = randy.Next(60, 140) / 100.0;
            PonderadorOriginal = PonderadorDeVelocidad;
            direccion = randy.Next(0, 62832) / 10000.0;
            grupo.Add(this);
            jefe = this;
        }

        public void IniciarMovimiento()
        {
            thread_movimiento = new Thread(Thread_Mover);
            movimiento = true;
            thread_movimiento.Start();
        }

        public void Thread_Mover()
        {
            var sleep = 25;
            var distancia =  PonderadorDeVelocidad;
            while (movimiento)
            {
                RandomizeDirection();
                CanvasPosX += distancia * Math.Cos(direccion);
                CanvasPosY += distancia * Math.Sin(direccion);

                #region Control de Fuerzas
                direccion = ndDelegate(this);
                
                #endregion

                #region Control de Bordes
                if (CanvasPosX < 0)
                    redirigirGrupo(0);
                else if (CanvasPosX > 1000)
                    redirigirGrupo(Math.PI);
                if (CanvasPosY < 0)
                    redirigirGrupo(Math.PI / 2);
                else if (CanvasPosY > 680)
                    redirigirGrupo(Math.PI * 1.5);
                //Control de bases
                if (tipo == "soldado" && CanvasPosY < 203 && CanvasPosX < 343)
                    redirigirGrupo(0);
                else if (tipo == "soldado" && CanvasPosY < 206 && CanvasPosX < 343)
                    redirigirGrupo(Math.PI / 2);
                if (tipo == "erudito" && CanvasPosX > 700 && CanvasPosY > 550)
                    redirigirGrupo(Math.PI * 1.5);
                else if (tipo == "erudito" && CanvasPosX > 695 && CanvasPosY > 555)
                    redirigirGrupo(Math.PI);
                //Control de obstaculos
                if (CanvasPosX > 670 && CanvasPosY < 180) //agua
                    redirigirGrupo(Math.PI);
                else if (CanvasPosX > 675 && CanvasPosY < 183)
                    redirigirGrupo(Math.PI / 2);
                if (CanvasPosX < 65 && CanvasPosY > 219 && CanvasPosY < 417) //arboles
                    redirigirGrupo(0);


                #endregion

                if (MoverElemento != null)
                {
                    MoverElemento(this);
                }
                Thread.Sleep(sleep);
            }
        }

        public void RandomizeDirection()
        {
            direccion += randy.Next(-1745, 1745) / 5000.0;
            if (direccion > 6.2832)
            {
                direccion -= 6.2832;
            }
        }

        public void redirigirGrupo(double direccion)
        {
            foreach (CriaturaBack c in this.jefe.grupo)
            {
                c.direccion = direccion;
            }
        }

        

    }
}
