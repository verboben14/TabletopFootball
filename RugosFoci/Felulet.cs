using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RugosFoci
{
    class ViewModel
    {
        public Geometry PalyaVonalak { get; set; }
        public Geometry KapuVonalak { get; set; }
        public Geometry KapuHalok { get; set; }
        public Geometry PalyaPontok { get; set; }
        public Geometry JatekosJelzoPontok { get; set; }
        public Geometry JatekosKorzetek { get; set; }
        public List<Jatekos> SajatJatekosok { get; set; }
        public List<Jatekos> EllenfelJatekosok { get; set; }
        public List<Kapu> Kapuk { get; set; }
        public Labda Labda { get; set; }
        public Kapus SajatKapus { get; set; }
        public Kapus EllenfelKapus { get; set; }
        public int SajatGol { get; set; }
        public int EllenfelGol { get; set; }
        public int HatralevoIdo { get; set; }
        public bool VegeAMeccsnek { get; set; }
        public bool Megallitva { get; set; }
    }

    abstract class Elem
    {
        protected double cx, cy; //az elem középpontjának koordinátái
        public double Cy
        {
            get { return cy; }
        }
        public double Cx
        {
            get { return cx; }
        }

        protected Geometry alak;

        public Geometry Alak
        {
            get
            {
                Geometry copy = alak.Clone();
                TranslateTransform tt = new TranslateTransform(cx, cy);
                copy.Transform = tt;
                return copy.GetFlattenedPathGeometry();
            }
        }

        public Elem(double cx, double cy)
        {
            this.cx = cx;
            this.cy = cy;
        }

        public bool Utkozes(Geometry alak2)
        {
            return Geometry.Combine(this.Alak, alak2, GeometryCombineMode.Intersect, null).GetArea() > 0;
        }
    }

    class Kapu : Elem
    {
        public const double w = Labda.r * 3; //az kapu szélesség fele
        public const double h = Labda.r * 7; //a kapu magasság fele

        public Kapu(double cx, double cy)
            : base(cx, cy)
        {
            alak = new RectangleGeometry(new Rect(-w, -h, 2 * w, 2 * h));
        }
    }

    class Jatekos : Elem
    {
        public const double r = 2 * Labda.r; //a játékos kör alakú, ez a sugara
        public const double jatekosKoruliSugar = 10 * Labda.r;
        EllipseGeometry jatekosKor;
        public EllipseGeometry JatekosKor
        {
            get { return jatekosKor; }
        }

        public bool NalaALabda { get; set; }

        public Jatekos(double cx, double cy)
            : base(cx, cy)
        {
            alak = new EllipseGeometry(new Point(0, 0), r, r);
            jatekosKor = new EllipseGeometry(new Point(cx, cy), jatekosKoruliSugar, jatekosKoruliSugar);
        }

        public void Huzas(double cx, double cy, double eredeticx, double eredeticy)
        {
            if (jatekosKoruliSugar < Math.Sqrt((cx - eredeticx) * (cx - eredeticx) + (cy - eredeticy) * (cy - eredeticy)))
            {
                double dx = cx - eredeticx;
                double dy = cy - eredeticy;
                double theta = Math.Atan2(dy, dx);
                this.cx = eredeticx + (jatekosKoruliSugar * Math.Cos(theta));
                this.cy = eredeticy + (jatekosKoruliSugar * Math.Sin(theta));
            }
            else
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        public void Athelyezes(double cx, double cy)
        {
            this.cx = cx; this.cy = cy;
        }
    }

    class Kapus : Elem
    {
        public const double r = 2 * Labda.r;

        public Kapus(double cx, double cy)
            : base(cx, cy)
        {
            alak = new EllipseGeometry(new Point(0, 0), r, r);
        }

        public void Mozog(int cy, double fh) //y tengely mentén eltoljuk
        {
            if ((this.cy > fh / 2 - Kapu.h && cy < 0) || (this.cy < fh / 2 + Kapu.h && cy > 0))
                this.cy += cy;

        }
    }
    class Labda : Elem
    {
        public const double r = 10; //a labda sugara
        double dx, dy; //a labda mozgásához szükséges változók
        public double Dy
        {
            get { return dy; }
            set { dy = value; }
        }
        public double Dx
        {
            get { return dx; }
            set { dx = value; }
        }


        bool valakinelALabda;
        public bool ValakinelALabda
        {
            get { return valakinelALabda; }
            set { valakinelALabda = value; }
        }
        public Labda(double cx, double cy)
            : base(cx, cy)
        {
            dx = 0; dy = 0;
            alak = new EllipseGeometry(new Point(0, 0), r, r);
            valakinelALabda = false;
        }

        void IranybaPattan(Point p)
        {
            double kulX = cx - p.X;
            double kulY = -(cy - p.Y);
            double theta = Math.Atan2(kulY, kulX);
            if (!(dx == 0 && dy == 0))
            {
                if (Math.Sqrt(dx * dx + dy * dy) * Math.Cos(theta) == 0 && -Math.Sqrt(dx * dx + dy * dy) * Math.Sin(theta) == 0)
                {
                    dx = -dx;
                    dy = -dy;
                    return;
                }
                dx = Math.Sqrt(dx * dx + dy * dy) * Math.Cos(theta);
                dy = -Math.Sqrt(dx * dx + dy * dy) * Math.Sin(theta);
            }
            else if (cx != p.X && cy != p.Y)
            {
                dx = Math.Cos(theta);
                dy = Math.Sin(theta);
            }
        }

        void JatekosVonzas()
        {
            if (dx > 0)
                dx -= dx * 0.035;
            else if (dx < 0)
                dx += -dx * 0.035;
            if (dy > 0)
                dy -= dy * 0.035;
            else if (dy < 0)
                dy += -dy * 0.035;
        }

        public bool Move(ViewModel vm, double fw, double fh) //ha a labda bemegy a kapuba akkor igazat ad vissza
        {
            cy += dy;
            cx += dx;
            Geometry labdaAlak = this.Alak;
            Geometry sajatKapuAlak = vm.Kapuk[0].Alak;
            Geometry ellenfelKapuAlak = vm.Kapuk[1].Alak;
            Geometry ellenfelKapusAlak = vm.EllenfelKapus.Alak;
            Geometry sajatKapusAlak = vm.SajatKapus.Alak;

            for (int i = 0; i < vm.SajatJatekosok.Count; i++)
            {
                if (vm.SajatJatekosok[i].NalaALabda || vm.EllenfelJatekosok[i].NalaALabda)
                    valakinelALabda = true;
                //játékos körén belül van a labda
                if (Utkozes(vm.SajatJatekosok[i].JatekosKor) && !vm.SajatJatekosok[i].NalaALabda)
                    if (Math.Abs(dx) < 2 && Math.Abs(dy) < 2)
                    {
                        cx = vm.SajatJatekosok[i].JatekosKor.Center.X;
                        cy = vm.SajatJatekosok[i].JatekosKor.Center.Y;
                        vm.SajatJatekosok[i].NalaALabda = true;
                        dx = 0; dy = 0;
                    }
                    else
                        JatekosVonzas();
                if (Utkozes(vm.EllenfelJatekosok[i].JatekosKor) && !vm.EllenfelJatekosok[i].NalaALabda)
                    if (Math.Abs(dx) < 2 && Math.Abs(dy) < 2)
                    {
                        cx = vm.EllenfelJatekosok[i].JatekosKor.Center.X;
                        cy = vm.EllenfelJatekosok[i].JatekosKor.Center.Y;
                        vm.EllenfelJatekosok[i].NalaALabda = true;
                        dx = 0; dy = 0;
                    }
                    else
                        JatekosVonzas();

                //már nincs a játékosnál a labda
                if (vm.SajatJatekosok[i].NalaALabda && !Utkozes(vm.SajatJatekosok[i].JatekosKor))
                    vm.SajatJatekosok[i].NalaALabda = false;
                if ((vm.EllenfelJatekosok[i].NalaALabda && !Utkozes(vm.EllenfelJatekosok[i].JatekosKor)))
                    vm.EllenfelJatekosok[i].NalaALabda = false;
                //ütközés játékosokkal

                if (Utkozes(vm.SajatJatekosok[i].Alak))
                    IranybaPattan(new Point(vm.SajatJatekosok[i].Cx, vm.SajatJatekosok[i].Cy));
                if (Utkozes(vm.EllenfelJatekosok[i].Alak))
                    IranybaPattan(new Point(vm.EllenfelJatekosok[i].Cx, vm.EllenfelJatekosok[i].Cy));
            }

            //ütközés kapusokkal
            if (Utkozes(ellenfelKapusAlak))
                IranybaPattan(new Point(vm.EllenfelKapus.Cx, vm.EllenfelKapus.Cy));
            if (Utkozes(sajatKapusAlak))
                IranybaPattan(new Point(vm.SajatKapus.Cx, vm.SajatKapus.Cy));

            //ütközés kapuval
            if (Utkozes(ellenfelKapuAlak))
            {
                if (labdaAlak.Bounds.Left > ellenfelKapuAlak.Bounds.Left && (labdaAlak.Bounds.Top < ellenfelKapuAlak.Bounds.Top || labdaAlak.Bounds.Bottom > ellenfelKapuAlak.Bounds.Bottom))
                    dy = -dy;

                //Kapufával
                if (labdaAlak.Bounds.Right > ellenfelKapuAlak.Bounds.Left && labdaAlak.Bounds.Left < ellenfelKapuAlak.Bounds.Left && (labdaAlak.Bounds.Top < ellenfelKapuAlak.Bounds.Top || labdaAlak.Bounds.Bottom > ellenfelKapuAlak.Bounds.Bottom))
                {
                    if (labdaAlak.Bounds.Top < ellenfelKapuAlak.Bounds.Top)
                        IranybaPattan(new Point(ellenfelKapuAlak.Bounds.Left, ellenfelKapuAlak.Bounds.Top));
                    if (labdaAlak.Bounds.Bottom > ellenfelKapuAlak.Bounds.Bottom)
                        IranybaPattan(new Point(ellenfelKapuAlak.Bounds.Left, ellenfelKapuAlak.Bounds.Bottom));
                }
                if (labdaAlak.Bounds.Left > ellenfelKapuAlak.Bounds.Left && labdaAlak.Bounds.Top > ellenfelKapuAlak.Bounds.Top && labdaAlak.Bounds.Bottom < ellenfelKapuAlak.Bounds.Bottom)
                {
                    vm.SajatGol++;
                    return true;
                }
            }
            if (Utkozes(sajatKapuAlak))
            {
                if (labdaAlak.Bounds.Right < sajatKapuAlak.Bounds.Right && (labdaAlak.Bounds.Top < sajatKapuAlak.Bounds.Top || labdaAlak.Bounds.Bottom > sajatKapuAlak.Bounds.Bottom))
                    dy = -dy;

                //Kapufával
                if (labdaAlak.Bounds.Left < sajatKapuAlak.Bounds.Right && labdaAlak.Bounds.Right > sajatKapuAlak.Bounds.Right && (labdaAlak.Bounds.Top < sajatKapuAlak.Bounds.Top || labdaAlak.Bounds.Bottom > sajatKapuAlak.Bounds.Bottom))
                {
                    if (labdaAlak.Bounds.Top < sajatKapuAlak.Bounds.Top)
                        IranybaPattan(new Point(sajatKapuAlak.Bounds.Right, sajatKapuAlak.Bounds.Top));
                    if (labdaAlak.Bounds.Bottom > sajatKapuAlak.Bounds.Bottom)
                        IranybaPattan(new Point(sajatKapuAlak.Bounds.Right, sajatKapuAlak.Bounds.Bottom));
                }
                if (labdaAlak.Bounds.Right < sajatKapuAlak.Bounds.Right && labdaAlak.Bounds.Top > sajatKapuAlak.Bounds.Top && labdaAlak.Bounds.Bottom < sajatKapuAlak.Bounds.Bottom)
                {
                    vm.EllenfelGol++;
                    return true;
                }
            }

            if (labdaAlak.Bounds.Bottom > fh - 1 || labdaAlak.Bounds.Top < 1)
                dy = -dy;
            if (labdaAlak.Bounds.Right > fw - 1 || labdaAlak.Bounds.Left < 1)
                dx = -dx;
            return false;
        }

        public void BeleRug(double chx, double chy)
        {
            dx = chx;
            dy = chy;
            valakinelALabda = false;
        }

        static Random rnd = new Random();
        public void SetPos(double x, double y)
        {
            cx = x;
            cy = y;
            dx = rnd.Next(-5, 6);
            dy = rnd.Next(-5, 6);
        }
    }

    class Felulet : FrameworkElement
    {
        ViewModel vm;
        DispatcherTimer mozgasTimer, idoTimer;

        public static string SajatCsapat { get; set; }
        public static string EllenfelCsapat { get; set; }

        public Felulet()
        {
            vm = new ViewModel()
            {
                SajatJatekosok = new List<Jatekos>(),
                EllenfelJatekosok = new List<Jatekos>(),
                Kapuk = new List<Kapu>(),
                Labda = new Labda(0, 0),
                SajatKapus = new Kapus(0, 0),
                EllenfelKapus = new Kapus(0, 0)
            };

            Loaded += Felulet_Loaded;

            mozgasTimer = new DispatcherTimer();
            mozgasTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            mozgasTimer.Tick += dt_Tick;
            mozgasTimer.Start();

            idoTimer = new DispatcherTimer();
            idoTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            vm.HatralevoIdo = 5 * 60;
            idoTimer.Tick += idoTimer_Tick;
            idoTimer.Start();
        }

        void idoTimer_Tick(object sender, EventArgs e)
        {
            if (vm.HatralevoIdo > 0)
                vm.HatralevoIdo--;
            else
            {
                vm.VegeAMeccsnek = true;
                idoTimer.Stop();
                mozgasTimer.Stop();
            }
            InvalidateVisual();
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (!vm.Labda.ValakinelALabda)
                if (vm.Labda.Move(vm, ActualWidth, ActualHeight))
                    vm.Labda.SetPos(ActualWidth / 2, ActualHeight / 2);
            InvalidateVisual();
        }

        static Random rnd = new Random();
        void ElemekLetrehozasa()
        {
            vm.Labda = new Labda((ActualWidth / 2), (ActualHeight / 2));
            vm.Labda.Dx = rnd.Next(-5, 6);
            vm.Labda.Dy = rnd.Next(-5, 6);

            vm.SajatJatekosok.Add(new Jatekos((ActualWidth / 5 + 30), (ActualHeight / 3)));
            vm.SajatJatekosok.Add(new Jatekos((ActualWidth / 5 + 30), 2 * (ActualHeight / 3)));
            vm.SajatJatekosok.Add(new Jatekos(5 * (ActualWidth / 12), (ActualHeight / 6)));
            vm.SajatJatekosok.Add(new Jatekos(5 * (ActualWidth / 12), 5 * (ActualHeight / 6)));
            vm.SajatJatekosok.Add(new Jatekos(8 * (ActualWidth / 15), (ActualHeight / 2) - Jatekos.r * 5));
            vm.SajatJatekosok.Add(new Jatekos(6 * (ActualWidth / 9), (ActualHeight / 2)));

            vm.EllenfelJatekosok.Add(new Jatekos(4 * (ActualWidth / 5) - 30, (ActualHeight / 3)));
            vm.EllenfelJatekosok.Add(new Jatekos(4 * (ActualWidth / 5) - 30, 2 * (ActualHeight / 3)));
            vm.EllenfelJatekosok.Add(new Jatekos(7 * (ActualWidth / 12), (ActualHeight / 6)));
            vm.EllenfelJatekosok.Add(new Jatekos(7 * (ActualWidth / 12), 5 * (ActualHeight / 6)));
            vm.EllenfelJatekosok.Add(new Jatekos(7 * (ActualWidth / 15), (ActualHeight / 2) + Jatekos.r * 5));
            vm.EllenfelJatekosok.Add(new Jatekos(3 * (ActualWidth / 9), (ActualHeight / 2)));

            vm.Kapuk.Add(new Kapu(Kapu.w, (ActualHeight / 2)));
            vm.Kapuk.Add(new Kapu((ActualWidth - Kapu.w), (ActualHeight / 2)));

            vm.SajatKapus = new Kapus(Kapu.w + (ActualWidth * 0.04), (ActualHeight / 2));
            vm.EllenfelKapus = new Kapus(ActualWidth - (Kapu.w + (ActualWidth * 0.04)), (ActualHeight / 2));
        }

        void PalyaFelepites()
        {
            GeometryGroup PalyaVonalak = new GeometryGroup();
            GeometryGroup PalyaPontok = new GeometryGroup();
            GeometryGroup JatekosJelzoPontok = new GeometryGroup();
            GeometryGroup JatekosKorzetek = new GeometryGroup();
            GeometryGroup KapuVonalak = new GeometryGroup();
            GeometryGroup KapuHalok = new GeometryGroup();

            //pálya körbe
            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w, 10), new Point(2 * Kapu.w, ActualHeight - 10)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w, 10), new Point(ActualWidth - 2 * Kapu.w, 10)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w, 10), new Point(ActualWidth - 2 * Kapu.w, ActualHeight - 10)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w, ActualHeight - 10), new Point(2 * Kapu.w, ActualHeight - 10)));

            //középvonal, középkör
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth / 2, 10), new Point(ActualWidth / 2, ActualHeight - 10)));
            PalyaVonalak.Children.Add(new EllipseGeometry(new Point(ActualWidth / 2, ActualHeight / 2), ActualWidth / 10, ActualWidth / 10));

            //kapuk
            KapuVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w, ActualHeight / 2 - Kapu.h), new Point(2, ActualHeight / 2 - Kapu.h)));
            KapuVonalak.Children.Add(new LineGeometry(new Point(2, ActualHeight / 2 - Kapu.h), new Point(2, ActualHeight / 2 + Kapu.h)));
            KapuVonalak.Children.Add(new LineGeometry(new Point(2, ActualHeight / 2 + Kapu.h), new Point(2 * Kapu.w, ActualHeight / 2 + Kapu.h)));

            KapuVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - 2, ActualHeight / 2 - Kapu.h)));
            KapuVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - 2, ActualHeight / 2 + Kapu.h)));
            KapuVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2, ActualHeight / 2 + Kapu.h), new Point(ActualWidth - 2 * Kapu.w, ActualHeight / 2 + Kapu.h)));

            //kapu hálók
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w / 3, ActualHeight / 2 - Kapu.h), new Point(Kapu.w / 3, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w / 3 * 2, ActualHeight / 2 - Kapu.h), new Point(Kapu.w / 3 * 2, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w, ActualHeight / 2 - Kapu.h), new Point(Kapu.w, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w / 3 * 4, ActualHeight / 2 - Kapu.h), new Point(Kapu.w / 3 * 4, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w / 3 * 5, ActualHeight / 2 - Kapu.h), new Point(Kapu.w / 3 * 5, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(Kapu.w * 2, ActualHeight / 2 - Kapu.h), new Point(Kapu.w * 2, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 - Kapu.h / 4 * 3), new Point(Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 3)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 - Kapu.h / 4 * 2), new Point(Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 - Kapu.h / 4 * 1), new Point(Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 1)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2), new Point(Kapu.w * 2, ActualHeight / 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 + Kapu.h / 4 * 1), new Point(Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 1)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 + Kapu.h / 4 * 2), new Point(Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(0, ActualHeight / 2 + Kapu.h / 4 * 3), new Point(Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 3)));

            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w / 3, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w / 3, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w / 3 * 2, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w / 3 * 2, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w / 3 * 4, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w / 3 * 4, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w / 3 * 5, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w / 3 * 5, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 - Kapu.h), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 + Kapu.h)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 - Kapu.h / 4 * 3), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 3)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 - Kapu.h / 4 * 2), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 - Kapu.h / 4 * 1), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 - Kapu.h / 4 * 1)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 + Kapu.h / 4 * 1), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 1)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 + Kapu.h / 4 * 2), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 2)));
            KapuHalok.Children.Add(new LineGeometry(new Point(ActualWidth, ActualHeight / 2 + Kapu.h / 4 * 3), new Point(ActualWidth - Kapu.w * 2, ActualHeight / 2 + Kapu.h / 4 * 3)));


            //tizenhatosok, ötösök
            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w, ActualHeight / 4), new Point(ActualWidth / 5, ActualHeight / 4)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth / 5, ActualHeight / 4), new Point(ActualWidth / 5, 3 * ActualHeight / 4)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth / 5, 3 * ActualHeight / 4), new Point(2 * Kapu.w, 3 * ActualHeight / 4)));
            PathFigure pf = new PathFigure();
            pf.StartPoint = new Point(ActualWidth / 5, ActualHeight / 2 - Kapu.h - ActualHeight / 20);
            pf.Segments.Add(new ArcSegment(new Point(ActualWidth / 5, ActualHeight / 2 + Kapu.h + ActualHeight / 20), new Size(ActualWidth / 17, ActualWidth / 15), 0, false, SweepDirection.Clockwise, true));

            PathFigure pf2 = new PathFigure();
            pf2.StartPoint = new Point(ActualWidth - ActualWidth / 5, ActualHeight / 2 - Kapu.h - ActualHeight / 20);
            pf2.Segments.Add(new ArcSegment(new Point(ActualWidth - ActualWidth / 5, ActualHeight / 2 + Kapu.h + ActualHeight / 20), new Size(ActualWidth / 17, ActualWidth / 15), 0, false, SweepDirection.Counterclockwise, true));
            PathGeometry pg = new PathGeometry(new PathFigure[] { pf });
            PathGeometry pg2 = new PathGeometry(new PathFigure[] { pf2 });
            PalyaVonalak.Children.Add(pg);
            PalyaVonalak.Children.Add(pg2);

            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w, ActualHeight / 4), new Point(4 * ActualWidth / 5, ActualHeight / 4)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(4 * ActualWidth / 5, ActualHeight / 4), new Point(4 * ActualWidth / 5, 3 * ActualHeight / 4)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(4 * ActualWidth / 5, 3 * ActualHeight / 4), new Point(ActualWidth - 2 * Kapu.w, 3 * ActualHeight / 4)));

            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w, ActualHeight / 2 - Kapu.h - ActualHeight / 20), new Point(2 * Kapu.w + ActualWidth / 20, ActualHeight / 2 - Kapu.h - ActualHeight / 20)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w + ActualWidth / 20, ActualHeight / 2 - Kapu.h - ActualHeight / 20), new Point(2 * Kapu.w + ActualWidth / 20, ActualHeight / 2 + Kapu.h + ActualHeight / 20)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(2 * Kapu.w + ActualWidth / 20, ActualHeight / 2 + Kapu.h + ActualHeight / 20), new Point(2 * Kapu.w, ActualHeight / 2 + Kapu.h + ActualHeight / 20)));

            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w, ActualHeight / 2 - Kapu.h - ActualHeight / 20), new Point(ActualWidth - 2 * Kapu.w - ActualWidth / 20, ActualHeight / 2 - Kapu.h - ActualHeight / 20)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w - ActualWidth / 20, ActualHeight / 2 - Kapu.h - ActualHeight / 20), new Point(ActualWidth - 2 * Kapu.w - ActualWidth / 20, ActualHeight / 2 + Kapu.h + ActualHeight / 20)));
            PalyaVonalak.Children.Add(new LineGeometry(new Point(ActualWidth - 2 * Kapu.w - ActualWidth / 20, ActualHeight / 2 + Kapu.h + ActualHeight / 20), new Point(ActualWidth - 2 * Kapu.w, ActualHeight / 2 + Kapu.h + ActualHeight / 20)));

            //pálya pontok
            PalyaPontok.Children.Add(new EllipseGeometry(new Point(ActualWidth / 2, ActualHeight / 2), 7, 7));
            PalyaPontok.Children.Add(new EllipseGeometry(new Point(6 * ((2 * Kapu.w + ActualWidth / 20) + ActualWidth / 5) / 11, ActualHeight / 2), 7, 7));
            PalyaPontok.Children.Add(new EllipseGeometry(new Point(ActualWidth - (6 * ((2 * Kapu.w + ActualWidth / 20) + ActualWidth / 5) / 11), ActualHeight / 2), 7, 7));

            //játékos eredeti pozíció, játékoskörzetek
            for (int i = 0; i < vm.SajatJatekosok.Count; i++)
            {
                JatekosKorzetek.Children.Add(vm.SajatJatekosok[i].JatekosKor);
                JatekosJelzoPontok.Children.Add(new EllipseGeometry(new Point(vm.SajatJatekosok[i].Cx, vm.SajatJatekosok[i].Cy), 6, 6));
                JatekosKorzetek.Children.Add(vm.EllenfelJatekosok[i].JatekosKor);
                JatekosJelzoPontok.Children.Add(new EllipseGeometry(new Point(vm.EllenfelJatekosok[i].Cx, vm.EllenfelJatekosok[i].Cy), 6, 6));
            }

            vm.PalyaVonalak = PalyaVonalak;
            vm.PalyaPontok = PalyaPontok;
            vm.JatekosJelzoPontok = JatekosJelzoPontok;
            vm.JatekosKorzetek = JatekosKorzetek;
            vm.KapuVonalak = KapuVonalak;
            vm.KapuHalok = KapuHalok;
        }

        void Felulet_Loaded(object sender, RoutedEventArgs e)
        {
            (this.Parent as Window).Background = Brushes.Green;
            (this.Parent as Window).KeyDown += Felulet_KeyDown;
            (this.Parent as Window).MouseDown += Felulet_MouseDown;
            (this.Parent as Window).MouseUp += Felulet_MouseUp;

            ElemekLetrehozasa();
            PalyaFelepites();
        }

        Jatekos kivalasztottJatekos;
        double eredetiKivalasztottcx, eredetiKivalasztottcy;
        bool leVoltNyomva;

        void Felulet_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (this.Parent as Window).MouseMove -= Felulet_MouseMove;
            if (kivalasztottJatekos != null)
            {
                vm.Labda.BeleRug(-0.3 * (kivalasztottJatekos.Cx - eredetiKivalasztottcx), -0.3 * (kivalasztottJatekos.Cy - eredetiKivalasztottcy));
                kivalasztottJatekos.NalaALabda = false;
                kivalasztottJatekos.Athelyezes(eredetiKivalasztottcx, eredetiKivalasztottcy);
                kivalasztottJatekos = null;
                eredetiKivalasztottcx = 0;
                eredetiKivalasztottcy = 0;
                leVoltNyomva = false;
            }
        }

        void Felulet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!leVoltNyomva)
            {
                (this.Parent as Window).MouseMove += Felulet_MouseMove;
                leVoltNyomva = true;
                Point p = e.GetPosition(this);
                foreach (Jatekos j in vm.SajatJatekosok)
                    if (j.Utkozes(new EllipseGeometry(p, 0.1, 0.1))/* && j.NalaALabda*/ )
                    {
                        kivalasztottJatekos = j;
                        eredetiKivalasztottcx = j.Cx;
                        eredetiKivalasztottcy = j.Cy;
                    }
                foreach (Jatekos j in vm.EllenfelJatekosok)
                    if (j.Utkozes(new EllipseGeometry(p, 0.1, 0.1))/* && j.NalaALabda*/ )
                    {
                        kivalasztottJatekos = j;
                        eredetiKivalasztottcx = j.Cx;
                        eredetiKivalasztottcy = j.Cy;
                    }
            }
        }

        void Felulet_MouseMove(object sender, MouseEventArgs e)
        {
            if (kivalasztottJatekos != null)
            {
                kivalasztottJatekos.Huzas(e.GetPosition(this).X, e.GetPosition(this).Y, eredetiKivalasztottcx, eredetiKivalasztottcy);
            }
        }

        void Felulet_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.Labda.Dy--; break;
                case Key.S: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.Labda.Dy++; break;
                case Key.A: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.Labda.Dx--; break;
                case Key.D: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.Labda.Dx++; break;
                case Key.Escape: (this.Parent as Window).Close(); break;
                case Key.Space: if (vm.HatralevoIdo > 0) { vm.Megallitva = !vm.Megallitva; mozgasTimer.IsEnabled = !mozgasTimer.IsEnabled; idoTimer.IsEnabled = !idoTimer.IsEnabled; InvalidateVisual(); } break;

                case Key.Up: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.SajatKapus.Mozog(-8, ActualHeight); break;
                case Key.Down: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.SajatKapus.Mozog(8, ActualHeight); break;
                case Key.NumPad2: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.EllenfelKapus.Mozog(8, ActualHeight); break;
                case Key.NumPad5: if (idoTimer.IsEnabled && mozgasTimer.IsEnabled) vm.EllenfelKapus.Mozog(-8, ActualHeight); break;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //Pálya kirajzolása
            drawingContext.DrawGeometry(Brushes.DarkGreen, new Pen(), vm.JatekosKorzetek);
            drawingContext.DrawGeometry(null, new Pen(Brushes.White, 4), vm.PalyaVonalak);
            drawingContext.DrawGeometry(null, new Pen(Brushes.White, 8), vm.KapuVonalak);
            drawingContext.DrawGeometry(Brushes.White, new Pen(), vm.PalyaPontok);
            drawingContext.DrawGeometry(Brushes.White, new Pen(), vm.JatekosJelzoPontok);
            drawingContext.DrawGeometry(Brushes.Red, new Pen(Brushes.Black, 1), new EllipseGeometry(new Point(Kapu.w * 2 + 20, 35), 7, 7));
            drawingContext.DrawGeometry(Brushes.Blue, new Pen(Brushes.Black, 1), new EllipseGeometry(new Point(ActualWidth - 2 * Kapu.w - 20, 35), 7, 7));
            foreach (Kapu k in vm.Kapuk)
                drawingContext.DrawGeometry(null, new Pen(), k.Alak);

            //Játékosok kirajzolása

            drawingContext.DrawGeometry(Brushes.Yellow, new Pen(Brushes.Black, 1), vm.SajatKapus.Alak);
            drawingContext.DrawGeometry(Brushes.Black, new Pen(Brushes.White, 1), vm.EllenfelKapus.Alak);

            for (int i = 0; i < vm.SajatJatekosok.Count; i++)
            {
                drawingContext.DrawGeometry(Brushes.Red, new Pen(Brushes.Black, 1), vm.SajatJatekosok[i].Alak);
                drawingContext.DrawGeometry(Brushes.Blue, new Pen(Brushes.Black, 1), vm.EllenfelJatekosok[i].Alak);
            }
            drawingContext.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 1), vm.Labda.Alak);
            drawingContext.DrawGeometry(null, new Pen(Brushes.White, 1), vm.KapuHalok);

            FormattedText txtSajatGolok = new FormattedText(SajatCsapat + " " + vm.SajatGol.ToString() + " - " + vm.EllenfelGol.ToString() + " " + EllenfelCsapat, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal), 25, Brushes.Violet);
            drawingContext.DrawGeometry(Brushes.White, new Pen(), txtSajatGolok.BuildGeometry(new Point(4 * Kapu.w, 30)));

            FormattedText txtIdo = new FormattedText("Hátralévő idő: " + (((int)(vm.HatralevoIdo / 60) == 0) ? ((vm.HatralevoIdo % 60).ToString()) : (((int)(vm.HatralevoIdo / 60)).ToString() + ":" + ((vm.HatralevoIdo % 60) < 10 ? "0" + (vm.HatralevoIdo % 60).ToString() : (vm.HatralevoIdo % 60).ToString()))), System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal), 25, Brushes.Violet);
            drawingContext.DrawGeometry(Brushes.White, new Pen(), txtIdo.BuildGeometry(new Point(3 * ActualWidth / 4, 30)));

            if (vm.Megallitva)
            {
                FormattedText pause = new FormattedText("PAUSE", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal), 48, Brushes.White);
                Geometry g = pause.BuildGeometry(new Point(0, 0));
                double s, x = 0, y = 0;
                if (ActualWidth / ActualHeight < g.Bounds.Width / g.Bounds.Height)
                {
                    s = ActualWidth / g.Bounds.Width;
                    y = (ActualHeight - s * g.Bounds.Height) / 2;
                }
                else
                {
                    s = ActualHeight / g.Bounds.Height;
                    x = (ActualWidth - s * g.Bounds.Width) / 2;
                }

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(new ScaleTransform(s, s));
                tg.Children.Add(new TranslateTransform(x - 28, y - 80));
                g.Transform = tg;
                drawingContext.DrawGeometry(Brushes.OrangeRed, new Pen(Brushes.Black, 2), g.GetFlattenedPathGeometry());
            }
            if (vm.VegeAMeccsnek)
            {
                string szoveg = "";
                if (vm.SajatGol > vm.EllenfelGol)
                    szoveg = "YOU WON!";
                else if (vm.EllenfelGol > vm.SajatGol)
                    szoveg = "YOU LOST";
                else
                    szoveg = "IT'S A DRAW";
                FormattedText vege = new FormattedText(szoveg, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.ExtraBold, FontStretches.Normal), 48, Brushes.White);
                Geometry g = vege.BuildGeometry(new Point(0, 0));
                double s, x = 0, y = 0;
                if (ActualWidth / ActualHeight < g.Bounds.Width / g.Bounds.Height)
                {
                    s = ActualWidth / g.Bounds.Width;
                    y = (ActualHeight - s * g.Bounds.Height) / 2;
                }
                else
                {
                    s = ActualHeight / g.Bounds.Height;
                    x = (ActualWidth - s * g.Bounds.Width) / 2;
                }

                TransformGroup tg = new TransformGroup();
                tg.Children.Add(new ScaleTransform(s, s));
                switch (szoveg)
                {
                    case "YOU WON!": tg.Children.Add(new TranslateTransform(x, y - 40)); break;
                    case "YOU LOST": tg.Children.Add(new TranslateTransform(x, y - 40)); break;
                    case "IT'S A DRAW": tg.Children.Add(new TranslateTransform(x - 17, y - 40)); break;
                }
                g.Transform = tg;
                drawingContext.DrawGeometry(Brushes.OrangeRed, new Pen(Brushes.Black, 2), g.GetFlattenedPathGeometry());
            }
        }
    }
}
