//#define LINQ_DIST
#define POINT_NUM
//#define SCREEN_RES
//#define DYN_SEED
#define STATIC_GEN
#define GRID_GEN

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StaticVoronoi : MonoBehaviour {

	public int m_BiomeSize = 32;

#if POINT_NUM
	public int p_Num = 3;
#else
	int p_Num = 0;
#endif
	
	public bool drawPoints = true;

#if SCREEN_RES
	int x_size = Screen.width, y_size = Screen.height;
#else
	public int x_size = 512, y_size = 512;
#endif
	Texture2D tex;
	VoronoiMap map;
	List<Point> points;
#if DYN_SEED
	int seed = new System.Random().Next();
#else
	public int seed;
#endif

	// Use this for initialization
	void Start() {
		if(p_Num == 0) p_Num = Random.Range(3, 20);
		Generate();
	}

	void Update() {
#if STATIC_GEN
		if(Input.GetMouseButtonDown(0)) {
			Point newp = new Point(Input.mousePosition.x-Screen.width/2+x_size/2, Input.mousePosition.y-Screen.height/2+y_size/2);
			//if(points == null && map != null) points = map.v;
			points.Add(newp);
			Debug.Log("Added point at "+newp.ToString()+"!");
			Generate();
		}
#endif
	}

	void OnGUI() {
		if(tex != null) {
			GUI.DrawTexture(new Rect(Screen.width/2-x_size/2, Screen.height/2-y_size/2, x_size, y_size), tex);
		}
	}

	void Generate() {
		TM.Start();
		tex = new Texture2D(x_size, y_size);
#if STATIC_GEN
		if(points == null) {
			map = new VoronoiMap(x_size, y_size, seed, p_Num);
			points = map.v;
		} else {
			map = new VoronoiMap(x_size, y_size, points);
		}
		tex = map.GetVoronoi(drawPoints);
#else
		/*map = new VoronoiMap(x_size, y_size) {m_CellSize = m_BiomeSize};
		for(int i = 0; i < x_size; i++) {
			for(int j = 0; j < y_size; j++)
				tex.SetPixel(i, j, map.GetValue(i, j));
		}*/
#endif
		Debug.Log("Generated in "+TM.Stop()+" ms!");
	}

}

public class VoronoiMap {
#if STATIC_GEN
	public int size_x, size_y, seed;
	public List<Point> v;
	public VoronoiMap() : this(Screen.width, Screen.height) 
	{
	}
	public VoronoiMap(int x_res, int y_res) { //Dynamic model: you can use getValue on there because it uses JittledGrid
		size_x = x_res;
		size_y = y_res;
	}
	public VoronoiMap(int x_res, int y_res, int s, int n) { //Static model of Voronoi Map
		v = Point.GetRandomPoints(n, x_res, y_res, s);
		size_x = x_res;
		size_y = y_res;
		seed = s;
	}
	public VoronoiMap(int x_res, int y_res, List<Point> p) { //Static model of Voronoi Map
		v = p;
		size_x = x_res;
		size_y = y_res;
	}
	public Texture2D GetVoronoi(bool drawPoints = true) { //Static voronoi: this uses RandomGrid type
		if(v == null) {
			Debug.LogError("Necesitas definir la lista de puntos."); 
			return null;
		}
		Texture2D tex = new Texture2D(size_x, size_y) {filterMode = FilterMode.Point};
		for(int i = 0; i < size_x; i++) {
			for(int j = 0; j < size_y; j++)
				tex.SetPixel(i, j, getColor(i, j, v));
		}
		if(drawPoints) {
			DrawPoints(ref tex);
		}
		tex.Apply();
		return tex;
	}
	private Color getColor(float x, float y, List<Point> points) {
		#if LINQ_DIST
		Point v = points
			.Select(p => new { Pt = p, Dist = distance(p.x, p.y, x, y) })
				.Aggregate((p1, p2) => p1.Dist < p2.Dist ? p1 : p2)
				.Pt;
		return ((v == null) ? Color.black : v.c);
		#else
		Color c = Color.black;
		float d = -1;
		foreach(Point p in points)
		{
			float actual = distance(p.x, p.y, x, y);
			if (actual < d || d == -1)
			{
				d = actual;
				c = p.c;
			}
		}
		return c;
		#endif
	}
	public void DrawPoints(ref Texture2D t, List<Point> points = null) {
		if(points == null) points = v;
		foreach (Point p in points)
		{
			for (int i = -1; i <= 1; i++)
				for (int j = -1; j <= 1; j++)
					t.SetPixel((int)p.x + i, (int)p.y + j, Color.black);
		}
		t.Apply();
	}
#else
	public int m_CellSize;
	public Color GetValue(int x, int y) {
		//TAREAS: Aqui hay que calcular el grid correspondiente y aplicarle un jitter a cada uno usando la seed. 
		//El problema aqui es que al ser dinamico los demas puntos no estaran creados ('v' no estara disponible) por lo que habra que hacer un apaño
		//v = new List<Point>();
		for(int i = -1; i <= 1; i++) {
			for(int j = -1; j <= 1; j++) {
				Point dp = new Point(i*(m_CellSize/2), j*(m_CellSize/2));
			}
		}
		return Color.black;
		//return getColor(x, y, v);
	}
#endif
	private float distance(float x1, float y1, float x2, float y2) {
		//return Mathf.Abs(x_pos - x) + Mathf.Abs(y_pos - y);
		return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2));
	}
}

public class Point {
	public Point(Point pt) : this(pt.x, pt.y, pt.c) 
	{
	}
	public Point(float x, float y) : this(x, y, new Color(Random.value, Random.value, Random.value)) 
	{
	}
#if STATIC_GEN
	public Point(int x_res, int y_res, int s) { //Static
		Point sp = RandomPoint(x_res, y_res, s);
		x = sp.x;
		y = sp.y;
		c = sp.c;
		seed = s;
	}
#endif
	public Point(float x, float y, Color cl) {
		this.x = x;
		this.y = y;
		c = cl;
	}
	public float x, y;
	public Color c;
	public int seed;
	//For static voronoi method
#if STATIC_GEN
	public Point RandomPoint(int max_x, int max_y, int s) {
		System.Random rnd = new System.Random(s);
#if GRID_GEN
		return new Point(max_x/2, max_y/2, new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));
#else
		return new Point(rnd.Next(0, max_x), rnd.Next(0, max_y), new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));
#endif	
	}
	public static List<Point> GetRandomPoints(int res_x, int res_y, int s) {
		List<Point> p = GetRandomPoints(Random.Range(3, 20), res_x, res_y, s);
		return p;
	}
	public static List<Point> GetRandomPoints(int num_Points, int x_res, int y_res, int s) {
		List<Point> pList = new List<Point>();
#if GRID_GEN
		for(int i = 0; i < num_Points; i++) {
			for(int j = 0; j < num_Points; j++) {
				Point p = new Point((x_res/num_Points)*i, (y_res/num_Points)*j, s+(i*num_Points+j));
				if(!pList.Contains(p)) {
					pList.Add(p);
				}
			}
		}
#else
		int i = 0;
		while(i < num_Points) {
			Point p = new Point(x_res, y_res, s+i);
			if(!pList.Contains(p)) {
				pList.Add(p);
				i++;
			}
		}
#endif
		return pList;
	}
#endif
	public override string ToString() {
		return "("+x+", "+y+") ["+c+"]";
	}
}

public class TM {
	
	static System.Diagnostics.Stopwatch watch;
	public static long ElapsedTime;
	
	public static void Start() {
		watch = System.Diagnostics.Stopwatch.StartNew();
	}
	
	public static long Stop() {
		long internalTime = 0;
		if(watch != null) {
			watch.Stop();
			internalTime = watch.ElapsedMilliseconds;
		}
		watch = null;
		ElapsedTime += internalTime;
		return internalTime;
	}
	
}