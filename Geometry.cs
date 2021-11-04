﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DShapes
{
    delegate void ActionRef<T>(ref T item);
    /// <summary>
    /// Тип проекции на экран
    /// </summary>
    public enum ProjectionType { ISOMETRIC, PERSPECTIVE, TRIMETRIC }
       /// <summary>
    /// Точка в пространстве
    /// </summary>
    class Point
    {
        double x, y, z;
        public static ProjectionType projection = ProjectionType.TRIMETRIC;
        public static PointF worldCenter;
        static Matrix isometricMatrix = new Matrix(3,3).fill(Math.Sqrt(3),0,-Math.Sqrt(3),1,2,1, Math.Sqrt(2),-Math.Sqrt(2), Math.Sqrt(2)) * (1/ Math.Sqrt(6));
        static Matrix trimetricMatrix = new Matrix(4, 4).fill(Math.Sqrt(3)/2, Math.Sqrt(2)/4, 0, 1, 0, Math.Sqrt(2)/2, 0, 1, 0.5,-Math.Sqrt(6)/4,0,0,0,0,0,1);
        static Matrix centralMatrix = new Matrix(4, 4).fill(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, k, 0, 0, 0, 1);
        const double k = 0.001f;
        public Point(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int X { get => (int)x; set => x = value; }
        public int Y { get => (int)y; set => y = value; }
        public int Z { get => (int)z; set => z = value; }

        public double Xf { get => x; set => x = value; }
        public double Yf { get => y; set => y = value; }
        public double Zf { get => z; set => z = value; }

        /// <summary>
        /// Перевод точки из 3D в 2D
        /// </summary>
        /// <returns>Точку на экране в вещестаенных координатах</returns>
        public PointF to2D(){
            if (projection == ProjectionType.PERSPECTIVE)
            {
                Matrix res = new Matrix(1, 4).fill(Xf, Yf, Zf, 1) * centralMatrix * (1/(k*Zf + 1));
                return new PointF(worldCenter.X + (float)res[0,0], worldCenter.Y + (float)res[0,1]);
            }
            else if (projection == ProjectionType.TRIMETRIC)
            {
                Matrix res = new Matrix(1, 4).fill(Xf, Yf, Zf, 1) * trimetricMatrix;
                return new PointF(worldCenter.X + (float)res[0,0], worldCenter.Y + (float)res[0, 1]);
            }
            else
            {
                Matrix res = new Matrix(3, 3).fill(1, 0, 0, 0, 1, 0, 0, 0, 0) * isometricMatrix * new Matrix(3, 1).fill(Xf, Yf, Zf);
                return new PointF(worldCenter.X + (float)res[0, 0], worldCenter.Y + (float)res[1, 0]);
            }
        }
    }
    /// <summary>
    /// Отрезок в пространстве
    /// </summary>
    class Line
    {
        public Point start,end;

        public Line(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }

        public Point Start { get => start; set => start = value; }
        public Point End { get => end; set => end = value; }
    }
    /// <summary>
    /// Грань фигуры, состоящая из конечного числа отрезков
    /// </summary>
    class Face
    {
        List<Line> edges;

        public Face()
        {
            edges = new List<Line>();
        }

        public Face(IEnumerable<Line> edges) : this()
        {
            this.edges.AddRange(edges);
        }

        public Face addEdge(Line edge)
        {
            edges.Add(edge);
            return this;
        }
        public Face addEdges(IEnumerable<Line> edges)
        {
            this.edges.AddRange(edges);
            return this;
        }

        public List<Line> Edges { get => edges; }

        /// <summary>
        /// Получение центра тяжести грани
        /// </summary>
        /// <returns><c>Point</c> - центр тяжести</returns>
        public Point getCenter()
        {
            double x = 0, y = 0, z = 0;
            foreach(var line in edges)
            {
                x += line.Start.Xf;
                y += line.Start.Yf;
                z += line.Start.Zf;
            }
            return new Point(x / edges.Count, y / edges.Count, z / edges.Count);
        }
    }

    /// <summary>
    /// Объёмная фигура, состоящая из граней
    /// </summary>
    class Shape
    {
        List<Face> faces;
      
        public Shape()
        {
            faces = new List<Face>();
        }

        public Shape(IEnumerable<Face> faces) : this()
        {
            this.faces.AddRange(faces);
        }

        public Shape addFace(Face face)
        {
            faces.Add(face);
            return this;
        }
        public Shape addFaces(IEnumerable<Face> faces)
        {
            this.faces.AddRange(faces);
            return this;
        }

        public List<Face> Faces { get => faces; }

        /// <summary>
        /// Преобразует все точки в фигуре по заданной функции
        /// </summary>
        /// <param name="f">Функция, преобразующая точку фигуры</param>
        public void transformPoints(ActionRef<Point> f)
        {
            foreach (var face in Faces)
            {
                foreach (var line in face.Edges)
                {
                    f(ref line.start);
                    f(ref line.end);
                }
            }
        }
       

        /// <summary>
        /// Виртуальный метод, чтобы наследники могли возвращать какую-то инфу для сохранения в файл
        /// </summary>
        /// <returns></returns>
        public virtual String getAdditionalInfo()
        {
            return "";
        }
    }

    class Tetrahedron : Shape
    {

    }

    class Octahedron : Shape
    {

    }

    class Hexahedron : Shape
    {

    }

    class Icosahedron : Shape
    {

    }

    class Dodecahedron : Shape
    {
        public override String getAdditionalInfo()
        {
            return "I AM DODECAHEDRON";
        }
    }
    /// <summary>
    /// Класс фигур вращения
    /// </summary>
   
    class RotationShape : Shape
    {
        List<Point> formingline;
        Line axiz;
        int Divisions;
        List<Point> allpoints;
        List<Line> edges;//ребра
        public RotationShape()
        {
            allpoints = new List<Point>();
            edges = new List<Line>();
        }
        public List<Line> Edges { get => edges; }
        public Shape addEdge(Line edge)
        {
            edges.Add(edge);
            return this;
        }
        public Shape addEdges(IEnumerable<Line> ed)
        {
            this.edges.AddRange(ed);
            return this;
        }

        public RotationShape(IEnumerable<Point> points) : this()
        {
            this.allpoints.AddRange(points);
        }
        public RotationShape(Line ax, int Div, IEnumerable<Point> line) : this()
        {
            this.axiz=ax;
            this.Divisions = Div;
            this.formingline.AddRange(line);
        }

        public RotationShape addPoint(Point p)
        {
            allpoints.Add(p);
            return this;
        }
        public RotationShape addPoints(IEnumerable<Point> points)
        {
            this.allpoints.AddRange(points);
            return this;
        }

        public List<Point> Points { get => allpoints; }
        
    }
   

    class Geometry
    {
        /// <summary>
        /// Переводит угол из градусов в радианы
        /// </summary>
        /// <param name="angle">Угол в градусах</param>
        ///
        public static double degreesToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Косинус из угла в градусах, ограниченный 5 знаками после запятой
        /// </summary>
        /// <param name="angle">Угол в градусах</param>
        /// <returns></returns>
        public static double Cos (int angle)
        {
            return Math.Round(Math.Cos(degreesToRadians(angle)), 5);
        }
        /// <summary>
        /// Синус из угла в градусах, ограниченный 5 знаками после запятой
        /// </summary>
        /// <param name="angle">Угол в градусах</param>
        /// <returns></returns>
        public static double Sin(int angle)
        {
            return Math.Round(Math.Sin(degreesToRadians(angle)), 5);
        }
        /// <summary>
        /// Перевод точки в другую точку
        /// </summary>
        /// <param name="p">Исзодная точка</param>
        /// <param name="matrix">Матрица перевода</param>
        /// 
        public static Point transformPoint(Point p, Matrix matrix)

        {
            var matrfrompoint = new Matrix(4, 1).fill(p.X, p.Y, p.Z,1);

            var matrPoint = matrix * matrfrompoint;//применение преобразования к точке
            //Point newPoint = new Point(matrPoint[0, 0] / matrPoint[3, 0], matrPoint[1, 0] / matrPoint[3, 0], matrPoint[2, 0] / matrPoint[3, 0]);
            Point newPoint = new Point(matrPoint[0, 0], matrPoint[1, 0], matrPoint[2, 0]);
            return newPoint;

        }
        /// <summary>
        /// Перевод всех точек для тела вращения в другие точки
        /// </summary>
        /// <param name="matrix">Матрица перевода</param>
        /// 
        public static List<Point> transformPointsRotationFig(Matrix matrix,List<Point> allpoints)
        {
            List<Point> clone = allpoints;
            List<Point> res = new List<Point>();
            foreach (var p in clone)
            {

                Point newp = transformPoint(p, matrix);
                res.Add(newp);
            }
            return res;
        }/// <summary>
        /// Поворот образующей для фигуры вращения
        /// </summary>
        /// <param name="general">образующая</param>
        /// <param name="axis">Ось вращения</param>
        /// <param name="angle">угол вращения</param>

        /// <returns></returns>
        public static List<Point> RotatePoint(List<Point> general, AxisType axis, double angle)
        {
            List<Point> res;
            double mysin = Math.Sin(Geometry.degreesToRadians(angle));
            double mycos = Math.Cos(Geometry.degreesToRadians(angle));
            Matrix rotation = new Matrix(0, 0);

            switch (axis)
            {
                case AxisType.X:
                    rotation = new Matrix(4, 4).fill(1, 0, 0, 0, 0, mycos, -mysin, 0, 0, mysin, mycos, 0, 0, 0, 0, 1);
                    break;
                case AxisType.Y:
                    rotation = new Matrix(4, 4).fill(mycos, 0, mysin, 0, 0, 1, 0, 0, -mysin, 0, mycos, 0, 0, 0, 0, 1);
                    break;
                case AxisType.Z:
                    rotation = new Matrix(4, 4).fill(mycos, -mysin, 0, 0, mysin, mycos, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                    break;
            }

            res = Geometry.transformPointsRotationFig(rotation, general);

            return res;            
        }
    }

    /// <summary>
    /// Класс для получения фигур различного типа
    /// </summary>
    class ShapeGetter
    {
        /// <summary>
        /// Получает фигуру фиксированного размера (в среднем до 200 пикселей по размеру)
        /// </summary>
        /// <param name="type">Тип фигуры</param>
        /// <returns></returns>
        public static Shape getShape(ShapeType type)
        {
            switch (type)
            {
                case ShapeType.TETRAHEDRON: return getTetrahedron();
                case ShapeType.OCTAHEDRON: return getOctahedron();
                case ShapeType.HEXAHEDRON: return getHexahedron();
                case ShapeType.ICOSAHEDRON: return getIcosahedron();
                case ShapeType.DODECAHEDRON: return getDodecahedron();
               

                default: throw new Exception("C# очень умный (нет)");
            }
        }
       
        /// <summary>
        /// Получение тетраэдра
        /// </summary>
        /// <returns></returns>
        public static Tetrahedron getTetrahedron()
        {
            Tetrahedron res = new Tetrahedron();
            Point a = new Point(0, 0, 0);
            Point c = new Point(200, 0, 200);
            Point f = new Point(200, 200, 0);
            Point h = new Point(0, 200, 200);
            res.addFace(new Face().addEdge(new Line(a, f)).addEdge(new Line(f, c)).addEdge(new Line(c, a)));
            res.addFace(new Face().addEdge(new Line(f, c)).addEdge(new Line(c, h)).addEdge(new Line(h, f)));
            res.addFace(new Face().addEdge(new Line(c, h)).addEdge(new Line(h, a)).addEdge(new Line(a, c)));
            res.addFace(new Face().addEdge(new Line(f, h)).addEdge(new Line(h, a)).addEdge(new Line(a, f)));
            return res;
        }

        /// <summary>
        /// Получение октаэдра
        /// </summary>
        /// <returns></returns>
        public static Octahedron getOctahedron()
        {
            Octahedron res = new Octahedron();
            //Point a = new Point(100, 100, 0);
            //Point b = new Point(200, 100, 100);
            //Point c = new Point(100, 100, 200);
            //Point d = new Point(0, 100, 100);
            //Point e = new Point(100, 200, 100);
            //Point f = new Point(100, 0, 100);
            var cube = getHexahedron();
            Point a = cube.Faces[0].getCenter();
            Point b = cube.Faces[1].getCenter();
            Point c = cube.Faces[2].getCenter();
            Point d = cube.Faces[3].getCenter();
            Point e = cube.Faces[4].getCenter();
            Point f = cube.Faces[5].getCenter();

            res.addFace(new Face().addEdge(new Line(a, f)).addEdge(new Line(f, b)).addEdge(new Line(b, a)));
            res.addFace(new Face().addEdge(new Line(b, c)).addEdge(new Line(c, f)).addEdge(new Line(f, b)));
            res.addFace(new Face().addEdge(new Line(c, d)).addEdge(new Line(d, f)).addEdge(new Line(f, c)));
            res.addFace(new Face().addEdge(new Line(d, a)).addEdge(new Line(a, f)).addEdge(new Line(f, d)));
            res.addFace(new Face().addEdge(new Line(a, e)).addEdge(new Line(e, b)).addEdge(new Line(b, a)));
            res.addFace(new Face().addEdge(new Line(b, e)).addEdge(new Line(e, c)).addEdge(new Line(c, b)));
            res.addFace(new Face().addEdge(new Line(c, e)).addEdge(new Line(e, d)).addEdge(new Line(d, c)));
            res.addFace(new Face().addEdge(new Line(d, e)).addEdge(new Line(e, a)).addEdge(new Line(a, d)));
            return res;
        }

        /// <summary>
        /// Получение гексаэдра (куба)
        /// </summary>
        /// <returns></returns>
        public static Hexahedron getHexahedron()
        {
            Hexahedron res = new Hexahedron();
            Point a = new Point(0, 0, 0);
            Point b = new Point(200, 0, 0);
            Point c = new Point(200, 0, 200);
            Point d = new Point(0, 0, 200);
            Point e = new Point(0, 200, 0);
            Point f = new Point(200, 200, 0);
            Point g = new Point(200, 200, 200);
            Point h = new Point(0, 200, 200);
            res.addFace(new Face().addEdge(new Line(a, b)).addEdge(new Line(b, c)).addEdge(new Line(c, d)).addEdge(new Line(d, a)));
            res.addFace(new Face().addEdge(new Line(b, c)).addEdge(new Line(c, g)).addEdge(new Line(g, f)).addEdge(new Line(f, b)));
            res.addFace(new Face().addEdge(new Line(f, g)).addEdge(new Line(g, h)).addEdge(new Line(h, e)).addEdge(new Line(e, f)));
            res.addFace(new Face().addEdge(new Line(h, e)).addEdge(new Line(e, a)).addEdge(new Line(a, d)).addEdge(new Line(d, h)));
            res.addFace(new Face().addEdge(new Line(a, b)).addEdge(new Line(b, f)).addEdge(new Line(f, e)).addEdge(new Line(e, a)));
            res.addFace(new Face().addEdge(new Line(d, c)).addEdge(new Line(c, g)).addEdge(new Line(g, h)).addEdge(new Line(h, d)));
            return res;
        }

        /// <summary>
        /// Получение икосаэдра
        /// </summary>
        /// <returns></returns>
        public static Icosahedron getIcosahedron()
        {
            Icosahedron res = new Icosahedron();
            Point circleCenter = new Point(100, 100, 100);
            List<Point> circlePoints = new List<Point>();
            for(int angle = 0; angle < 360; angle += 36)
            {
                if( angle % 72 == 0)
                {
                    circlePoints.Add(new Point(circleCenter.X + (100 * Math.Cos(Geometry.degreesToRadians(angle))), circleCenter.Y + 100, circleCenter.Z + (100 * Math.Sin(Geometry.degreesToRadians(angle)))));
                    continue;
                }
                circlePoints.Add(new Point(circleCenter.X + (100 * Math.Cos(Geometry.degreesToRadians(angle))), circleCenter.Y, circleCenter.Z + (100 * Math.Sin(Geometry.degreesToRadians(angle)))));
            }
            Point a = new Point(100, 50, 100);
            Point b = new Point(100, 250, 100);
            for(int i = 0; i < 10; i++)
            {
                res.addFace(new Face().addEdge(new Line(circlePoints[i], circlePoints[(i + 1) % 10])).addEdge(new Line(circlePoints[(i + 1) % 10], circlePoints[(i + 2) % 10])).addEdge(new Line(circlePoints[(i+2) % 10], circlePoints[i])));
            }
            res.addFace(new Face().addEdge(new Line(circlePoints[1], a)).addEdge(new Line(a, circlePoints[3])).addEdge(new Line(circlePoints[3], circlePoints[1])));
            res.addFace(new Face().addEdge(new Line(circlePoints[3], a)).addEdge(new Line(a, circlePoints[5])).addEdge(new Line(circlePoints[5], circlePoints[3])));
            res.addFace(new Face().addEdge(new Line(circlePoints[5], a)).addEdge(new Line(a, circlePoints[7])).addEdge(new Line(circlePoints[7], circlePoints[5])));
            res.addFace(new Face().addEdge(new Line(circlePoints[7], a)).addEdge(new Line(a, circlePoints[9])).addEdge(new Line(circlePoints[9], circlePoints[7])));
            res.addFace(new Face().addEdge(new Line(circlePoints[9], a)).addEdge(new Line(a, circlePoints[1])).addEdge(new Line(circlePoints[1], circlePoints[9])));

            res.addFace(new Face().addEdge(new Line(circlePoints[0], b)).addEdge(new Line(b, circlePoints[2])).addEdge(new Line(circlePoints[2], circlePoints[0])));
            res.addFace(new Face().addEdge(new Line(circlePoints[2], b)).addEdge(new Line(b, circlePoints[4])).addEdge(new Line(circlePoints[4], circlePoints[2])));
            res.addFace(new Face().addEdge(new Line(circlePoints[4], b)).addEdge(new Line(b, circlePoints[6])).addEdge(new Line(circlePoints[6], circlePoints[4])));
            res.addFace(new Face().addEdge(new Line(circlePoints[6], b)).addEdge(new Line(b, circlePoints[8])).addEdge(new Line(circlePoints[8], circlePoints[6])));
            res.addFace(new Face().addEdge(new Line(circlePoints[8], b)).addEdge(new Line(b, circlePoints[0])).addEdge(new Line(circlePoints[0], circlePoints[8])));
            return res;
        }

        /// <summary>
        /// Получение додекаэдра
        /// </summary>
        /// <returns></returns>
        public static Dodecahedron getDodecahedron()
        {
            Dodecahedron res = new Dodecahedron();
            var icosahedron = getIcosahedron();
            List<Point> centers = new List<Point>();
            for (int i = 0; i < icosahedron.Faces.Count; i++)
            {
                Face face = icosahedron.Faces[i];
                var c = face.getCenter();
                centers.Add(c);
            }

            //res.addFace(new Face().addEdge(new Line(centers[9], centers[0])).addEdge(new Line(centers[0], centers[1])).addEdge(new Line(centers[1], centers[10])).addEdge(new Line(centers[10], centers[14])).addEdge(new Line(centers[14], centers[9])));
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    res.addFace(new Face().addEdge(new Line(centers[i], centers[(i + 1) % 10])).addEdge(new Line(centers[(i + 1) % 10], centers[(i + 2) % 10])).addEdge(new Line(centers[(i + 2) % 10], centers[15 + (i / 2 + 1) % 5])).addEdge(new Line(centers[15 + (i / 2 + 1) % 5], centers[15 + i / 2])).addEdge(new Line(centers[15 + i/ 2], centers[i])));

                    continue;
                }
                res.addFace(new Face().addEdge(new Line(centers[i], centers[(i + 1) % 10])).addEdge(new Line(centers[(i + 1) % 10], centers[(i + 2) % 10])).addEdge(new Line(centers[(i + 2) % 10], centers[10 + (i / 2 + 1) % 5])).addEdge(new Line(centers[10 + (i / 2 + 1) % 5], centers[10 + i / 2])).addEdge(new Line(centers[10 + i / 2], centers[i])));
            }
            res.addFace(new Face().addEdge(new Line(centers[15], centers[16])).addEdge(new Line(centers[16], centers[17])).addEdge(new Line(centers[17], centers[18])).addEdge(new Line(centers[18], centers[19])).addEdge(new Line(centers[19], centers[15])));
            res.addFace(new Face().addEdge(new Line(centers[10], centers[11])).addEdge(new Line(centers[11], centers[12])).addEdge(new Line(centers[12], centers[13])).addEdge(new Line(centers[13], centers[14])).addEdge(new Line(centers[14], centers[10])));

            return res;
        }

        /// 
        /// <summary>
        /// Получение фигуры вращения
        /// </summary>
        /// <returns></returns>
        public static RotationShape getRotationShape(List<Point> general, int divisions, AxisType axis)
        {
            RotationShape res = new RotationShape();
             List<Point> genline = general;
            int GeneralCount = genline.Count();
            //Line axis;
            int Count = divisions;
            double angle = (360.0 / Count);//угол 
            List<Line> edges;//ребра
           
            res.addPoints(genline);//добавили образующую
            for (int i = 1; i < divisions; i++)//количество разбиений
            {
                res.addPoints(Geometry.RotatePoint(genline, axis, angle * i));
            }
            //
         
            //Фигура вращения задаётся тремя параметрами: образующей(набор точек), осью вращения и количеством разбиений
            //зададим ребра и грани
            for (int i = 0; i < divisions; i++)
            {
                for (int j = 0;  j < GeneralCount; j++)
                {
                    int index = i * GeneralCount + j;//индекс точки
                    if (index < divisions * GeneralCount)
                    {
                        int e = (index + GeneralCount) % res.Points.Count;
                        if ((index + 1) % GeneralCount == 0)
                        {

                            // res.addFace(new Face().addEdge(new Line( res.Points[current], res.Points[e])));
                            res.addEdge(new Line(res.Points[index], res.Points[e]));
                        }
                        else
                        {
                            res.addEdge(new Line(res.Points[index], res.Points[index + 1]));
                            res.addEdge(new Line(res.Points[index], res.Points[e]));
                            int e1 = (index + 1 + GeneralCount) % res.Points.Count;
                            //добавим грань
                            res.addFace(new Face().addEdge(new Line(res.Points[index], res.Points[index + 1])).addEdge(new Line(res.Points[index + 1], res.Points[e1])).addEdge(new Line(res.Points[e1], res.Points[e])).addEdge(new Line(res.Points[e], res.Points[index])));
                        }
                    }
                    
                }


            }
           
            return res;
        }
        
    }
}
